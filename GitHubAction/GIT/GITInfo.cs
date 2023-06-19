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

        public string GetCurrentBranch(string tag)
        {

            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript($"git branch --remotes --contains {tag}");
                var results = powershell.Invoke();
                string resultString = $"From git branch --remotes --contains {tag}: " + String.Join(',', results);
                if (String.IsNullOrWhiteSpace(String.Join(',', results)))
                {
                    powershell.Commands.Clear();
                    powershell.AddScript("git rev-parse --abbrev-ref HEAD");
                    results = powershell.Invoke();
                    resultString = $"From git rev-parse --abbrev-ref HEAD: " + String.Join(',', results);
                }

                if (String.IsNullOrWhiteSpace(String.Join(',', results)))
                {
                    powershell.Commands.Clear();
                    powershell.AddScript($"git rev-parse tags/{tag}~0");
                    var commitId = powershell.Invoke();
                    powershell.Commands.Clear();
                    powershell.AddScript($"git branch --contains {commitId}");
                    results = powershell.Invoke();
                    resultString = $"From : git branch --contains {commitId}: " + String.Join(',', results);
                }


                if (String.IsNullOrWhiteSpace(String.Join(',', results)))
                {
                    resultString = "No GIT Commands Returned Data.";
                }

                    return resultString;
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