using System.Reflection.Metadata.Ecma335;

using Package.Domain.Services;

namespace Package.Application;

public class GitLabEnvironmentVariableService : IEnvironmentVariableService
{
    /// <inheritdoc />
    public Uri? GetSourceUri()
    {
        if (Environment.GetEnvironmentVariable("CI_PROJECT_URL") != null)
        {
            return new Uri(Environment.GetEnvironmentVariable("CI_PROJECT_URL")!);
        }

        return null;
    }

    /// <inheritdoc />
    public string GetBranch()
    {
        return Environment.GetEnvironmentVariable("CI_COMMIT_REF_NAME") ?? String.Empty;
    }
}