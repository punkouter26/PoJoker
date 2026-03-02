# Local Setup: Day 1 Guide

Welcome to **Po.Joker**! This guide gets you running in 5 minutes.

## System Requirements

- **OS**: Windows, macOS, or Linux
- **.NET SDK**: [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- **Docker**: [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional but recommended)
- **Node.js**: 18+ (for E2E tests)
- **Git**: Latest version

## Quick Start (5 minutes)

### Option 1: Manual Setup (Docker Compose)

```bash
# 1. Start Azurite for local storage
docker-compose up -d azurite

# 2. Set environment variables
export ASPNETCORE_ENVIRONMENT=Development
export POJOKER_USE_MOCK_AI=true              # No Azure OpenAI calls
export POJOKER_DISABLE_KEYVAULT=true         # Skip Key Vault

# 3. Run the app
dotnet run --project src/Po.Joker/Po.Joker.csproj --urls http://localhost:5123
```

### Option 2: Minimal Setup (In-Memory Storage)

For quick UI testing without external dependencies:

```bash
# Just run, no Docker needed
export POJOKER_USE_MOCK_AI=true
export POJOKER_DISABLE_KEYVAULT=true

dotnet run --project src/Po.Joker/Po.Joker.csproj
```

## Configuration

### Mock AI Mode (Recommended for Dev)

When `POJOKER_USE_MOCK_AI=true`, the app uses a `MockAnalysisService` instead of Azure OpenAI:

```csharp
// Generated predictions are random but realistic
// Generated ratings are randomized
// No API calls = fast, deterministic testing
```

### Production AI Mode (Requires Azure)

1. **Get Azure OpenAI Credentials**
   ```bash
   # Create Azure resources (if not exists)
   az group create --name rg-po-joker --location eastus
   
   # Create Azure OpenAI
   az cognitiveservices account create \
     --name cogn-pojoker \
     --resource-group rg-po-joker \
     --kind OpenAI \
     --sku S0 \
     --location eastus
   ```

2. **Set Environment Variables**
   ```bash
   export Azure__KeyVaultUri=https://kv-poshared.vault.azure.net/
   export Azure__StorageAccountName=stpojoker26
   export POJOKER_USE_MOCK_AI=false
   ```

3. **Authenticate with Azure**
   ```bash
   az login
   ```

## File Structure

```
PoJoker/
├── src/
│   ├── Po.Joker/                    # Main Blazor app
│   │   ├── Components/              # Razor components
│   │   ├── Pages/                   # Blazor pages
│   │   ├── Services/                # Business logic
│   │   ├── Infrastructure/          # Cross-cutting concerns
│   │   └── Program.cs               # DI & middleware setup
│   └── ...
├── tests/
│   ├── Po.Joker.Tests.Unit/        # Unit tests
│   ├── Po.Joker.Tests.Integration/ # Integration tests
│   └── e2e/                         # Playwright E2E tests
├── infra/                           # Bicep deployment templates
├── docker-compose.yml               # Local services
└── PoJoker.sln                      # Solution file
```

## Development Workflow

### Running Tests

```bash
# Unit tests
dotnet test tests/Po.Joker.Tests.Unit/Po.Joker.Tests.Unit.csproj

# Integration tests
dotnet test tests/Po.Joker.Tests.Integration/Po.Joker.Tests.Integration.csproj

# E2E tests (requires app running)
cd tests/e2e
npm install
npm run test
```

### Running in Watch Mode

```bash
# Auto-rebuild on file changes
dotnet watch run --project src/Po.Joker/Po.Joker.csproj
```

## Docker Compose Configuration

The `docker-compose.yml` provides **Azurite** for local Table Storage emulation:

```yaml
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    container_name: pojoker-azurite
    ports:
      - "10000:10000"    # Blob Storage
      - "10001:10001"    # Queue Storage
      - "10002:10002"    # Table Storage
    volumes:
      - azurite-data:/data
```

**Table Storage connection string:**
```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUQbIQVnlg==;TableEndpoint=http://127.0.0.1:10002/;
```

Stop Azurite:
```bash
docker-compose down
```

## Troubleshooting

### "Can't reach JokeAPI"
- **Solution**: Check internet connection. JokeAPI is external (not mocked).
- Endpoint: https://v2.jokeapi.dev/joke/Any?type=twopart

### "Key Vault authentication failed"
- **Solution**: Set `POJOKER_DISABLE_KEYVAULT=true` for local dev
- For production, ensure Managed Identity is assigned to your App Service

### "Azurite not starting"
```bash
# Delete old container
docker-compose down -v

# Restart
docker-compose up -d azurite
```

### "Port 5123 already in use"
```bash
# Use a different port
dotnet run --project src/Po.Joker/Po.Joker.csproj --urls http://localhost:5999
```

### "Tests failing with connection errors"
- Ensure Docker is running
- Ensure `docker-compose up -d` has been run
- Check Azurite logs: `docker logs pojoker-azurite`

## Environment Variables Reference

| Variable | Default | Purpose |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` for debug logs |
| `ASPNETCORE_URLS` | `https://localhost:7123` | Server URL |
| `POJOKER_USE_MOCK_AI` | `false` | Use mock analysis (faster) |
| `POJOKER_DISABLE_KEYVAULT` | `false` | Skip Key Vault (for local dev) |
| `Azure__KeyVaultUri` | `` | Key Vault URL |
| `Azure__StorageAccountName` | `` | Storage account name |
| `SERILOG__MINIMUMLEVEL` | `Information` | Log level |

## Next Steps

1. ✅ Run the app: `dotnet run --project src/Po.Joker`
2. 🌐 Open browser: http://localhost:5123
3. 🧪 Run tests: `dotnet test`
4. 📖 Read [ProductSpec.md](./ProductSpec.md) for business logic
5. 🏗️ Check [Architecture.mmd](./Architecture.mmd) for system design

## Getting Help

- **Issue Tracker**: GitHub Issues
- **Documentation**: `/docs` folder
- **Architecture Diagrams**: `/docs/*.mmd` files
- **Code Comments**: Extensive inline documentation

**Happy joking! 🃏**
