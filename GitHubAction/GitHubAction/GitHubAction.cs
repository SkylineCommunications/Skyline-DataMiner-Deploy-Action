using GitHubAction.Domain.Entities;
using GitHubAction.Factories;
using GitHubAction.Presenters;
using Package.Builder;
using Package.Builder.Exceptions;
using Package.Domain.Enums;
using Package.Domain.Exceptions;
using Package.Domain.Models;
using Package.Domain.Services;

namespace GitHubAction
{
    public class GitHubAction
    {
        private readonly IPackageService _packageService;
        private readonly IPackagePresenter _packagePresenter;
        private readonly IOutputPresenter _outputPresenter;
        private readonly IInputFactory _inputParserSerivce;
        private readonly TimeSpan _deploymentBackOff;
        private readonly TimeSpan _deploymentMaxBackOff;
        private ISourceUriService _sourceUriService;

        public GitHubAction(IPackageService packageService, IInputFactory inputParser, IPackagePresenter packagePresenter, IOutputPresenter outputPresenter, ISourceUriService sourceUriService) 
            : this(packageService, inputParser, packagePresenter, outputPresenter, TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(2), sourceUriService)
        {

        }

        public GitHubAction(IPackageService packageService, IInputFactory inputParser, IPackagePresenter packagePresenter,
            IOutputPresenter outputPresenter, TimeSpan minimumBackOff, TimeSpan maximumBackOff, ISourceUriService sourceUriService)
        {
            _packageService = packageService;
            _inputParserSerivce = inputParser;
            _packagePresenter = packagePresenter;
            _outputPresenter = outputPresenter;
            _deploymentBackOff = minimumBackOff;
            _deploymentMaxBackOff = maximumBackOff;
            _sourceUriService = sourceUriService;
        }

        public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
        {
            var inputs = _inputParserSerivce.ParseAndValidateInputs(args);
            if (inputs == null)
            {
                _outputPresenter.PresentInvalidArguments();
                return 3;
            }

            var sourceUri = _sourceUriService.GetSourceUri();

            UploadedPackage? uploadedPackage = null;

            try
            { 
                // Upload
                if (inputs is { Stage: Stage.All or Stage.Upload })
                {


                    var localPackageConfig = new LocalPackageConfig(
                        new FileInfo(inputs.SolutionPath!),
                        inputs.PackageName!,
                        inputs.Version!,
                        SolutionType.DmScript,
                        sourceUri);

                    var createdPackage = await CreatePackageAsync(localPackageConfig);
                    if (createdPackage == null) return 4;

                    uploadedPackage = await UploadPackageAsync(inputs.ApiKey, createdPackage);
                    if(uploadedPackage == null) return 5;
                    _outputPresenter.PresentOutputVariable("artifact-id", uploadedPackage.ArtifactId);
                    
                }

                // Deploy
                if (inputs is {Stage: Stage.All or Stage.Deploy})
                {
                    if (uploadedPackage == null)
                    {
                        uploadedPackage = new UploadedPackage(inputs.ArtifactId!);
                    }
                    var deployingPackage = await DeployPackageAsync(inputs.ApiKey, uploadedPackage);
                    if (deployingPackage == null) return 6;

                    var deployedPackage = await ConfirmSuccesfullDeploymentAsync(inputs.ApiKey, _deploymentBackOff, _deploymentMaxBackOff, inputs.TimeOut, deployingPackage);
                    if(deployedPackage == null)  return 7;
                    _packagePresenter.PresentPackageDeploymentFinished(deployedPackage.Status);
                }
                
            }
            catch (KeyException)
            {
                _packagePresenter.PresentUnauthorizedKey();
                return 8;
            }

            return 0;
        }

        private async Task<DeployedPackage?> ConfirmSuccesfullDeploymentAsync(string key, TimeSpan deploymentBackOff, TimeSpan deploymentMaxBackOff,
            TimeSpan deploymentTimeout, DeployingPackage deployingPackage)
        {
            DeployedPackage deployedPackage;
            try
            {
                _packagePresenter.PresentWaitingForFinishedPackageDeployment((int) deploymentBackOff.TotalSeconds);
                await Task.Delay((int) deploymentBackOff.TotalMilliseconds);

                deployedPackage = (await Utils.ExecuteWithRetryAsync(
                    async () =>
                    {
                        try
                        {
                            return await _packageService.GetDeployedPackageAsync(deployingPackage, key);
                        }
                        catch (GetDeploymentPackageException)
                        {
                            _packagePresenter.PresentCouldNotFetchTheDeployedPackage();
                            return null;
                        }
                    },
                    (output) => output is {Status: "Succeeded" or "Timeout" or "Error" },
                    (backOffDelaySeconds) =>
                        _packagePresenter.PresentWaitingMoreForFinishedPackageDeployment(backOffDelaySeconds),
                    deploymentBackOff,
                    deploymentMaxBackOff,
                    deploymentTimeout.Subtract(deploymentBackOff)))!;
            }
            catch (TimeoutException)
            {
                _packagePresenter.PresentPackageDeploymentTimeout();
                return null;
            }

            if (deployedPackage is {Status: "Timeout" or "Error"})
            {
                _packagePresenter.PresentPackageDeploymentFailed(deployedPackage);
                return null;
            }

            return deployedPackage;
        }

        private async Task<DeployingPackage?> DeployPackageAsync(string key, UploadedPackage uploadedPackage)
        {
            _packagePresenter.PresentStartingPackageDeployment();

            DeployingPackage deployingPackage;
            try
            {
                deployingPackage = await _packageService.DeployPackageAsync(uploadedPackage, key);
            }
            catch (DmsUnavailableException)
            {
                _packagePresenter.PresentDataMinerSystemUnavailable();
                return null;
            }
            catch (DeployPackageException e)
            {
                _packagePresenter.PresentStartingPackageDeploymentFailed(e);
                return null;
            }

            return deployingPackage;
        }

        private async Task<UploadedPackage?> UploadPackageAsync(string key, CreatedPackage createdPackage)
        {
            _packagePresenter.PresentStartingPackageUpload();

            UploadedPackage uploadedPackage;
            try
            {
                uploadedPackage = await _packageService.UploadPackageAsync(createdPackage, key);
            }
            catch (UploadPackageException e)
            {
                _packagePresenter.PresentPackageUploadFailed(e);
                return null;
            }

            _packagePresenter.PresentPackageUploadSucceeded();
            return uploadedPackage;
        }

        private async Task<CreatedPackage?> CreatePackageAsync(LocalPackageConfig localPackageConfig)
        {
            _packagePresenter.PresentStartCreatingPackage();
            CreatedPackage createdPackage;
            try
            {
                createdPackage = await _packageService.CreatePackageAsync(localPackageConfig);
            }
            catch (CreatePackageException e)
            {
                _packagePresenter.PresentPackageCreationFailed(e);
                return null;
            }
            catch (UnsupportedSolutionException)
            {
                _packagePresenter.PresentUnsupportedSolutionType();
                return null;
            }

            _packagePresenter.PresentPackageCreationSucceeded();
            return createdPackage;
        }
    }
}