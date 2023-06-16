using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Net;

namespace GIT
{
    public class GITInfo : IGITInfo
    {

        public GITInfo()
        {
            // git config --global --add safe.directory "$GITHUB_WORKSPACE"
            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript($"git config --global --add safe.directory \"{Directory.GetCurrentDirectory()}\"");
                powershell.Invoke();
            }
        }

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