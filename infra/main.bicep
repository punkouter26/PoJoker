// Po.Joker Azure Infrastructure
// Deploys: App Service Plan, Web App, Storage Account, Application Insights, Log Analytics Workspace, Budget
targetScope = 'resourceGroup'

@description('The environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for all resources')
param baseName string = 'pojoker'

@description('The SKU for the App Service Plan')
@allowed(['B1', 'B2', 'B3', 'S1', 'S2', 'S3', 'P1v2', 'P2v2', 'P3v2'])
param appServicePlanSku string = 'B1'

@description('Use an existing App Service Plan from another resource group (e.g., poshared). When provided, the plan will not be created.')
param existingAppServicePlanResourceGroup string = 'poshared'

@description('Existing App Service Plan name to use from the specified resource group.')
param existingAppServicePlanName string

@description('OpenAI API Key (optional - for production joke generation)')
@secure()
param openAiApiKey string = ''

@description('OpenAI Model deployment name')
param openAiModel string = 'gpt-4o-mini'

@description('Email address for budget alerts')
param budgetAlertEmail string = 'punkouter26@gmail.com'

// Computed names
var resourcePrefix = '${baseName}-${environment}'
var appServicePlanName = '${resourcePrefix}-plan'
var webAppName = '${resourcePrefix}-app'
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

// App Service Plan reference: use existing plan when provided
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' existing = if (!empty(existingAppServicePlanName)) {
  name: existingAppServicePlanName
  scope: resourceGroup(existingAppServicePlanResourceGroup)
}

// Web App
module webApp 'br/public:avm/res/web/site:0.15.1' = {
  name: 'webApp'
  params: {
    name: webAppName
    location: location
    kind: 'app,linux'
    serverFarmResourceId: empty(existingAppServicePlanName) ? resourceId('Microsoft.Web/serverfarms', appServicePlanName) : appServicePlan.id
    tags: commonTags
    managedIdentities: {
      systemAssigned: true
    }
    httpsOnly: true
    siteConfig: {
      // When using an existing plan, AlwaysOn depends on that plan's tier; default to true here
      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      healthCheckPath: '/health'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnetcore'
        }
      ]
    }
    appSettingsKeyValuePairs: {
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.outputs.connectionString
      ASPNETCORE_ENVIRONMENT: environment == 'prod' ? 'Production' : 'Development'
      Azure__StorageAccountName: storageAccount.outputs.name
      OpenAI__ApiKey: openAiApiKey
      OpenAI__Model: openAiModel
      JokeSettings__CacheEnabled: 'true'
      JokeSettings__RateLimitPerMinute: '60'
      // App Insights Snapshot Debugger and Profiler (T115)
      DiagnosticServices_EXTENSION_VERSION: '~3'
      InstrumentationEngine_EXTENSION_VERSION: 'disabled'
      SnapshotDebugger_EXTENSION_VERSION: 'disabled'
      XDT_MicrosoftApplicationInsights_BaseExtensions: 'disabled'
      XDT_MicrosoftApplicationInsights_Mode: 'recommended'
      XDT_MicrosoftApplicationInsights_PreemptSdk: '1'
      APPINSIGHTS_PROFILERFEATURE_VERSION: environment == 'prod' ? '1.0.0' : 'disabled'
      APPINSIGHTS_SNAPSHOTFEATURE_VERSION: environment == 'prod' ? '1.0.0' : 'disabled'
    }
  }
}

// Role assignment for Web App to access Storage Account
var roleAssignmentName = guid(resourceGroup().id, storageAccountName, webAppName, 'Storage Blob Data Contributor')
resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: webApp.outputs.systemAssignedMIPrincipalId!
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
output webAppName string = webApp.outputs.name
output webAppHostname string = webApp.outputs.defaultHostname
output webAppResourceId string = webApp.outputs.resourceId
@secure()
output appInsightsConnectionString string = appInsights.outputs.connectionString
@secure()
output appInsightsInstrumentationKey string = appInsights.outputs.instrumentationKey
output storageAccountName string = storageAccount.outputs.name
output storageAccountResourceId string = storageAccount.outputs.resourceId
output logAnalyticsWorkspaceId string = logAnalytics.id
output budgetName string = budget.outputs.budgetName
