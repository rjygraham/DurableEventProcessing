targetScope = 'resourceGroup'

param coreName string
param regions array
param primaryRegionIndex int

var sqlDatabaseName = 'sensors'
var grainsContainerName = 'grains'
var location = regions[primaryRegionIndex]
var frontDoorName = '${coreName}-fd'

resource comsosdb 'Microsoft.DocumentDB/databaseAccounts@2021-03-01-preview' = {
  name: '${coreName}-cosmosdb'
  location: location
  properties: {
    createMode: 'Default'
    databaseAccountOfferType: 'Standard'
    enableMultipleWriteLocations: true
    locations: [for (region, i) in regions: {
      locationName: region
      failoverPriority: i
      isZoneRedundant: true
    }]
  }

  resource sqlDatabase 'sqlDatabases' = {
    name: sqlDatabaseName
    properties: {
      resource: {
        id: sqlDatabaseName
      }
      options: {
        autoscaleSettings: {
          maxThroughput: 4000
        }
      }
    }

    resource grainsContainer 'containers' = {
      name: grainsContainerName
      properties: {
        resource: {
          id: grainsContainerName
          partitionKey: {
            paths: [
              '/pk'
            ]
            kind: 'Hash'
          }
          defaultTtl: 180
        }
      }
    }
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2020-11-01-preview' = {
  name: toLower(replace('${coreName}acr', '-', ''))
  location: location
  sku: {
    name: 'Premium'
  }
  properties: {
  }

  resource replications 'replications' = [for region in skip(regions, 1): {
    name: region
    dependsOn: [
      containerRegistry
    ]
    location: region
    properties: {
      regionEndpointEnabled: true
    }
  }]
}

output comsosDbName string = comsosdb.name
output containerRegistryName string = containerRegistry.name
