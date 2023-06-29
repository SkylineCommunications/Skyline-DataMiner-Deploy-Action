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

    /// <inheritdoc />
    public Uri? GetReleaseUri()
    {
        // https://github.com/SkylineCommunications/AutomationScriptTest_SDK_DataAcq/releases/tag/3.0.1-Alpha3

        var githubServerUrl = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        if (githubServerUrl != null && repository != null && Environment.GetEnvironmentVariable("GITHUB_REF_TYPE") == "tag")
        {
            return new Uri(new Uri(githubServerUrl), $"{repository}/releases/tag/{Environment.GetEnvironmentVariable("GITHUB_REF_NAME")}");
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string GetBranch()
    {
        return Environment.GetEnvironmentVariable("GITHUB_REF_NAME") ?? String.Empty;
    }
}