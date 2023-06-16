using Package.Domain.Services;

namespace Package.Application;

public class GitHubEnvironmentVariableService : IEnvironmentVariableService
{
    /// <inheritdoc />
    public Uri? GetSourceUri()
    {
        var githubServerUrl = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");

        if (githubServerUrl != null && repository != null)
        {
            return new Uri(new Uri(githubServerUrl), repository);
        }

        return null;
    }

    public string GetBranch()
    {
        return Environment.GetEnvironmentVariable("GITHUB_REF_NAME")??String.Empty;
    }
}