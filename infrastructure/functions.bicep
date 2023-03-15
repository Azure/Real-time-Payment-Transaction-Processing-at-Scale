@description('Function app name')
param functionAppName string

@description('Storage account name')
param storageAccountName string

@description('Cosmos DB account name')
param cosmosAccountName string

@description('Cosmos DB database name')
param paymentsDatabase string

@description('Cosmos DB transactions container name')
param transactionsContainer string

@description('Cosmos DB customer container name')
param customerContainer string

@description('Cosmos DB preferred regions name')
param preferredRegions string

@description('Traffic manager name, lowercase')
param trafficManagerName string

@description('Traffic manager endpoint priority')
param endPointPriority int

@description('Resource location')
param location string = resourceGroup().location

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: functionAppName
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: functionAppName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource plan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${functionAppName}Plan'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
  }
}

resource blob 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      use32BitWorkerProcess: false
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${blob.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${blob.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${blob.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${blob.listKeys().keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'CosmosDBConnection__accountEndpoint'
          value: 'https://${cosmosAccountName}.documents.azure.com:443/'
        }
        {
          name: 'paymentsDatabase'
          value: paymentsDatabase
        }
        {
          name: 'transactionsContainer'
          value: transactionsContainer
        }
        {
          name: 'customerContainer'
          value: customerContainer
        }
        {
          name: 'preferredRegions'
          value: preferredRegions
        }
      ]
    }
    httpsOnly: true
  }
}

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: cosmosAccountName
}

resource roleAssignmentCosmos 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2022-08-15' = {
  name: guid(cosmos.id, 'CosmosContributor', functionApp.id)
  parent: cosmos
  properties: {
    scope: cosmos.id
    roleDefinitionId: resourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', cosmos.name, '00000000-0000-0000-0000-000000000002') //Cosmos DB Built-in Data Contributor
    principalId: functionApp.identity.principalId
  }
}

resource trafficManager 'Microsoft.Network/trafficmanagerprofiles@2022-04-01-preview' existing = {
  name: trafficManagerName
}

resource tmEndpoint 'Microsoft.Network/trafficmanagerprofiles/AzureEndpoints@2022-04-01-preview' = {
  name: functionAppName
  parent: trafficManager
  properties: {
    priority: endPointPriority
    targetResourceId: functionApp.id
    endpointStatus: 'Enabled'
  }
}
