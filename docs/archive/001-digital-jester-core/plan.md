# Implementation Plan: Digital Jester Core - AI Punchline Guessing Loop

**Branch**: `001-digital-jester-core` | **Date**: 2025-12-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-digital-jester-core/spec.md`

## Summary

Build a passive, autonomous comedy application where an AI Jester fetches jokes from an external API, attempts to guess punchlines using Azure OpenAI (gpt-35-turbo), and presents the performance with Medieval Dark theming and browser-native TTS. The solution uses Blazor Web App (Interactive Auto) with Vertical Slice Architecture, Azure Table Storage for device-local leaderboard persistence, and strict $5/month budget compliance via Free/Basic tier Azure services.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (Preview, locked in global.json)  
**Primary Dependencies**: Blazor Web App (Interactive Auto), MediatR, Polly, Serilog, Azure.AI.OpenAI SDK  
**Storage**: Azure Table Storage (partitioned by Category), Browser localStorage (device-local), Azurite for local dev  
**AI Service**: Azure AI Foundry with gpt-35-turbo deployment (cost-optimized for token efficiency)  
**External APIs**: JokeAPI (https://v2.jokeapi.dev/) for two-part jokes with category filtering  
**Audio**: Web Speech API (TTS), Web Audio API (AudioContext for programmatic drum roll/trombone)  
**Testing**: xUnit (Unit/Integration), bUnit (Components), Playwright (E2E on Chromium)  
**Target Platform**: Azure App Service (Linux, F1/B1 tier), Web (Chromium mobile/desktop)  
**Project Type**: web (Server + Client projects per Blazor Web App template)  
**Performance Goals**: Unit tests < 20ms, joke cycle < 60s, AI timeout 15s, API p95 < 200ms  
**Constraints**: 80% code coverage, mobile-first responsive, $5/month Azure budget, no authentication  
**Scale/Scope**: Single-user device-local, up to 1,000 jokes in leaderboard, single resource group

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status |
|-----------|-------------|--------|
| I. Foundation | .NET 10, Directory.Packages.props, Nullable enabled | ✅ Compliant |
| II. Architecture | VSA in /src/Po.Joker/Features/, Shared project DTOs only | ✅ Compliant |
| III. Backend | Minimal APIs, global IExceptionHandler (RFC 7807), Polly pipelines | ✅ Compliant |
| III. Frontend | CSS Isolation (Medieval Dark), mobile-first, Radzen for charts only | ✅ Compliant |
| III. Dev Env | Secret Manager (local), Key Vault (prod), Azurite, launch.json | ✅ Compliant |
| IV. Testing | TDD workflow, 80% coverage, xUnit/bUnit/Playwright, DIAG page | ✅ Compliant |
| V. Operations | Bicep in /infra, azd deploy, OIDC auth, $5 budget alert | ✅ Compliant |

**Gate Status**: ✅ PASS - All checks passed. Design phase complete.

### Post-Design Re-evaluation (Phase 1)

| Check | Decision | Justification |
|-------|----------|---------------|
| No EF Core/Domain project | ✅ Justified | Table Storage + DTOs sufficient for device-local CRUD; no complex domain logic |
| Radzen.Blazor dependency | ✅ Justified | Only for FR-016 rating charts; standard Blazor lacks charting |
| No authentication | ✅ Justified | Per clarification: device-local, personal leaderboards only |
| Azure Table Storage over SQL | ✅ Justified | ADR-001: $0.04/GB vs $5+/month SQL; budget constraint |

## Project Structure

### Documentation (this feature)

```text
specs/001-digital-jester-core/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI specs)
│   └── jokes-api.yaml
└── checklists/
    └── requirements.md  # Spec quality validation
```

### Source Code (repository root)

```text
src/
├── Po.Joker/                        # Server project (Blazor Web App host + API)
│   ├── Features/                    # Vertical Slice Architecture
│   │   ├── Jokes/
│   │   │   ├── FetchJokeEndpoint.cs
│   │   │   ├── FetchJokeQuery.cs
│   │   │   └── JokeApiClient.cs
│   │   ├── Analysis/
│   │   │   ├── AnalyzeJokeEndpoint.cs
│   │   │   ├── AnalyzeJokeCommand.cs
│   │   │   └── AiJesterService.cs
│   │   ├── Leaderboards/
│   │   │   ├── GetLeaderboardEndpoint.cs
│   │   │   └── GetLeaderboardQuery.cs
│   │   └── Diagnostics/
│   │       └── DiagnosticsEndpoint.cs
│   ├── Infrastructure/
│   │   ├── ExceptionHandling/
│   │   │   └── JesterExceptionHandler.cs
│   │   ├── Resilience/
│   │   │   └── PollyPipelines.cs
│   │   ├── Telemetry/
│   │   │   └── OpenTelemetryConfig.cs
│   │   └── Storage/
│   │       └── TableStorageClient.cs
│   ├── Components/
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── MainLayout.razor.css
│   │   └── Pages/
│   │       ├── Home.razor
│   │       ├── Leaderboard.razor
│   │       └── Diag.razor
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── medieval-dark.css
│   │   └── audio/
│   │       └── (generated via Web Audio API)
│   └── Program.cs
├── Po.Joker.Client/                 # Client project (WASM interactive components)
│   ├── Components/
│   │   ├── JesterStage.razor
│   │   ├── JesterStage.razor.css
│   │   ├── JokeCard.razor
│   │   ├── RatingChart.razor
│   │   └── SafeModeToggle.razor
│   ├── Services/
│   │   ├── SpeechService.cs         # Web Speech API interop
│   │   └── AudioService.cs          # Web Audio API interop
│   ├── wwwroot/
│   │   └── js/
│   │       ├── speech-interop.js
│   │       └── audio-interop.js
│   └── _Imports.razor
└── Po.Joker.Shared/                 # DTOs, contracts, validation only
    ├── DTOs/
    │   ├── JokeDto.cs
    │   ├── JokePerformanceDto.cs
    │   ├── JokeRatingDto.cs
    │   └── LeaderboardEntryDto.cs
    ├── Contracts/
    │   ├── IAnalysisService.cs
    │   └── IJokeService.cs
    └── Validation/
        └── JokeValidator.cs

