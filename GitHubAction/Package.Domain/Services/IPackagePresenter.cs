using Package.Builder;
using Package.Domain.Exceptions;
using Package.Domain.Models;

namespace Package.Domain.Services
{
    public interface IPackagePresenter
    {
        void PresentUnauthorizedKey();
        void PresentPackageDeploymentFinished(string deploymentStatus);
        void PresentPackageDeploymentTimeout();
        void PresentDataMinerSystemUnavailable();
        void PresentPackageUploadFailed(UploadPackageException e);
        void PresentStartingPackageUpload();
        void PresentPackageUploadSucceeded();
        void PresentStartingPackageDeployment();
        void PresentWaitingForFinishedPackageDeployment(int backOffDelaySeconds);
        void PresentWaitingMoreForFinishedPackageDeployment(int backOffDelaySeconds);
        void PresentStartCreatingPackage();
        void PresentPackageCreationFailed(CreatePackageException e);
        void PresentPackageCreationSucceeded();
        void PresentUnsupportedSolutionType();
        void PresentCouldNotFetchTheDeployedPackage();
        void PresentStartingPackageDeploymentFailed(DeployPackageException e);
        void PresentPackageCreationLogging(string line);
        void PresentPackageDeploymentFailed(DeployedPackage deployedPackage);
    }
}
