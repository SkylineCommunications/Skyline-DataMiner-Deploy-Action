using GitHubAction.Domain.Entities;

namespace GitHubAction.Services;
public interface IInputParserService
{
    Inputs? ParseAndValidateInputs(string[] args);
}
