targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string

// Tags that should be applied to all resources.
var tags = {
  'azd-env-name': environmentName
}

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

// Container Apps Environment for hosting the application
module containerAppsEnvironment 'modules/container-apps-environment.bicep' = {
  name: 'container-apps-environment'
  scope: rg
  params: {
    name: 'cae-${environmentName}'
    location: location
    tags: tags
  }
}

// Azure Storage Account for Table Storage
module storage 'modules/storage.bicep' = {
  name: 'storage'
  scope: rg
  params: {
    name: 'st${replace(environmentName, '-', '')}${uniqueString(rg.id)}'
    location: location
    tags: tags
    principalId: principalId
  }
}

// Container App for the API
module api 'modules/container-app.bicep' = {
  name: 'api'
  scope: rg
  params: {
    name: 'ca-${environmentName}-api'
    location: location
    tags: union(tags, { 'azd-service-name': 'api' })
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    containerImage: 'mcr.microsoft.com/dotnet/samples:aspnetapp' // Placeholder, replaced by azd deploy
    targetPort: 8080
    env: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Production'
      }
      {
        name: 'ConnectionStrings__tables'
        value: storage.outputs.tableEndpoint
      }
    ]
  }
}

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = ''
output AZURE_RESOURCE_GROUP string = rg.name
output API_URL string = api.outputs.uri
output STORAGE_TABLE_ENDPOINT string = storage.outputs.tableEndpoint
