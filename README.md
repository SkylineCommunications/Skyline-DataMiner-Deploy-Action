# Skyline DataMiner Deploy Action

This action builds a DataMiner Application Package (.dmapp) from your solution and deploys it to your cloud connected DataMiner System. The action will wait until the deployment is finished, with a configurable timeout. At this moment only DataMiner Automation Script solutions are supported.

## Inputs

### `api-key`
**Required** The API-key generated in the [DCP Admin app](https://admin.dataminer.services) as authentication for a certain DataMiner System. E.g. `'g597e77412e34297b827c2570a8fa2bc'`.

### `solution-path`

**Required** The path to the .sln file of the solution. Atm only DataMiner Automation Script solutions are supported. E.g. `'./Example/Script.sln'`.

### `package-name`

**Required** The chosen name for the package. E.g. `'MyPackageName'`.

### `version`

**Required** 
The version number for the package (format A.B.C). E.g. `'1.0.1'`.

### `timeout`

**Optional** The maximum time spend on waiting for the deployment to finish (format: HH:MM). Default '12:00' E.g. `'5:00'`.

## Example usage

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
        uses: actions/Skyline-DataMiner-Deploy-Action@v1 # Uses the action in the root directory
        id: deploy_package_step
        with:
          api-key: 'g597e77412e34297b827c2570a8fa2bc'
          solution-path: './Example/Script.sln'
          package-name: 'MyPackageName'
          version: '1.0.1'
          timeout: '5:00'
```
## License

Code and documentation in this project are released under the [MIT License](https://github.com/SkylineCommunications/Skyline-DataMiner-Deploy-Action/blob/feature/preRelease/LICENSE.txt). 
