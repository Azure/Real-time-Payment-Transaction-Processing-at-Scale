# Cosmos DB NoSQL API - Payments

## Introduction

This repository provides a code sample in .NET on how you might use a combination of Azure Functions, Cosmos DB, OpenAI and EventHub to implement a payment tracking process.

## Scenario

The scenario centers around a payments and transactions solution. Members having accounts, each account with corresponding balances, overdraft limits and credit/debit transactions. 

Transaction data is replicated across multiple geographic regions for both reads and writes, while maintaining consistency. Updates are made efficiently with the patch operation.

Business rules govern if a transaction is allowed. 

An AI powered co-pilot enables agents to analyze transactions using natural language.

## Solution Architecture

The solution architecture is represented by this diagram:

<p align="center">
    <img src="img/architecture.png" width="100%">
</p>

## Deployment

### Prerequisites

- Powershell
- Azure CLI 2.49.0 or greater

### Prerequisites for running/debugging locally

- Backend (Function App, Console Apps, etc.)
  - Visual Studio Code or Visual Studio 2022
  - .NET 7 SDK
- Frontend (React web app)
  - Visual Studio Code
  - Ensure you have the latest version of NPM and node.js:
    - Install NVM from https://github.com/coreybutler/nvm-windows
    - Run nvm install latest
    - Run nvm list (to see the versions of NPM/node.js available)
    - Run nvm use latest (to use the latest available version)

To start the React web app:

1. Navigate to the `ui` folder
2. Run npm install to restore the packages
3. Run npm run dev
4. Open localhost:3000 in a web browser

#### Running the backend locally

- Since the app uses role-based access control (RBAC), if you want to run the Function App locally, you have to assign yourself to the "Cosmos DB Built-in Data Contributor" role via the Azure Cloud Shell or Azure CLI with the following:

    ```bash
    az cosmosdb sql role assignment create --account-name YOUR_COSMOS_DB_ACCOUNT_NAME --resource-group YOUR_RESOURCE_GROUP_NAME --scope "/" --principal-id YOUR_AZURE_AD_PRINCIPAL_ID --role-definition-id 00000000-0000-0000-0000-000000000002
    ```

- Event Hubs is also using RBAC. The Member Repository sends an Event Hubs event when patching members. The `CorePayments.EventMonitor` monitor listens for Event Hub events and displays the output. For the events to work, you need to add yourself to the "Azure Event Hubs Data Owner" role via the Azure Cloud Shell or Azure CLI with the following:

    ```bash
    az role assignment create --assignee "YOUR_EMAIL_ADDRESS" --role "Azure Event Hubs Data Owner" --scope "/subscriptions/YOUR_AZURE_SUBSCRIPTION_ID/resourceGroups/YOUR_RESOURCE_GROUP_NAME/providers/Microsoft.EventHub/namespaces/YOUR_EVENT_HUBS_NAMESPACE"
    ```

    > Make sure you're signed in to Azure from the Visual Studio or VS Code terminal before running the Function App locally. You need to run `az login` and `az account set --subscription YOUR_AZURE_SUBSCRIPTION_ID` first.

### Standard Deployments

From the `deploy/powershell` folder, run the following command. This should provision all of the necessary infrastructure, deploy builds to the function apps, deploy the frontend, and deploy necessary artifacts to the Synapse workspace.

```pwsh
.\Unified-Deploy.ps1 -resourceGroup <resource-group-name> `
                     -subscription <subscription-id>
```

### Cloud Shell Based Deployments

Create a cloud shell environment in a tenant that contains the target subscription.  Clone the repository and then execute the `CloudShell-Deploy.ps1` script as illustrated in the following snippet.  This will provision all of the required infrastructure and deploy the API and web app services into AKS.

```pwsh
git clone --recurse-submodules https://github.com/AzureCosmosDB/RealTimeTransactions.git
cd RealTimeTransactions
chmod +x ./deploy/powershell/*
./deploy/powershell/CloudShell-Deploy.ps1 -resourceGroup <rg-name> `
                                          -subscription <target-subscription>
```

### Azure VM Based Deployments

Run the following script to provision a development VM with Visual Studio 2022 Community and required dependencies preinstalled.

```pwsh
.\deploy\powershell\Deploy-Vm.ps1 -resourceGroup <rg-name> -location EastUS
```

When the script completes, the console output should display the name of the provisioned VM similar to the following:

```
The resource prefix used in deployment is libxarwttxjde
The deployed VM name used in deployment is libxarwttxjdevm
```

Use RDP to remote into the freshly provisioned VM with the username `BYDtoChatGPTUser` and password `Test123456789!`.  Open up a powershell terminal and run the following script to provision the infrastructure and deploy the API and frontend. This will provision all of the required infrastructure, deploy the API and web app services into AKS, and import data into Cosmos.

```pwsh
git clone https://github.com/hatboyzero/PaymentsProcessing.git
cd PaymentsProcessing
./deploy/powershell/Unified-Deploy.ps1 -resourceGroup <rg-name> `
                                       -location EastUS `
                                       -subscription <target-subscription>
```

### Publish the React web app after making changes

If you make changes to the React web app and want to redeploy it, run the following from the `deploy/powershell` folder:


```pwsh
./Publish-Site.ps1 -resourceGroup <resource-group-name> `
                   -storageAccount <storage-account-name (webpayxxxx)>
```

### Enabling/Disabling Deployment Steps

The following flags can be used to enable/disable specific deployment steps in the `Unified-Deploy.ps1` script.

| Parameter Name | Description |
|----------------|-------------|
| stepDeployBicep | Enables or disables the provisioning of resources in Azure via Bicep templates (located in `./infrastructure`). Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/powershell/Deploy-Bicep.ps1` script.
| stepPublishFunctionApp | Enables or disables the publish and zip deployment of the `CorePayments.FunctionApp` project to the regional function apps present in the target resource group. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Publish-FunctionApp.ps1` script.
| stepDeployOpenAi | Enables or disables the provisioning of (or detection of an existing) Azure OpenAI service. If an explicit OpenAi resource group is not defined in the `openAiRg` parameter, the target resource group defaults to that passed in the `resourceGroup` parameter. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Deploy-OpenAi.ps1` script.
| stepPublishSite | Enables or disables the build and deployment of the static HTML site to the hosting storage account in the target resource group. Valid values are 0 (Disabled) and 1 (Enabled). See the `deploy/infrastructure/Publish-Site.ps1` script.
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

### Quickstart

1. After deployment is complete, go to the resource group for your deployment and open the Azure Storage Account prefixed with `web`.  This is the storage account hosting the static web app.
1. Select the `Static website` blade in the left-hand navigation pane and copy the site URL from the `Primary endpoint` field in the detail view.

    <p align="center">
        <img src="img/website-url.png" width="100%">
    </p>

1. Browse to the URL copied in the previous step to access the web app.
