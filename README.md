# Po.Joker - The Digital Jester 🃏

An autonomous AI comedy application that fetches multi-part jokes, predicts punchlines using Azure OpenAI, rates them across multiple dimensions, and delivers an immersive medieval-themed comedy show—**all without user input**.

**Status:** Active Development  
![.NET 10](https://img.shields.io/badge/.NET-10.0%20Preview-purple)
![Blazor](https://img.shields.io/badge/Blazor-Web%20App-blue)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4)
![Tests](https://img.shields.io/badge/Tests-Unit%20%26%20Integration-green)
![License](https://img.shields.io/badge/License-MIT-green)

## 🎭 Features

- **Passive Comedy Loop**: Autonomous, never-ending comedy show
- **AI Punchline Prediction**: GPT-4 attempts to guess punchlines before reveal
- **Medieval Dark Theme**: Immersive Gothic UI with custom fonts
- **Text-to-Speech**: British male voice narration
- **Audio Effects**: Drum roll before reveals, trombone on failures
- **Joke Ratings**: Cleverness, Rudeness, Complexity, and Difficulty scores
- **Leaderboards**: Browse historical jokes by rating category
- **Diagnostics**: Health checks for all external dependencies

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Aspire local development)

### Local Development with .NET Aspire (Recommended)

**Single command to start everything:**

```bash
dotnet run --project src/PoJoker.AppHost
```

This will:
- 🚀 Start the **Aspire Dashboard** for monitoring all services
- 📦 Spin up **Azurite** container for local storage (persistent across restarts)
- 🌐 Launch the **Po.Joker** Blazor application
- 🔗 Automatically wire up all service connections

The **Aspire Dashboard** will open automatically and show:
- All running services and their status
- Real-time logs from all services
- Distributed traces across requests
- Metrics and health checks

### Configure AI Features (Optional)

```bash
cd src/PoJoker.AppHost
dotnet user-secrets set "Azure:OpenAI:Endpoint" "your-endpoint"
```

### Alternative: Standalone Development

If you prefer not to use Aspire:

1. **Start Azurite manually**
   ```bash
   azurite --silent --location ./azurite --debug ./azurite/debug.log
   ```

2. **Run the application directly**
   ```bash
   dotnet run --project src/Po.Joker
   ```

### Running Tests

```bash
# Unit tests
dotnet test tests/Po.Joker.Tests.Unit

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# E2E tests (requires browser)
dotnet test tests/Po.Joker.Tests.E2E
```

## 🏗️ Architecture

This application uses **.NET Aspire** for orchestration and service discovery.

```
PoJoker/
├── src/
│   ├── PoJoker.AppHost/        # 🚀 Aspire orchestrator (start here!)
│   ├── PoJoker.ServiceDefaults/ # Shared service defaults (telemetry, resilience)
│   ├── Po.Joker/               # Server-side Blazor + API endpoints
│   │   ├── Features/           # Vertical slice architecture
│   │   │   ├── Jokes/          # Joke fetching from JokeAPI
│   │   │   ├── Analysis/       # AI punchline prediction
│   │   │   ├── Leaderboards/   # Historical joke rankings
│   │   │   └── Diagnostics/    # Health checks
│   │   └── Infrastructure/     # Cross-cutting concerns
│   ├── Po.Joker.Client/        # Client-side Blazor WASM
│   └── Po.Joker.Shared/        # DTOs and contracts
├── tests/
│   ├── Po.Joker.Tests.Unit/    # xUnit unit tests
│   └── Po.Joker.Tests.E2E/     # Playwright E2E tests
├── infra/                       # Bicep IaC templates
└── docs/                        # Documentation
```

### Aspire Stack

| Component | Purpose |
|-----------|---------|
| **PoJoker.AppHost** | Orchestrates all services, manages Azurite container |
| **PoJoker.ServiceDefaults** | OpenTelemetry, health checks, service discovery, resilience |
| **Po.Joker** | Main Blazor Server + WASM application |
| **Azurite** | Local Azure Table Storage emulator (Docker container) |

## ☁️ Azure Deployment

### Using GitHub Actions (Recommended)

1. **Configure OIDC authentication (recommended)**
   - Create an Azure AD App Registration
   - Add Federated Credentials for your GitHub repository
   - Set repository secrets:
     - `AZURE_CLIENT_ID`
     - `AZURE_TENANT_ID`
     - `AZURE_SUBSCRIPTION_ID`
     - `OPENAI_API_KEY`

2. **(Alternative) Use a Service Principal secret**
   - Create a Service Principal and a client secret in Azure AD.
   - Add a repository secret named `AZURE_CREDENTIALS` with a JSON value like:

```json
{
  "clientSecret":"<client-secret>",
  "subscriptionId":"<subscription-id>",
  "tenantId":"<tenant-id>",
  "clientId":"<client-id>"
}
```

   - The workflow should use the `azure/login@v2` action with:

```yaml
with:
  creds: ${{ secrets.AZURE_CREDENTIALS }}
```

   - Ensure the secret value is valid JSON and contains these keys exactly (case-sensitive): `clientId`, `clientSecret`, `tenantId`, `subscriptionId`.
   - After adding the secret, the workflow includes a verification step (`az account show` and `azd --version`) that will fail early if login doesn't succeed.

3. **Deploy via workflow**
   - Push to `main` branch for automatic deployment
   - Use manual workflow dispatch for specific environments

### Manual Deployment with Azure Developer CLI (azd)

```bash
# Login to Azure
azd auth login

# Initialize environment (first time only)
azd init

# Provision infrastructure and deploy to Azure Container Apps
azd up
```

This will:
- Create Azure Container Registry, Container App Environment, and Container App
- Build and push the Docker image
- Deploy the application to Azure Container Apps

## 📊 Monitoring

### Application Insights

The application includes comprehensive monitoring:
- Request/response logging
- AI latency tracking
- Error rate monitoring
- Custom metrics for joke analysis

### KQL Queries

See [docs/kql/](docs/kql/) for monitoring queries:
- `error-rates.kql` - Error monitoring and anomaly detection
- `ai-latency.kql` - AI service performance tracking

## 🔧 Configuration

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `OpenAI__ApiKey` | OpenAI API key for punchline prediction | No* |
| `Azure__StorageConnectionString` | Azure Table Storage connection | No* |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights connection | No |

*Uses mock/local services when not configured

### App Settings

```json
{
  "JokeSettings": {
    "CacheEnabled": true,
    "RateLimitPerMinute": 60,
    "SafeMode": true
  }
}
```

## � Documentation & Architecture

Comprehensive documentation with visual diagrams is available in the `/docs` folder:

### High-Signal Documentation
- **[ProductSpec.md](docs/ProductSpec.md)** – Business logic, features, success metrics, and scope
- **[DevOps.md](docs/DevOps.md)** – CI/CD pipeline, environment secrets, deployment strategies
- **[LocalSetup.md](docs/LocalSetup.md)** – Day 1 onboarding guide, Docker Compose, troubleshooting

### Architecture Diagrams (View in Mermaid)

**Full Detail Diagrams:**
- **[Architecture.mmd](docs/Architecture.mmd)** – C4 System Context: Azure services, Managed Identity, all connections
- **[ApplicationFlow.mmd](docs/ApplicationFlow.mmd)** – User journey: Auth → Comedy loop → Leaderboard navigation
- **[DataModel.mmd](docs/DataModel.mmd)** – Entity Relationship Diagram: Joke, Performance Score, Leaderboard entities
- **[ComponentMap.mmd](docs/ComponentMap.mmd)** – Component tree: Frontend hierarchy + Backend service dependencies
- **[DataPipeline.mmd](docs/DataPipeline.mmd)** – CRUD workflow: Fetch → Predict → Rate → Store → Display

**Simplified Diagrams (High-level Overview):**
- [Architecture_SIMPLE.mmd](docs/Architecture_SIMPLE.mmd)
- [ApplicationFlow_SIMPLE.mmd](docs/ApplicationFlow_SIMPLE.mmd)
- [DataModel_SIMPLE.mmd](docs/DataModel_SIMPLE.mmd)
- [ComponentMap_SIMPLE.mmd](docs/ComponentMap_SIMPLE.mmd)
- [DataPipeline_SIMPLE.mmd](docs/DataPipeline_SIMPLE.mmd)

**View Diagrams:**
1. Copy `.mmd` file content
2. Paste into [Mermaid Live Editor](https://mermaid.live)
3. Or use VS Code [Markdown Preview Mermaid Support](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid)

### Visual Assets
- **Screenshots** – UI mockups of all pages in `/docs/screenshots/`
- **Improvement Suggestions** – [ImprovementSuggestions.md](docs/ImprovementSuggestions.md)

## 🏗️ System Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│ User Browser (WebSocket/HTTP)                               │
└─────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────▼──────────┐
                    │ Blazor Web App     │
                    │ (.NET 10)          │
                    │ • JesterStage      │
                    │ • Leaderboard      │
                    │ • Diagnostics      │
                    └─────────┬──────────┘
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   ┌────▼────┐      ┌──────▼───────┐    ┌─────▼────┐
   │ JokeAPI │      │ Azure OpenAI │    │  Storage │
   │ Public  │      │ (GPT-4)      │    │ (Tables) │
   │ REST    │      │ + Key Vault  │    │ + Azurite│
   └─────────┘      └──────────────┘    └──────────┘
```

**See [Architecture.mmd](docs/Architecture.mmd) for detailed C4 diagram**

## �📝 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/jokes/fetch` | GET | Fetch a random joke from JokeAPI |
| `/api/jokes/analyze` | POST | Get AI punchline prediction and ratings |
| `/api/leaderboard` | GET | Retrieve joke leaderboard |
| `/api/diagnostics` | GET | Health check status |
| `/health` | GET | Simple health probe |

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [JokeAPI](https://jokeapi.dev/) for the joke database
- [Azure OpenAI](https://azure.microsoft.com/products/ai-services/openai-service) for AI capabilities
- [Radzen Blazor](https://blazor.radzen.com/) for UI components
