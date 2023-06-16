using LibGit2Sharp;

namespace GIT
{
    public class GITInfo : IGITInfo
    {
        public string GetCurrentBranch()
        {
            using (Repository localRepo = new Repository(Directory.GetCurrentDirectory()))
            {
                var thisBranch = localRepo.Head;
                return thisBranch.FriendlyName;
            }
        }

        public string GetSourceUrl()
        {
            using (Repository localRepo = new Repository(Directory.GetCurrentDirectory()))
            {
                var remoteURL = localRepo.Network.Remotes.First().Url;
                return remoteURL;
            }
        }

    }
}