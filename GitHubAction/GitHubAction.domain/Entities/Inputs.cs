
using System.Globalization;
using System.Text.RegularExpressions;
using Serilog;

namespace GitHubAction.Domain.Entities;
public class Inputs
{
    // always required
    public string ApiKey { get; init; }
    public TimeSpan TimeOut { get; init; }
    public Stage Stage { get; init; }

    public string? SolutionPath { get; init; } 
    public string? PackageName { get; init; }
    public string? Version { get; init; }

    public string? ArtifactId { get; init; }


    public static Inputs? SerializeArguments(Dictionary<string, string> givenArgs)
    {
        var argumentsAreValid = true;
        var apiKey = givenArgs[InputArgurments.ApiKey];
        var stageString = givenArgs[InputArgurments.Stage];
        var timeOutString = givenArgs[InputArgurments.Timeout];
        var solutionPath = givenArgs[InputArgurments.SolutionPath];
        var packageName = givenArgs[InputArgurments.PackageName];
        var version = givenArgs[InputArgurments.Version];
        var artifactId = givenArgs[InputArgurments.ArtifactId];

        if (!ValidateArgumentNotEmpty(InputArgurments.Stage, stageString)) return null;
        if (!ValidateArgumentNotEmpty(InputArgurments.ApiKey, apiKey)) return null;
        if (!ValidateArgumentNotEmpty(InputArgurments.Timeout, timeOutString)) return null;
        if (!ValidateTimeout(timeOutString, out TimeSpan timeOut)) return null;

        if (!Enum.TryParse(stageString, out Stage stage))
        {
            Log.Error("Invalid stage argument. Valid values are: {0}", string.Join(", ", Enum.GetNames<Stage>()));
            return null;
        }

        

        switch (stage)
        {
            case Stage.All:
            case Stage.BuildAndPublish:
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.SolutionPath, solutionPath);
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.PackageName, packageName);
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.Version, version);
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
                Log.Error("Could not validate stage: {0}", stage.ToString());
                return null;
        }
    }


    private static bool ValidateArgumentNotEmpty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Log.Error("Missing argument \"{argument}\"", key);
            return false;
        }

        return true;
    }

    private static bool ValidateVersion(string key, string version)
    {
        var versionRegex = new Regex("^\\d+\\.\\d+\\.\\d+$"); //validate format
        if (!versionRegex.IsMatch(version))
        {
            Log.Error(
                "Invalid format for argument {argument}. The provided value {value} does not match the format \"x.x.x\"",
                key, version);
            return false;
        }

        return true;
    }

    private static bool ValidateTimeout(string timeOutString, out TimeSpan timeout)
    {
        if (!TimeSpan.TryParseExact(timeOutString, "HH:MM", CultureInfo.CurrentCulture, out timeout))
        {
            Log.Error("Invalid time format. The valid format is: HH:MM");
            return false;
        }

        if (timeout < TimeSpan.FromMinutes(1))
        {
            Log.Error("The time until timeout has to be at least 1 minute.");
            return false;
        }

        if (timeout > TimeSpan.FromHours(12))
        {
            Log.Error("The time until timeout has to be at most 12 hours.");
            return false;
        }

        return true;
    }
}
