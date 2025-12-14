# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 10 (locked in global.json)  
**Primary Dependencies**: Blazor Web App (Interactive Auto), MediatR, Polly, Serilog  
**Storage**: Azure Storage (Azurite for local dev), Entity Framework Core  
**Testing**: xUnit (Unit/Integration), bUnit (Components), Playwright (E2E)  
**Target Platform**: Azure App Service, Web (Chromium mobile/desktop)
**Project Type**: web (Server + Client projects)  
**Performance Goals**: Unit tests < 20ms, API response p95 < 200ms  
**Constraints**: 80% code coverage minimum, mobile-first responsive UI  
**Scale/Scope**: Single resource group deployment, $5/month budget cap

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status |
|-----------|-------------|--------|
| I. Foundation | .NET 10, Directory.Packages.props, Nullable enabled | ⬜ |
| II. Architecture | VSA in /src/Po.[AppName].Api/Features/, Shared project DTOs only | ⬜ |
| III. Backend | Minimal APIs, global IExceptionHandler, Polly pipelines | ⬜ |
| III. Frontend | CSS Isolation, mobile-first, Radzen only for complex UI | ⬜ |
| III. Dev Env | Secret Manager (local), Key Vault (prod), Azurite, launch.json | ⬜ |
| IV. Testing | TDD workflow, 80% coverage, xUnit/bUnit/Playwright, DIAG page | ⬜ |
| V. Operations | Bicep in /infra, azd deploy, OIDC auth, $5 budget alert | ⬜ |

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# PoJoker Blazor Web App Structure (per Constitution II. Architecture)
src/
├── Po.Joker.Api/                    # Server project (API + SSR)
│   ├── Features/                    # Vertical Slice Architecture
│   │   ├── [FeatureName]/
│   │   │   ├── [FeatureName]Endpoint.cs
│   │   │   ├── [FeatureName]Command.cs
│   │   │   └── [FeatureName]Query.cs
│   │   └── ...
│   ├── Infrastructure/
│   │   ├── ExceptionHandling/       # Global IExceptionHandler
│   │   ├── Resilience/              # Polly pipelines
│   │   └── Telemetry/               # OpenTelemetry, Serilog
│   └── Program.cs
├── Po.Joker.Client/                 # Client project (WASM components)
│   ├── Components/
│   │   └── Pages/
│   └── wwwroot/
├── Po.Joker.Shared/                 # DTOs, contracts, validation only
│   ├── DTOs/
│   ├── Contracts/
│   └── Validation/
└── Po.Joker.Domain/                 # Domain models (Commands only)
    ├── Entities/
    └── Repositories/

tests/
├── Po.Joker.Api.Tests/              # xUnit integration tests
├── Po.Joker.Client.Tests/           # bUnit component tests
├── Po.Joker.Domain.Tests/           # xUnit unit tests
└── Po.Joker.E2E.Tests/              # Playwright E2E tests

docs/
├── adrs/                            # Architecture Decision Records
├── coverage/                        # Combined coverage reports
└── kql/                             # Azure monitoring queries

infra/                               # Bicep IaC files
├── main.bicep
├── modules/
└── parameters/

scripts/                             # LLM-created helper scripts
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
