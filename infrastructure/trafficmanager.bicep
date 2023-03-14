@description('Traffic Manager profile name')
param trafficManagerName string 

@description('Flag to enable geographic routing otherwise priority')
param enableGeographicRouting bool

resource traffic 'Microsoft.Network/trafficmanagerprofiles@2022-04-01-preview' = {
  name: trafficManagerName
  location: 'global'
  properties: {
    profileStatus: 'Enabled'
    trafficRoutingMethod: enableGeographicRouting ? 'Geographic' : 'Priority'
    dnsConfig: {
      relativeName: trafficManagerName
      ttl: 30
    }
    monitorConfig: {
      protocol: 'HTTPS'
      port: 443
      path: '/'
    }
  }
}
