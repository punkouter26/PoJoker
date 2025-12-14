<!--
SYNC IMPACT REPORT
==================
Version Change: 0.0.0 → 1.0.0 (MAJOR - Initial constitution ratification)

Modified Principles: N/A (Initial creation)

Added Sections:
  - I. Foundation (naming, .NET 10, packages, null safety)
  - II. Architecture (VSA, Blazor, CQRS, repository structure)
  - III. Implementation - Backend (Minimal APIs, exception handling, resilience, Swagger)
  - III. Implementation - Frontend (CSS Isolation, responsive, Radzen constraints)
  - III. Implementation - Dev Environment (secrets, Azurite, debug launch)
  - IV. Quality & Testing (TDD, coverage, xUnit, bUnit, Playwright)
  - V. Operations & Azure (Bicep, azd, OIDC, cost management, telemetry)

Removed Sections: N/A (Initial creation)

Templates Requiring Updates:
  - .specify/templates/plan-template.md: ✅ updated (Technical Context for .NET 10/Blazor, Constitution Check table, Project Structure)
  - .specify/templates/spec-template.md: ✅ compatible (no changes required)
  - .specify/templates/tasks-template.md: ✅ updated (TDD workflow, path conventions, E2E phase, Azure infra phase)

Follow-up TODOs: None
-->

# PoJoker Constitution

## Core Principles

### I. Foundation

**Solution Naming**
- The `.sln` file name (e.g., `PoJoker`) is the base identifier
- MUST be used as the name for all Azure services/resource groups (e.g., `PoJoker-rg`)
- MUST be used as the user-facing HTML `<title>`
- Bookmarked items MUST use the short name pattern `Po****` (e.g., `PoJoker`)

**.NET Version**
- All projects MUST target .NET 10
- `global.json` MUST be locked to a `10.0.xxx` SDK version
- Use latest C# features via `Directory.Build.props`
- If no code exists, use `dotnet new blazor` to create Blazor and API templates

**Package Management**
- All NuGet packages MUST be managed centrally in a `Directory.Packages.props` file at repository root
- Run `dotnet format` on the `.sln` to ensure consistency

**Null Safety**
- Nullable Reference Types (`<Nullable>enable</Nullable>`) MUST be enabled in all `.csproj` files

### II. Architecture

**Code Organization**
- The API MUST use Vertical Slice Architecture (VSA)
- All API logic (endpoints, CQRS handlers) MUST be co-located by feature in `/src/Po.[AppName].Api/Features/`

**Blazor Architecture**
- Use the Blazor Web App template with Interactive Auto render mode
- The solution MUST contain:
  - A **Server** project (for API and SSR)
  - A **Client** project (for WASM components)
- Logic MUST be shared via interfaces to allow components to fetch data directly from the DB during SSR and via API during WASM execution

**CQRS Strategy**
- Strictly separate Reads (Queries) from Writes (Commands)
- **Commands**: Use standard Domain logic/Repositories
- **Queries**: MUST bypass the Domain Model and project directly to DTOs (using EF `AsNoTracking()`) for performance

**Repository Structure**
- Adhere to the standard root folder structure: `/src`, `/tests`, `/docs`, `/infra`, `/scripts`
- The `...Shared` project MUST only contain DTOs, contracts, and shared validation logic
- The `...Shared` project MUST NOT contain business logic or data access code

**Documentation (ADRs)**
- Any architectural change affecting more than one feature OR introducing a new dependency MUST be documented as an Architecture Decision Record (ADR) in `/docs/adrs/`

**Helper Scripts**
- `/scripts` contains helper scripts created by the coding LLM

### III. Implementation

#### Backend

**API Design**
- Use Minimal APIs for all new endpoints

**Global Exception Handling**
- Do NOT use try/catch blocks in every endpoint
- Implement a global `IExceptionHandler` (or Middleware) to:
  - Catch unhandled exceptions
  - Log them
  - Convert them to RFC 7807 Problem Details JSON objects centrally

**Resilience**
- Implement Polly resilience pipelines for all outgoing HTTP calls and Database connections
- Standard policies MUST include: Retry (with jitter), Circuit Breaker, and Timeout

**API Documentation**
- Swagger (OpenAPI) generation MUST be enabled
- `.http` files MUST be maintained for manual verification

