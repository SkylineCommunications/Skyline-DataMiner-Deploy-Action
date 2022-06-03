namespace GitHubAction.Domain.Presenters;

public interface IGithubPresenter
{
    void PresentOutputVariable(string name, string value);
    void PresentInvalidArguments();
}