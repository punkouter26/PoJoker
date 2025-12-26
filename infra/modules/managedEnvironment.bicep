@description('Managed Environment name')
param name string
param location string = resourceGroup().location
param tags object = {}
param logAnalyticsCustomerId string
@secure()
param logAnalyticsSharedKey string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsSharedKey
      }
    }
  }
}

output id string = containerAppEnvironment.id
output name string = containerAppEnvironment.name
output resourceName string = containerAppEnvironment.name