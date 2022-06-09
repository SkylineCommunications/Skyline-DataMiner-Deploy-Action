using Microsoft.Extensions.Logging;
using Serilog;

namespace GitHubAction.Presenters.Impl;

public class GithubPresenter : IGithubPresenter
{
    private readonly ILogger<GithubPresenter> _logger;
    public GithubPresenter(ILogger<GithubPresenter> logger)
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
        Environment.Exit(400); // Bad Request
    }
}