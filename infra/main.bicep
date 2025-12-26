// Po.Joker Azure Infrastructure
// Deploys: Container App Environment, Container App, Container Registry, Storage Account, Application Insights, Log Analytics Workspace, Budget
targetScope = 'resourceGroup'

@description('The environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for all resources')
param baseName string = 'pojoker'

@description('Azure AI Foundry / OpenAI endpoint')
param openAiEndpoint string = 'https://poshared-openai.cognitiveservices.azure.com/'

@description('Azure AI Foundry deployment name')
param openAiDeploymentName string = 'gpt-4o-mini'

@description('Resource group containing the shared Azure AI Foundry account')
param existingOpenAiResourceGroup string = 'poshared'

@description('Name of the shared Azure AI Foundry account to use')
param existingOpenAiAccountName string = 'poshared-openai'

@description('Email address for budget alerts')
param budgetAlertEmail string = 'punkouter26@gmail.com'

@description('Container image to deploy')
param containerImage string = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

// Deployment target is always containerApp
param deploymentTarget string = 'containerApp'

@description('Existing Key Vault name to grant access to the Container App or Web App')
param keyVaultName string = 'pojoker-kv'

// Computed names
var resourcePrefix = '${baseName}-${environment}'
var containerAppName = '${resourcePrefix}-app'
var containerAppEnvName = '${resourcePrefix}-env'
var containerRegistryName = replace(toLower('${baseName}${environment}acr'), '-', '')
var storageAccountName = replace(toLower('${baseName}${environment}st'), '-', '')
var logAnalyticsName = '${resourcePrefix}-logs'
var appInsightsName = '${resourcePrefix}-insights'


// Tags applied to all resources
var commonTags = {
  Application: 'Po.Joker'
  Environment: environment
  ManagedBy: 'Bicep'
}

// Log Analytics Workspace
module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics'
  params: {
    name: logAnalyticsName
    location: location
    tags: commonTags
    retentionInDays: 30
  }
}

// Application Insights
module appInsights 'br/public:avm/res/insights/component:0.6.0' = {
  name: 'appInsights'
  params: {
    name: appInsightsName
    location: location
    workspaceResourceId: logAnalytics.outputs.id
    applicationType: 'web'
    tags: commonTags
    disableIpMasking: false
    retentionInDays: 90
  }
}

// Storage Account for leaderboard data
module storageAccount 'br/public:avm/res/storage/storage-account:0.19.0' = {
  name: 'storageAccount'
  params: {
    name: storageAccountName
    location: location
    skuName: 'Standard_LRS'
    kind: 'StorageV2'
    tags: commonTags
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    blobServices: {
      containers: [
        {
          name: 'leaderboards'
          publicAccess: 'None'
        }
        {
          name: 'jokes-cache'
          publicAccess: 'None'
        }
      ]
    }
    tableServices: {
      tables: [
        {
          name: 'ratings'
        }
        {
          name: 'sessions'
        }
      ]
    }
  }
}

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: containerRegistryName
  location: location
  tags: commonTags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
  }
}

// Shared Azure AI Foundry / OpenAI account
resource openAi 'Microsoft.CognitiveServices/accounts@2024-10-01-preview' existing = {
  name: existingOpenAiAccountName
  scope: resourceGroup(existingOpenAiResourceGroup)
}

// Container App Environment
module managedEnv 'modules/managedEnvironment.bicep' = {
  name: 'managedEnv'
  params: {
    name: containerAppEnvName
    location: location
    tags: commonTags
    logAnalyticsCustomerId: logAnalytics.outputs.customerId
    logAnalyticsSharedKey: logAnalytics.outputs.sharedKey
  }
}

// Container App (module)
module containerApp 'modules/containerApp.bicep' = {
  name: 'containerApp'
  params: {
    name: containerAppName
    location: location
    tags: union(commonTags, {
      'azd-service-name': 'joker'
    })
    managedEnvironmentId: managedEnv.outputs.id
    containerImage: containerImage
    containerRegistryServer: containerRegistry.properties.loginServer
    containerRegistryName: containerRegistry.name
    registryPassword: containerRegistry.listCredentials().passwords[0].value
    appInsightsConnectionString: appInsights.outputs.connectionString
    storageAccountName: storageAccount.outputs.name
    openAiEndpoint: openAiEndpoint
    openAiDeploymentName: openAiDeploymentName
  }
}



// Role assignment for Container App to access Storage Account (when using Container Apps)
var storageRoleAssignmentName = guid(resourceGroup().id, storageAccountName, containerAppName, 'Storage Blob Data Contributor')
resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'containerApp') {
  name: storageRoleAssignmentName
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: containerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}



// Role assignment for Container App to access Storage Tables
var tableRoleAssignmentName = guid(resourceGroup().id, storageAccountName, containerAppName, 'Storage Table Data Contributor')
resource tableRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'containerApp') {
  name: tableRoleAssignmentName
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3') // Storage Table Data Contributor
    principalId: containerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}



// Key Vault role assignment - grant Container App access to read secrets from Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

var keyVaultRoleAssignmentName = guid(resourceGroup().id, keyVault.name, containerAppName, 'KeyVaultSecretsUser')
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'containerApp') {
  name: keyVaultRoleAssignmentName
  scope: keyVault
  dependsOn: [
    containerApp
  ]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: containerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}



// Budget for cost management
module budget './modules/budget.bicep' = {
  name: 'budget'
  params: {
    budgetName: '${resourcePrefix}-monthly-budget'
    monthlyAmount: 5
    alertThresholdPercent: 80
    alertContactEmails: [budgetAlertEmail]
  }
}

// Outputs
output containerAppName string = containerApp.outputs.name
output containerAppFqdn string = containerApp.outputs.fqdn
output containerAppUrl string = 'https://${containerApp.outputs.fqdn}'

output containerRegistryName string = containerRegistry.name
output containerRegistryLoginServer string = containerRegistry.properties.loginServer
@secure()
output appInsightsConnectionString string = appInsights.outputs.connectionString
@secure()
output appInsightsInstrumentationKey string = appInsights.outputs.instrumentationKey
output storageAccountName string = storageAccount.outputs.name
output storageAccountResourceId string = storageAccount.outputs.resourceId
output logAnalyticsWorkspaceId string = logAnalytics.id
output budgetName string = budget.outputs.budgetName
