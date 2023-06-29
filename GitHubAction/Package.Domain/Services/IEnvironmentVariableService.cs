namespace Package.Domain.Services;

public interface IEnvironmentVariableService
{
    Uri? GetSourceUri();
    Uri? GetReleaseUri();
    string GetBranch();
}