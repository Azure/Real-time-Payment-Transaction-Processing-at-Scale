@description('OpenAI service name')
param openAiName string

@description('Resource location')
param location string = resourceGroup().location

@description('OpenAI deployments')
param deployments array = []

resource openAiAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAiName
  location: location
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openAiName
    publicNetworkAccess: 'Enabled'
  }
  sku: { name: 'S0' }
}

@batchSize(1)
resource openAiDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [for deployment in deployments: {
  parent: openAiAccount
  name: deployment.name
  properties: {
    model: {
      format: 'OpenAI'
      name: deployment.model
      version: deployment.version
    }
  }
  sku: contains(deployment, 'sku') ? deployment.sku : {
    name: 'Standard'
  }
}]

output endpoint string = openAiAccount.properties.endpoint
output id string = openAiAccount.id
output name string = openAiAccount.name
