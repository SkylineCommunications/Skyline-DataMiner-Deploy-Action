namespace GIT
{
    public interface IGitInfo
    {
        string GetCurrentBranch(string tag);
        string GetCommitterMail();
    }
}