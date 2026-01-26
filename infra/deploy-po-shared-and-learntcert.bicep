// Deploy Cognitive Services into existing PoShared RG and a Container App into PoLearnCert RG

@description('Name for the Cognitive Services account')
param cognitiveName string = 'cog-po-shared'

@description('Location for Cognitive Services')
param cognitiveLocation string = 'eastus2'

@description('Name of the Container App')
param containerAppName string = 'polearncert-api'

@description('Location for Container App')
param containerLocation string = 'eastus'

@description('Container Apps Environment resource id in target resource group (optional)')
param containerAppsEnvironmentId string = ''

@description('Container image to deploy')
param containerImage string = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

// Deploy Cognitive Services into existing resource group PoShared
resource poSharedRg 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: 'PoShared'
}

module cognitive 'modules/ai-cognitive.bicep' = {
  name: 'cognitive'
  scope: poSharedRg
  params: {
    name: cognitiveName
    location: cognitiveLocation
    tags: {
      'azd-env-name': 'po-shared'
      'purpose': 'ai'
    }
    skuName: 'S0'
    kind: 'CognitiveServices'
  }
}

// Create a user-assigned managed identity in PoShared for Foundry/model deployments
resource foundryIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: 'mi-poShared-foundry'
  scope: poSharedRg
  location: cognitiveLocation
  tags: {
    purpose: 'foundry-deployer'
  }
}

// Grant the managed identity Owner on the PoShared resource group (broad access for deployments)
resource foundryIdentityRole 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(poSharedRg.id, foundryIdentity.principalId, 'owner-role')
  scope: poSharedRg
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '8e3af657-a8ff-443c-a75c-2fe8c4bcb635')
    principalId: foundryIdentity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Deploy Container App into existing resource group PoLearnCert
resource poLearnRg 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: 'PoLearnCert'
}

module api 'modules/container-app.bicep' = {
  name: 'api'
  scope: poLearnRg
  params: {
    name: containerAppName
    location: containerLocation
    tags: {
      'azd-env-name': 'polearncert'
      'azd-service-name': 'api'
    }
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerImage: containerImage
    targetPort: 8080
    env: [
      {
        name: 'AZURE_COGNITIVE_SERVICE_ENDPOINT'
        value: cognitive.outputs.endpoint
      }
    ]
  }
}

output cognitiveId string = cognitive.outputs.id
output containerAppUri string = api.outputs.uri
output foundryIdentityId string = foundryIdentity.id
output foundryIdentityClientId string = foundryIdentity.clientId
output foundryIdentityPrincipalId string = foundryIdentity.principalId
