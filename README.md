# cosmos-payments-demo

## Deployment

### Standard Deployments

From the `deploy/powershell` folder, run the following:

```pwsh
.\Unified-Deploy.ps1 -resourceGroup <resource-group-name> `
                     -subscription <subscription-id>
```

### Deployments using an existing OpenAI service

From the `deploy/powershell` folder, run the following:

```pwsh
.\Unified-Deploy.ps1 -resourceGroup <resource-group-name> `
                     -subscription <subscription-id> `
                     -openAiName <openAi-service-name> `
                     -openAiRg <openAi-resource-group-name> `
                     -openAiDeployment <openAi-completions-deployment-name>
```

### Enabling/Disabling Deployment Steps

The following flags can be used to enable/disable specific deployment steps in the `Unified-Deploy.ps1` script.

| Parameter Name | Description |
|----------------|-------------|
| stepDeployBicep | Enables or disables the provisioning of resources in Azure via Bicep templates (located in `./infrastructure`). Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/powershell/Deploy-Bicep.ps1` script.
| stepPublishFunctionApp | Enables or disables the publish and zip deployment of the `CorePayments.FunctionApp` project to the regional function apps present in the target resource group. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Publish-FunctionApp.ps1` script.
| stepDeployOpenAi | Enables or disables the provisioning of (or detection of an existing) Azure OpenAI service. If an explicit OpenAi resource group is not defined in the `openAiRg` parameter, the target resource group defaults to that passed in the `resourceGroup` parameter. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Deploy-OpenAi.ps1` script.
| stepPublishSite | Enables of disables the build and deployment of the static HTML site to the hosting storage account in the target resource group. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Publish-Site.ps1` script.
| stepLoginAzure | Enables or disables interactive Azure login. If disabled, the deployment assumes that the current Azure CLI session is valid. Valid values are 0 (Disabled). 

Example command:
```pwsh
cd deploy/powershell
./Unified-Deploy.ps1 -resourceGroup myRg `
                     -subscription 0000... `
                     -openAiName myOpenAi `
                     -openAiRg myOpenAiRg `
                     -openAiDeployment completions `
                     -stepLoginAzure 0 `
                     -stepDeployBicep 0 `
                     -stepPublishFunctionApp 1 `
                     -stepPublishSite 1
```