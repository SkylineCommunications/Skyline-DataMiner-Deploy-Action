using Catalog.Domain;

using Package.Domain.Models;
using Package.Domain.Services;

using UploadArtifactApi;

namespace Package.Application;

public class PackageService : IPackageService
{
    private readonly IPackageGateway _packageGateway;
    private readonly IPackageBuilder _packageBuilder;

    public PackageService(IPackageGateway packageGateway, IPackageBuilder packageBuilder)
    {
        _packageGateway = packageGateway;
        _packageBuilder = packageBuilder;
    }

    public async Task<CreatedPackage> CreatePackageAsync(LocalPackageConfig localPackageConfig)
    {
        return await _packageBuilder.CreatePackageAsync(localPackageConfig);
    }

    public async Task<UploadedPackage> UploadPackageAsync(CreatedPackage createdPackage, string key, CatalogData catalog)
    {
        return await _packageGateway.UploadPackageAsync(createdPackage, key, catalog);
    }

    public async Task<DeployingPackage> DeployPackageAsync(UploadedPackage uploadedPackage, string key)
    {
        return await _packageGateway.DeployPackageAsync(uploadedPackage, key);
    }

    public async Task<DeployedPackage> GetDeployedPackageAsync(DeployingPackage deployingPackage, string key)
    {
        return await _packageGateway.GetDeployedPackageAsync(deployingPackage, key);
    }
}
