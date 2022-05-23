using Package.Builder;
using Package.Builder.Exceptions;
using Package.Domain.Exceptions;
using Package.Domain.Models;
using Package.Domain.Services;

namespace GitHubAction
{
    public class GitHubAction
    {
        private readonly IPackageService _packageService;
        private readonly IPackagePresenter _packagePresenter;

        public GitHubAction(
            IPackageService packageService,
            IPackagePresenter packagePresenter)
        {
            _packageService = packageService;
            _packagePresenter = packagePresenter;
        }

        public async Task RunAsync(
            string key,
            LocalPackageConfig localPackageConfig,
            TimeSpan deploymentBackOff, 
            TimeSpan deploymentMaxBackOff, 
            TimeSpan deploymentTimeout)
        {
            try
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
                    return;
                }
                catch (UnsupportedSolutionException)
                {
                    _packagePresenter.PresentUnsupportedSolutionType();
                    return;
                }

                _packagePresenter.PresentPackageCreationSucceeded();
                _packagePresenter.PresentStartingPackageUpload();

                UploadedPackage uploadedPackage;
                try
                {
                    uploadedPackage = await _packageService.UploadPackageAsync(createdPackage, key);
                }
                catch (UploadPackageException e)
                {
                    _packagePresenter.PresentPackageUploadFailed(e);
                    return;
                }

                _packagePresenter.PresentPackageUploadSucceeded();
                _packagePresenter.PresentStartingPackageDeployment();

                DeployingPackage deployingPackage;
                try
                {
                    deployingPackage = await _packageService.DeployPackageAsync(uploadedPackage, key);
                }
                catch (DmsUnavailableException)
                {
                    _packagePresenter.PresentDataMinerSystemUnavailable();
                    return;
                }
                catch (DeployPackageException e)
                {
                    _packagePresenter.PresentStartingPackageDeploymentFailed(e);
                    return;
                }

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
                        (output) => output != null!,
                        (backOffDelaySeconds) => _packagePresenter.PresentWaitingMoreForFinishedPackageDeployment(backOffDelaySeconds),
                        deploymentBackOff,
                        deploymentMaxBackOff,
                        deploymentTimeout.Subtract(deploymentBackOff)))!;
                }
                catch (TimeoutException)
                {
                    _packagePresenter.PresentPackageDeploymentTimeout();
                    return;
                }

                _packagePresenter.PresentPackageDeploymentFinished(deployedPackage.Status);
                return;
            }
            catch (KeyException)
            {
                _packagePresenter.PresentUnauthorizedKey();
                return;
            }
        }
    }
}