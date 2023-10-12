namespace GIT
{
    public interface IGitInfo
    {
	    void Initialize(string basePath);

        string GetCurrentBranch(string tag);
        string GetCommitterMail();
    }
}