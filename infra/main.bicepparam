using 'main.bicep'

// Development environment parameters
param environment = 'dev'
param baseName = 'pojoker'

// Shared Azure AI Foundry/OpenAI (poshared resource group)
param openAiEndpoint = 'https://poshared-openai.cognitiveservices.azure.com/'
param openAiDeploymentName = 'gpt-4o-mini'
param existingOpenAiResourceGroup = 'poshared'
param existingOpenAiAccountName = 'poshared-openai'

// Budget alert email
param budgetAlertEmail = 'punkouter26@gmail.com'

// Initial container image (will be updated by CI/CD)
param containerImage = 'mcr.microsoft.com/dotnet/samples:aspnetapp'
