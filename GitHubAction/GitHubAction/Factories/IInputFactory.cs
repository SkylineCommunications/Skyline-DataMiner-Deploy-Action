using GitHubAction.Domain.Entities;

namespace GitHubAction.Factories;
public interface IInputFactory
{
    Inputs? ParseAndValidateInputs(string[] args);
}
