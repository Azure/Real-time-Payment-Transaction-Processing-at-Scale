# Deployment - Azure VM

## Prerequisites

- Azure subscription
- Subscription access to Azure OpenAI service. Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUOFA5Qk1UWDRBMjg0WFhPMkIzTzhKQ1dWNyQlQCN0PWcu)

## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription.

1. Run the following script to provision a development VM with Visual Studio 2022 Community and required dependencies preinstalled.

    ```pwsh
    .\deploy\powershell\Deploy-Vm.ps1 -resourceGroup <rg_name> -location <location> -password <password>
    ```

    `<password>` is the password for the `BYDtoChatGPTUser` account that will be created on the VM. It must be at least 12 characters long and meet the complexity requirements of Azure VMs.

    When the script completes, the console output should display the name of the provisioned VM similar to the following:

    ```txt
    The resource prefix used in deployment is libxarwttxjde
    The deployed VM name used in deployment is libxarwttxjdevm
    ```

1. Use RDP to remote into the freshly provisioned VM with the username `BYDtoChatGPTUser` and the password you provided earlier on.  

1. Add the `BYDtoChatGPTUser` account to the `docker-users` local group on the VM. Sign out and sign back in to the VM to apply the changes.

1. Install WSL2 by running the following command in a command prompt:

    ```cmd
    wsl --install
    ```

    > If the above command returns "Windows Subsystem for Linux is already installed.", then execute the following command to update WSL: `wsl --update`. You do not need to restart if you are only updating WSL.

1. Restart the VM to complete the setup.

1. Log back in with the `BYDtoChatGPTUser` account and start `Docker Desktop`. Ensure the Docker engine is up and running. Keep `Docker Desktop` running in the background.

1. Clone the repository:

    ```cmd
    git clone --recurse-submodules https://github.com/AzureCosmosDB/RealTimeTransactions.git
    ```

1. Open PowerShell, navigate to the `RealTimeTransactions` folder, and run the following script to provision the infrastructure and deploy the API and frontend. This will provision all of the required infrastructure, deploy the API and web app services into AKS, and provision and load artifacts into a Synapse Analytics workspace.

    ```pwsh
    ./deploy/powershell/Unified-Deploy.ps1 -resourceGroup <rg_name> -subscription <target_subscription_id>
    ```

>**NOTE**: Make sure to set the `<location>` value to a region that supports Azure OpenAI services.  See [Azure OpenAI service regions](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all) for more information.

### Enabling/Disabling Deployment Steps

The following flags can be used to enable/disable specific deployment steps in the `Unified-Deploy.ps1` script.

| Parameter Name | Description |
|----------------|-------------|
| stepDeployBicep | Enables or disables the provisioning of resources in Azure via Bicep templates (located in `./infrastructure`). Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/powershell/Deploy-Bicep.ps1` script.
| stepBuildPush | Enables or disables the build and push of Docker images into the Azure Container Registry (ACR). Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/BuildPush.ps1` script.
| stepDeployFD | Enables or disables deploying Azure Front Door. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Deploy-FDOrigins.ps1` script.
| stepDeployImages | Enables or disables deploying the Docker images from the `CoreClaims.WebAPI` and `CoreClaims.WorkerService` projects to AKS. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Deploy-Images-Aks.ps1` script.
| stepPublishSite | Enables or disables the build and deployment of the static HTML site to the hosting storage account in the target resource group. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Publish-Site.ps1` script.
| stepLoginAzure | Enables or disables interactive Azure login. If disabled, the deployment assumes that the current Azure CLI session is valid. Valid values are 0 (Disabled).

Example command:

```pwsh
cd deploy/powershell
./Unified-Deploy.ps1 -resourceGroup myRg `
                     -subscription 0000... `
                     -stepLoginAzure 0 `
                     -stepDeployBicep 0 `
                     -stepDeployFD 0 `
                     -stepBuildPush 1 `
                     -stepDeployImages 1 `
                     -stepPublishSite 1
```
