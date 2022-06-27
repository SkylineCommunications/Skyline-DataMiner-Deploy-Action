using Package.Domain.Services;

namespace Package.Application;

public class GitHubSourceUriService : ISourceUriService
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
}