targetScope = 'resourceGroup'

param roleName string
param roleId string
param existingAks object

resource aksManagedIdentityOperatorRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().name, existingAks.properties.identityProfile.kubeletidentity.objectId, roleName)
  properties: {
    principalId: existingAks.properties.identityProfile.kubeletidentity.objectId
    roleDefinitionId: '${subscription().id}/providers/Microsoft.Authorization/roleDefinitions/${roleId}'
  }
  scope: resourceGroup()
}
