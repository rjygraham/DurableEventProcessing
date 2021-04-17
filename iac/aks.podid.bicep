targetScope = 'resourceGroup'

param podIdUserManagedIdentity object
param existingAks object


var podIdentityProperties = {
  podIdentityProfile: {
    enabled: true
  }
}

resource aksWithPodIdentity 'Microsoft.ContainerService/managedClusters@2021-02-01' = {
  name: existingAks.name
  location: existingAks.location
  identity: existingAks.identity
  properties: union(existingAks.properties, podIdentityProperties)
}
