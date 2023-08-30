@description('Cosmos DB account name, max length 44 characters, lowercase')
param cosmosAccountName string = 'cosmospay${suffix}'

@description('EventHub namespace, max length 44 characters, lowercase')
param eventHubNamespace string = 'eventhubpay${suffix}'

@description('Function App name')
param functionAppName string = 'functionpay${suffix}'

@description('Storage account name, max length 24 characters, lowercase')
param storageAccountName string = 'blobpay${suffix}'

@description('Front Door name')
param frontDoorName string = 'apipaymentsfd${suffix}'

@description('Enable Cosmos Multi Master')
param enableCosmosMultiMaster bool = false

@description('Locations for resource deployment')
param locations string = 'SouthCentral US, NorthCentral US, East US'

@description('Suffix for resource deployment')
param suffix string = uniqueString(resourceGroup().id)

@description('Static website storage account name, max length 24 characters, lowercase')
param websiteStorageAccountName string = 'webpaysa${suffix}'

@description('OpenAI service name')
param openAiName string = 'openaipayments${suffix}'

@description('API Managed Identity name')
param apiMiName string = 'miapi${suffix}'

@description('Worker Managed Identity name')
param workerMiName string = 'miworker${suffix}'

@description('App Insights name')
param aiName string = 'ai${suffix}'

@description('AKS name')
param aksName string = 'aks${suffix}'

var locArray = split(toLower(replace(locations, ' ', '')), ',')
var regionNames = {
  eastus: 'East US'
  northcentralus: 'North Central US'
  southcentralus: 'South Central US'
}

module cosmosdb 'cosmos.bicep' = {
  scope: resourceGroup()
  name: 'cosmosDeploy'
  params: {
    accountName: cosmosAccountName
    locations: locArray
    enableCosmosMultiMaster: enableCosmosMultiMaster
    apiPrincipalId: apiIdentity.properties.principalId
    workerPrincipalId: workerIdentity.properties.principalId
  }
}

module openAi 'openai.bicep' = {
  name: 'openAiDeploy'
  params: {
    openAiName: openAiName
    location: locArray[2]
    deployments: [
      {
        name: 'completions'
        model: 'gpt-35-turbo'
        version: '0301'
        sku: {
          name: 'Standard'
          capacity: 60
        }
      }
    ]
  }
}

// module blob 'blob.bicep' = [for (location, i) in locArray: {
//   name: 'blobDeploy-${location}'
//   params: {
//     storageAccountName: '${storageAccountName}${i}'
//     location: location
//     apiPrincipalId: apiIdentity.properties.principalId
//     workerPrincipalId: workerIdentity.properties.principalId
//   }
// }]

// @batchSize(1)
// module function 'functions.bicep' = [for (location, i) in locArray: {
//   name: 'functionDeploy-${location}'
//   params: {
//     cosmosAccountName: cosmosdb.outputs.cosmosAccountName
//     eventHubNamespaceName: eventHubNamespace
//     functionAppName: '${functionAppName}${i}'
//     storageAccountName: '${storageAccountName}${i}'
//     paymentsDatabase: cosmosdb.outputs.cosmosDatabaseName
//     transactionsContainer: cosmosdb.outputs.cosmosTransactionsContainerName
//     customerContainer: cosmosdb.outputs.cosmosCustomerContainerName
//     memberContainer: cosmosdb.outputs.cosmosMemberContainerName
//     preferredRegions: join(concat(array(regionNames[location]), map(filter(locArray, l => l != location), l => regionNames[l])), ',')
//     isMasterRegion: i == 0 || enableCosmosMultiMaster ? true : false
//     location: location
//     openAiName: openAiName
//     openAiDeployment: openAiDeployment
//     openAiResourceGroup: openAiResourceGroup
//   }
//   dependsOn: [
//     blob
//   ]
// }]

@batchSize(1)
module aks 'AKS-Construction/bicep/main.bicep' = [for (location, i) in locArray: {
  name: 'aksconstruction${i}'
  params: {
    location : location
    resourceName: '${aksName}${i}'
    enable_aad: true
    enableAzureRBAC : true
    registries_sku: 'Basic'
    omsagent: true
    retentionInDays: 30
    agentCount: 1

    //Managed workload identity 
    workloadIdentity: true

    //Workload Identity requires OidcIssuer to be configured on AKS
    oidcIssuer: true

    //We'll also enable the CSI driver for Key Vault
    keyVaultAksCSI : true

    JustUseSystemPool: true

    httpApplicationRouting: true
  }
  dependsOn: [cosmosdb, openAi]
}]

@batchSize(1)
resource ai 'Microsoft.Insights/components@2020-02-02' = [for (location, i) in locArray: {
  name: '${aiName}${i}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    RetentionInDays: 30
    WorkspaceResourceId: aks[i].outputs.LogAnalyticsId
  }
}]

resource apiIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: apiMiName
  location: locArray[0]

  resource fedCreds 'federatedIdentityCredentials' = [for (location, i) in locArray: {
    name: '${apiMiName}-fed${i}'
    properties: {
      audiences: aks[i].outputs.aksOidcFedIdentityProperties.audiences
      issuer: aks[i].outputs.aksOidcFedIdentityProperties.issuer
      subject: 'system:serviceaccount:default:payments-api-sa'
    }
  }]
}

resource workerIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: workerMiName
  location: locArray[0]

  resource fedCreds 'federatedIdentityCredentials' = [for (location,i) in locArray: {
    name: '${workerMiName}-fed${i}'
    properties: {
      audiences: aks[i].outputs.aksOidcFedIdentityProperties.audiences
      issuer: aks[i].outputs.aksOidcFedIdentityProperties.issuer
      subject: 'system:serviceaccount:default:payments-worker-sa'
    }
  }]
}

module frontdoor 'frontdoor.bicep' = {
  name: 'frontdoorDeploy'
  params: {
    enableMultiMaster: enableCosmosMultiMaster
    frontDoorName: frontDoorName
    aksNames: [for i in range(0, length(locArray)): aks[i].outputs.aksClusterName]
  }
}

module staticwebsite 'staticwebsite.bicep' = {
  name: 'staticwebsiteDeploy'
  params: {
    storageAccountName: websiteStorageAccountName
    location: locArray[0]
  }
}
