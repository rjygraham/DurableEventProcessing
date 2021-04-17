targetScope = 'resourceGroup'

param location string
param acrName string
param aksPrincipalId string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2020-11-01-preview' existing = {
  name: acrName
}

resource acrRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(acrName, aksPrincipalId, 'Contributor')
  properties: {
    principalId: aksPrincipalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleAssignments', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
  }
  scope: containerRegistry
}
