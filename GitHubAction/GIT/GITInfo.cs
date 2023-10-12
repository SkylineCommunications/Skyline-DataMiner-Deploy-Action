namespace GIT
{
    using System.IO;
    using System.Management.Automation;

    public class GitInfo : IGitInfo
    {
        private bool _isGitLab;
        private string currentDirectory;

        public void Initialize(string basePath)
	    {
		    if (!String.IsNullOrWhiteSpace(basePath))
		    {
			    _isGitLab = true;
		    }
            
		    if (_isGitLab)
		    {
			    currentDirectory = basePath;
		    }
		    else
		    {
			    currentDirectory = Directory.GetCurrentDirectory();
			    AllowWritesOnDirectory(currentDirectory);
		    }

		    using (PowerShell powershell = PowerShell.Create())
            {
                MoveToCorrectLocation(powershell);

                powershell.AddScript("git --version");
                var gitVersion = powershell.Invoke().FirstOrDefault()?.ToString();
                powershell.Commands.Clear();

                powershell.AddScript($"git config --global --add safe.directory {currentDirectory}");
                powershell.Invoke();
                powershell.Commands.Clear();

                powershell.AddScript($"git fetch --all --tags --force --quiet");
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
                    throw new InvalidOperationException($"GIT Initial Setup Failed with PowerShell Errors: {resultString} {Environment.NewLine} ps version: {version} OS: {checkOS} git version: {gitVersion} --end");
                }
            }
        }

	    public string GetCommitterMail()
        {
            string mail = "";
            using (PowerShell powershell = PowerShell.Create())
            {
	            MoveToCorrectLocation(powershell);

                powershell.AddScript($"git show -s --format='%ae'");
                var result = powershell.Invoke();
                powershell.Commands.Clear();

                mail = String.Join(',', result);

                if (powershell.HadErrors)
                {
                    string resultString = "errors: " + String.Join(",", powershell.Streams.Error.ReadAll());
                    throw new InvalidOperationException("Getting Current Branch through Git failed with errors:" + resultString);
                }
            }

            if (mail == null) mail = String.Empty;
            return mail;
        }

        public string GetCurrentBranch(string tag)
        {
            using (PowerShell powershell = PowerShell.Create())
            {
	            MoveToCorrectLocation(powershell);

                powershell.AddScript($"git branch --remotes --contains {tag}");
                var results = powershell.Invoke();
                powershell.Commands.Clear();

                string resultString = String.Join(',', results).Replace("origin/", "");

                if (String.IsNullOrWhiteSpace(String.Join(',', results)))
                {
                    resultString = "GIT Branch commands returned no Data.";
                    if (powershell.HadErrors)
                    {
                        resultString += "errors: " + String.Join(",", powershell.Streams.Error.ReadAll());
                    }

                    throw new InvalidOperationException("Getting Current Branch through Git failed with errors:" + resultString);
                }

                return resultString;
            }
        }

        private void MoveToCorrectLocation(PowerShell powershell)
        {
	        if (_isGitLab)
	        {
		        powershell.AddScript($"cd {currentDirectory}");
		        powershell.Invoke();
		        powershell.Commands.Clear();
	        }
        }

        private void AllowWritesOnDirectory(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return;
            
            var directory = new DirectoryInfo(path) { Attributes = System.IO.FileAttributes.Normal };
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos("*", System.IO.SearchOption.AllDirectories);
            foreach (var info in fileSystemInfos)
            {
                info.Attributes = System.IO.FileAttributes.Normal;
            }
        }
    }
}