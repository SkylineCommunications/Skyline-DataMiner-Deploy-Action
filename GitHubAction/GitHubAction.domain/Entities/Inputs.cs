
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
}
