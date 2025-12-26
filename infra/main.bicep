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

@description('Deployment target: containerApp or appService')
@allowed(['containerApp','appService'])
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
var appServicePlanName = '${resourcePrefix}-plan'
var webAppName = '${resourcePrefix}-webapp'

// Tags applied to all resources
var commonTags = {
  Application: 'Po.Joker'
  Environment: environment
  ManagedBy: 'Bicep'
}

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: commonTags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Application Insights
module appInsights 'br/public:avm/res/insights/component:0.6.0' = {
  name: 'appInsights'
  params: {
    name: appInsightsName
    location: location
    workspaceResourceId: logAnalytics.id
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
resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppEnvName
  location: location
  tags: commonTags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// Container App
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  tags: commonTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.name
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'appinsights-connection-string'
          value: appInsights.outputs.connectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'pojoker'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environment == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'Azure__StorageAccountName'
              value: storageAccount.outputs.name
            }
            {
              name: 'Azure__OpenAI__Endpoint'
              value: openAiEndpoint
            }
            {
              name: 'Azure__OpenAI__DeploymentName'
              value: openAiDeploymentName
            }
            {
              name: 'JokeSettings__CacheEnabled'
              value: 'true'
            }
            {
              name: 'JokeSettings__RateLimitPerMinute'
              value: '60'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

// Optional App Service (Linux) resources - deployed when `deploymentTarget` == 'appService'
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = if (deploymentTarget == 'appService') {
  name: appServicePlanName
  location: location
  tags: commonTags
  sku: {
    name: 'P1v2'
    tier: 'PremiumV2'
    capacity: 1
  }
  kind: 'linux'
  properties: {}
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = if (deploymentTarget == 'appService') {
  name: webAppName
  location: location
  tags: commonTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
      appSettings: [
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${containerRegistry.properties.loginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: containerRegistry.name
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.outputs.connectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'Azure__StorageAccountName'
          value: storageAccount.outputs.name
        }
      ]
      alwaysOn: true
      use32BitWorkerProcess: false
    }
    httpsOnly: true
  }
}

// Role assignment for Container App to access Storage Account (when using Container Apps)
var storageRoleAssignmentName = guid(resourceGroup().id, storageAccountName, containerAppName, 'Storage Blob Data Contributor')
resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'containerApp') {
  name: storageRoleAssignmentName
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for App Service to access Storage Account (when using App Service)
var storageRoleAssignmentNameAppSvc = guid(resourceGroup().id, storageAccountName, webAppName, 'Storage Blob Data Contributor')
resource storageRoleAssignmentAppService 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'appService') {
  name: storageRoleAssignmentNameAppSvc
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: webApp.identity.principalId
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
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for App Service to access Storage Tables
var tableRoleAssignmentNameAppSvc = guid(resourceGroup().id, storageAccountName, webAppName, 'Storage Table Data Contributor')
resource tableRoleAssignmentAppService 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'appService') {
  name: tableRoleAssignmentNameAppSvc
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3') // Storage Table Data Contributor
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Key Vault role assignment - grant Container App or App Service access to read secrets from Key Vault
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
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

var keyVaultRoleAssignmentNameAppSvc = guid(resourceGroup().id, keyVault.name, webAppName, 'KeyVaultSecretsUser')
resource keyVaultRoleAssignmentAppService 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deploymentTarget == 'appService') {
  name: keyVaultRoleAssignmentNameAppSvc
  scope: keyVault
  dependsOn: [
    webApp
  ]
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: webApp.identity.principalId
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
output containerAppName string = containerApp.name
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output webAppName string = deploymentTarget == 'appService' ? webApp.name : ''
output webAppUrl string = deploymentTarget == 'appService' ? 'https://${webApp.properties.defaultHostName}' : ''
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
