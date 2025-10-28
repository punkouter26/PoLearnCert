# Implementation Plan: PoLearnCert Certification Quiz Platform

**Branch**: `001-cert-quiz-platform` | **Date**: 2025-10-26 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/001-cert-quiz-platform/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

PoLearnCert is an online learning platform for technical certification exam preparation through rapid-fire quizzes. The platform delivers 20-question quiz sessions with immediate feedback and explanations, supports AZ-900 and Security+ certifications initially, tracks personal performance statistics, and provides competitive leaderboards. The system uses Blazor WebAssembly for the frontend, .NET 9.0 Web API for the backend, Azure Table Storage for persistence, and integrates a separate C# console app with Azure OpenAI for question generation.

## Technical Context

**Language/Version**: .NET 9.0 (latest patch)  
**Primary Dependencies**: Blazor WebAssembly, ASP.NET Core Web API, ASP.NET Core Identity, Azure.Data.Tables SDK, Azure.AI.OpenAI SDK  
**Storage**: Azure Table Storage (Azurite for local development)  
**Testing**: xUnit for unit and integration tests; Playwright MCP with TypeScript for manual E2E tests  
**Target Platform**: Web application (browser-based client + hosted API)  
**Project Type**: Web application (Blazor WASM frontend + .NET API backend)  
**Performance Goals**: <1 second answer feedback, <10 minutes average quiz completion, <30 seconds leaderboard updates, 100+ concurrent users  
**Constraints**: Mobile-first responsive UI, sub-second API responses, RFC 7807 Problem Details error handling, ports 5000 (HTTP) and 5001 (HTTPS)  
**Scale/Scope**: 200+ questions per certification, support for 100+ concurrent users, stat tracking for unlimited quiz sessions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. .NET 9.0 Only (REQUIRED)
- ✅ **PASS**: Technical Context specifies .NET 9.0 latest patch
- Action: Create `global.json` with SDK version 9.x.x in Phase 1

### II. Vertical Slice Architecture (PREFERRED)
- ✅ **PASS**: Web application structure supports vertical slicing by user story
- Note: Will organize code by feature (Quiz, Statistics, Leaderboards, Certifications) rather than technical layers

### III. Test-First Development (REQUIRED)
- ✅ **PASS**: xUnit specified for unit and integration tests
- ✅ **PASS**: Playwright MCP TypeScript for manual E2E tests
- Action: Set up Po.LearnCert.UnitTests and Po.LearnCert.IntegrationTests projects in Phase 1
- Action: Configure Azurite for integration test isolation

### IV. API Observability & Error Handling (REQUIRED)
- ✅ **PASS**: Requirements include Swagger/OpenAPI exposure
- ✅ **PASS**: RFC 7807 Problem Details error handling specified in constraints
- Action: Implement `/api/health` endpoint (readiness + liveness) in Phase 1
- Action: Configure Serilog with structured logging and local sinks in Phase 1
- Action: Create global exception handling middleware for Problem Details in Phase 1

### V. Imperative Minimal Rules (INFORMATIONAL)
- ✅ **PASS**: Plan follows REQUIRED/PREFERRED/INFORMATIONAL classification

### VI. Mobile-First Responsive UI (REQUIRED)
- ✅ **PASS**: Mobile-first responsive UI specified in constraints
- Action: Use Blazor built-in components initially; justify any Radzen.Blazor adoption
- Action: Test main flows on desktop and mobile emulation in Phase 1

### VII. SOLID Principles & Simplicity (PREFERRED)
- ✅ **PASS**: Plan will follow SOLID principles
- Note: Keep components simple; justify complexity when introduced

### SDK & Ports (REQUIRED)
- ✅ **PASS**: .NET 9.0 SDK specified
- ✅ **PASS**: Ports 5000 (HTTP) and 5001 (HTTPS) in constraints
- Action: Configure launchSettings.json with required ports in Phase 1

### Project Naming (REQUIRED)
- ✅ **PASS**: Project prefix `Po.LearnCert.*` follows `Po.AppName.*` pattern
- Action: Create projects: Po.LearnCert.Api, Po.LearnCert.Client, Po.LearnCert.Shared

### Table Naming (REQUIRED)
- ✅ **PASS**: Will follow `PoLearnCert[TableName]` pattern
- Action: Document table names in data-model.md (e.g., PoLearnCertUsers, PoLearnCertQuestions)

### Data Persistence (REQUIRED)
- ✅ **PASS**: Azure Table Storage specified with Azurite for local dev
- Action: Configure Azurite connection strings in appsettings.Development.json

### Repository Layout (REQUIRED)
- ✅ **PASS**: Web application structure matches constitution requirements
- Action: Create directory structure in Phase 1

**Constitution Check Result**: ✅ **ALL GATES PASSED** - Proceeding to Phase 0 Research

## Project Structure

### Documentation (this feature)

```text
specs/001-cert-quiz-platform/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── api-spec.yaml    # OpenAPI specification
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
/src
  /Po.LearnCert.Api          # ASP.NET Core Web API backend
    /Features                # Vertical slices by feature
      /Quiz                  # Quiz session management
      /Statistics            # Performance tracking
      /Leaderboards          # Rankings and competition
      /Certifications        # Certification info pages
      /Authentication        # User auth and account management
    /Middleware              # Global exception handling, logging
    /Configuration           # Startup, DI, app settings
    Program.cs
    appsettings.json
    appsettings.Development.json
  
  /Po.LearnCert.Client       # Blazor WebAssembly frontend
    /Features                # Vertical slices matching backend
      /Quiz                  # Quiz UI components
      /Statistics            # Dashboard and charts
      /Leaderboards          # Leaderboard display
      /Certifications        # Certification browsing
      /Authentication        # Login/register UI
    /Shared                  # Shared UI components
    /Services                # HTTP clients, state management
    Program.cs
    wwwroot/
  
  /Po.LearnCert.Shared       # Shared DTOs, models, contracts
    /Models                  # Data transfer objects
    /Contracts               # Request/response contracts
    /Constants               # Shared constants

/tests
  /Po.LearnCert.UnitTests
    /Features                # Unit tests organized by feature
  
  /Po.LearnCert.IntegrationTests
    /Features                # Integration tests with Azurite
    TestFixture.cs           # Shared test infrastructure

/docs
  PRD.MD                     # Product requirements (from spec)
  STEPS.MD                   # Implementation steps
  README.MD                  # Project overview and setup

/.github
  /workflows
    ci.yml                   # CI pipeline (build, test, format check)

/tools
  /Po.LearnCert.QuestionGenerator  # C# console app for AI-powered question generation
    Program.cs                      # Azure OpenAI integration
    appsettings.json                # Azure OpenAI configuration
```

**Structure Decision**: Selected web application structure (Option 2) with Blazor WASM frontend and .NET API backend. Both are organized using vertical slice architecture with features aligned to user stories: Quiz (P1), Statistics (P2), Leaderboards (P3), and Certifications (P4). Authentication is a cross-cutting feature supporting all slices. The separation between Client and Api projects enables independent deployment if needed in the future, while Shared project prevents duplication of DTOs and contracts.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
