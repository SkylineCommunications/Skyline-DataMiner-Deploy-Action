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
	    var tagName = Environment.GetEnvironmentVariable("CI_COMMIT_TAG");
        if (String.IsNullOrWhiteSpace(tagName))
        {
            // No tag name
	        return null;
        }

        // https://gitlab.com/data-acq/DeployAction
        var githubServerUrl = Environment.GetEnvironmentVariable("CI_PROJECT_URL");
        if (String.IsNullOrWhiteSpace(githubServerUrl))
        {
	        return null;
        }

        // https://gitlab.com/data-acq/DeployAction/-/tags/1.0.1
        return new Uri(new Uri(githubServerUrl), $"-/tags/{tagName}");
    }

    /// <inheritdoc />
    public string GetBranch()
    {
	    var branchName = Environment.GetEnvironmentVariable("CI_COMMIT_BRANCH");
	    var branchOrTagName = Environment.GetEnvironmentVariable("CI_COMMIT_REF_NAME");
	    var tagName = Environment.GetEnvironmentVariable("CI_COMMIT_TAG");

        Console.WriteLine($"Branch Name: {branchName}");
        Console.WriteLine($"BranchOrTag Name: {branchOrTagName}");
        Console.WriteLine($"Tag Name: {tagName}");

        return Environment.GetEnvironmentVariable("CI_COMMIT_REF_NAME") ?? String.Empty;
    }
}