namespace Package.Domain.Models;

public class UploadedPackage
{
    public string ArtifactId { get; }

    public UploadedPackage(string artifactId)
    {
        ArtifactId = artifactId;
    }
}