# Po.Joker - The Digital Jester 🃏

A passive AI comedy application where an AI Jester fetches jokes, attempts to predict punchlines, and provides entertaining analysis—all without user input.

![.NET 10](https://img.shields.io/badge/.NET-10.0%20Preview-purple)
![Blazor](https://img.shields.io/badge/Blazor-Web%20App-blue)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4)
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

1. **Configure OIDC authentication**
   - Create an Azure AD App Registration
   - Add Federated Credentials for your GitHub repository
   - Set repository secrets:
     - `AZURE_CLIENT_ID`
     - `AZURE_TENANT_ID`
     - `AZURE_SUBSCRIPTION_ID`
     - `OPENAI_API_KEY`

2. **Deploy via workflow**
   - Push to `main` branch for automatic deployment
   - Use manual workflow dispatch for specific environments

### Manual Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create --name rg-pojoker-dev --location eastus2

# Deploy infrastructure
az deployment group create \
  --resource-group rg-pojoker-dev \
  --template-file infra/main.bicep \
  --parameters environment=dev

# Publish and deploy app
dotnet publish src/Po.Joker -c Release -o ./publish
az webapp deploy --resource-group rg-pojoker-dev \
   --name PoJoker \
  --src-path ./publish
```

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

## 📝 API Endpoints

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
