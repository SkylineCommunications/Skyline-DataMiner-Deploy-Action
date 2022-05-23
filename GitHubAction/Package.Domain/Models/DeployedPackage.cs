namespace Package.Domain.Models;

public class DeployedPackage
{
    public string ArtifactId { get; }
    public Guid DeploymentId { get; }
    public string Status { get; set; }

    public DeployedPackage(string artifactId, Guid deploymentId, string status)
    {
        ArtifactId = artifactId;
        DeploymentId = deploymentId;
        Status = status;
    }
}