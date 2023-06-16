using Catalog.Domain;

using Package.Builder;
using Package.Builder.Exceptions;
using Package.Domain.Exceptions;
using Package.Domain.Models;

namespace Package.Domain.Services;

public interface IPackageService
{
    /// <summary>
    /// Creates a package using the provided config data.
    /// </summary>
    /// <param name="localPackageConfig">Object of type <see cref="LocalPackageConfig"/> representing the info needed to build a package.</param>
    /// <returns>A <see cref="CreatedPackage"/> object representing the created package.</returns>
    /// <exception cref="UnsupportedSolutionException">When the provided solution is not supported.</exception>
    /// <exception cref="CreatePackageException">When an exception is encountered during the creation of a package.</exception>
    Task<CreatedPackage> CreatePackageAsync(LocalPackageConfig localPackageConfig);

    /// <summary>
    /// Uploads the package.
    /// </summary>
    /// <param name="createdPackage"></param>
    /// <param name="key">Provided Key</param>
    /// <returns>The uploaded package if succeeded.</returns>
    /// <exception cref="KeyException">When the <paramref name="key"/> isn't valid or authorized to execute this action.</exception>
    /// <exception cref="UploadPackageException">When something went wrong while uploading the package.</exception>
    Task<UploadedPackage> UploadPackageAsync(CreatedPackage createdPackage, string key, CatalogData catalog);

    /// <summary>
    /// Starts the deployment of the package on the DMS.
    /// </summary>
    /// <param name="uploadedPackage"></param>
    /// <param name="key">Provided Key</param>
    /// <returns>The deploying package if starting the deployment succeeded.</returns>
    /// <exception cref="KeyException">When the <paramref name="key"/> isn't valid or authorized to execute this action.</exception>
    /// <exception cref="DmsUnavailableException">When the DMS, authorized by the <paramref name="key"/>, isn't available/online.</exception>
    /// <exception cref="DeployPackageException">When something went wrong while deploying the package.</exception>
    Task<DeployingPackage> DeployPackageAsync(UploadedPackage uploadedPackage, string key);

    /// <summary>
    /// Fetches the finished deployment of the package on the DMS with a retry mechanism because the deployment could still be in progress/queued/...
    /// </summary>
    /// <param name="deployingPackage"></param>
    /// <param name="key">Provided Key</param>
    /// <returns>The deployed package if the deployment finished.</returns>
    /// <remarks>
    ///     - A deployed package can also be a failed deployment.
    ///     - This can be a long running operation depending on the input parameters (<paramref name="timeout"/>).
    /// </remarks>
    /// <exception cref="KeyException">When the <paramref name="key"/> isn't valid or authorized to execute this action.</exception>
    /// <exception cref="GetDeploymentPackageException">When something went wrong while retrieving the deployment package.</exception>
    Task<DeployedPackage> GetDeployedPackageAsync(DeployingPackage deployingPackage, string key);
}
