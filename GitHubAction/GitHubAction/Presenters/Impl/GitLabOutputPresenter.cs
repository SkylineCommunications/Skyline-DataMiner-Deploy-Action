using Microsoft.Extensions.Logging;

namespace GitHubAction.Presenters.Impl;

public class GitLabOutputPresenter : IOutputPresenter
{
    private readonly IOutputPathProvider _pathProvider;
    private readonly ILogger _logger;
    public GitLabOutputPresenter(IOutputPathProvider pathProvider, ILogger<GitLabOutputPresenter> logger)
    {
        _pathProvider = pathProvider;
        _logger = logger;
    }


    public void PresentOutputVariable(string name, string value)
    {
        using var fileStream = File.AppendText(Path.Combine(_pathProvider.BasePath, "SkylineOutput.env"));
        fileStream.WriteLine($"{name}={value}");
        _logger.LogInformation("{name}: {value}", name, value);
    }

    public void PresentInvalidArguments()
    {
        _logger.LogError("There was a problem with the provided arguments...");
    }
}