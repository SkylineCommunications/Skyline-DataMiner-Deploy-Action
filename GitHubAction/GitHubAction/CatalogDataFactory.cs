namespace GitHubAction
{
    using Catalog.Domain;

    using GIT;

    using global::GitHubAction.Domain.Entities;

    using Package.Domain.Models;

    using System.Text.RegularExpressions;

    internal class CatalogDataFactory
    {
        public static CatalogData Create(Inputs inputs, CreatedPackage createdPackage, IGITInfo git)
        {
            CatalogData catalog = new CatalogData()
            {
                ContentType = createdPackage.Type,
                Branch = git.GetCurrentBranch()??String.Empty,
                Version = String.IsNullOrWhiteSpace(inputs.Version) ? "0.0.0" : inputs.Version,
                Identifier = git.GetSourceUrl()??String.Empty,
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
            if (!String.IsNullOrWhiteSpace(inputs.BuildNumber))
            {
                isPreRelease = true;
                catalog.Version = $"0.0.{inputs.BuildNumber}";
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(inputs.Version))
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
