---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **PoJoker Structure**: `src/Po.Joker.Api/`, `src/Po.Joker.Client/`, `src/Po.Joker.Shared/`, `src/Po.Joker.Domain/`
- **Tests**: `tests/Po.Joker.Api.Tests/`, `tests/Po.Joker.Client.Tests/`, `tests/Po.Joker.Domain.Tests/`, `tests/Po.Joker.E2E.Tests/`
- **Vertical Slices**: Feature code in `src/Po.Joker.Api/Features/[FeatureName]/`
- Paths shown below assume PoJoker structure per Constitution II. Architecture

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  
  Tasks MUST follow TDD workflow (Constitution IV):
  1. Write tests FIRST (Red)
  2. Implement to pass tests (Green)
  3. Refactor while maintaining green tests
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure per Constitution I. Foundation

- [ ] T001 Create solution structure: `src/`, `tests/`, `docs/`, `infra/`, `scripts/`
- [ ] T002 Run `dotnet new blazor` for Server and Client projects (if no code exists)
- [ ] T003 [P] Create `global.json` locked to .NET 10.0.xxx SDK
- [ ] T004 [P] Create `Directory.Build.props` with latest C# features and `<Nullable>enable</Nullable>`
- [ ] T005 [P] Create `Directory.Packages.props` for centralized NuGet management
- [ ] T006 [P] Create `launch.json` for one-step F5 debug launch
- [ ] T007 Run `dotnet format` on solution to ensure consistency

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure per Constitution II & III that MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T008 Setup Vertical Slice Architecture folder structure in `src/Po.Joker.Api/Features/`
- [ ] T009 [P] Implement global `IExceptionHandler` for RFC 7807 Problem Details
- [ ] T010 [P] Configure Serilog structured logging
- [ ] T011 [P] Setup OpenTelemetry with `ActivitySource` and `Meter`
- [ ] T012 [P] Configure Polly resilience pipelines (Retry, Circuit Breaker, Timeout)
- [ ] T013 Setup Secret Manager for local development (`dotnet user-secrets`)
- [ ] T014 [P] Configure Azure Key Vault integration (Production environment only)
- [ ] T015 [P] Setup Azurite for local storage development
- [ ] T016 Enable Swagger/OpenAPI generation
- [ ] T017 [P] Create DIAG Razor page with .NET Health Checks for external service verification
- [ ] T018 Create `Po.Joker.Shared` project with DTOs/contracts structure

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (TDD - Write FIRST, must FAIL) 🔴

> **Constitution IV: Red → Green → Refactor workflow is MANDATORY**

- [ ] T019 [P] [US1] xUnit unit test for [Handler] in `tests/Po.Joker.Domain.Tests/`
- [ ] T020 [P] [US1] xUnit integration test (happy path) in `tests/Po.Joker.Api.Tests/`
- [ ] T021 [P] [US1] bUnit component test in `tests/Po.Joker.Client.Tests/`

### Implementation for User Story 1 (Make tests GREEN) 🟢

- [ ] T022 [P] [US1] Create DTOs in `src/Po.Joker.Shared/DTOs/`
- [ ] T023 [P] [US1] Create Domain entity in `src/Po.Joker.Domain/Entities/`
- [ ] T024 [US1] Implement Command handler in `src/Po.Joker.Api/Features/[Feature]/`
- [ ] T025 [US1] Implement Query handler (AsNoTracking) in `src/Po.Joker.Api/Features/[Feature]/`
- [ ] T026 [US1] Create Minimal API endpoint in `src/Po.Joker.Api/Features/[Feature]/`
- [ ] T027 [US1] Create Blazor component with CSS Isolation in `src/Po.Joker.Client/Components/`
- [ ] T028 [US1] Create `.http` file for endpoint verification
- [ ] T029 [US1] Verify 80% code coverage threshold met

**Checkpoint**: User Story 1 fully functional and tested independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (TDD - Write FIRST, must FAIL) 🔴