#### Frontend (Blazor)

**Styling Strategy**
- Use CSS Isolation for component-specific styles
- For global layout and utility, use a utility-first approach (e.g., Tailwind or strict Bootstrap utility classes)
- Avoid "magic number" CSS

**Responsive Design**
- The UI MUST be mobile-first (portrait mode), responsive, fluid, and touch-friendly

**UI Framework**
- Use standard Blazor controls
- Radzen.Blazor MAY only be used for complex requirements (e.g., Grids, Charts)

#### Development Environment

**Secret Management**
- **Local**: Secrets MUST be stored using .NET Secret Manager (`dotnet user-secrets`)
- `appsettings.json` MUST only contain non-sensitive configuration
- **Production**: `Program.cs` MUST be configured to read from Azure Key Vault only when `ASPNETCORE_ENVIRONMENT` is `'Production'`

**Local Storage**
- Use Azurite for local development and integration testing

**Debug Launch**
- Commit a `launch.json` to support one-step 'F5' debug launch for the API and browser

### IV. Quality & Testing

**Workflow**
- Apply a TDD workflow (Red → Green → Refactor) for all business logic
- UI and E2E tests MUST be written contemporaneously with feature code

**Code Coverage**
- Enforce a minimum 80% line coverage threshold for all new business logic
- A combined coverage report MUST be generated in `docs/coverage/`

**Performance**
- Do NOT cap the number of tests
- Enforce execution time limits (e.g., "Unit tests MUST run in under 20ms")

**Unit Tests (xUnit)**
- MUST cover all backend business logic (e.g., MediatR handlers)
- All external dependencies MUST be mocked

**Component Tests (bUnit)**
- MUST cover all new Blazor components (rendering, interactions)
- Mock dependencies like `IHttpClientFactory`

**Integration Tests (xUnit)**
- A "happy path" test MUST be created for every new API endpoint against an in-memory database emulator

**Diagnostics Page**
- MUST create a DIAG Razor page that can verify connections to all external services
- MUST verify access to all Key Vault values is working
- Use .NET Health Checks

**E2E Tests (Playwright)**
- Tests MUST target Chromium (mobile and desktop views)
- **Full-Stack E2E (Default)**: Runs the entire stack to validate true user flow
- Integrate automated accessibility and visual regression checks

### V. Operations & Azure

**Provisioning**
- All Azure infrastructure MUST be provisioned using Bicep (from `/infra`)
- Deploy via Azure Developer CLI (`azd`)

**Security**
- The GitHub Actions workflow MUST use Federated Credentials (OIDC) for secure, secret-less connection to Azure

**CI/CD**
- The YML file MUST build the code and deploy it to the resource group (e.g., `PoJoker-rg`) as an App Service

**Required Services**
- Bicep MUST provision in the same resource group:
  - App Service
  - Azure Storage
  - Application Insights
  - Log Analytics

**Cost Management**
- A $5 monthly cost budget MUST be created
- An Action Group MUST email `punkouter26@gmail.com` when 80% of the threshold is met

**Logging & Telemetry**
- Use Serilog for structured logging
- Use modern OpenTelemetry (`ActivitySource`, `Meter`) for all custom telemetry and metrics
- Enable Application Insights Snapshot Debugger and Profiler on the App Service

**KQL Library**
- Populate `/docs/kql/` with essential monitoring queries

## Governance

**Constitution Authority**
- This constitution supersedes all other development practices
- All PRs/reviews MUST verify compliance with these principles

**Amendment Procedure**
- Amendments require:
  1. Documentation of the proposed change
  2. Approval from project owner
  3. Migration plan for affected code/infrastructure
- All amendments MUST be reflected in the Sync Impact Report

**Versioning Policy**
- Constitution versions follow semantic versioning:
  - **MAJOR**: Backward-incompatible governance/principle removals or redefinitions
  - **MINOR**: New principle/section added or materially expanded guidance
  - **PATCH**: Clarifications, wording, typo fixes, non-semantic refinements

**Compliance Review**
- Complexity MUST be justified against the Simplicity principle
- New dependencies MUST be documented in `/docs/adrs/`
- All deviations from principles require explicit justification in PR description

**Version**: 1.0.0 | **Ratified**: 2025-12-12 | **Last Amended**: 2025-12-12
