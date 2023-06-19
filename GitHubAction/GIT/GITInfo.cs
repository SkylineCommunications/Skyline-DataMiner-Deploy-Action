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
        }

        public string GetCurrentBranch()
        {

            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript("git branch --show - current");
                var results = powershell.Invoke();
                return String.Join(',', results);
            }
            
            //using (Repository localRepo = new Repository(Directory.GetCurrentDirectory().TrimEnd('/')))
            //{
            //    var thisBranch = localRepo.Head;
            //    return thisBranch.FriendlyName;
            //}
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