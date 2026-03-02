# DevOps: Deployment Pipeline & Environment Secrets

## Deployment Strategy

Po.Joker uses **Infrastructure-as-Code (IaC)** with Bicep templates for repeatable, predictable Azure deployments.

### Deployment Environments

| Environment | URL | AI Mode | Storage | Duration |
|---|---|---|---|---|
| **Local (dev)** | `http://localhost:5123` | Mock AI | Azurite Docker | Development |
| **E2E Testing** | Docker container | Mock AI | In-memory | < 10 min |
| **Staging** | Azure Container Apps | Azure OpenAI | Table Storage | Validation |
| **Production** | Azure App Service | Azure OpenAI | Table Storage | 24/7 |

## CI/CD Pipeline

```
GitHub Push
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
│ Step 1: Build & Test           │
│ - dotnet build                 │
│ - dotnet test                  │
│ - Code quality checks          │
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
│ Step 2: Container Build        │
│ - docker build                 │
│ - Push to Azure Container Reg  │
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
│ Step 3: Infrastructure         │
│ - Deploy Bicep templates       │
│ - Create resources             │
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
│ Step 4: Application Deploy     │
│ - Deploy container to App Svc  │
│ - Run smoke tests              │
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    ↓
Production ✅
```

## Environment Secrets Management

All secrets are stored in **Azure Key Vault** and injected at runtime using Managed Identity.

### Local Development Secrets

```bash
# File: ~/.Microsoft/UserSecrets/po-joker-dev/secrets.json

{
  "Azure:KeyVaultUri": "https://kv-poshared.vault.azure.net/",
  "Azure:StorageAccountName": "stpojoker26",
  "Azure:OpenAI:DeploymentName": "gpt-4.1-nano",
  "OpenAI:ApiKey": "(retrieved from Key Vault)",
  "JokeAPI:ApiKey": "(if applicable)"
}
```

**Load via:**
```bash
dotnet user-secrets init
dotnet user-secrets set "Azure:KeyVaultUri" "https://kv-poshared.vault.azure.net/"
```

### Production Secrets (Azure Key Vault)

| Secret Name | Description | Example |
|---|---|---|
| `azure-openai-api-key` | Azure OpenAI API key | `sk-...` |
| `jokeapi-api-key` | JokeAPI key (if applicable) | `abc123...` |
| `storage-account-key` | Storage account key (backup) | `DefaultEndpointsProtocol=...` |
| `application-insights-key` | App Insights instrumentation key | `uuid-...` |

### Environment Variables

#### Development (`.env` / `dotenv`)
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5123
POJOKER_DISABLE_KEYVAULT=false          # Use Key Vault
POJOKER_USE_MOCK_AI=true               # Use mock analysis service
```

#### Staging / Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443
POJOKER_DISABLE_KEYVAULT=false         # Require Key Vault
POJOKER_USE_MOCK_AI=false              # Use real Azure OpenAI
```

## Docker Build & Deployment

### Build Image
```bash
docker build -t po-joker:latest -f src/Po.Joker/Dockerfile .
```

### Run Locally
```bash
# With docker-compose (includes Azurite)
docker-compose up --build

# Or standalone with volume
docker run -p 5123:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e POJOKER_USE_MOCK_AI=true \
  -v azurite-data:/data \
  po-joker:latest
```

### Push to Azure Container Registry
```bash
az acr build --registry myacr --image po-joker:latest .
```

## Resource Provisioning (Bicep)

### Core Resources
```bicep
// infra/main.bicep
module webApp './modules/app-service.bicep' = {
  name: 'webApp'
  params: {
    location: location
    appName: 'po-joker'
    containerImage: containerImage
  }
}

module storage './modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageName: 'stpojoker26'
  }
}

module keyVault './modules/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    location: location
    vaultName: 'kv-poshared'
  }
}
```

### Deploy
```bash
az deployment group create \
  --resource-group rg-po-joker \
  --template-file infra/main.bicep \
  --parameters environment=prod containerImage=myacr.azurecr.io/po-joker:latest
```

## Monitoring & Alerts

### Application Insights
- **Exceptions**: Alert on first-chance exceptions
- **Performance**: Track dependency call durations
- **Availability**: Monitor health endpoint every 5 minutes
- **Custom Events**: Track joke fetch success rate

### Azure Monitor Alerts
| Alert | Threshold | Action |
|---|---|---|
| **Http 5xx Errors** | > 5 in 5 min | Page on-call |
| **Dependency Failures** | > 10% failure | Notify team |
| **CPU > 80%** | Sustained | Auto-scale |
| **Storage Queue Length** | > 1000 | Investigate |

### Health Checks Endpoint
```
GET /health
Response: 
{
  "status": "Healthy",
  "checks": {
    "JokeAPI": "Healthy",
    "AzureOpenAI": "Healthy",
    "TableStorage": "Healthy"
  }
}
```

## Rollback Strategy

1. **Fast Rollback**: Maintain previous container image (N-1 version)
2. **DB Rollback**: No breaking schema changes (backward compatible)
3. **GitHub Tags**: Tag releases for quick reference
4. **Blue-Green**: Deploy new version, switch traffic if healthy

```bash
# Rollback to previous version
az webapp deployment slot swap \
  --resource-group rg-po-joker \
  --name po-joker \
  --slot staging
```

## Disaster Recovery

| Scenario | RTO | RPO | Action |
|---|---|---|---|
| **Regional Outage** | 1 hour | 0 hours | Failover to secondary region |
| **DB Corruption** | 30 min | 1 hour | Restore from daily snapshot |
| **Key Vault Access Lost** | 15 min | Immediate | Reissue Managed Identity |

Leaderboard data is replicated to geo-redundant storage (GRS).

## Cost Optimization

- **App Service**: auto-scale down during off-peak
- **OpenAI**: Request lower-cost models (nano vs. turbo)
- **Storage**: Lifecycle policy to cool/archive after 30 days
- **VM Size**: Currently B1S (suitable for comedy workload)

## Maintenance Windows

- **Weekly**: Automated dependency updates via Dependabot
- **Monthly**: Full regression test suite
- **Quarterly**: Security audit + penetration testing
- **Annually**: Disaster recovery drill
