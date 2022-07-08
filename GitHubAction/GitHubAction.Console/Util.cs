namespace GitHubAction.Console;

public static class Util
{
    public enum SourceHost
    {
        Undefined = 0,
        GitHub = 1,
        GitLab = 2,
    }

    public static SourceHost GetSourceHost()
    {
        if (Environment.GetEnvironmentVariable("GITHUB_SERVER_URL") != null)
        {
            return SourceHost.GitHub;
        }

        if (Environment.GetEnvironmentVariable("CI_PROJECT_URL") != null)
        {
            return SourceHost.GitLab;
        }

        return SourceHost.Undefined;
    }
}