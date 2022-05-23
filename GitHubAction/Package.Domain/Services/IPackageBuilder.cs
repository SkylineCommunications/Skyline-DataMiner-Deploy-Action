using Package.Builder;
using Package.Builder.Exceptions;
using Package.Domain.Models;
namespace Package.Domain.Services
{
    public interface IPackageBuilder
    {
        /// <summary>
        /// Creates a package using the provided config data.
        /// </summary>
        /// <param name="localPackageConfig">Object of type <see cref="LocalPackageConfig"/> representing the info needed to build a package.</param>
        /// <returns>A <see cref="CreatedPackage"/> object representing the created package.</returns>
        /// <exception cref="UnsupportedSolutionException">When the provided solution is not supported.</exception>
        /// <exception cref="CreatePackageException">When an exception is encountered during the creation of a package.</exception>
        Task<CreatedPackage> CreatePackageAsync(LocalPackageConfig localPackageConfig);
    }
}
