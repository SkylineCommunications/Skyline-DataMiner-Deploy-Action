namespace Package.Builder
{
    using System.Diagnostics;

    using Package.Builder.Exceptions;
    using Package.Domain.Enums;
    using Package.Domain.Models;
    using Package.Domain.Services;

    using Skyline.DataMiner.CICD.DMApp.Automation;
    using Skyline.DataMiner.CICD.DMApp.Common;
    using Skyline.DataMiner.CICD.Loggers;

    public class TempLogger : ILogCollector
    {
        public IList<string> Logging { get; }

        public void ReportError(string error)
        {
            // Do Nothing
        }

        public void ReportStatus(string status)
        {
            // Do Nothing
        }

        public void ReportWarning(string warning)
        {
            // Do Nothing
        }
    }

    public class PackageBuilder : IPackageBuilder
    {
        public async Task<CreatedPackage> CreatePackageAsync(
            LocalPackageConfig localPackageConfig)
        {
            if (localPackageConfig.Type != SolutionType.DmScript)
            {
                throw new UnsupportedSolutionException($"Solution of type {localPackageConfig.Type} is not supported.");
            }

            var logger = new TempLogger();
            DMAppVersion version = DMAppVersion.FromProtocolVersion(localPackageConfig.Version);
            var dmappPackageCreator = PackageCreatorForAutomation.Factory.FromRepository(logger, Path.GetFullPath(localPackageConfig.SolutionFile.FullName), localPackageConfig.PackageName, version);
            var dmappPackage = await dmappPackageCreator.CreateAsync();

            return new CreatedPackage(
                dmappPackage,
                localPackageConfig.PackageName,
                localPackageConfig.Type.ToString(),
                localPackageConfig.Version);
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
