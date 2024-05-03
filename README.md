# Skyline DataMiner Deploy Action

> [!IMPORTANT]
> This GitHub action has been deprecated and is replaced by .NET tools which makes it easier to create workflows/pipelines/... outside GitHub and still be able to deploy package to DataMiner.
>
> The old readme file can be found here: [old readme](oldReadme.md)

## Migration to .NET Tools

Instead of using the GitHub action, we are now using .NET tools. These are more flexible as they can be used on any commonly used platform like GitHub, GitLab, Azure DevOps, ...

The GitHub Action did 3 things in 1 which wasn't always what was wanted by our users:

- Creating a package
- Uploading a package to Catalog
- Deploying a package to a DataMiner

These 3 steps have been converted into their own .NET tool and can be used separately depending on the scenario.

Below is an example of a full migration of the GitHub action:

```yaml
       - name: Install .NET Tools
         run: |
           dotnet tool install -g Skyline.DataMiner.CICD.Tools.Packager
           dotnet tool install -g Skyline.DataMiner.CICD.Tools.CatalogUpload
           dotnet tool install -g Skyline.DataMiner.CICD.Tools.DataMinerDeploy

       - name: Create package name
         id: packageName
         run: |
          tempName="${{ github.repository }} ${{ github.ref_name }}"
          echo name=${tempName//[\"\/\\<>|:*?]/_} >> $GITHUB_OUTPUT
         shell: bash

       - name: Create dmapp package
         run: dataminer-package-create dmapp "${{ github.workspace }}" --type automation --version ${{ github.ref_name }} --output "${{ github.workspace }}" --name "${{ steps.packageName.outputs.name }}"

       - name: Upload to Catalog
         id: uploadToCatalog
         run: echo "id=$(dataminer-catalog-upload --path-to-artifact "${{ github.workspace }}/${{ steps.packageName.outputs.name }}.dmapp" --dm-catalog-token ${{ secrets.DATAMINER_DEPLOY_KEY }})" >> $GITHUB_OUTPUT

       - name: Deploy to DataMiner
         run: dataminer-package-deploy from-catalog --artifact-id "${{ steps.uploadToCatalog.outputs.id }}" --dm-catalog-token ${{ secrets.DATAMINER_DEPLOY_KEY }}

```

More information on how to use these .NET tools can be found on their respective README files:

- [Creating a package](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Packages/blob/main/Tools.Packager/README.md)
- [Uploading a package to the Catalog](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Tools.CatalogUpload/blob/main/CICD.Tools.CatalogUpload/README.md)
- [Deploying a package to a DataMiner](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Tools.DataMinerDeploy/blob/main/CICD.Tools.DataMinerDeploy/README.md)
