namespace GIT
{
    using System.IO;
    using System.Management.Automation;

    public class GitInfo : IGitInfo
    {
        public GitInfo()
        {
            Console.WriteLine("Creating GitInfo");

            Console.WriteLine("Starting 'AllowWritesOnDirectory'");
            AllowWritesOnDirectory(Directory.GetCurrentDirectory());
            Console.WriteLine("Finished AllowWritesOnDirectory");
            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript("git --version");
                Console.WriteLine("Invoking 'git --version'");
                var gitVersion = powershell.Invoke().FirstOrDefault()?.ToString();
                Console.WriteLine("Finished 'git --version'");
                powershell.Commands.Clear();

                powershell.AddScript($"git config --global --add safe.directory {Directory.GetCurrentDirectory()}");
                Console.WriteLine("Invoking 'git config ...'");
                powershell.Invoke();
                Console.WriteLine("Finished 'git config ...");
                powershell.Commands.Clear();

                powershell.AddScript($"git fetch --all --tags --force --quiet");
                Console.WriteLine("Invoking 'git fetch ...'");
                powershell.Invoke();
                Console.WriteLine("Finished 'git fetch ...'");
                powershell.Commands.Clear();

                if (powershell.HadErrors)
                {
	                Console.WriteLine("PowerShell had errors");
                    powershell.AddScript("$PSVersionTable.PSVersion");
                    var version = String.Join(",", powershell.Invoke());
                    powershell.Commands.Clear();

                    powershell.AddScript("$PSVersionTable.Platform");
                    var checkOS = String.Join(",", powershell.Invoke());
                    powershell.Commands.Clear();

                    string resultString = "errors: " + String.Join(",", powershell.Streams.Error.ReadAll());
                    Console.Write($"Throwing exception: {resultString}");
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