@description('Cosmos DB account name, max length 44 characters, lowercase')
param cosmosAccountName string = 'cosmospay${suffix}'

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

@description('OpenAi Deployment')
param openAiDeployment string = 'completions'

@description('OpenAI Resource Group')
param openAiRg string = resourceGroup().name

@description('API Managed Identity name')
param apiMiName string = 'miapi${suffix}'

@description('Worker Managed Identity name')
param workerMiName string = 'miworker${suffix}'

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

module logAnalytics 'loganalytics.bicep' = {
  name: 'logAnalyticsDeploy'
  params: {
    name: 'payments-${suffix}'
    location: locArray[0]
  }
}

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: openAiName
  scope: resourceGroup(openAiRg)
}

module containerApps 'containerapp.bicep' = [ for (location, i) in locArray: {
  name: 'conatinerApps${i}'
  params: {
    aiConnectionString: logAnalytics.outputs.aiConnectionString
    cosmosEndpoint: cosmosdb.outputs.cosmosAccountEndpoint
    laCustomerId: logAnalytics.outputs.laCustomerId
    laSharedKey: logAnalytics.outputs.laSharedKey
    location: location
    name: 'payments-${suffix}${i}'
    openAiCompletionsDeployment: openAiDeployment
    openAiEndpoint: openAi.properties.endpoint
    openAiKey: openAi.listKeys().key1
    suffix: '${suffix}${i}'
    workerClientId: workerIdentity.properties.clientId
    apiClientId: apiIdentity.properties.clientId
    apiMiId: apiIdentity.id
    workerMiId: workerIdentity.id
    isMasterRegion: i == 0 || enableCosmosMultiMaster ? true : false
    preferredLocations: join(concat(array(regionNames[location]), map(filter(locArray, l => l != location), l => regionNames[l])), ',')
  }
}]

resource apiIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: apiMiName
  location: locArray[0]
}

resource workerIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: workerMiName
  location: locArray[0]
}

module frontdoor 'frontdoor.bicep' = {
  name: 'frontdoorDeploy'
  params: {
    enableMultiMaster: enableCosmosMultiMaster
    frontDoorName: frontDoorName
    fqdns: [for i in range(0, length(locArray)): containerApps[i].outputs.apiFqdn]
  }
}

module staticwebsite 'staticwebsite.bicep' = {
  name: 'staticwebsiteDeploy'
  params: {
    storageAccountName: websiteStorageAccountName
    location: locArray[0]
  }
}
