# Skyline DataMiner Deploy Action

> [!IMPORTANT]
> This GitHub action has been deprecated and is replaced by .NET tools which makes it easier to create workflows/pipelines/... outside GitHub and still be able to deploy package to DataMiner.
>
> The old readme file can be found here: [old readme](oldReadme.md)

## Transition to .NET Tools

Our workflow has evolved from relying solely on GitHub actions to embracing the versatility of .NET tools. This transition offers enhanced flexibility, enabling seamless integration across various widely utilized platforms such as GitHub, GitLab, Azure DevOps, and more.

Previously, the GitHub Action encompassed a bundled approach, performing three distinct tasks:

1. Generating a package
1. Uploading the package to the Catalog
1. Deploying the package to a DataMiner

However, this bundled functionality didn't always align with the specific needs of our users. In response, we have modularized these tasks into individual .NET tools, allowing for tailored usage based on the unique requirements of each scenario.

Below, we present an example detailing the migration process from the GitHub action:

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
         run: echo id=$(dataminer-catalog-upload with-registration --path-to-artifact "${{ github.workspace }}/${{ steps.packageName.outputs.name }}.dmapp" --dm-catalog-token ${{ secrets.api-key }} --uri-sourcecode "${{ github.server_url }}/${{ github.repository }}" --artifact-version ${{ inputs.referenceName }}) >> $GITHUB_OUTPUT
 
       - name: Deploy to DataMiner
         run: dataminer-package-deploy from-catalog --artifact-id "${{ steps.uploadToCatalog.outputs.id }}" --dm-catalog-token ${{ secrets.DATAMINER_DEPLOY_KEY }}

```

More information on how to use these .NET tools can be found on their respective README files:

- [Creating a package](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Packages/blob/main/Tools.Packager/README.md)
- [Uploading a package to the Catalog](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Tools.CatalogUpload/blob/main/CICD.Tools.CatalogUpload/README.md)
- [Deploying a package to a DataMiner](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Tools.DataMinerDeploy/blob/main/CICD.Tools.DataMinerDeploy/README.md)
