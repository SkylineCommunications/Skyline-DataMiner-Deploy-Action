using Package.Domain.Models;
using Package.Domain.Services;

namespace Package.Gateway;

public class MockedPackageGateway : IPackageGateway
{
    public Task<UploadedPackage> UploadPackageAsync(CreatedPackage createdPackage, string key)
    {
        return Task.FromResult(new UploadedPackage(Guid.NewGuid().ToString()));
    }

    public Task<DeployingPackage> DeployPackageAsync(UploadedPackage uploadedPackage, string key)
    {
        return Task.FromResult(new DeployingPackage(uploadedPackage.ArtifactId, Guid.NewGuid()));
    }

    public Task<DeployedPackage> GetDeployedPackageAsync(DeployingPackage deployingPackage, string key)
    {
        return Task.FromResult(new DeployedPackage(deployingPackage.ArtifactId, deployingPackage.DeploymentId, "Succeeded"));
    }
}