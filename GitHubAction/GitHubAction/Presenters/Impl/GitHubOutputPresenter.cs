using System.Text;

using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GitHubAction.Presenters.Impl;

public class GitHubOutputPresenter : IOutputPresenter
{
    private readonly ILogger _logger;
    public GitHubOutputPresenter(ILogger<GitHubOutputPresenter> logger)
    {
        _logger = logger;
    }


    public void PresentOutputVariable(string name, string value)
    {
		//  Log.ForContext("type", "githubCommand").Information("::set-output name={0}::{1}", name, value);
		//Log.ForContext("type", "githubCommand").Information("echo \"{0}={1}\" >> $GITHUB_OUTPUT", name, value);
		//>> $GITHUB_OUTPUT

		var gitHubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
		if (!string.IsNullOrWhiteSpace(gitHubOutputFile))
		{
			using StreamWriter textWriter = new(gitHubOutputFile, true, Encoding.UTF8);
			textWriter.WriteLine($"{name}={value}");
		}
	}

    public void PresentInvalidArguments()
    {
        _logger.LogError("There was a problem with the provided arguments...");
    }
}