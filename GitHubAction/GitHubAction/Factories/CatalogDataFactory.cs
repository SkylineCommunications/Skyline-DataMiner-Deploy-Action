namespace GitHubAction.Factories
{
    using Catalog.Domain;

    using GIT;

    using global::GitHubAction.Domain.Entities;

    using Package.Domain.Models;

    using System.Text.RegularExpressions;

    internal class CatalogDataFactory
    {
        public static CatalogData Create(Inputs inputs, CreatedPackage createdPackage, IGITInfo git, string sourceUri, string branch)
        {
            // Fallback
            if (string.IsNullOrWhiteSpace(sourceUri))
            {
                sourceUri = git.GetSourceUrl();
            }

            if (string.IsNullOrWhiteSpace(branch) || branch == inputs.Version)
            {
                branch = git.GetCurrentBranch(inputs.Version);
            }

            CatalogData catalog = new CatalogData()
            {
                ContentType = createdPackage.Type,
                // Branch = git.GetCurrentBranch() ?? String.Empty,
                Branch = branch,
                Version = string.IsNullOrWhiteSpace(inputs.Version) ? "0.0.0" : inputs.Version,
                Identifier = sourceUri,
                Name = inputs.PackageName ?? "PackageName",
            };

            // Get Identifier from GIT

            // Get Branch from GIT

            bool isPreRelease = CleanAndAddVersion(inputs, catalog);
            catalog.IsPreRelease = isPreRelease;

            return catalog;
        }

        private static bool CleanAndAddVersion(Inputs inputs, CatalogData catalog)
        {
            bool isPreRelease;
            if (!string.IsNullOrWhiteSpace(inputs.BuildNumber))
            {
                isPreRelease = true;
                catalog.Version = $"0.0.{inputs.BuildNumber}";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(inputs.Version))
                {
                    if (Regex.IsMatch(inputs.Version, "^[0-9.]+-.*$"))
                    {
                        isPreRelease = true;
                        catalog.Version = inputs.Version;
                    }
                    else
                    {
                        isPreRelease = false;
                        catalog.Version = inputs.Version;
                    }
                }
                else
                {
                    isPreRelease = true;
                    catalog.Version = "0.0.0";
                }
            }

            return isPreRelease;
        }
    }
}
