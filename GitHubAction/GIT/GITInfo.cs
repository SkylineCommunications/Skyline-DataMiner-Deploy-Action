namespace GIT
{
    using System.IO;
    using System.Management.Automation;

    public class GitInfo : IGitInfo
    {
        public GitInfo()
        {
            Console.WriteLine("Creating GitInfo");

            if (Environment.GetEnvironmentVariable("CI_PROJECT_URL") == null)
            {
	            Console.WriteLine("Starting 'AllowWritesOnDirectory'");
                AllowWritesOnDirectory(Directory.GetCurrentDirectory());
                Console.WriteLine("Finished AllowWritesOnDirectory");
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current Directory: {currentDirectory}");
            string[] subDirectories = Directory.GetDirectories(currentDirectory);
            Console.WriteLine($"#SubDirectories: {subDirectories.Length}");
            string[] files = Directory.GetFiles(currentDirectory);
            Console.WriteLine($"#Files: {files}");

            Console.WriteLine("Starting PowerShell");
            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript("git --version");
                var gitVersion = powershell.Invoke().FirstOrDefault()?.ToString();
                powershell.Commands.Clear();

                powershell.AddScript($"git config --global --add safe.directory {Directory.GetCurrentDirectory()}");
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

            Console.WriteLine("Finished creating GitInfo");
        }

        public string GetCommitterMail()
        {
            string mail = "";
            using (PowerShell powershell = PowerShell.Create())
            {
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

        private void AllowWritesOnDirectory(string path)
        {
	        Console.WriteLine($"AllowWritesOnDirectory|Path: {path}");
            if (String.IsNullOrWhiteSpace(path))
                return;

            Console.WriteLine("Creating DirectoryInfo");
            var directory = new DirectoryInfo(path) { Attributes = System.IO.FileAttributes.Normal };
            Console.WriteLine("Getting FileSystemInfos");
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos("*", System.IO.SearchOption.AllDirectories);
            Console.WriteLine($"#FileSystemInfos: {fileSystemInfos.Length}");
            foreach (var info in fileSystemInfos)
            {
                info.Attributes = System.IO.FileAttributes.Normal;
            }
        }
    }
}