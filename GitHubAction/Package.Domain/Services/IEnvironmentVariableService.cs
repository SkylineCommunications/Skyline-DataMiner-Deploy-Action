namespace Package.Domain.Services;

public interface IEnvironmentVariableService
{
    Uri? GetSourceUri();
    string GetBranch();
}