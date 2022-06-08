

using System.Globalization;
using System.Text.RegularExpressions;
using GitHubAction.Domain.Entities;
using GitHubAction.Presenters;

namespace GitHubAction.Services.Impl;
public class InputParserService : IInputParserService
{
    internal static List<string> ValidArgs = new() { InputArgurments.ApiKey, InputArgurments.PackageName, InputArgurments.SolutionPath, InputArgurments.Version, InputArgurments.Timeout, InputArgurments.Stage, InputArgurments.ArtifactId };
    private readonly IInputParserPresenter _presenter;
    public InputParserService( IInputParserPresenter presenter)
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
        string apiKey = "";
        string stageString = "";
        string timeOutString = "";
        string solutionPath = "";
        string packageName = "";
        string version = "";
        string artifactId = "";
        try
        {
            apiKey = givenArgs[InputArgurments.ApiKey];
            stageString = givenArgs[InputArgurments.Stage];
            timeOutString = givenArgs[InputArgurments.Timeout];
            solutionPath = givenArgs[InputArgurments.SolutionPath];
            packageName = givenArgs[InputArgurments.PackageName];
            version = givenArgs[InputArgurments.Version];
            artifactId = givenArgs[InputArgurments.ArtifactId];
        }
        catch (KeyNotFoundException ex)
        {
            _presenter.PresentKeyNotFound(ex.Message);
        }

        
        

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

                return new Inputs()
                {
                    ApiKey = apiKey,
                    PackageName = packageName,
                    SolutionPath = solutionPath,
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


    private bool ValidateArgumentNotEmpty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _presenter.PresentMissingArgument(key);
            return false;
        }

        return true;
    }

    private bool ValidateVersion(string key, string version)
    {
        var versionRegex = new Regex("^\\d+\\.\\d+\\.\\d+$"); //validate format
        if (!versionRegex.IsMatch(version))
        {
            _presenter.PresentInvalidVersionFormat();
            return false;
        }

        return true;
    }

    private bool ValidateTimeout(string timeOutString, out TimeSpan timeout)
    {

        if (!TimeSpan.TryParseExact(timeOutString, "h\\:mm", CultureInfo.CurrentCulture, out timeout))
        {
            _presenter.PresentInvalidTimeFormat();
            return false;
        }

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
