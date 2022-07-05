using System.Globalization;
using System.Text.RegularExpressions;
using GitHubAction.Domain.Entities;
using GitHubAction.Presenters;

namespace GitHubAction.Factories.Impl;
public class InputFactory : IInputFactory
{
    internal static List<string> ValidArgs = new() { InputArgurments.ApiKey, InputArgurments.PackageName, InputArgurments.SolutionPath, InputArgurments.Version, InputArgurments.Timeout, InputArgurments.Stage, InputArgurments.ArtifactId, InputArgurments.BasePath };
    private readonly IInputFactoryPresenter _presenter;
    public InputFactory( IInputFactoryPresenter presenter)
    {
        _presenter = presenter;
    }

    public Inputs? ParseAndValidateInputs(string[] args)
    {
        var givenArgs = new Dictionary<string, string>();

        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        for (var i = 0; i < args.Length; i += 2)
        {
            var key = args[i].Remove(0, 2); // remove --
            var value = args[i + 1];

            if (!ValidArgs.Contains(key))
            {
                _presenter.PresentUnkownArgument(key);
                continue;
            }

            givenArgs[key] = value;
        }

        return SerializeArguments(givenArgs);
    }

    public Inputs? SerializeArguments(Dictionary<string, string> givenArgs)
    {
        var argumentsAreValid = true;

        if (!givenArgs.TryGetValue(InputArgurments.ApiKey, out var apiKey))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.ApiKey} not found");

        if (!givenArgs.TryGetValue(InputArgurments.Stage, out var stageString))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.Stage} not found");

        if (!givenArgs.TryGetValue(InputArgurments.Timeout, out var timeOutString))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.Timeout} not found");

        if (!givenArgs.TryGetValue(InputArgurments.BasePath, out var basePath))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.BasePath} not found");

        if (!givenArgs.TryGetValue(InputArgurments.SolutionPath, out var solutionPath))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.SolutionPath} not found");

        if (!givenArgs.TryGetValue(InputArgurments.PackageName, out var packageName))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.PackageName} not found");

        if (!givenArgs.TryGetValue(InputArgurments.Version, out var version))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.Version} not found");

        if (!givenArgs.TryGetValue(InputArgurments.ArtifactId, out var artifactId))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.ArtifactId} not found");

        if (!ValidateArgumentNotEmpty(InputArgurments.Stage, stageString)) return null;
        if (!ValidateArgumentNotEmpty(InputArgurments.ApiKey, apiKey)) return null;
        if (!ValidateArgumentNotEmpty(InputArgurments.Timeout, timeOutString)) return null;
        if (!ValidateTimeout(timeOutString, out TimeSpan timeOut)) return null;

        if (!Enum.TryParse(stageString, out Stage stage))
        {
            _presenter.PresentInvalidStage();
            return null;
        }

        switch (stage)
        {
            case Stage.All:
            case Stage.Upload:
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.SolutionPath, solutionPath);
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.PackageName, packageName);
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.Version, version);

                if (!argumentsAreValid) return null;

                argumentsAreValid &= ValidateVersion(InputArgurments.Version, version);

                if (!argumentsAreValid) return null;

                if(!string.IsNullOrEmpty(basePath))
                    solutionPath = Path.Combine(basePath, solutionPath);

                return new Inputs()
                {
                    ApiKey = apiKey,
                    PackageName = packageName,
                    SolutionPath = solutionPath,
                    BasePath = basePath,
                    Stage = stage,
                    TimeOut = timeOut,
                    Version = version
                };
            case Stage.Deploy:
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.ArtifactId, artifactId);

                if (!argumentsAreValid) return null;

                return new Inputs()
                {
                    ApiKey = apiKey,
                    ArtifactId = artifactId,
                    TimeOut = timeOut,
                    Stage = stage
                };
            default:
                _presenter.PresentStageNotValidated(stage.ToString());
                return null;
        }
    }


    private bool ValidateArgumentNotEmpty(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _presenter.PresentMissingArgument(key);
            return false;
        }

        return true;
    }

    private bool ValidateVersion(string key, string? version)
    {
        var versionRegex = new Regex("^\\d+\\.\\d+\\.\\d+$"); //validate format
        if (!versionRegex.IsMatch(version))
        {
            _presenter.PresentInvalidVersionFormat();
            return false;
        }

        return true;
    }

    private bool ValidateTimeout(string? timeOutInSecondsString, out TimeSpan timeout)
    {

        if (!int.TryParse(timeOutInSecondsString, out int timeOutInSeconds))
        {
            timeout = TimeSpan.Zero;
            _presenter.PresentInvalidTimeFormat();
            return false;
        }

        timeout = TimeSpan.FromSeconds(timeOutInSeconds);

        if (timeout < TimeSpan.FromMinutes(1))
        {
            _presenter.PresentTimeOutToLow();
            return false;
        }

        if (timeout > TimeSpan.FromHours(12))
        {
            _presenter.PresentTimeOutToHigh();
            return false;
        }

        return true;
    }
}
