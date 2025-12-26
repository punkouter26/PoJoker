@description('Container App name')
param name string
param location string = resourceGroup().location
param tags object = {}
param managedEnvironmentId string
param containerImage string
param containerRegistryServer string
param containerRegistryName string
param registryPassword string
param appInsightsConnectionString string
param storageAccountName string
param openAiEndpoint string
param openAiDeploymentName string

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: managedEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryName
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: registryPassword
        }
        {
          name: 'appinsights-connection-string'
          value: appInsightsConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: toLower(replace(name, '/', '-'))
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
              value: 'Production'
            }
            {
              name: 'Azure__StorageAccountName'
              value: storageAccountName
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

output id string = containerApp.id
output name string = containerApp.name
output fqdn string = containerApp.properties.configuration.ingress.fqdn
output principalId string = containerApp.identity.principalId
output resource containerAppResource = containerApp