- [ ] T030 [P] [US2] xUnit unit test for [Handler] in `tests/Po.Joker.Domain.Tests/`
- [ ] T031 [P] [US2] xUnit integration test (happy path) in `tests/Po.Joker.Api.Tests/`
- [ ] T032 [P] [US2] bUnit component test in `tests/Po.Joker.Client.Tests/`

### Implementation for User Story 2 (Make tests GREEN) 🟢

- [ ] T033 [P] [US2] Create DTOs in `src/Po.Joker.Shared/DTOs/`
- [ ] T034 [US2] Implement Command/Query handlers in `src/Po.Joker.Api/Features/[Feature]/`
- [ ] T035 [US2] Create Minimal API endpoint with Swagger docs
- [ ] T036 [US2] Create Blazor component (mobile-first, CSS Isolation)
- [ ] T037 [US2] Create `.http` file and verify 80% coverage

**Checkpoint**: User Stories 1 AND 2 both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (TDD - Write FIRST, must FAIL) 🔴

- [ ] T038 [P] [US3] xUnit unit test in `tests/Po.Joker.Domain.Tests/`
- [ ] T039 [P] [US3] xUnit integration test in `tests/Po.Joker.Api.Tests/`
- [ ] T040 [P] [US3] bUnit component test in `tests/Po.Joker.Client.Tests/`

### Implementation for User Story 3 (Make tests GREEN) 🟢

- [ ] T041 [P] [US3] Create DTOs in `src/Po.Joker.Shared/DTOs/`
- [ ] T042 [US3] Implement handlers in `src/Po.Joker.Api/Features/[Feature]/`
- [ ] T043 [US3] Create endpoint and Blazor component
- [ ] T044 [US3] Verify 80% coverage threshold

**Checkpoint**: All user stories independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N-1: E2E Testing (Playwright)

**Purpose**: Full-stack validation per Constitution IV. Quality & Testing

- [ ] TXXX [P] Playwright E2E test for User Story 1 (Chromium desktop)
- [ ] TXXX [P] Playwright E2E test for User Story 1 (Chromium mobile)
- [ ] TXXX [P] Playwright E2E test for User Story 2 (Chromium desktop + mobile)
- [ ] TXXX [P] Playwright E2E test for User Story 3 (Chromium desktop + mobile)
- [ ] TXXX Integrate automated accessibility checks
- [ ] TXXX Integrate visual regression checks
- [ ] TXXX Generate combined coverage report in `docs/coverage/`

---

## Phase N: Polish, Infrastructure & Azure Deployment

**Purpose**: Operations and cross-cutting concerns per Constitution V

### Azure Infrastructure (Bicep + azd)

- [ ] TXXX Create `infra/main.bicep` with App Service, Storage, App Insights, Log Analytics
- [ ] TXXX [P] Create `infra/modules/` for reusable Bicep modules
- [ ] TXXX Configure $5 monthly budget with 80% alert to punkouter26@gmail.com
- [ ] TXXX Enable App Insights Snapshot Debugger and Profiler

### CI/CD (GitHub Actions)

- [ ] TXXX Create `.github/workflows/deploy.yml` with OIDC authentication
- [ ] TXXX Configure build and deploy to `PoJoker-rg` App Service
- [ ] TXXX Setup Federated Credentials for secret-less Azure connection

### Documentation & Polish

- [ ] TXXX [P] Populate `docs/kql/` with essential monitoring queries
- [ ] TXXX [P] Update README.md with setup and deployment instructions
- [ ] TXXX Code cleanup and refactoring (maintain green tests)
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX Security hardening review
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **E2E Testing (Phase N-1)**: Depends on user stories being complete
- **Infrastructure & Deployment (Phase N)**: Can run in parallel with E2E testing

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story (TDD Workflow - Constitution IV)

- Tests MUST be written FIRST and FAIL (Red)
- Implementation makes tests pass (Green)
- Refactor while maintaining green tests
- DTOs before handlers
- Handlers before endpoints
- Endpoints before UI components
- Verify 80% coverage before checkpoint

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in tests/contract/test_[name].py"
Task: "Integration test for [user journey] in tests/integration/test_[name].py"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].py"
Task: "Create [Entity2] model in src/models/[entity2].py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
