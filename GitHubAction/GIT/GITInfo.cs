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
            AllowWritesOnDirectory(Directory.GetCurrentDirectory());
            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript("git --version");
                var gitVersion = powershell.Invoke().FirstOrDefault()?.ToString();
                powershell.Commands.Clear();

                powershell.AddScript($"git config --global --add safe.directory {Directory.GetCurrentDirectory()}");
                powershell.Invoke();
                powershell.Commands.Clear();

                if (powershell.HadErrors)
                {
                    powershell.AddScript("$PSVersionTable.PSVersion");
                    var version = String.Join(",", powershell.Invoke());
                    powershell.Commands.Clear();

                    powershell.AddScript("$PSVersionTable.Platform");
                    var checkOS = String.Join(",", powershell.Invoke());
                    powershell.Commands.Clear();

                    string resultString = "errors: " + String.Join(",", powershell.Streams.Error.ReadAll());
                    throw new InvalidOperationException("GIT Initial Setup Failed with PowerShell Errors: " + resultString + Environment.NewLine + " ps version: " + version + " OS: " + checkOS + " git version:" + gitVersion "--end");
                }
            }
        }


        private void AllowWritesOnDirectory(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return;

            var directory = new DirectoryInfo(path) { Attributes = System.IO.FileAttributes.Normal };
            foreach (var info in directory.GetFileSystemInfos("*", System.IO.SearchOption.AllDirectories))
            {
                info.Attributes = System.IO.FileAttributes.Normal;
            }
        }

        public string GetCurrentBranch(string tag)
        {
            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript($"git fetch --all");
                powershell.Invoke();
                powershell.Commands.Clear();

                powershell.AddScript($"git branch --remotes --contains {tag}");
                var results = powershell.Invoke();
                powershell.Commands.Clear();

                string resultString = String.Join(',', results).Replace("origin/", "");

                if (String.IsNullOrWhiteSpace(String.Join(',', results)))
                {
                    resultString = "GIT Branch Commands Returned Data.";
                    if (powershell.HadErrors)
                    {
                        resultString += "errors: " + String.Join(",", powershell.Streams.Error.ReadAll());
                    }

                    throw new InvalidOperationException("Getting Current Branch through Git failed with errors:" + resultString);
                }

                return resultString;
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