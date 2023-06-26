@description('Event Hub namespace name')
param eventHubNamespace string

@description('Resource location')
param location string = resourceGroup().location

var eventHubs = ['PaymentEvents']

resource namespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' = {
  name: eventHubNamespace
  location: location
  sku: {
    name: 'Standard'
    capacity: 1
    tier: 'Standard'
  }
}

resource hubs 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = [for eh in eventHubs: {
  name: eh
  parent: namespace
  properties: {
    partitionCount: 32
    messageRetentionInDays: 1
    status: 'Active'
  }
}]
