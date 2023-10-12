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
    public Uri? GetReleaseUri()
    {
        // TODO: Figure out where the artifacts are stored
        return null;

        // TODO: Figure out if the pipeline runs for a regular push or a tag.

        // https://gitlab.com/data-acq/DeployAction/-/tags/1.0.1

        var githubServerUrl = Environment.GetEnvironmentVariable("CI_PROJECT_URL");
        if (githubServerUrl != null /*&& Environment.GetEnvironmentVariable("GITHUB_REF_TYPE") == "tag"*/)
        {
	        return new Uri(new Uri(githubServerUrl), $"-/tags/{Environment.GetEnvironmentVariable("CI_COMMIT_REF_NAME")}");
        }
        else
        {
	        return null;
        }
    }

    /// <inheritdoc />
    public string GetBranch()
    {
        return Environment.GetEnvironmentVariable("CI_COMMIT_REF_NAME") ?? String.Empty;
    }
}