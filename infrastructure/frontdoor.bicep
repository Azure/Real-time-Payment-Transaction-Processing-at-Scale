@description('Front Door profile name')
param frontDoorName string 

@description('Web FQDNs')
param fqdns array

@description('Enable Multi Master')
param enableMultiMaster bool

resource frontDoorProfile 'Microsoft.Cdn/profiles@2022-11-01-preview' = {
  name: frontDoorName
  location: 'global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
}

resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2022-11-01-preview' = {
  name: frontDoorName
  parent: frontDoorProfile
  location: 'global'
  properties: {
    enabledState: 'Enabled'
  }
}

resource frontDoorOriginGroup 'Microsoft.Cdn/profiles/originGroups@2022-11-01-preview' = {
  name: 'payment-functions'
  parent: frontDoorProfile
  properties: {
    loadBalancingSettings: {
      additionalLatencyInMilliseconds: 1000
      sampleSize: 4
      successfulSamplesRequired: 3
    }
    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 10
    }
  }
}

resource frontDoorOrigin 'Microsoft.Cdn/profiles/originGroups/origins@2022-11-01-preview' = [for (fqdn, i) in fqdns: {
  name: 'fdo-payments-${i}'
  parent: frontDoorOriginGroup
  properties: {
    hostName: fqdn
    httpPort: 80
    httpsPort: 443
    originHostHeader: fqdn
    priority: enableMultiMaster ? 1 : (i + 1)
    weight: 1000
  }
}]

resource frontDoorRoute 'Microsoft.Cdn/profiles/afdEndpoints/routes@2021-06-01' = {
  name: 'FunctionRoutes'
  parent: frontDoorEndpoint
  dependsOn: [
    frontDoorOrigin // This explicit dependency is required to ensure that the origin group is not empty when the route is created.
  ]
  properties: {
    originGroup: {
      id: frontDoorOriginGroup.id
    }
    supportedProtocols: [
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
  }
}
