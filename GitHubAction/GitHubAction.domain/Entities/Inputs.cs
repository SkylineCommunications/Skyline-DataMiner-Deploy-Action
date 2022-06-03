
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Package.Domain.Enums;
using Serilog;

namespace GitHubAction.Domain.Entities;
public class Inputs
{
    // always required
    public string ApiKey { get; init; }
    public TimeSpan TimeOut { get; init; }
    public string Stage { get; init; }

    public string? SolutionPath { get; init; } 
    public string? PackageName { get; init; }
    public string? Version { get; init; }

    public string? ArtifactId { get; init; }


    public static Inputs? SerializeArguments(Dictionary<string, string> givenArgs)
    {
        var argumentsAreValid = true;
        var apiKey = givenArgs[InputArgurments.ApiKey];
        var stage = givenArgs[InputArgurments.Stage];
        var timeOut = givenArgs[InputArgurments.Timeout];
        var solutionPath = givenArgs[InputArgurments.SolutionPath];
        var packageName = givenArgs[InputArgurments.PackageName];
        var version = givenArgs[InputArgurments.Version];
        var artifactId = givenArgs[InputArgurments.ArtifactId];

        if (!ValidateArgument(InputArgurments.Stage, stage, stage)) return null;
        if (!ValidateArgument(InputArgurments.ApiKey, apiKey, stage)) return null;
        if (!ValidateArgument(InputArgurments.Timeout, timeOut, stage)) return null;

        switch (stage)
        {
            case Stages.All:
            case Stages.BuildAndPublish:
                // validate solution path
                argumentsAreValid &= ValidateArgument(InputArgurments.SolutionPath, solutionPath, stage);
                argumentsAreValid &= ValidateArgument(InputArgurments.PackageName, packageName, stage);
                argumentsAreValid &= ValidateArgument(InputArgurments.Version, version, stage);

                if (!argumentsAreValid) return null;

                return new Inputs()
                {
                    ApiKey = apiKey,
                    PackageName = packageName,
                    SolutionPath = solutionPath,
                    Stage = stage,
                    TimeOut = TimeSpan.Parse(timeOut),
                    Version = version
                };
            case Stages.Deploy:
                // validate package name
                argumentsAreValid &= ValidateArgument(InputArgurments.ArtifactId, artifactId, stage);

                if (!argumentsAreValid) return null;

                return new Inputs()
                {
                    ApiKey = apiKey,
                    ArtifactId = artifactId,
                    TimeOut = TimeSpan.Parse(timeOut),
                    Stage = stage
                };
            default:
                Log.Error("Invalid stage argument. Valid values are: BuildAndPublish, Deploy or All");
                return null;
        }
    }


    private static bool ValidateArgument(string key, string value, string stage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Log.Error("Missing argument \"{argument}\" for stage {0}", key, stage);
            return false;
        }

        switch (key)
        {
            case InputArgurments.Version:
                var versionRegex = new Regex("^\\d+\\.\\d+\\.\\d+$"); //validate format
                if (!versionRegex.IsMatch(value))
                {
                    Log.Error(
                        "Invalid format for argument {argument}. The provided value {value} does not match the format \"x.x.x\"",
                        key, value);
                    return false;
                }

                break;

            case InputArgurments.Timeout:
                if (!TimeSpan.TryParse(value, out var timeout))
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

                break;
        }


        return true;
    }
}
