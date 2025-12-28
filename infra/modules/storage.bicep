@description('The name of the Storage Account')
param name string

@description('The location for the resource')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The principal ID to assign roles to')
param principalId string

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: take(toLower(replace(name, '-', '')), 24)
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true // Required for Table Storage SDK
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Table Service
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2024-01-01' = {
  name: 'default'
  parent: storageAccount
}

// Role assignment for Storage Table Data Contributor
resource storageTableDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')
  scope: storageAccount
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for Storage Blob Data Contributor
resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  scope: storageAccount
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalType: 'ServicePrincipal'
  }
}

output id string = storageAccount.id
output name string = storageAccount.name
output tableEndpoint string = storageAccount.properties.primaryEndpoints.table
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
