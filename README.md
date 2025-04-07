# Skyline DataMiner Deploy Action

> [!IMPORTANT]
> This GitHub action no longer runs under its own docker image. The docker image has been deprecated and is replaced by .NET tools which makes it easier to create workflows/pipelines/... outside GitHub and still be able to deploy packages to DataMiner.
> You can still use this GitHub action in GitHub workflows. It will perform the dotnet tool calls on the current runner.

## **Important Changes Effective from May 1, 2025**

- artifact-id is deprecated and will be removed.

- Deployments now require specifying the catalog-guid within a catalog.yml file, as explained here.

- The api-key must now exclusively contain an Organization Key.

- A new mandatory input, agent-destination-id, specifying the target DataMiner Agent is required.

These changes improve security, enhance flexibility, and ensure compatibility with newer workflows and tooling.

> [!IMPORTANT]
> Only Legacy Automation Script solutions are supported. Automation Scripts build upon the Skyline.DataMiner.Sdk are not supported by this Action. Please use the .NET Tools and built-in SDK actions (build and publish).

## **Important Changes Since Version 2.0.0**

- The **catalog identifier** (GUID identifying the catalog item on [catalog.dataminer.services](https://catalog.dataminer.services/)) is now required. It must be specified in a `catalog.yml` file as described [here](https://docs.dataminer.services/user-guide/Cloud_Platform/Catalog/Register_Catalog_Item.html#manifest-file).
  
- If a `README.md` file or an `Images` folder exists in the same directory (or a parent directory) as the `.dmapp` file, they will be automatically registered alongside the package.

- Either the repository’s name or a GitHub topic must be used to infer the catalog item type.

### **Inferring Catalog Item Type:**

The GitHub action can automatically determine the artifact type in two ways:

1. **Repository Naming Convention:**
   - If the repository follows the naming conventions outlined in the [GitHub Repository Naming Convention](https://docs.dataminer.services/develop/CICD/Skyline%20Communications/Github/Use_Github_Guidelines.html#repository-naming-convention), the tool can infer the type from the repository name itself without needing a GitHub topic.

2. **GitHub Topic:**
   - If the repository does not follow the naming convention, the tool relies on the presence of a GitHub topic that matches one of the [Artifact Types](#artifact-types) to determine the type.

If neither the repository name follows the convention nor the appropriate GitHub topic is present, the tool will fail to detect the type and return an error.

### Artifact Types

- **AS**: automationscript
- **C**: connector
- **CF**: companionfile
- **CHATOPS**: chatopsextension
- **D**: dashboard
- **DISMACRO**: dismacro
- **DOC**: documentation
- **F**: functiondefinition
- **GQIDS**: gqidatasource
- **GQIO**: gqioperator
- **LSO**: lifecycleserviceorchestration
- **PA**: processautomation
- **PLS**: profileloadscript
- **S**: solution
- **SC**: scriptedconnector
- **T**: testingsolution
- **UDAPI**: userdefinedapi
- **V**: visio

> [!IMPORTANT]
> The valid types for this GitHub Action are limited to 'automationscript', 'gqidatasource', 'gqioperator', 'lifecycleserviceorchestration', 'profileloadscript' and 'userdefinedapi'

## Transition to .NET Tools

Our workflow has evolved from relying solely on GitHub actions to embracing the versatility of .NET tools. This transition offers enhanced flexibility, enabling seamless integration across various widely utilized platforms such as GitHub, GitLab, Azure DevOps, and more.

The GitHub Action encompassed a bundled approach, performing three distinct tasks within a single docker image in the background:

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
         run: dataminer-catalog-upload with-registration --path-to-artifact "${{ github.workspace }}/${{ steps.packageName.outputs.name }}.dmapp" --dm-catalog-token ${{ secrets.api-key }} --artifact-version ${{ inputs.referenceName }}
 
       - name: Deploy to DataMiner
         run: dataminer-package-deploy from-catalog --catalog-id TODO:FillInCatalogGuidHere --catalog-version ${{ secrets.api-key }} --agent-destination-id TODO:FillInAgentDestinationGuidHere --dm-catalog-token ${{ secrets.api-key }}

```

More information on how to use these .NET tools can be found on their respective README files:

- [Creating a package](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Packages/blob/main/Tools.Packager/README.md)
- [Uploading a package to the Catalog](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Tools.CatalogUpload/blob/main/CICD.Tools.CatalogUpload/README.md)
- [Deploying a package to a DataMiner](https://github.com/SkylineCommunications/Skyline.DataMiner.CICD.Tools.DataMinerDeploy/blob/main/CICD.Tools.DataMinerDeploy/README.md)

## Github Action

This action builds a DataMiner Artifact from your Automation Script solution and deploys it to your cloud-connected DataMiner System. The action will wait until the deployment is finished, with a configurable timeout. At present, only DataMiner Automation Script solutions created by DIS are supported.

The action consists of 2 stages: `Upload` and `Deploy`.

## Stages

### Upload

This stage creates an artifact and uploads it to dedicated storage in the cloud. The output of this stage will be the ID of the artifact, which can be used in the deploy stage.

### Deploy

This stage deploys the artifact from the artifact storage to your cloud-connected DataMiner System.

## Limitations

This action currently only supports the creation of artifacts with Automation scripts as a solution.

## Inputs

### `api-key`

**Required**. The API key generated in the [DCP Admin app](https://admin.dataminer.services) to authenticate to a certain DataMiner Organization. E.g. `${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}`. For more information about creating a key, refer to the [DataMiner documentation](https://docs.dataminer.services/user-guide/Cloud_Platform/CloudAdminApp/Managing_DCP_keys.html).

### `solution-path`

**Required**. The path to the .sln file of the solution. At present, only DataMiner Automation Script solutions are supported. E.g. `'./Example/AutomationScript.sln'`. Required for stages `'Upload'` and `'All'`.

### `github-token`

**Optional**. The secrets.GITHUB_TOKEN.  Required for stages `'Upload'` and `'All'`.

### `artifact-name`

**Optional**. The chosen name for the artifact. E.g. `'MyPackageName'`. Required for stages `'Upload'` and `'All'`.

### `version`

**Optional**.
The version number for the artifact. Only required for a release run. Format A.B.C for a stable release or A.B.C-text for a pre-release. E.g. `'1.0.1'`. Required for stages `'Upload'` and `'All'` if no build-number was provided instead.

### `timeout`

**Optional-Deprecated**. The maximum time spent waiting for the deployment to finish, in seconds. Default '900'. E.g. `'300'`.

### `stage`

**Optional**. The stage of the action to run. Options are: `'Upload'`, `'Deploy'` and `'All'`. Default: 'All'.

### `destination-agent-id`

**Optional**. The destination agent ID to deploy to. To obtain this ID for an existing DataMiner System, navigate to its details page in the Admin app. The ID is the last GUID of the URL. This is required when the dm-catalog-token is an OrganizationToken.Required for stages `'Deploy'` and `'All'`.

### `build-number`

**Optional**.
The build number of a workflow run. Only required for a development run. Required for stages `'Upload'` and `'All'` if no version was provided instead.

## Example usage

### All stages at once

```yaml
on: [push]

jobs:
  deploy_artifact_job:
    runs-on: ubuntu-latest
    name: Deploy the artifact on the DataMiner System job
    steps:
      # To use this action, the repository must be checked out 
      - name: Checkout
        uses: actions/checkout@v4
      - name: Set up NuGet
        uses: nuget/setup-nuget@v2.0.1     
      - name: NuGet restore solution
        run: nuget restore "AutomationScript.sln" -OutputDirectory ${{ github.workspace }}/packages
      - name: Deploy the artifact on the DataMiner System step
        uses: SkylineCommunications/Skyline-DataMiner-Deploy-Action@v1
        id: deploy_artifact_step
        with:
          api-key: ${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}
          solution-path: './Example/AutomationScript.sln'
          github-token: ${{ secrets.GITHUB_TOKEN }}
          artifact-name: 'MyArtifactName'
          version: '1.0.1'
          agent-destination-id: 'abc-abc-abc'
```

### Separate stages

```yaml
on: [push]


jobs:
  build:
    name: build
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      - name: Set up NuGet
        uses: nuget/setup-nuget@v2.0.1     
      - name: NuGet restore solution
        run: nuget restore "AutomationScript.sln" -OutputDirectory ${{ github.workspace }}/packages
      - name: Deploy the artifact on the DataMiner System step
        uses: SkylineCommunications/Skyline-DataMiner-Deploy-Action@v1
        id: Build_and_upload_artifact_step
        with:
          api-key: ${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}
          solution-path: './Example/AutomationScript.sln'
          github-token: ${{ secrets.GITHUB_TOKEN }}
          artifact-name: 'MyArtifactName'
          version: '1.0.1'
          stage: Upload

  deploy:
    name: deploy
    runs-on: ubuntu-latest
    needs: build
    steps:
      - name: Deploy
        uses: SkylineCommunications/Skyline-DataMiner-Deploy-Action@v1
        with:
          api-key: ${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}
          github-token: ${{ secrets.GITHUB_TOKEN }}
          agent-destination-id: 'abc-abc-abc'
          version: '1.0.1'
          stage: Deploy
```

## License

Code and documentation in this project are released under the [MIT License](https://github.com/SkylineCommunications/Skyline-DataMiner-Deploy-Action/blob/feature/preRelease/LICENSE.txt).
