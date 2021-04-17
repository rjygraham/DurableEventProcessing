targetScope = 'resourceGroup'

param location string
param coreResourceGroupName string
param nodeResourceGroupName string
param regionalName string
param eventHubSku string
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
param cosmosDbAccountName string

var eventHubName = 'telemetry'
var apiConsumerGroupName = 'api-silo'
var processorConsumerGroupName = 'processor-silo'
var replicatorConsumerGroupName = 'replicator'

resource podIdUserManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${aks.name}-pod-umid'
  location: location
}

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: '${regionalName}-ai'
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'other'
    WorkspaceResourceId: logAnalyticsResourceId
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2021-03-01-preview' existing = {
  name: cosmosDbAccountName
  scope: resourceGroup(coreResourceGroupName)
}

resource aks 'Microsoft.ContainerService/managedClusters@2021-02-01' = {
  name: '${regionalName}-aks'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    kubernetesVersion: aksKubernetesVersion
    enableRBAC: aksEnableRbac
    dnsPrefix: '${regionalName}-aks-dns'
    nodeResourceGroup: nodeResourceGroupName
    agentPoolProfiles: [
      {
        name: 'system'
        osDiskSizeGB: 0
        count: 1
        vmSize: 'Standard_DS2_v2'
        osType: 'Linux'
        osDiskType: 'Managed'
        type: 'VirtualMachineScaleSets'
        mode: 'System'
        maxPods: 110
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
      }
      {
        name: 'api'
        osDiskSizeGB: 0
        count: 1
        vmSize: 'Standard_D2s_v3'
        osType: 'Linux'
        osDiskType: 'Managed'
        type: 'VirtualMachineScaleSets'
        mode: 'User'
        maxPods: 110
        minCount: 1
        maxCount: 5
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        enableAutoScaling: aksEnableAutoScaling
        nodeLabels: {
          'workload': 'api'
        }
      }
      {
        name: 'processor'
        osDiskSizeGB: 0
        count: 1
        vmSize: 'Standard_D2s_v3'
        osType: 'Linux'
        osDiskType: 'Managed'
        type: 'VirtualMachineScaleSets'
        mode: 'User'
        maxPods: 110
        minCount: 1
        maxCount: 5
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        enableAutoScaling: aksEnableAutoScaling
        nodeLabels: {
          'workload': 'processor'
        }
      }
      {
        name: 'replicator'
        osDiskSizeGB: 0
        count: 0
        vmSize: 'Standard_D2s_v3'
        osType: 'Linux'
        osDiskType: 'Managed'
        type: 'VirtualMachineScaleSets'
        mode: 'User'
        maxPods: 110
        minCount: 0
        maxCount: 5
        availabilityZones: [
          '1'
          '2'
          '3'
        ]
        enableAutoScaling: aksEnableAutoScaling
        nodeLabels: {
          'workload': 'replicator'
        }
      }
    ]
    networkProfile: {
      loadBalancerSku: 'standard'
      networkPlugin: aksNetworkPlugin
    }
    apiServerAccessProfile: {
      enablePrivateCluster: aksEnablePrivateCluster
    }
    addonProfiles: {
      httpApplicationRouting: {
        enabled: aksEnableHttpApplicationRouting
      }
      azurePolicy: {
        enabled: aksEnableAzurePolicy
      }
      omsAgent: {
        enabled: aksEnableOmsAgent
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticsResourceId
        }
      }
    }
  }
}

resource orleansStorageAccount 'Microsoft.Storage/storageAccounts@2021-01-01' = {
  name: toLower(replace('${regionalName}orlnstg', '-', ''))
  location: location
  dependsOn: [
    aks
  ]
  kind: 'StorageV2'
  sku: {
    tier: 'Standard'
    name: 'Standard_ZRS'
  }
  properties: {}

  resource orleansTableService 'tableServices' = {
    name: 'default'

    resource orleansClustersTable 'tables' = {
      name: 'clusters'
    }
  }
}

resource replicatorStorageAccount 'Microsoft.Storage/storageAccounts@2021-01-01' = {
  name: toLower(replace('${regionalName}repstg', '-', ''))
  location: location
  dependsOn: [
    orleansStorageAccount
  ]
  kind: 'BlockBlobStorage'
  sku: {
    tier: 'Premium'
    name: 'Premium_ZRS'
  }
  properties: {}

  resource replicatorBlobService 'blobServices' = {
    name: 'default'

    resource replicatorCheckpointsContainer 'containers' = {
      name: 'checkpoints'
      properties: {
        publicAccess: 'None'
      }
    }
  }
}

