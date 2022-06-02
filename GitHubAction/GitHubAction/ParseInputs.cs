using System.Text.RegularExpressions;
using GitHubAction.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GitHubAction;

public class ParseInputs
{
    internal static List<string> ValidArgs = new() { InputArgurments.ApiKey, InputArgurments.PackageName, InputArgurments.SolutionPath, InputArgurments.Version, InputArgurments.Timeout, InputArgurments.Stage, InputArgurments.ArtifactId };

    public static Inputs? ParseAndValidateInputs(string[] args, ILogger logger)
    {
        var givenArgs = new Dictionary<string, string>();

        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        for (var i = 0; i < args.Length; i += 2)
        {
            var key = args[i].Remove(0,2); // remove --
            var value = args[i + 1];

            if (!ValidArgs.Contains(key))
            {
                logger.LogWarning("Unknown argument \"{argument}\"", key);
                continue;
            }

            givenArgs[key] = value;
        }

        return Inputs.SerializeArguments(givenArgs);
    }
}