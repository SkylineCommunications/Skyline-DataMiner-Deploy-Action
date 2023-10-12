using Microsoft.Extensions.Logging;

using Package.Builder;
using Package.Domain.Exceptions;
using Package.Domain.Models;
using Package.Domain.Services;

namespace GitHubAction;
public class ConsolePackagePresenter : IPackagePresenter

{
    private readonly ILogger<ConsolePackagePresenter> _logger;

    public ConsolePackagePresenter(ILogger<ConsolePackagePresenter> logger)
    {
        Console.WriteLine("Creating ConsolePackagePresenter");
        _logger = logger;
    }

    public void PresentUnauthorizedKey()
    {
        _logger.LogError("The provided key was not valid or authorized.");
    }

    public void PresentPackageDeploymentFinished(string deploymentStatus)
    {
        _logger.LogInformation("Finished package deployment! Status: {deploymentStatus}.", deploymentStatus);
    }

    public void PresentPackageDeploymentTimeout()
    {
        _logger.LogError("Timed out waiting for the package deployment to finish!");
    }

    public void PresentDataMinerSystemUnavailable()
    {
        _logger.LogError("The DataMiner System was not available.");
    }

    public void PresentPackageUploadFailed(UploadPackageException e)
    {
        _logger.LogError("The package could not be uploaded. " + e);
    }

    public void PresentStartingPackageUpload()
    {
        _logger.LogInformation("Start package upload...");
    }

    public void PresentPackageUploadSucceeded()
    {
        _logger.LogInformation("Finished package upload!");
    }

    public void PresentPackageCreationLogging(string line)
    {
        _logger.LogInformation(line);
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

    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }

    public void PresentStartCreatingPackage()
    {
        _logger.LogInformation("Start creating package...");
    }

    public void PresentPackageCreationFailed(CreatePackageException e)
    {
        _logger.LogError("An exception occurred during creation of the package: {exception}.", e.ToString());
    }

    public void PresentPackageCreationSucceeded()
    {
        _logger.LogInformation("Finished package creation!");
    }

    public void PresentUnsupportedSolutionType()
    {
        _logger.LogError("The solution type is not supported.");
    }

    public void PresentCouldNotFetchTheDeployedPackage()
    {
        _logger.LogInformation("Couldn't fetch the deployed package right now...");
    }

    public void PresentStartingPackageDeploymentFailed(DeployPackageException e)
    {
        _logger.LogError("The package deployment couldn't be started. Error message: {0}", e.Message);
    }

    public void PresentPackageDeploymentFailed(DeployedPackage deployedPackage)
    {
        _logger.LogError("Package deployment failed with the following status: {0}", deployedPackage.Status);
    }
}
