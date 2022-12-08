using Package.Domain.Enums;

namespace Package.Domain.Models
{
    public class LocalPackageConfig
    {
        public FileInfo SolutionFile { get; }
        public string PackageName { get; }
        public string Version { get; }
        public SolutionType Type { get; }
        public Uri? SourceUri { get; }
        public string BuildNumber { get; }

        public LocalPackageConfig(FileInfo solutionFile,
            string packageName,
            string version,
            SolutionType type,
            Uri? sourceUri,
            string buildNumber = "")
        {
            SolutionFile = new FileInfo(solutionFile.FullName);
            PackageName = packageName;
            Version = version;
            Type = type;
            SourceUri = sourceUri;
            BuildNumber = buildNumber;
        }
    }
}
