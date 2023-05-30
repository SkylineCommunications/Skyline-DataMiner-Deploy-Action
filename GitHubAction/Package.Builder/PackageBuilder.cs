namespace Package.Builder
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    using Package.Builder.Exceptions;
    using Package.Domain.Enums;
    using Package.Domain.Models;
    using Package.Domain.Services;

    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.Loggers;

    public class GitHubActionLogger : ILogCollector
    {
        public IList<string> Logging { get; }
        public bool HasError { get; private set; }
        private IPackagePresenter _presenter;

        public GitHubActionLogger(IPackagePresenter presenter)
        {
            Logging = new List<string>();
            HasError = false;
            _presenter = presenter;
        }

        public void ReportError(string error)
        {
            HasError = true;
            Logging.Add("ERROR: " + error);
        }

        public void ReportStatus(string status)
        {
            Logging.Add("STATUS: " + status);
        }

        public void ReportWarning(string warning)
        {
            Logging.Add("WARNING: " + warning);
        }

        public void SendToPresenter()
        {
            foreach (var line in Logging)
            {
                _presenter.PresentPackageCreationLogging(line);
            }        
        }
    }

    public class PackageBuilder : IPackageBuilder
    {
        IPackagePresenter _presenter;

        public PackageBuilder(IPackagePresenter presenter)
        {
            _presenter = presenter;
        }

        public async Task<CreatedPackage> CreatePackageAsync(
            LocalPackageConfig localPackageConfig)
        {
            if (localPackageConfig.Type != SolutionType.DmScript)
            {
                throw new UnsupportedSolutionException($"Solution of type {localPackageConfig.Type} is not supported.");
            }

            var logger = new GitHubActionLogger(_presenter);
            DMAppVersion version;

            if (!String.IsNullOrWhiteSpace(localPackageConfig.Version))
            {
                if (Regex.IsMatch(localPackageConfig.Version, "^[0-9]+.[0-9]+.[0-9]+(-CU[0-9]+)?$"))
                {
                    version = DMAppVersion.FromDataMinerVersion(localPackageConfig.Version);
                }
                else if (Regex.IsMatch(localPackageConfig.Version, "[0-9]+.[0-9]+.[0-9]+.[0-9]"))
                {
                    version = DMAppVersion.FromProtocolVersion(localPackageConfig.Version);
                }
                else
                {
                    // Supports pre-releases
                    version = DMAppVersion.FromPreRelease(localPackageConfig.Version);
                }
            }
            else
            {
                // TODO: replace this again with commented line one upload API supports x.x.x-CUx version string.
                //version = DMAppVersion.FromBuildNumber(Convert.ToInt32(localPackageConfig.BuildNumber));
                version = DMAppVersion.FromDataMinerVersion("0.0." + localPackageConfig.BuildNumber);
            }

            // var dmappPackageCreator = AppPackageCreatorForAutomation.Factory.FromRepository(logger, Environment.GetEnvironmentVariable("GITHUB_WORKSPACE"), localPackageConfig.PackageName, version);
            var dmappPackageCreator = AppPackageCreatorForAutomation.Factory.FromRepository(logger, localPackageConfig?.SolutionFile?.Directory?.FullName ?? "", localPackageConfig.PackageName, version);
           
            try
            {
                var dmappPackage = await dmappPackageCreator.CreateAsync();
               var result = new CreatedPackage(
                    dmappPackage,
                    localPackageConfig.PackageName,
                    localPackageConfig.Type.ToString(),
                    version.ToString());       
                logger.SendToPresenter();
                if (logger.HasError)
                {
                    throw new InvalidOperationException("Failed to Create Package!");
                }

                return result;
            }
            catch (Exception e)
            {
                logger.ReportError("Exception during Dmapp Creation:" + e);
                logger.SendToPresenter();
                throw new InvalidOperationException("Failed to Create Package!");
            }
        }

        /// <summary>
        /// Executes a console application and returns its exit code.
        /// </summary>
        /// <param name="consoleAppFile">The console application file.</param>
        /// <param name="arguments">The arguments that need to be passed to the console.</param>
        /// <exception cref="CreatePackageException"></exception>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Console app file or arguments are null.</exception>
        /// <exception cref="FileNotFoundException">Console app is not found.</exception>
        private static async Task<int> ExecuteConsoleApplicationAsync(
            FileInfo consoleAppFile,
            string arguments)
        {
            if (consoleAppFile == null) throw new ArgumentNullException(nameof(consoleAppFile));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (!consoleAppFile.Exists) throw new FileNotFoundException(consoleAppFile.Name);
            var consoleOutput = string.Empty;
            try
            {
                var myProcessStartInfo =
                    new ProcessStartInfo()
                    {
                        FileName = consoleAppFile.FullName,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Arguments = arguments
                    };

                using var myProcess = new Process();
                myProcess.StartInfo = myProcessStartInfo;
                myProcess.Start();
                var myStreamReader = myProcess.StandardOutput;
                // stream reader is required to have console executed properly. Removing it will break this method. No alternative has been found yet.
                consoleOutput = await myStreamReader.ReadToEndAsync();
                await myProcess.WaitForExitAsync();
                var consoleExitCode = myProcess.ExitCode;
                return consoleExitCode;
            }
            catch (Exception e)
            {
                throw new CreatePackageException("An Exception occurred in " +
                                                 nameof(ExecuteConsoleApplicationAsync) + consoleOutput + " " +
                                                 e.ToString());
            }
        }
    }
}
