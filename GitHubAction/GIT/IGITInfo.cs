namespace GIT
{
    public interface IGITInfo
    {
        string GetCurrentBranch();
        string GetSourceUrl();
    }
}