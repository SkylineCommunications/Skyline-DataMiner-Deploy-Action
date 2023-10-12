using Microsoft.Extensions.Logging;

namespace GitHubAction.Presenters.Impl;

public class DefaultOutputPresenter : IOutputPresenter
{
    private readonly ILogger _logger;
    public DefaultOutputPresenter(ILogger<DefaultOutputPresenter> logger)
    {
        Console.WriteLine("Creating DefaultOutputPresenter");
        _logger = logger;
    }


    public void PresentOutputVariable(string name, string value)
    {
        // No action required
        _logger.LogInformation("{name}: {value}", name, value);
    }

    public void PresentInvalidArguments()
    {
        _logger.LogError("There was a problem with the provided arguments...");
    }
}