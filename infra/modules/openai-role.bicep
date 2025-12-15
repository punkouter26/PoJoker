// Module to assign role to OpenAI account in different resource group
// Deployed with scope set to the target resource group
targetScope = 'resourceGroup'

@description('Name of the OpenAI account')
param openAiAccountName string

@description('Principal ID of the web app managed identity')
param principalId string

// Reference the existing OpenAI account in this resource group
resource openAi 'Microsoft.CognitiveServices/accounts@2024-10-01-preview' existing = {
  name: openAiAccountName
}

// Assign Cognitive Services OpenAI User role
var roleAssignmentName = guid(openAi.id, principalId, 'CognitiveServicesOpenAIUser')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  scope: openAi
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd') // Cognitive Services OpenAI User
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output roleAssignmentId string = roleAssignment.id
