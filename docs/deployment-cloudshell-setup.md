# Prepare Cloud Shell Setup

Before users in your team can deploy the solution using Cloud Shell, you need to perform the following steps:

1. Create three Azure Container Registry (ACR) instances in the target subscription, one in each of the following regions: `South Central US`, `North Central US`, and `East US`. Ensure anonymous pull access is enabled on the ACR instance (see [here](https://learn.microsoft.com/en-us/azure/container-registry/anonymous-pull-access) for more information).

2. Clone the repository:

    ```cmd
    git clone --recurse-submodules https://github.com/AzureCosmosDB/RealTimeTransactions.git
    ```

3. Open the `CloudShell-Deploy.ps1` script from the `deploy\powershell` folder with the text editor of your choice. In line 4, update the default values for the parameter `acrResourceGroup` with the value corresponding to the ACR instances created in step 1.

4. Save the changes to the `CloudShell-Deploy.ps1` script, commit them to the `main` branch, and push the changes to the remote repository.

    ```cmd
    git commit -m "Updated ACR details for Cloud Shell deployment"
    git push
    ```

5. Execute the `Prepare-CloudShell-Deploy.ps1` script. This will build the portal and API Docker images and push them to the ACR instance created in step 1.

    ```pwsh
    ./deploy/powershell/Prepare-CloudShell-Deploy.ps1 -resourceGroup <rg_name> -subscription <target_subscription_id>
    ```

    `<rg_name>` is the name of the resource group where the ACR instance was created in step 1.

    `<target_subscription_id>` is the ID of the target subscription.

    This is an example of the command above:

    ```pwsh
    ./deploy/powershell/Prepare-CloudShell-Deploy.ps1 -resourceGroup "ms-byd-to-chatgpt" -subscription "00000000-0000-0000-0000-000000000000"
    ```

>**NOTE**:
>Make sure you pull the latest changes from the `main` branch and rerun step 5 above each time you want to update the portal and API Docker images in the ACR instance as a result of changes made to the code.
