namespace GitHubAction;

public interface IOutputPathProvider
{
    string BasePath { get; set; }
}

public class OutputPathProvider : IOutputPathProvider
{
    /// <inheritdoc />
    public string BasePath { get; set; }
}