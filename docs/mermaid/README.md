# PoJoker Architecture Diagrams

This folder contains C4 model diagrams and user workflow documentation for the PoJoker application, in both detailed and simplified versions.

## Full Detail Diagrams (Recommended for Development & Review)

### C4_SYSTEM.mmd / C4_SYSTEM.svg
- **Purpose**: System context diagram showing PoJoker and its external dependencies
- **Shows**: User interactions, system boundary, external APIs (OpenAI, JokeAPI, Azure Table Storage)
- **Best for**: Understanding high-level system scope and external integrations
- **Audience**: All team members, stakeholders, architecture reviews

### C4_CONTAINER.mmd / C4_CONTAINER.svg
- **Purpose**: Container diagram showing major deployable/runtime components
- **Shows**: Blazor WASM client (Po.Joker.Client), ASP.NET Server (Po.Joker), Shared libraries, external systems
- **Best for**: Understanding deployment architecture and component communication
- **Audience**: Architects, DevOps, senior developers

### C4_COMPONENT.mmd / C4_COMPONENT.svg
- **Purpose**: Component diagram detailing internal architecture of major containers
- **Shows**: 
  - Client: JesterStage, PerformanceOrchestrator, PerformanceDisplay, API clients
  - Server: Feature handlers, MediatR commands, validators, AI/Joke services
  - External integrations
- **Best for**: In-depth code navigation, understanding feature flow, refactoring planning
- **Audience**: Core development team

### WORKFLOW_USER.mmd / WORKFLOW_USER.svg
- **Purpose**: User workflow sequence diagram showing the complete performance flow
- **Shows**: 13-step sequence from joke fetch through AI analysis, answer submission, result comparison, and win celebration
- **Best for**: Understanding feature requirements, UX flow documentation, testing scenarios
- **Audience**: QA, product team, developers working on performance features

## Simplified Diagrams (Recommended for Onboarding & Presentations)

### SIMPLE_C4_SYSTEM.mmd / SIMPLE_C4_SYSTEM.svg
- **Simplification**: 3 major elements (User → PoJoker → External APIs)
- **Use when**: Explaining PoJoker to non-technical stakeholders, presentations, quick overview docs

### SIMPLE_C4_CONTAINER.mmd / SIMPLE_C4_CONTAINER.svg
- **Simplification**: 3-4 major blocks (Client, Server, Shared, Externals) with minimal internal detail
- **Use when**: Onboarding new team members, deployment documentation, DevOps runbooks

### SIMPLE_C4_COMPONENT.mmd / SIMPLE_C4_COMPONENT.svg
- **Simplification**: 5-6 key architectural layers without granular services
- **Use when**: Training new developers, documentation for junior engineers, architectural briefings

### SIMPLE_WORKFLOW_USER.mmd / SIMPLE_WORKFLOW_USER.svg
- **Simplification**: 5-6 major steps consolidated from 13-step full flow
- **Use when**: User stories, requirement documents, high-level feature planning

## File Format Reference

| File Type | Extension | Use Case | Tools |
|-----------|-----------|----------|-------|
| Source Diagram | `.mmd` | Editing, version control, CI/CD integration | Any text editor, Mermaid Live Editor |
| Rendered Image | `.svg` | Documentation, presentations, wikis | Web browser, all document viewers |

## Editing & Updating Diagrams

1. **Edit Source**: Modify `.mmd` files in any text editor
2. **Preview**: Paste content into [Mermaid Live Editor](https://mermaid.live)
3. **Regenerate SVG**: From the docs/mermaid folder, run:
   ```powershell
   npx mmdc -i filename.mmd -o filename.svg
   ```
   Or install mermaid-cli globally:
   ```powershell
   npm install -g @mermaid-js/mermaid-cli
   mmdc -i C4_SYSTEM.mmd -o C4_SYSTEM.svg
   ```

## Architecture Reference

### Technology Stack
- **Frontend**: Blazor WebAssembly (C#/.NET)
- **Backend**: ASP.NET Core with MediatR (CQRS pattern)
- **External Services**: Azure OpenAI, JokeAPI v2, Azure Table Storage
- **Pattern**: Vertical slice architecture (Features folder organization)

### Key Components
- **JesterStage**: Main Blazor container component (client-side UI orchestration)
- **PerformanceOrchestrator**: State machine managing performance flow logic
- **MediatR Handlers**: CQRS pattern for server-side business logic
- **AI Service**: Azure OpenAI GPT-4o-mini integration for punchline prediction

## When to Use Which Diagram

| Scenario | Recommended Diagram |
|----------|-------------------|
| New developer onboarding | SIMPLE_C4_SYSTEM → SIMPLE_C4_CONTAINER |
| Understanding feature flow | WORKFLOW_USER (full) |
| Code refactoring planning | C4_COMPONENT (full) |
| System architecture review | C4_SYSTEM + C4_CONTAINER |
| Deployment documentation | SIMPLE_C4_CONTAINER |
| Feature requirements | SIMPLE_WORKFLOW_USER |
| Integration troubleshooting | C4_CONTAINER |

---

**Generated**: December 14, 2025  
**Tool**: Mermaid C4 Model  
**Repository**: PoJoker / Po.Joker
