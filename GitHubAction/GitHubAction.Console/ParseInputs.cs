using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace GitHubAction.Console;

public class ParseInputs
{
    internal static List<string> ExpectedArgs = new() { Input.ApiKey, Input.PackageName, Input.SolutionPath, Input.Version, Input.Timeout };

    public static Dictionary<string, string>? ParseAndValidateInputs(string[] args, ILogger logger)
    {
        var stillMissingArguments = new List<string>(ExpectedArgs);

        var inputs = new Dictionary<string, string>();
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (args.Length != ExpectedArgs.Count * 2)
        {
            logger.LogWarning(
                "The amount of passed arguments ({passedAmountOfArgs}) differs from the expected amount ({expectedAmountOfArgs})", 
                args.Length, 
                ExpectedArgs.Count * 2);
        }

        for (var i = 0; i < args.Length; i += 2)
        {
            // remove --
            var key = args[i].Remove(0,2);
            var value = args[i + 1];
            if (!ExpectedArgs.Contains(key))
            {
                logger.LogWarning("Unknown argument \"{argument}\"", key);
                continue;
            }

            if (string.IsNullOrEmpty(value))
            {
                logger.LogError("Missing argument \"{argument}\"", key);
                continue;
            }

            stillMissingArguments.Remove(key);

            switch (key)
            {
                case Input.Version:
                    var regex = new Regex("^\\d+\\.\\d+\\.\\d+$"); //validate format
                    if (!regex.IsMatch(value))
                    {
                        logger.LogError(
                            "Invalid format for argument {argument}. The provided value {value} does not match the format \"x.x.x\"", key, value);
                        continue;
                    }
                    break;
                case Input.Timeout:
                    if (!TimeSpan.TryParse(value, out var timeout))
                    {
                        logger.LogError("Invalid time format. The valid format is: HH:MM");
                        continue;
                    }

                    if (timeout < TimeSpan.FromMinutes(1))
                    {
                        logger.LogError("The time until timeout has to be at least 1 minute.");
                        continue;
                    }

                    if (timeout > TimeSpan.FromHours(12))
                    {
                        logger.LogError("The time until timeout has to be at most 12 hours.");
                        continue;
                    }

                    break;
            }

            inputs[key] = value;
        }

        if (stillMissingArguments.Count > 0)
        {
            logger.LogError("Missing the following arguments: {arguments}", string.Join(", ", stillMissingArguments));
        }

        return inputs.Count == ExpectedArgs.Count ? inputs : null;
    }
}