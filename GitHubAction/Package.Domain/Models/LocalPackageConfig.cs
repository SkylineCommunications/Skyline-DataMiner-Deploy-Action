using Package.Domain.Enums;

namespace Package.Domain.Models
{
    public class LocalPackageConfig
    {
        public FileInfo SolutionFile { get; }
        public DirectoryInfo ConvertedSolutionWorkingDirectory { get; }
        public string PackageName { get; }
        public string Version { get; }
        public SolutionType Type { get; }

        public LocalPackageConfig(FileInfo solutionFile,
            DirectoryInfo convertedSolutionWorkingDirectory,
            string packageName,
            string version,
            SolutionType type)
        {
            SolutionFile = new FileInfo(solutionFile.FullName);
            ConvertedSolutionWorkingDirectory = convertedSolutionWorkingDirectory;
            PackageName = packageName;
            Version = version;
            Type = type;
        }
    }
}
