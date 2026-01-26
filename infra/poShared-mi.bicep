targetScope = 'resourceGroup'

@description('Name of the user-assigned managed identity')
param identityName string = 'mi-poShared-foundry'

@description('Location for the identity')
param location string = resourceGroup().location

resource foundryIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: identityName
  location: location
  tags: {
    purpose: 'foundry-deployer'
  }
}

output identityId string = foundryIdentity.id