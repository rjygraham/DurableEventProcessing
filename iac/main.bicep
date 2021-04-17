targetScope = 'subscription'

@allowed([
  'brazilsouth'
  'canadacentral'
  'centralus'
  'eastus'
  'eastus2'
  'francecentral'
  'japaneast'
  'northeurope'
  'southcentralus'
  'southeastasia'
  'uksouth'
  'westeurope'
  'westus2'
])
@description('Regions to which the the solution will be deployed.')
param regions array

@description('Zero based index of the region from list of regions with will serve as the primary metadata region.')
param primaryRegionIndex int

@description('Short application name. Should be 4 characters or less.')
param name string

@description('Name of the environment (e.g. dev, test, prod, sndbx). Should be 5 charaters or less.')
param environment string

@allowed([
  'Basic'
  'Standard'
])
param eventHubSku string

@minValue(1)
@maxValue(7)
param evHubMessageRetentionInDays int
param evHubPartitionCount int
param aksKubernetesVersion string
param aksEnableRbac bool
param aksNetworkPlugin string
param aksEnablePrivateCluster bool
param aksEnableHttpApplicationRouting bool
param aksEnableOmsAgent bool
param aksEnableAzurePolicy bool
param aksEnableAutoScaling bool
param logAnalyticsResourceId string

var environmentName = '${name}-${environment}'
var sharedName = '${environmentName}-core'
var regionNameMap = {
  'brazilsouth': 'bzs'
  'canadacentral': 'cac'
  'centralus': 'cus'
  'eastus': 'eus'
  'eastus2': 'eus2'
  'francecentral': 'frc'
  'japaneast': 'jpe'
  'northeurope': 'neu'
  'southcentralus': 'scus'
  'southeastasia': 'sea'
  'uksouth': 'uks'
  'westeurope': 'weu'
  'westus2': 'wus2'
}

resource coreResourceGroup 'Microsoft.Resources/resourceGroups@2020-10-01' = {
  name: toUpper('${environmentName}-CORE')
  location: regions[primaryRegionIndex]
}

module coreDeployment 'core.bicep' = {
  name: 'core.deployment'
  scope: coreResourceGroup
  params: {
    coreName: toLower('${environmentName}-core')
    regions: regions
    primaryRegionIndex: primaryRegionIndex
  }
}

resource regionResourceGroups 'Microsoft.Resources/resourceGroups@2020-10-01' = [for region in regions: {
  name: toUpper('${environmentName}-${regionNameMap[region]}')
  location: regions[primaryRegionIndex]
}]

module regionDeployments 'region.bicep' = [for (region, i) in regions: {
  name: 'region.${region}.deployment'
  scope: regionResourceGroups[i]
  params: {
    location: region
    coreResourceGroupName: coreResourceGroup.name
    nodeResourceGroupName: '${regionResourceGroups[i].name}-NODES'
    regionalName: '${environmentName}-${regionNameMap[region]}'
    eventHubSku: eventHubSku
    evHubMessageRetentionInDays: evHubMessageRetentionInDays
    evHubPartitionCount: evHubPartitionCount
    aksKubernetesVersion: aksKubernetesVersion
    aksEnableRbac: aksEnableRbac
    aksNetworkPlugin: aksNetworkPlugin
    aksEnablePrivateCluster: aksEnablePrivateCluster
    aksEnableHttpApplicationRouting: aksEnableHttpApplicationRouting
    aksEnableOmsAgent: aksEnableOmsAgent
    aksEnableAzurePolicy: aksEnableAzurePolicy
    aksEnableAutoScaling: aksEnableAutoScaling
    logAnalyticsResourceId: logAnalyticsResourceId
    cosmosDbAccountName: coreDeployment.outputs.comsosDbName
  }
}]

module aksAcrRoleAssingmentDeployments 'aks.acr.role.bicep' = [for (region, i) in regions: {
  name: '${region}.aks.acr.contrib.role.deployment'
  scope: coreResourceGroup
  params: {
    location: region
    acrName: coreDeployment.outputs.containerRegistryName
    aksPrincipalId: regionDeployments[i].outputs.aks.properties.identityProfile.kubeletidentity.objectId
  }
}]

module aksManagedIdentityOperatorKeyVaultDeployments 'aks.podid.rg.roleassignment.bicep' = [for (region, i) in regions: {
  name: '${region}.aks.kvlt.mio.role.deployment'
  scope: regionResourceGroups[i]
  params: {
    roleName: 'Managed Identity Operator'
    roleId: 'f1a07417-d97a-45cb-824c-7a7467783830'
    existingAks: regionDeployments[i].outputs.aks
  }
}]

module aksManagedIdentityOperatorNodePoolDeployments 'aks.podid.rg.roleassignment.bicep' = [for (region, i) in regions: {
  name: '${region}.aks.nodes.mio.role.deployment'
  scope: resourceGroup('${regionResourceGroups[i].name}-NODES')
  params: {
    roleName: 'Managed Identity Operator'
    roleId: 'f1a07417-d97a-45cb-824c-7a7467783830'
    existingAks: regionDeployments[i].outputs.aks
  }
}]

module aksVirtualMachineContributorNodePoolDeployments 'aks.podid.rg.roleassignment.bicep' = [for (region, i) in regions: {
  name: '${region}.aks.nodes.vmc.role.deployment'
  scope: resourceGroup('${regionResourceGroups[i].name}-NODES')
  params: {
    roleName: 'Virtual Machine Contributor'
    roleId: '9980e02c-c2be-4d73-94e8-173b1dc7cf3c'
    existingAks: regionDeployments[i].outputs.aks
  }
}]

module aksPodIdentityDeployment 'aks.podid.bicep' = [for (region, i) in regions: {
  name: '${region}.aks.podidentity.deployment'
  dependsOn: [
    aksManagedIdentityOperatorKeyVaultDeployments
    aksManagedIdentityOperatorNodePoolDeployments
    aksVirtualMachineContributorNodePoolDeployments
  ]
  scope: regionResourceGroups[i]
  params: {
    podIdUserManagedIdentity: regionDeployments[i].outputs.podIdUserManagedIdentity
    existingAks: regionDeployments[i].outputs.aks
  }
}]

output aksPodIdentityCommands array = [for i in range(0, length(regions)): 'az aks pod-identity add -g ${regionResourceGroups[i].name} --cluster-name ${regionDeployments[i].outputs.aks.name} --namespace default  -n ${regionDeployments[i].outputs.aks.name}-podid --identity-resource-id ${regionDeployments[i].outputs.podIdUserManagedIdentity.resourceId}']
output aksCredentialCommands array = [for i in range(0, length(regions)): 'az aks get-credentials -g ${regionResourceGroups[i].name} -n ${regionDeployments[i].outputs.aks.name}']
