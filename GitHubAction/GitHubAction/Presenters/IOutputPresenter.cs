namespace GitHubAction.Presenters;

public interface IOutputPresenter
{
    void PresentOutputVariable(string name, string value);
    void PresentInvalidArguments();
}