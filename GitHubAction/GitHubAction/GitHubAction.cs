using GitHubAction.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        private readonly TimeSpan _deploymentBackOff;
        private readonly TimeSpan _deploymentMaxBackOff;

        public GitHubAction(
            IPackageService packageService,
            IPackagePresenter packagePresenter,
            ILogger<GitHubAction> logger)
        {
            _packageService = packageService;
            _packagePresenter = packagePresenter;
            _logger = logger;
            _deploymentBackOff = TimeSpan.FromSeconds(3);
            _deploymentMaxBackOff = TimeSpan.FromMinutes(2);
        }

        public async Task RunAsync(string[] args, CancellationToken cancellationToken)
        {
            var inputs = ParseInputs.ParseAndValidateInputs(args, _logger);
            if (inputs == null)
            {
                _logger.LogError("There was a problem with the provided arguments...");
                Environment.Exit(400); // Bad Request
                return;
            }

            UploadedPackage? uploadedPackage = null;

            try
            { 
                // BuildAndPublish
                if (inputs.Stage == Stages.All || inputs.Stage == Stages.BuildAndPublish)
                {
                    var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));

                    var localPackageConfig = new LocalPackageConfig(
                        new FileInfo(inputs.SolutionPath!),
                        workingDirectory,
                        inputs.PackageName!,
                        inputs.Version!,
                        SolutionType.DmScript);

                    var createdPackage = await CreatePackage(localPackageConfig);
                    if (createdPackage == null) return;

                    uploadedPackage = await UploadPackage(inputs.ApiKey, createdPackage);
                    if(uploadedPackage == null) return;
                }

                // Deploy
                if (inputs.Stage == Stages.All || inputs.Stage == Stages.Deploy)
                {
                    if (uploadedPackage == null)
                    {
                        uploadedPackage = new UploadedPackage(inputs.ArtifactId!);
                    }
                    var deployingPackage = await DeployPackage(inputs.ApiKey, uploadedPackage);
                    if (deployingPackage == null) return;

                    var deployedPackage = await ConfirmSuccesfullDeployment(inputs.ApiKey, _deploymentBackOff, _deploymentMaxBackOff, inputs.TimeOut, deployingPackage);
                    if(deployedPackage == null)  return;
                    _packagePresenter.PresentPackageDeploymentFinished(deployedPackage.Status);
                }
                
            }
            catch (KeyException)
            {
                _packagePresenter.PresentUnauthorizedKey();
            }
        }

        private async Task<DeployedPackage?> ConfirmSuccesfullDeployment(string key, TimeSpan deploymentBackOff, TimeSpan deploymentMaxBackOff,
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
                    (output) => output is {Status: "Succeeded"},
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

            return deployedPackage;
        }

        private async Task<DeployingPackage?> DeployPackage(string key, UploadedPackage uploadedPackage)
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

        private async Task<UploadedPackage?> UploadPackage(string key, CreatedPackage createdPackage)
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

        private async Task<CreatedPackage?> CreatePackage(LocalPackageConfig localPackageConfig)
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