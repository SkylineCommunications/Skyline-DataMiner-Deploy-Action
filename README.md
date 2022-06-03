# Skyline DataMiner Deploy Action

This action builds a DataMiner Application Package (.dmapp) from your solution and deploys it to your cloud connected DataMiner System. The action will wait until the deployment is finished, with a configurable timeout. At this moment only DataMiner Automation Script solutions are supported.

The action exists of 2 stages `Build and Publish` and `Deploy`

## Stages

### Build and Publish

This stage builds the DataMiner Application Package and then publishes it to our private artifact storage. This stage will then return then Id of this artifact in the output.

### Deploy

This stage Deploys the DataMiner Application Package from the Artifact storage to your cloud connected DataMiner System.

## Inputs

### `api-key`
**Required** The API-key generated in the [DCP Admin app](https://admin.dataminer.services) as authentication for a certain DataMiner System. E.g. `'g597e77412e34297b827c2570a8fa2bc'`.

### `solution-path`

**Optional** The path to the .sln file of the solution. Atm only DataMiner Automation Script solutions are supported. E.g. `'./Example/Script.sln'`. Required for stages `'BuildAndPublish'` and `'All'`.

### `package-name`

**Optional** The chosen name for the package. E.g. `'MyPackageName'`. Required for stages `'BuildAndPublish'` and `'All'`.

### `version`

**Optional** 
The version number for the package (format A.B.C). E.g. `'1.0.1'`. Required for stages `'BuildAndPublish'` and `'All'`.

### `timeout`

**Optional** The maximum time spend on waiting for the deployment to finish (format: HH:MM). Default '12:00' E.g. `'5:00'`.

### `stage`

**Optional** The stage off the action to run. Options are: `'BuildAndPublish'`, `'Deploy'` and `'All'`. Default: 'All'

### `artifact-id`

**Optional** The private artifact to deploy. This is only needed when 'stage' is `'Deploy'`


## Outputs

### `artifact-id`

The Id of the private artifact that has been deployed. This is only filled in for stages `'BuildAndPublish'` and `'All'`.

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
          timeout: '5:00'
```

```yaml
on: [push]


jobs:
  build:
    name: build
    runs-on: ubuntu-latest
    outputs:
      artifact-id: ${{ steps.Build_and_upload_package_step.outputs.artifact-id }}
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
          timeout: '5:00'
          stage: BuildAndPublish

  deploy:
    name: deploy
    runs-on: ubuntu-latest
    needs: build
    steps:
      - name: Deploy
        uses: SkylineCommunications/Skyline-DataMiner-Deploy-Action@v1
        with:
          api-key: d9d676acfbad463184534979cbad9fb2
          stage: Deploy
          artifact-id: ${{ needs.build.outputs.artifact-id }}
```    

## License

Code and documentation in this project are released under the [MIT License](https://github.com/SkylineCommunications/Skyline-DataMiner-Deploy-Action/blob/feature/preRelease/LICENSE.txt). 
