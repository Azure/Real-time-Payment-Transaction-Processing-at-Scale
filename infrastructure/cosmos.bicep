@description('Cosmos DB account name, max length 44 characters, lowercase')
param accountName string

@description('Locations for the Cosmos DB account.')
param locations array

@description('Enable Cosmos Multi Master')
param enableCosmosMultiMaster bool = false

var databaseName = 'payments'

var containers = [
  {
    name: 'transactions'
    partitionKeys: ['/accountId']
    enableIndex: true
    enableAnalyticalStore: true
  }
  {
    name: 'customerTransactions'
    partitionKeys: ['/accountId']
    enableIndex: true
    enableAnalyticalStore: true
  }
  {
    name: 'leases'
    partitionKeys: ['/id' ]
    enableIndex: true
    enableAnalyticalStore: false
  }
]

@description('Maximum autoscale throughput for the container')
@minValue(1000)
@maxValue(1000000)
param autoscaleMaxThroughput int = 1000

resource account 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: toLower(accountName)
  kind: 'GlobalDocumentDB'
  location: locations[0]
  properties: {
    locations: [for (l, i) in locations: {
      locationName: l
      failoverPriority: i
      isZoneRedundant: false
    }]
    enableMultipleWriteLocations: enableCosmosMultiMaster
    enableAutomaticFailover: !enableCosmosMultiMaster
    consistencyPolicy: {
      defaultConsistencyLevel: enableCosmosMultiMaster ? 'BoundedStaleness' : 'Strong'
    }
    databaseAccountOfferType: 'Standard'
    enableAnalyticalStorage: true
    analyticalStorageConfiguration: {
      schemaType: 'FullFidelity'
    }
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  parent: account
  name: databaseName
  properties: {
    options: {
      autoscaleSettings: {
        maxThroughput: autoscaleMaxThroughput
      }
    }
    resource: {
      id: databaseName
    }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = [for (config, i) in containers: {
  parent: database
  name: config.name
  properties: {
    resource: {
      id: config.name
#disable-next-line BCP040
      '${config.enableAnalyticalStore ? 'analyticalStorageTtl' : any(null)}': any(config.enableAnalyticalStore ? -1 : null)
      partitionKey: {
        paths: [for pk in config.partitionKeys: pk]
        kind: length(config.partitionKeys) == 1 ? 'Hash' : 'MultiHash'
        version: length(config.partitionKeys) == 1 ? 1 : 2
      }
      indexingPolicy: {
        automatic: config.enableIndex
        indexingMode: config.enableIndex ? 'consistent' : 'none'
      }
    }
  }
}]

output cosmosAccountName string = account.name
output cosmosDatabaseName string = database.name
output cosmosTransactionsContainerName string = container[0].name
output cosmosCustomerContainerName string = container[1].name
