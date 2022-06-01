using Microsoft.Extensions.Logging;
using Package.Builder;
using Package.Domain.Exceptions;
using Package.Domain.Services;

namespace GitHubAction;
public class ConsolePackagePresenter : IPackagePresenter

{
    private readonly ILogger<ConsolePackagePresenter> _logger;

    public ConsolePackagePresenter(ILogger<ConsolePackagePresenter> logger)
    {
        _logger = logger;
    }

    public void PresentUnauthorizedKey()
    {
        _logger.LogError("The provided key was not valid or authorized.");
        Environment.Exit(403); // Forbidden
    }

    public void PresentPackageDeploymentFinished(string deploymentStatus)
    {
        _logger.LogInformation("Finished package deployment! Status: {deploymentStatus}.", deploymentStatus);
    }

    public void PresentPackageDeploymentTimeout()
    {
        _logger.LogError("Timed out waiting for the package deployment to finish!");
        Environment.Exit(504); // Gateway Timeout
    }

    public void PresentDataMinerSystemUnavailable()
    {
        _logger.LogError("The DataMiner System was not available.");
        Environment.Exit(502); // Bad Gateway
    }

    public void PresentPackageUploadFailed(UploadPackageException e)
    {
        _logger.LogError("The package could not be uploaded.");
        Environment.Exit(500); // Internal Server Error
    }

    public void PresentStartingPackageUpload()
    {
        _logger.LogInformation("Start package upload...");
    }

    public void PresentPackageUploadSucceeded()
    {
        _logger.LogInformation("Finished package upload!");
    }

    public void PresentStartingPackageDeployment()
    {
        _logger.LogInformation("Start package deployment...");
    }

    public void PresentWaitingForFinishedPackageDeployment(int backOffDelaySeconds)
    {
        _logger.LogInformation("Waiting {backOffSeconds} seconds for the package deployment to finish...", backOffDelaySeconds);
    }

    public void PresentWaitingMoreForFinishedPackageDeployment(int backOffDelaySeconds)
    {
        _logger.LogInformation("Waiting another {backOffSeconds} seconds for the package deployment to finish...", backOffDelaySeconds);
    }

    public void PresentStartCreatingPackage()
    {
        _logger.LogInformation("Start creating package...");
    }

    public void PresentPackageCreationFailed(CreatePackageException e)
    {
        _logger.LogError("An exception occurred during creation of the package: {exception}.", e.ToString());
        Environment.Exit(500); // Internal Server Error
    }

    public void PresentPackageCreationSucceeded()
    {
        _logger.LogInformation("Finished package creation!");
    }

    public void PresentUnsupportedSolutionType()
    {
        _logger.LogError("The solution type is not supported.");
        Environment.Exit(403); // Bad Request
    }

    public void PresentCouldNotFetchTheDeployedPackage()
    {
        _logger.LogInformation("Couldn't fetch the deployed package right now...");
        Environment.Exit(500); // Internal Server Error
    }

    public void PresentStartingPackageDeploymentFailed(DeployPackageException e)
    {
        _logger.LogError("The package deployment couldn't be started. Error message: {0}", e.Message);
        Environment.Exit(500); // Internal Server Error
    }
}
