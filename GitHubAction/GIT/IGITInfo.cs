namespace GIT
{
    public interface IGITInfo
    {
        string GetCurrentBranch(string tag);
        string GetCommitterMail();
    }
}