resource evHubNamespace 'Microsoft.EventHub/namespaces@2018-01-01-preview' = {
  name: '${regionalName}-evhub'
  dependsOn: [
    replicatorStorageAccount
  ]
  location: location
  sku: {
    name: eventHubSku
    tier: eventHubSku
    capacity: 1
  }
  properties: {
    isAutoInflateEnabled: true
    maximumThroughputUnits: 20
    zoneRedundant: true
  }

  resource evHub 'eventhubs@2017-04-01' = {
    name: '${eventHubName}'
    properties: {
      messageRetentionInDays: evHubMessageRetentionInDays
      partitionCount: evHubPartitionCount
    }
  
    resource apiConsumerGroup 'consumergroups' = {
      name: apiConsumerGroupName
      properties: {}
    }
  
    resource processorConsumerGroup 'consumergroups' = {
      name: processorConsumerGroupName
      properties: {}
    }
  
    resource replicatorConsumerGroup 'consumergroups' = {
      name: replicatorConsumerGroupName
      properties: {}
    }

    resource apiAuthRule 'authorizationRules' = {
      name: '${apiConsumerGroupName}-sendlisten'
      properties: {
        rights: [
          'Send'
          'Listen'
        ]
      }
    }

    resource processorAuthRule 'authorizationRules' = {
      name: '${processorConsumerGroupName}-sendlisten'
      properties: {
        rights: [
          'Send'
          'Listen'
        ]
      }
    }
    
    resource replicatorAuthRule 'authorizationRules' = {
      name: '${replicatorConsumerGroupName}-sendlisten'
      properties: {
        rights: [
          'Send'
          'Listen'
        ]
      }
    }
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2020-04-01-preview' = {
  name: '${regionalName}-kvlt'
  location: location
  dependsOn: [
    evHubNamespace::evHub
  ]
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    enabledForTemplateDeployment: true
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    accessPolicies: []
  }

  resource cosmosDbAccountKey 'secrets' = {
    name: 'CosmosDbAccountKey'
    properties: {
      value: listkeys(cosmosDb.id, '2021-03-01-preview').primaryMasterKey
    }
  }

  resource orleansStorageAccountConnectionString 'secrets' = {
    name: 'OrleansStorageAccountConnectionString${toUpper(location)}'
    properties: {
      value: 'DefaultEndpointsProtocol=https;AccountName=${orleansStorageAccount.name};AccountKey=${listkeys(orleansStorageAccount.id, '2021-01-01').keys[0].value};EndpointSuffix=core.windows.net'
    }
  }

  resource replicatorStorageAccountConnectionString 'secrets' = {
    name: 'ReplicatorStorageAccountConnectionString${toUpper(location)}'
    properties: {
      value: 'DefaultEndpointsProtocol=https;AccountName=${replicatorStorageAccount.name};AccountKey=${listkeys(replicatorStorageAccount.id, '2021-01-01').keys[0].value};EndpointSuffix=core.windows.net'
    }
  }
  resource apiAuthRuleSecret 'secrets' = {
    name: 'ApiAuthRule${toUpper(location)}'
    properties: {
      value: listkeys(evHubNamespace::evHub::apiAuthRule.id, '2017-04-01').primaryConnectionString
    }
  }

  resource processorAuthRuleSecret 'secrets' = {
    name: 'ProcessorAuthRule${toUpper(location)}'
    properties: {
      value: listkeys(evHubNamespace::evHub::processorAuthRule.id, '2017-04-01').primaryConnectionString
    }
  }

  resource replicatorAuthRuleSecret 'secrets' = {
    name: 'ReplicatorAuthRule${toUpper(location)}'
    properties: {
      value: listkeys(evHubNamespace::evHub::replicatorAuthRule.id, '2017-04-01').primaryConnectionString
    }
  }
}

resource aksPodIdManagedIdentityOperator 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(aks.name, podIdUserManagedIdentity.name, 'Managed Identity Operator')
  dependsOn: [
    keyVault
  ]
  properties: {
    principalId: podIdUserManagedIdentity.properties.principalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleAssignments', 'f1a07417-d97a-45cb-824c-7a7467783830')
  }
  scope: podIdUserManagedIdentity
}

resource aksMetricPublisherRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(aks.name, 'Monitoring Metrics Publisher')
  dependsOn: [
    keyVault
  ]
  properties: {
    principalId: aks.properties.addonProfiles.omsAgent.identity.objectId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleAssignments', '3913510d-42f4-4e42-8a64-420c390055eb')
  }
  scope: aks
}

resource podIdAksKeyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(podIdUserManagedIdentity.name, keyVault.name, 'Key Vault Secrets User')
  dependsOn: [
    keyVault
  ]
  properties: {
    principalId: podIdUserManagedIdentity.properties.principalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleAssignments', '4633458b-17de-408a-b874-0445c86b69e6')
  }
  scope: keyVault
}

output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey

output podIdUserManagedIdentity object = {
  name: podIdUserManagedIdentity.name
  resourceId: podIdUserManagedIdentity.id
  clientId: podIdUserManagedIdentity.properties.clientId
  objectId: podIdUserManagedIdentity.properties.principalId
}

output aks object = {
  name: aks.name
  location: aks.location
  sku: aks.sku
  identity: aks.identity
  properties: aks.properties
}
