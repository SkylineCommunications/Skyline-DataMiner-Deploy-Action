namespace GitHubAction.Presenters;

public interface IInputFactoryPresenter
{
    void PresentInvalidStage();
    void PresentTimeOutToHigh();
    void PresentTimeOutToLow();
    void PresentInvalidTimeFormat();
    void PresentSolutionNotFound(string path);
    void PresentInvalidVersionFormat();
    void PresentMissingArgument(string key);
    void PresentStageNotValidated(string stage);
    void PresentUnkownArgument(string key);
    void PresentKeyNotFound(string message);
    void PresentLogging(string message);
}