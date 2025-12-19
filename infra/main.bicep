// PoLearnCert Infrastructure - Azure Resources
// Deploys: Storage Account (with Table Storage) + Web App (using shared App Service Plan)

targetScope = 'resourceGroup'

@description('The environment name (dev, staging, prod)')
param environmentName string = 'prod'

@description('The Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for resources')
param baseName string = 'polearncert'

@description('Resource ID of the shared App Service Plan')
param sharedAppServicePlanId string = '/subscriptions/bbb8dfbe-9169-432f-9b7a-fbf861b51037/resourceGroups/PoShared/providers/Microsoft.Web/serverfarms/PoSharedAppServicePlan1'

// Generate unique suffix for globally unique names
var resourceToken = toLower(uniqueString(subscription().id, resourceGroup().id, baseName))
var storageAccountName = toLower('${take(baseName, 10)}${take(resourceToken, 14)}')
var webAppName = baseName

// ========================================
// Storage Account with Table Storage
// ========================================
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    publicNetworkAccess: 'Enabled'
  }
  tags: {
    environment: environmentName
    application: baseName
  }
}

// Table Service
resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// Tables for the application
resource tableCertifications 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'Certifications'
}

resource tableQuestions 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'Questions'
}

resource tableQuizSessions 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'QuizSessions'
}

resource tableQuizAttempts 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'QuizAttempts'
}

resource tableUsers 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'Users'
}

resource tableLeaderboards 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: 'Leaderboards'
}

// ========================================
// Web App (App Service) - Uses shared plan from PoShared
// ========================================
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: 'centralus' // Must match the shared App Service Plan location
  kind: 'app'
  properties: {
    serverFarmId: sharedAppServicePlanId
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      alwaysOn: false // Free tier doesn't support alwaysOn
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      http20Enabled: true
      appSettings: [
        {
          name: 'AzureTableStorage__ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
  }
  tags: {
    environment: environmentName
    application: baseName
  }
}

// ========================================
// Outputs
// ========================================
@description('The name of the storage account')
output storageAccountName string = storageAccount.name

@description('The name of the web app')
output webAppName string = webApp.name

@description('The default hostname of the web app')
output webAppHostName string = webApp.properties.defaultHostName

@description('The URL of the web app')
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
