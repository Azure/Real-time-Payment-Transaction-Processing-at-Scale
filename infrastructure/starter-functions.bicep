@description('Function app name')
param functionAppName string

@description('Storage account name')
param storageAccountName string

@description('Cosmos DB account name')
param cosmosAccountName string

@description('Event Hub namespace name')
param eventHubNamespaceName string

@description('Cosmos DB database name')
param paymentsDatabase string

@description('Cosmos DB transactions container name')
param transactionsContainer string

@description('Cosmos DB customer container name')
param customerContainer string

@description('Cosmos DB member container name')
param memberContainer string

@description('Cosmos DB preferred regions name')
param preferredRegions string

@description('Is master region')
param isMasterRegion bool

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
    name: 'B1'
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
      cors: {
        allowedOrigins: [
          '*'
        ]
      }
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
          value: 'dotnet-isolated'
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
          name: 'EventHubConnection__fullyQualifiedNamespace'
          value: '${eventHubNamespaceName}.servicebus.windows.net'
        }
        {
          name: 'AnalyticsEngine__OpenAIEndpoint'
          value: ''
        }
        {
          name: 'AnalyticsEngine__OpenAIKey'
          value: ''
        }
        {
          name: 'AnalyticsEngine__OpenAICompletionsDeployment'
          value: ''
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
          name: 'memberContainer'
          value: memberContainer
        }
        {
          name: 'preferredRegions'
          value: preferredRegions
        }
        {
          name: 'isMasterRegion'
          value: '${isMasterRegion}'
        }
      ]
    }
    httpsOnly: true
  }
}

// Grant Permissions to Identity for EventHub
resource eventHub 'Microsoft.EventHub/namespaces@2022-10-01-preview' existing = {
  name: eventHubNamespaceName
}

@description('This is the built-in "Azure Event Hubs Data Owner" role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-event-hubs-data-owner')
resource eventHubDataOwnerRole 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'f526a384-b230-433a-b45c-95f59c4a2dec'
}

resource eventHubRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventHub.id, 'DataOwner', functionApp.id, location)
  scope: eventHub
  properties: {
    roleDefinitionId: eventHubDataOwnerRole.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Permissions to Identity for CosmosDB
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

output functionName string = functionApp.name
