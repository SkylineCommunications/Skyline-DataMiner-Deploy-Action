using System.Globalization;
using System.Text.RegularExpressions;

using GitHubAction.Domain.Entities;
using GitHubAction.Presenters;

using Skyline.DataMiner.CICD.FileSystem;

namespace GitHubAction.Factories.Impl;
public class InputFactory : IInputFactory
{
    internal static List<string> ValidArgs = new() { InputArgurments.ApiKey, InputArgurments.PackageName, InputArgurments.SolutionPath, InputArgurments.Version, InputArgurments.Timeout, InputArgurments.Stage, InputArgurments.ArtifactId, InputArgurments.BasePath, InputArgurments.BuildNumber, InputArgurments.Debug };
    private readonly IInputFactoryPresenter _presenter;
    private readonly IFileSystem _fileSystem;
    public InputFactory(IInputFactoryPresenter presenter, IFileSystem fileSystem)
    {
        Console.WriteLine("Creating InputFactory");
        _presenter = presenter;
        _fileSystem = fileSystem;
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

        if (!givenArgs.TryGetValue(InputArgurments.Debug, out var debugString))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.Debug} not found");

        bool isVersionPresent = givenArgs.TryGetValue(InputArgurments.Version, out var version);
        bool isBuildNumberPresent = givenArgs.TryGetValue(InputArgurments.BuildNumber, out var buildNumber);
        if (!isVersionPresent && !isBuildNumberPresent)
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.Version} or {InputArgurments.BuildNumber} not found");

        if (!givenArgs.TryGetValue(InputArgurments.ArtifactId, out var artifactId))
            _presenter.PresentKeyNotFound($"Argument {InputArgurments.ArtifactId} not found");

        //        if (!ValidateArgumentNotEmpty(InputArgurments.Identifier, identifierString)) return null;
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

                if (!argumentsAreValid) return null;

                if (!String.IsNullOrEmpty(basePath))
                    solutionPath = Path.Combine(basePath, solutionPath);

                string workSpace = _fileSystem.File.GetParentDirectory(solutionPath);
                _presenter.PresentLogging("Workspace: " + workSpace);
                _fileSystem.Directory.AllowWritesOnDirectory(workSpace);

                if (solutionPath != null && !_fileSystem.File.Exists(solutionPath))
                {
                    _presenter.PresentSolutionNotFound(solutionPath);
                    argumentsAreValid &= false;
                }

                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.PackageName, packageName);

                if (String.IsNullOrWhiteSpace(version) && String.IsNullOrWhiteSpace(buildNumber))
                {
                    _presenter.PresentMissingArgument(InputArgurments.Version + " or " + InputArgurments.BuildNumber);
                    argumentsAreValid &= false;
                }

                if (!argumentsAreValid) return null;

                //if(isVersionPresent)
                //    argumentsAreValid &= ValidateVersion(InputArgurments.Version, version);

                if (!argumentsAreValid) return null;

                string cleanPackageName = CleanPackageName(packageName);

                return new Inputs()
                {
                    ApiKey = apiKey,
                    PackageName = cleanPackageName,
                    SolutionPath = solutionPath,
                    BasePath = basePath,
                    Stage = stage,
                    TimeOut = timeOut,
                    Version = version,
                    BuildNumber = buildNumber,
                    Debug = String.Equals(debugString, "true", StringComparison.OrdinalIgnoreCase),
                };
            case Stage.Deploy:
                argumentsAreValid &= ValidateArgumentNotEmpty(InputArgurments.ArtifactId, artifactId);

                if (!argumentsAreValid) return null;

                return new Inputs()
                {
                    ApiKey = apiKey,
                    ArtifactId = artifactId,
                    TimeOut = timeOut,
                    Stage = stage,
                    Debug = String.Equals(debugString, "true", StringComparison.OrdinalIgnoreCase),
                };
            default:
                _presenter.PresentStageNotValidated(stage.ToString());
                return null;
        }
    }

    private string CleanPackageName(string packageName)
    {
        string cleanPackageName = StringExtensions.Replace(packageName, new char[] {  '\"', '<', '>', '|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '\\', '/' }, '_');

        return cleanPackageName;
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

    //private bool ValidateVersion(string key, string? version)
    //{
    //    if(version == null || (version != "" && !Regex.IsMatch(version, "[0-9]+.[0-9]+.[0-9]+(-CU[0-9]+)?") && !Regex.IsMatch(version, "[0-9]+.[0-9]+.[0-9]+.[0-9]")))
    //    {
    //        _presenter.PresentInvalidVersionFormat();
    //        return false;
    //    }

    //    return true;
    //}

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
