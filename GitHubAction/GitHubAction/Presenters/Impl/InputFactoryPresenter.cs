using GitHubAction.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace GitHubAction.Presenters.Impl;
public class InputFactoryPresenter : IInputFactoryPresenter
{
    private readonly ILogger<InputFactoryPresenter> _logger;

    public InputFactoryPresenter(ILogger<InputFactoryPresenter> logger)
    {
	    Console.WriteLine("Creating InputFactoryPresenter");
        _logger = logger;
    }

    public void PresentInvalidStage()
    {

        _logger.LogError("Invalid stage argument. Valid values are: {0}", string.Join(", ", Enum.GetNames<Stage>()));
    }

    public void PresentInvalidTimeFormat()
    {
        _logger.LogError("Timeout has to be a valid integer.");
    }

    public void PresentInvalidVersionFormat()
    {
        _logger.LogError("Invalid format for version. The provided version does not match the format \"x.x.x\"");
    }

    public void PresentKeyNotFound(string message)
    {
        _logger.LogDebug(message);
    }

    public void PresentMissingArgument(string key)
    {
        _logger.LogError("Missing argument \"{key}\"", key);
    }

    public void PresentStageNotValidated(string stage)
    {
        _logger.LogError("Could not validate stage: {stage}", stage);
    }

    public void PresentTimeOutToHigh()
    {
        _logger.LogError("The time until timeout has to be at most 12 hours.");
    }

    public void PresentTimeOutToLow()
    {
        _logger.LogError("The time until timeout has to be at least 1 minute.");
    }

    public void PresentUnkownArgument(string key)
    {
        _logger.LogError("Unknown argument \"{key}\"", key);
    }


    public void PresentSolutionNotFound(string path)
    {
        _logger.LogError("Solution File does not exist: " + path);
    }

    public void PresentLogging(string message)
    {
        _logger.LogInformation(message);
    }
}
