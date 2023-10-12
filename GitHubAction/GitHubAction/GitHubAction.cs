namespace GitHubAction
{
    using Catalog.Domain;

    using Domain.Entities;

    using Factories;

    using GIT;

    using Package.Builder;
    using Package.Builder.Exceptions;
    using Package.Domain.Enums;
    using Package.Domain.Exceptions;
    using Package.Domain.Models;
    using Package.Domain.Services;

    using Presenters;

    public class GitHubAction
    {
        private readonly TimeSpan _deploymentBackOff;
        private readonly TimeSpan _deploymentMaxBackOff;
        private readonly IEnvironmentVariableService _EnvVarService;
        private readonly IGitInfo _git;
        private readonly IInputFactory _inputParserSerivce;
        private readonly IOutputPathProvider _outputPathProvider;
        private readonly IOutputPresenter _outputPresenter;
        private readonly IPackagePresenter _packagePresenter;
        private readonly IPackageService _packageService;

        public GitHubAction(IPackageService packageService, IInputFactory inputParser, IPackagePresenter packagePresenter, IOutputPresenter outputPresenter, IEnvironmentVariableService sourceUriService, IOutputPathProvider outputPathProvider, IGitInfo git)
            : this(packageService, inputParser, packagePresenter, outputPresenter, TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(2), sourceUriService, outputPathProvider, git)
        {
        }

        public GitHubAction(IPackageService packageService, IInputFactory inputParser, IPackagePresenter packagePresenter,
            IOutputPresenter outputPresenter, TimeSpan minimumBackOff, TimeSpan maximumBackOff, IEnvironmentVariableService envVarService, IOutputPathProvider outputPathProvider, IGitInfo git)
        {
	        Console.WriteLine("Creating GitHubAction");
            _packageService = packageService;
            _inputParserSerivce = inputParser;
            _packagePresenter = packagePresenter;
            _outputPresenter = outputPresenter;
            _deploymentBackOff = minimumBackOff;
            _deploymentMaxBackOff = maximumBackOff;
            _EnvVarService = envVarService;
            _outputPathProvider = outputPathProvider;
            _git = git;
        }

        public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
        {
            Console.WriteLine("Parsing inputs");
            var inputs = _inputParserSerivce.ParseAndValidateInputs(args);
            Console.WriteLine($"Inputs are null: {inputs == null}");
            if (inputs == null)
            {
                _outputPresenter.PresentInvalidArguments();
                return 3;
            }

            var sourceUri = _EnvVarService.GetSourceUri();
            var branch = _EnvVarService.GetBranch();
            var releaseUri = _EnvVarService.GetReleaseUri();

            Console.WriteLine($"Source URI: {sourceUri}");
            Console.WriteLine($"Branch: {branch}");
            Console.WriteLine($"Release URI: {releaseUri}");

            UploadedPackage? uploadedPackage = null;

            string basePath = inputs.BasePath ?? Directory.GetCurrentDirectory();
            _outputPathProvider.BasePath = basePath;
            _git.Initialize(basePath);

            try
            {
                // Upload
                if (inputs is { Stage: Stage.All or Stage.Upload })
                {
                    var localPackageConfig = new LocalPackageConfig(
                        new FileInfo(inputs.SolutionPath!),
                        inputs.PackageName!,
                        inputs.Version!,
                        ArtifactContentType.DmScript,
                        sourceUri,
                        inputs.BuildNumber!,
                        inputs.Debug == true);
                    Console.WriteLine("Before CreatePackageAsync");
                    var createdPackage = await CreatePackageAsync(localPackageConfig);
                    Console.WriteLine($"After CreatePackageAsync (is null): {createdPackage == null}");
                    if (createdPackage == null) return 4;

                    Console.WriteLine("Before CatalogDataFactory.Create");
                    var catalog = CatalogDataFactory.Create(inputs, createdPackage, _git, sourceUri?.ToString() ?? "", branch, releaseUri?.ToString() ?? "");
                    Console.WriteLine("After CatalogDataFactory.Create");
                    Console.WriteLine("Before UploadPackageAsync");
                    uploadedPackage = await UploadPackageAsync(inputs.ApiKey, createdPackage, catalog);
                    Console.WriteLine($"After UploadPackageAsync (is null): {uploadedPackage == null}");

                    if (uploadedPackage == null) return 5;
                    _outputPresenter.PresentOutputVariable("ARTIFACT_ID", uploadedPackage.ArtifactId);
                }

                // Deploy
                if (inputs is { Stage: Stage.All or Stage.Deploy })
                {
                    if (uploadedPackage == null)
                    {
                        uploadedPackage = new UploadedPackage(inputs.ArtifactId!);
                    }
                    var deployingPackage = await DeployPackageAsync(inputs.ApiKey, uploadedPackage);
                    if (deployingPackage == null) return 6;

                    var deployedPackage = await ConfirmSuccesfullDeploymentAsync(inputs.ApiKey, _deploymentBackOff, _deploymentMaxBackOff, inputs.TimeOut, deployingPackage);
                    if (deployedPackage == null) return 7;
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
                _packagePresenter.PresentWaitingForFinishedPackageDeployment((int)deploymentBackOff.TotalSeconds);
                await Task.Delay((int)deploymentBackOff.TotalMilliseconds);

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
                    (output) => output is { Status: "Succeeded" or "Timeout" or "Error" },
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

            if (deployedPackage is { Status: "Timeout" or "Error" })
            {
                _packagePresenter.PresentPackageDeploymentFailed(deployedPackage);
                return null;
            }

            return deployedPackage;
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

        private async Task<UploadedPackage?> UploadPackageAsync(string key, CreatedPackage createdPackage, CatalogData catalog)
        {
            _packagePresenter.PresentStartingPackageUpload();

            UploadedPackage uploadedPackage;
            try
            {
                uploadedPackage = await _packageService.UploadPackageAsync(createdPackage, key, catalog);
            }
            catch (UploadPackageException e)
            {
                _packagePresenter.PresentPackageUploadFailed(e);
                return null;
            }

            _packagePresenter.PresentPackageUploadSucceeded();
            return uploadedPackage;
        }
    }
}