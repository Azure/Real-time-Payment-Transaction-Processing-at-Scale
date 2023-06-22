@description('Cosmos DB account name, max length 44 characters, lowercase')
param cosmosAccountName string = 'cosmospay${suffix}'

@description('Function App name')
param functionAppName string = 'functionpay${suffix}'

@description('Storage account name, max length 24 characters, lowercase')
param storageAccountName string = 'blobpay${suffix}'

@description('Front Door name')
param frontDoorName string = 'apipaymentsfd${suffix}'

@description('Enable Cosmos Multi Master')
param enableCosmosMultiMaster bool = false

@description('Locations for resource deployment')
param locations string = 'SouthCentralUS, NorthCentralUS'

@description('Suffix for resource deployment')
param suffix string = uniqueString(resourceGroup().id)

var locArray = split(replace(locations, ' ', ''), ',')

module cosmosdb 'cosmos.bicep' = {
  scope: resourceGroup()
  name: 'cosmosDeploy'
  params: {
    accountName: cosmosAccountName
    locations: locArray
    enableCosmosMultiMaster: enableCosmosMultiMaster
  }
}

module blob 'blob.bicep' = [for (location, i) in locArray: {
  name: 'blobDeploy-${location}'
  params: {
    storageAccountName: '${storageAccountName}${i}'
    location: location
  }
}]

@batchSize(1)
module function 'functions.bicep' = [for (location, i) in locArray: {
  name: 'functionDeploy-${location}'
  params: {
    cosmosAccountName: cosmosdb.outputs.cosmosAccountName
    functionAppName: '${functionAppName}${i}'
    storageAccountName: '${storageAccountName}${i}'
    paymentsDatabase: cosmosdb.outputs.cosmosDatabaseName
    transactionsContainer: cosmosdb.outputs.cosmosTransactionsContainerName
    customerContainer: cosmosdb.outputs.cosmosCustomerContainerName
    memberContainer: cosmosdb.outputs.cosmosMemberContainerName
    preferredRegions: join(concat(array(location), filter(locArray, l => l != location)), ',')
    isMasterRegion: i == 0 || enableCosmosMultiMaster ? true : false
    location: location
  }
  dependsOn: [
    blob
  ]
}]

module frontdoor 'frontdoor.bicep' = {
  name: 'frontdoorDeploy'
  params: {
    enableMultiMaster: enableCosmosMultiMaster
    frontDoorName: frontDoorName
    functionNames: [for i in range(0, length(locArray)): function[i].outputs.functionName]
  }
}
