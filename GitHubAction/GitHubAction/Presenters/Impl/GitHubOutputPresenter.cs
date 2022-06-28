using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GitHubAction.Presenters.Impl;

public class GitHubOutputPresenter : IOutputPresenter
{
    private readonly ILogger _logger;
    public GitHubOutputPresenter(ILogger logger)
    {
        _logger = logger;
    }


    public void PresentOutputVariable(string name, string value)
    {
        Log.ForContext("type", "githubCommand").Information("::set-output name={0}::{1}", name, value);
    }

    public void PresentInvalidArguments()
    {
        _logger.LogError("There was a problem with the provided arguments...");
    }
}