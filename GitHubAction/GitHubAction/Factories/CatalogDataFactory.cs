namespace GitHubAction.Factories
{
    using Catalog.Domain;

    using GIT;

    using global::GitHubAction.Domain.Entities;

    using Package.Domain.Models;

    using System.Text.RegularExpressions;

    internal class CatalogDataFactory
    {
        public static CatalogData Create(Inputs inputs, CreatedPackage createdPackage, IGITInfo git, string sourceUri, string branch, string releaseUri)
        {
            if (string.IsNullOrWhiteSpace(branch) || branch == inputs.Version)
            {
                branch = git.GetCurrentBranch(inputs.Version);
            }

            string committerMail = git.GetCommitterMail();

            CatalogData catalog = new CatalogData()
            {
                ContentType = createdPackage.Type,
                Branch = branch,
                Version = string.IsNullOrWhiteSpace(inputs.Version) ? "0.0.0" : inputs.Version,
                Identifier = sourceUri,
                Name = inputs.PackageName ?? "PackageName",
                CommitterMail = committerMail,
                ReleaseUri = releaseUri
            };

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
