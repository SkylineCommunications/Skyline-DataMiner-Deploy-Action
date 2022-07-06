# Skyline DataMiner Deploy Action

This action builds a DataMiner Application Package (.dmapp) from your Automation Script solution and deploys it to your cloud-connected DataMiner System. The action will wait until the deployment is finished, with a configurable timeout. At present, only DataMiner Automation Script solutions are supported.

The action consists of 2 stages: `Upload` and `Deploy`.

## Stages

### Upload

This stage creates a package and uploads it to dedicated storage in the cloud. The output of this stage will be the ID of the artifact, which can be used in the deploy stage.

### Deploy

This stage deploys the DataMiner Application Package from the artifact storage to your cloud-connected DataMiner System.

## Limitations

This action currently only supports the creation of DataMiner Application Packages with Automation scripts.

## Inputs

### `api-key`
**Required**. The API key generated in the [DCP Admin app](https://admin.dataminer.services) to authenticate to a certain DataMiner System. E.g. `${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}`. For more information about creating a key, refer to the [DataMiner documentation](https://docs.dataminer.services/user-guide/Cloud_Platform/CloudAdminApp/Managing_DCP_keys.html).

### `solution-path`

**Optional**. The path to the .sln file of the solution. At present, only DataMiner Automation Script solutions are supported. E.g. `'./Example/Script.sln'`. Required for stages `'Upload'` and `'All'`.

### `package-name`

**Optional**. The chosen name for the package. E.g. `'MyPackageName'`. Required for stages `'Upload'` and `'All'`.

### `version`

**Optional**.
The version number for the package (format A.B.C). E.g. `'1.0.1'`. Required for stages `'Upload'` and `'All'`.

### `timeout`

**Optional**. The maximum time spent waiting for the deployment to finish, in seconds. Default '900'. E.g. `'300'`.

### `stage`

**Optional**. The stage of the action to run. Options are: `'Upload'`, `'Deploy'` and `'All'`. Default: 'All'.

### `artifact-id`

**Optional**. The private artifact to deploy. This is only needed when 'stage' is `'Deploy'`.


## Outputs

### `artifact-id`

The ID of the private artifact that has been deployed. This is only filled in for stages `'Upload'` and `'All'`.

## Example usage

### All stages at once

```yaml
on: [push]

jobs:
  deploy_package_job:
    runs-on: ubuntu-latest
    name: Deploy the package on the DataMiner System job
    steps:
      # To use this action, the repository must be checked out 
      - name: Checkout	
        uses: actions/checkout@v3
      - name: Deploy the package on the DataMiner System step
        uses: SkylineCommunications/Skyline-DataMiner-Deploy-Action@v1
        id: deploy_package_step
        with:
          api-key: ${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}
          solution-path: './Example/Script.sln'
          package-name: 'MyPackageName'
          version: '1.0.1'
          timeout: '300'
```

```yaml
on: [push]


jobs:
  build:
    name: build
    runs-on: ubuntu-latest
    outputs:
      ARTIFACT_ID: ${{ steps.Build_and_upload_package_step.outputs.ARTIFACT_ID }}
    steps:
      - name: Checkout	
        uses: actions/checkout@v3
      - name: Deploy the package on the DataMiner System step
        uses: SkylineCommunications/Skyline-DataMiner-Deploy-Action@v1
        id: Build_and_upload_package_step
        with:
          api-key: ${{ secrets.NAME_OF_YOUR_APIKEY_SECRET }}
          solution-path: './Example/Script.sln'
          package-name: 'MyPackageName'
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
          stage: Deploy
          timeout: '300'
          artifact-id: ${{ needs.build.outputs.ARTIFACT_ID }}
```

## License

Code and documentation in this project are released under the [MIT License](https://github.com/SkylineCommunications/Skyline-DataMiner-Deploy-Action/blob/feature/preRelease/LICENSE.txt). 
