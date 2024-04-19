# Deployment - Cloud shell

## Prerequisites

- Azure subscription
- Subscription access to Azure OpenAI service. Start here to [Request Access to Azure OpenAI Service](https://aka.ms/oaiapply)
- Azure Cloud Shell environment (follow [these instructions](https://learn.microsoft.com/en-us/azure/cloud-shell/quickstart?tabs=azurecli) to setup your Cloud Shell)

## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription.

1. Create a cloud shell PowerShell environment in a tenant that contains the target subscription.  

1. Clone the repository:

    ```cmd
    git clone --recurse-submodules https://github.com/Azure/Real-time-Payment-Transaction-Processing-at-Scale.git
    ```

1. Set the proper folder permissions on the `scripts` folder:

    ```cmd
    cd ./Real-time-Payment-Transaction-Processing-at-Scale/
    chmod +x ./deploy/*
    ```

1. Execute the `CloudShell-Deploy.ps1` script. This will provision all of the required infrastructure, deploy the API and web app services into AKS, and provision and load artifacts into a Synapse Analytics workspace.

    ```pwsh
    ./deploy/powershell/CloudShell-Deploy.ps1 -resourceGroup <rg_name> -subscription <target_subscription_id>
    ```

    This is an example of the command above:

    ```pwsh
    ./deploy/powershell/CloudShell-Deploy.ps1 -resourceGroup "ms-claims-openai" -subscription "00000000-0000-0000-0000-000000000000"
    ```

>**NOTE**: The `<location>` specified must point to a region that supports the Azure OpenAI service. You can find the list of supported regions [here](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=cognitive-services).

>**NOTE**: If you already used the Cloud Shell in the target subscription, it is recommended to remove the `Real-time-Payment-Transaction-Processing-at-Scale` folder from the home directory before cloning the repository again. You can do this by running `rm ./Real-time-Payment-Transaction-Processing-at-Scale -r -f` in the Cloud Shell.
