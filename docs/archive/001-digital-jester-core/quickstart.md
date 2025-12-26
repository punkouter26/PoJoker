# Quickstart: Digital Jester Core

**Feature**: 001-digital-jester-core  
**Date**: 2025-12-12

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for Azure resources)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [Node.js 20+](https://nodejs.org/) (for Playwright E2E tests)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (local storage emulator)

## Local Development Setup

### 1. Clone and Navigate

```powershell
git clone https://github.com/your-repo/PoJoker.git
cd PoJoker
git checkout 001-digital-jester-core
```

### 2. Verify .NET SDK

```powershell
dotnet --version
# Should output 10.0.xxx matching global.json
```

### 3. Restore Dependencies

```powershell
dotnet restore PoJoker.sln
```

### 4. Configure Local Secrets

```powershell
# Navigate to the server project
cd src/Po.Joker

# Initialize user secrets
dotnet user-secrets init

# Set Azure OpenAI configuration (for local dev without Managed Identity)
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-35-turbo"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key-here"

# Return to root
cd ../..
```

> **Note**: In production, Managed Identity is used instead of API keys.

### 5. Start Azurite (Local Storage Emulator)

```powershell
# In a separate terminal
azurite --silent --location .azurite --debug .azurite/debug.log
```

Or use the VS Code Azurite extension.

### 6. Run the Application

```powershell
dotnet run --project src/Po.Joker
```

The application will start at:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

### 7. Verify the Setup

1. Open https://localhost:5001 in Chrome/Edge
2. Navigate to `/diag` to verify all health checks pass
3. Click "Start" to begin the passive comedy loop

## Running Tests

### Unit & Integration Tests (xUnit)

```powershell
dotnet test tests/Po.Joker.Tests --verbosity normal
```

### Component Tests (bUnit)

```powershell
dotnet test tests/Po.Joker.Client.Tests --verbosity normal
```

### E2E Tests (Playwright)

```powershell
# First time: install Playwright browsers
cd tests/Po.Joker.E2E.Tests
pwsh bin/Debug/net10.0/playwright.ps1 install chromium

# Run E2E tests
dotnet test --verbosity normal
```

### All Tests with Coverage

```powershell
dotnet test PoJoker.sln --collect:"XPlat Code Coverage"
# Coverage reports generated in docs/coverage/
```

## Azure Deployment

### 1. Login to Azure

```powershell
az login
azd auth login
```

### 2. Initialize Environment

```powershell
azd init --environment pojoker-dev
```

### 3. Provision Infrastructure

```powershell
azd provision
```

This creates:
- Resource Group: `PoJoker-rg`
- Azure Container Apps (Linux)
- Storage Account (Table Storage)
- Azure AI Foundry (Hub + Project with gpt-35-turbo)
- Application Insights + Log Analytics
- $5/month budget with 80% alert

### 4. Deploy Application

```powershell
azd deploy
```

### 5. Verify Deployment

```powershell
# Get the deployed URL
azd show --output json | jq -r '.services.web.endpoints[0]'

# Open in browser and navigate to /diag
```

## Project Structure Quick Reference

```
src/
├── Po.Joker/           # Server (API + SSR + Blazor host)
│   ├── Features/       # Vertical slices (Jokes, Analysis, Leaderboards, Diagnostics)
│   ├── Infrastructure/ # Cross-cutting (ExceptionHandling, Resilience, Telemetry)
│   └── Components/     # Razor components and pages
├── Po.Joker.Client/    # WASM interactive components
└── Po.Joker.Shared/    # DTOs, contracts, validation

tests/
├── Po.Joker.Tests/         # xUnit integration tests
├── Po.Joker.Client.Tests/  # bUnit component tests
└── Po.Joker.E2E.Tests/     # Playwright E2E tests

infra/                  # Bicep IaC
docs/                   # ADRs, coverage, KQL queries
```

## Key URLs

| URL | Description |
|-----|-------------|
| `/` | Main Jester Stage (passive loop) |
| `/leaderboard` | Joke leaderboards by category |
| `/diag` | Diagnostics and health checks |
| `/swagger` | OpenAPI documentation |

## Troubleshooting

### "No British voice available"

The app gracefully falls back to any English voice. Check browser TTS support:
```javascript
speechSynthesis.getVoices().filter(v => v.lang.startsWith('en'))
```

### "JokeAPI rate limited"

The app uses Polly retry with exponential backoff. Wait 1 minute for circuit breaker to reset.

### "AI timeout"

The Jester is "stumped" after 15 seconds. This is expected behavior; the loop continues.

### "Azurite connection refused"

Ensure Azurite is running:
```powershell
azurite --silent --location .azurite
```

### "Coverage below 80%"

Run coverage report and identify gaps:
```powershell
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:docs/coverage -reporttypes:Html
```
