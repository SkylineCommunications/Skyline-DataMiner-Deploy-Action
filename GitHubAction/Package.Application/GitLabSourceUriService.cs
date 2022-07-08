using System.Reflection.Metadata.Ecma335;
using Package.Domain.Services;

namespace Package.Application;

public class GitLabSourceUriService : ISourceUriService
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
}