tests/
├── Po.Joker.Tests/                  # xUnit integration tests
│   ├── Features/
│   │   ├── FetchJokeTests.cs
│   │   ├── AnalyzeJokeTests.cs
│   │   └── LeaderboardTests.cs
│   └── Infrastructure/
│       └── ExceptionHandlerTests.cs
├── Po.Joker.Client.Tests/           # bUnit component tests
│   ├── JesterStageTests.cs
│   ├── JokeCardTests.cs
│   └── RatingChartTests.cs
└── Po.Joker.E2E.Tests/              # Playwright E2E tests
    ├── PassiveLoopTests.cs
    └── LeaderboardTests.cs

docs/
├── adrs/
│   └── 001-table-storage-over-sql.md
├── coverage/
└── kql/
    ├── error-rates.kql
    └── ai-latency.kql

infra/
├── main.bicep
├── modules/
│   ├── appservice.bicep
│   ├── storage.bicep
│   ├── ai-foundry.bicep
│   ├── appinsights.bicep
│   └── budget.bicep
└── main.bicepparam

scripts/
└── setup-local-secrets.ps1
```

**Structure Decision**: Using Blazor Web App with combined Server hosting (API + SSR) and separate Client project for WASM components. This follows Constitution II while supporting Interactive Auto render mode. Azure Table Storage replaces SQL to stay within $5/month budget—documented in ADR-001.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| No Domain project | Single-user app with simple CRUD; no complex domain logic | Domain layer would add unnecessary abstraction for direct Table Storage ops |
| Table Storage over EF Core | $5/month budget constraint; Table Storage is ~$0.04/GB vs SQL ~$5/month minimum | EF Core + SQL exceeds budget; Table Storage provides sufficient query capability for leaderboards |
| Radzen.Blazor for charts | FR-016 requires visual score charts; standard Blazor has no charting | CSS-only charts insufficient for bar/radar visualizations |

---

## Phase 0: Research

### External Dependencies

#### JokeAPI (https://v2.jokeapi.dev/)
- **Format**: Two-part jokes with `setup` and `delivery` fields
- **Categories**: Programming, Misc, Dark, Pun, Spooky, Christmas
- **Safe Mode**: `?safe-mode` query parameter filters Dark/racist/sexist jokes
- **Rate Limits**: 120 requests/minute (free tier)
- **Response Example**:
  ```json
  {
    "type": "twopart",
    "setup": "Why do programmers prefer dark mode?",
    "delivery": "Because light attracts bugs.",
    "category": "Programming",
    "id": 232
  }
  ```

#### Azure OpenAI (gpt-35-turbo via Azure AI Foundry)
- **Endpoint**: `https://{resource}.openai.azure.com/openai/deployments/{deployment}/chat/completions`
- **SDK**: `Azure.AI.OpenAI` NuGet package
- **Authentication**: Managed Identity (User Assigned) for secret-less access
- **Token Cost**: ~$0.0015/1K input tokens, ~$0.002/1K output tokens
- **Timeout Strategy**: 15-second hard timeout via Polly; treat timeout as "stumped"

#### Web Speech API (Browser-native TTS)
- **API**: `window.speechSynthesis.speak(utterance)`
- **Voice Selection**: Filter for `lang === 'en-GB'` and `name.includes('Male')`
- **Fallback**: If no British male voice, use default English voice
- **Interop**: JS function called via Blazor `IJSRuntime`

#### Web Audio API (Programmatic sound effects)
- **Drum Roll**: OscillatorNode with frequency sweep + noise
- **Trombone Slide**: OscillatorNode with descending frequency modulation
- **No external audio files required**

### Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Storage | Azure Table Storage | Free tier adequate; partitioned by Category for O(1) leaderboard queries |
| AI Model | gpt-35-turbo | Cost-effective (~$0.002/1K tokens) vs gpt-4; sufficient reasoning for punchline prediction |
| TTS | Web Speech API | Zero cost; offline-capable; no API key management |
| Audio Effects | Web Audio API | Programmatic generation; no asset hosting costs |
| Charts | Radzen.Blazor | Only for P3 rating charts; avoids custom SVG complexity |
| Auth | None | Device-local data; no user accounts per clarification |

### Risk Mitigations

| Risk | Mitigation |
|------|------------|
| JokeAPI downtime | Polly retry with jitter (3 attempts, exponential backoff); thematic "Royal Scroll missing" error |
| AI timeout/failure | 15s hard timeout; "stumped" state continues loop; circuit breaker after 5 failures |
| TTS voice unavailable | Graceful fallback to default voice; log warning |
| Table Storage cold start | Pre-warm on app startup; connection pooling |
