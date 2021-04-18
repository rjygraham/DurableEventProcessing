targetScope = 'resourceGroup'

@description('Short application name. Should be 4 characters or less.')
param name string

@description('Name of the environment (e.g. dev, test, prod, sndbx). Should be 5 charaters or less.')
param environment string

@description('Array of the api-silo service IP addresses created in each AKS cluster')
param ipAddresses array

var frontDoorName = toLower('${name}-${environment}-core-fd')

resource frontDoor 'Microsoft.Network/frontDoors@2020-05-01' = {
  name: frontDoorName
  location: 'global'
  properties: {
    routingRules: [
      {
        name: 'default'
        properties: {
          frontendEndpoints: [
            {
              id: resourceId('Microsoft.Network/frontDoors/frontendEndpoints', frontDoorName, 'default')
            }
          ]
          acceptedProtocols: [
            'Http'
            'Https'
          ]
          patternsToMatch: [
            '/*'
          ]
          routeConfiguration: {
            '@odata.type': '#Microsoft.Azure.FrontDoor.Models.FrontdoorForwardingConfiguration'
            forwardingProtocol: 'MatchRequest'
            backendPool: {
              id: resourceId('Microsoft.Network/frontDoors/backendPools', frontDoorName, 'default')
            }
          }
          enabledState: 'Enabled'
        }
      }
    ]
    healthProbeSettings: [
      {
        name: 'default'
        properties: {
          path: '/health'
          protocol: 'Http'
          intervalInSeconds: 120
        }
      }
    ]
    loadBalancingSettings: [
      {
        name: 'default'
        properties: {
          sampleSize: 4
          successfulSamplesRequired: 2
        }
      }
    ]
    backendPools: [
      {
        name: 'default'
        properties: {
          backends: [for ipAddress in ipAddresses: {
              address: ipAddress
              httpPort: 80
              httpsPort: 443
              weight: 50
              priority: 1
              enabledState: 'Enabled'
          }]
          loadBalancingSettings: {
            id: resourceId('Microsoft.Network/frontDoors/loadBalancingSettings', frontDoorName, 'default')
          }
          healthProbeSettings: {
            id: resourceId('Microsoft.Network/frontDoors/healthProbeSettings', frontDoorName, 'default')
          }
        }
      }
    ]
    frontendEndpoints: [
      {
        name: 'default'
        properties: {
          hostName: '${frontDoorName}.azurefd.net'
          sessionAffinityEnabledState: 'Disabled'
        }
      }
    ]
  }
}
