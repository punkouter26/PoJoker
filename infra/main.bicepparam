using 'main.bicep'

// Development environment parameters
param environment = 'dev'
param baseName = 'pojoker'
param appServicePlanSku = 'B1'
param openAiModel = 'gpt-4o-mini'

// Shared App Service Plan (poshared resource group)
param existingAppServicePlanName = 'pojoker-shared-asp'
param existingAppServicePlanResourceGroup = 'poshared'

// Shared Azure AI Foundry/OpenAI (poshared resource group)
param openAiEndpoint = 'https://poshared-openai.cognitiveservices.azure.com/'
param openAiDeploymentName = 'gpt-4o-mini'
param existingOpenAiResourceGroup = 'poshared'
param existingOpenAiAccountName = 'poshared-openai'
