// Budget Module - Monthly spending limit with alerts
targetScope = 'resourceGroup'

@description('Name of the budget')
param budgetName string = 'pojoker-monthly-budget'

@description('Monthly budget amount in USD')
param monthlyAmount int = 5

@description('Alert threshold percentage (0-100)')
@minValue(1)
@maxValue(100)
param alertThresholdPercent int = 80

@description('Email addresses for budget alerts')
param alertContactEmails array = []

@description('Start date for budget tracking (YYYY-MM-DD format, first day of month)')
param startDate string = '${utcNow('yyyy')}-${utcNow('MM')}-01'

// Calculate budget end date (5 years from start)
var endYear = int(substring(startDate, 0, 4)) + 5
var endDate = '${endYear}-${substring(startDate, 5, 2)}-01'

// Tags
var tags = {
  Application: 'Po.Joker'
  Purpose: 'Cost Management'
  ManagedBy: 'Bicep'
}

resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: budgetName
  properties: {
    category: 'Cost'
    amount: monthlyAmount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: startDate
      endDate: endDate
    }
    filter: {
      dimensions: {
        name: 'ResourceGroupName'
        operator: 'In'
        values: [
          resourceGroup().name
        ]
      }
    }
    notifications: empty(alertContactEmails) ? {} : {
      firstThreshold: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: alertThresholdPercent
        contactEmails: alertContactEmails
        thresholdType: 'Actual'
      }
      forecastedThreshold: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        contactEmails: alertContactEmails
        thresholdType: 'Forecasted'
      }
    }
  }
}

// Output budget information
output budgetName string = budget.name
output budgetAmount int = monthlyAmount
output alertThreshold int = alertThresholdPercent
