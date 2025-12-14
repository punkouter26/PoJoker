using 'main.bicep'

// Development environment parameters
param environment = 'dev'
param baseName = 'pojoker'
param appServicePlanSku = 'B1'
param openAiModel = 'gpt-4o-mini'
// Note: openAiApiKey should be provided at deployment time via --parameters flag
// Example: az deployment group create --parameters openAiApiKey='your-key-here'
