using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using System.Net;

namespace GIT
{
    public class GITInfo : IGITInfo
    {   
        public string GetCurrentBranch()
        {
            using (Repository localRepo = new Repository(Directory.GetCurrentDirectory().TrimEnd('/')))
            {
                var thisBranch = localRepo.Head;
                return thisBranch.FriendlyName;
            }
        }

        public string GetSourceUrl()
        {
            using (Repository localRepo = new Repository(Directory.GetCurrentDirectory().TrimEnd('/')))
            {
                var remoteURL = localRepo.Network.Remotes.First().Url;
                return remoteURL;
            }
        }

    }
}