using System.Diagnostics;
using Package.Builder.Exceptions;
using Package.Domain.Enums;
using Package.Domain.Models;
using Package.Domain.Services;

namespace Package.Builder
{
    public class PackageBuilder : IPackageBuilder
    {
        public async Task<CreatedPackage> CreatePackageAsync(
            LocalPackageConfig localPackageConfig)
        {
            var convertedFilesDirectory =
                new DirectoryInfo(Path.Combine(localPackageConfig.ConvertedSolutionWorkingDirectory.FullName, "__SLC_CONVERTED__"));

            if (localPackageConfig.Type != SolutionType.DmScript)
            {
                throw new UnsupportedSolutionException($"Solution of type {localPackageConfig.Type} is not supported.");
            }

            try
            {
                await ConvertAutomationScriptSolutionAsync(localPackageConfig.SolutionFile, convertedFilesDirectory);

                var dmappPackage = await BuildDmappPackageForAutomationAsync(
                    localPackageConfig.ConvertedSolutionWorkingDirectory,
                    localPackageConfig.PackageName,
                    localPackageConfig.Version);

                return new CreatedPackage(
                    dmappPackage,
                    localPackageConfig.PackageName,
                    localPackageConfig.Type.ToString(),
                    localPackageConfig.Version);
            }
            catch (CreatePackageException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Converts a solution to the DataMiner Format.
        /// </summary>
        /// <param name="solutionFile">The solution file.</param>
        /// <param name="outputDirectoryInfo">The directory into which the output needs to be saved.</param>
        /// <param name="type">The solution type.</param>
        /// <exception cref="CreatePackageException">Error encountered while converting the solution.</exception>
        /// <returns>A boolean indicating the conversion state. True if successfull, false in case errors occurred.</returns>
        private static async Task ConvertAutomationScriptSolutionAsync(
            FileInfo solutionFile, 
            DirectoryInfo outputDirectoryInfo)
        {
            await AutomationScriptBuilder.Program.Main(new[]
                { solutionFile.FullName , outputDirectoryInfo.FullName });
        }

        /// <summary>
        /// Builds a DataMiner Application package.
        /// </summary>
        /// <param name="convertedSolutionWorkingDirectory">The working directory containing the converted solution files in a folder with name '__SLC_CONVERTED__'.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="version">The version of the package.</param>
        /// <param name="installScriptInfo"></param>
        /// <exception cref="ArgumentException">When convertedSolutionFilesDirectory is not found or does not contain a '__SLC_CONVERTED__' directory.</exception>
        /// <exception cref="ArgumentNullException">Arguments cannot be null.</exception>
        /// <returns></returns>
        /// <exception cref="CreatePackageException">When an exception is encountered building the package.</exception>
        private static async Task<FileInfo> BuildDmappPackageForAutomationAsync(
            DirectoryInfo convertedSolutionWorkingDirectory,
            string packageName,
            string version)
        {
            if (convertedSolutionWorkingDirectory == null)
                throw new ArgumentNullException(nameof(convertedSolutionWorkingDirectory));
            if (packageName == null) throw new ArgumentNullException(nameof(packageName));
            if (version == null) throw new ArgumentNullException(nameof(version));
            if (!convertedSolutionWorkingDirectory.Exists) 
                throw new ArgumentException("Directory not found: " + nameof(convertedSolutionWorkingDirectory));

            var convertedFilesDirectory =
                new DirectoryInfo(Path.Combine(convertedSolutionWorkingDirectory.FullName, "__SLC_CONVERTED__"));

            if (!convertedFilesDirectory.Exists)
                throw new ArgumentException("Directory "+ nameof(convertedSolutionWorkingDirectory) + " should contain a  '__SLC_CONVERTED__' directory.");

            try
            {
                await Skyline.SystemEngineering.CiCd.Tools.Program.Main(new[]
                {
                    convertedSolutionWorkingDirectory.FullName,
                    packageName,
                    "/",
                    "/",
                    "1",
                    "Automation",
                    "Customer",
                    version
                });

                var dmAppPackageFile =
                    new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), packageName + ".dmapp"));
                return dmAppPackageFile;
            }
            catch (Exception e)
            {
                throw new CreatePackageException("Building package failed with exception " + e);
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
            var consoleOutput=string.Empty;
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
