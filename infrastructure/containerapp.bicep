@description('Common name of resources')
param name string

@description('Location of resource')
param location string

@description('Resource suffix')
param suffix string

@description('CosmosDB endpoint')
param cosmosEndpoint string

@description('OpenAI Endpoint')
param openAiEndpoint string

@description('OpenAI Key')
param openAiKey string

@description('OpenAI Completions deployment')
param openAiCompletionsDeployment string

@description('App Insights connection string')
param aiConnectionString string

@description('Log Analytics Customer ID')
param laCustomerId string

@description('Log Analytics shared key')
param laSharedKey string

@description('API MI Client ID')
param apiClientId string

@description('API MI Resource ID')
param apiMiId string

@description('Worker MI Client ID')
param workerClientId string

@description('Worker MK Resource ID')
param workerMiId string

@description('Is Master Region')
param isMasterRegion bool = true

@description('Preferred Locations')
param preferredLocations string

var containerAppConfigs = [
  {
    name: 'api'
    identity: {
      type: 'UserAssigned'
      userAssignedIdentities: { '${apiMiId}': {} }
    }
    ingress: {
      allowInsecure: false
      clientCertificateMode: 'Ignore'
      exposedPort: 0
      external: true
      stickySessions: {
        affinity: 'sticky'
      }
      targetPort: 80
      traffic: [
        {
          latestRevision: true
          weight: 100
        }
      ]
      transport: 'Auto'
    }
    env: [
      {
        name: 'CosmosDBConnection__accountEndpoint'
        value: cosmosEndpoint
      }
      {
        name: 'DatabaseSettings__CustomerContainer'
        value: 'customerTransactions'
      }
      {
        name: 'DatabaseSettings__GlobalIndexContainer'
        value: 'globalIndex'
      }
      {
        name: 'DatabaseSettings__IsMasterRegion'
        value: string(isMasterRegion)
      }
      {
        name: 'DatabaseSettings__MemberContainer'
        value: 'members'
      }
      {
        name: 'DatabaseSettings__PaymentsDatabase'
        value: 'payments'
      }
      {
        name: 'DatabaseSettings__PreferredRegions'
        value: preferredLocations
      }
      {
        name: 'DatabaseSettings__TransactionsContainer'
        value: 'transactions'
      }
      {
        name: 'AnalyticsEngine__OpenAIEndpoint'
        value: openAiEndpoint
      }
      {
        name: 'AnalyticsEngine__OpenAIKey'
        value: openAiKey
      }
      {
        name: 'AnalyticsEngine__OpenAICompletionsDeployment'
        value: openAiCompletionsDeployment
      }
      {
        name: 'ApplicationInsights__ConnectionString'
        value: aiConnectionString
      }
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Production'
      }
      {
        name: 'ClientId'
        value: apiClientId
      }
    ]
  }
  {
    name: 'worker'
    identity: {
      type: 'UserAssigned'
      userAssignedIdentities: { '${workerMiId}': {} }
    }
    ingress: null
    env: [
      {
        name: 'CosmosDBConnection__accountEndpoint'
        value: cosmosEndpoint
      }
      {
        name: 'ClientId'
        value: workerClientId
      }
      {
        name: 'ApplicationInsights__ConnectionString'
        value: aiConnectionString
      }
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Production'
      }
    ]
  }
]

resource vnet 'Microsoft.Network/virtualNetworks@2022-09-01' = {
  name: 'vnet-${name}'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.244.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'aca-subnet'
        properties: {
          addressPrefix: '10.244.0.0/16'
          delegations: [
            {
              name: 'Microsoft.App.environments'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
              type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
            }
          ]
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  location: location
#disable-next-line BCP334
  name: '${replace(name, '-', '')}cr'
  sku: {
    name: 'Standard'
  }
  properties: {
    anonymousPullEnabled: true
  }
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'acaenv-${name}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: laCustomerId
        sharedKey: laSharedKey
      }
    }
    infrastructureResourceGroup: 'ME_${resourceGroup().name}${suffix}'
    vnetConfiguration: {
      infrastructureSubnetId: vnet.properties.subnets[0].id
      internal: false
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
      {
        maximumCount: 10
        minimumCount: 2
        name: 'Warm'
        workloadProfileType: 'E4'
      }
    ]
    zoneRedundant: false
  }
}

resource containerApps 'Microsoft.App/containerApps@2023-05-01' = [for (config, i) in containerAppConfigs: {
  name: 'aca-${config.name}-${name}'
  location: location
  identity: config.identity
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: config.ingress
      secrets: []
    }
    environmentId: containerAppEnvironment.id
    managedEnvironmentId: containerAppEnvironment.id
    template: {
      containers: [
        {
          env: config.env
          image: 'mcr.microsoft.com/k8se/quickstart:latest'
          name: 'aca${name}${config.name}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        maxReplicas: 10
        minReplicas: 2
      }
      volumes: []
    }
    workloadProfileName: 'Warm'
  }
}]

output apiFqdn string = containerApps[0].properties.configuration.ingress.fqdn
