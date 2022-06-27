using Microsoft.Extensions.Logging;

namespace GitHubAction.Presenters.Impl;

public class GitLabOutputPresenter : IOutputPresenter
{
    private readonly ILogger _logger;
    public GitLabOutputPresenter(ILogger logger)
    {
        _logger = logger;
    }


    public void PresentOutputVariable(string name, string value)
    {
        //TODO(LLS): Figure out how output variables work in GitLab
        _logger.LogInformation("{name}: {value}", name, value);
    }

    public void PresentInvalidArguments()
    {
        _logger.LogError("There was a problem with the provided arguments...");
        Environment.Exit(400); // Bad Request
    }
}