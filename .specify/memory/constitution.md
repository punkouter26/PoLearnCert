<!--
SYNC IMPACT REPORT
==================
Version Change: 1.0.0 → 1.1.0
Date: 2025-11-03

Modified Principles:
- I. .NET 9.0 Only - Updated to specify global.json pinning to 9.0.xxx
- II. Environment & Setup - NEW: Comprehensive development environment rules
- III. Solution Structure - EXPANDED: Detailed folder organization and naming
- IV. Vertical Slice Architecture - Clarified with tooling specifics (MediatR, Minimal APIs)
- V. Backend (API) Rules - EXPANDED: API docs, health checks, logging standards
- VI. Frontend (Client) Rules - NEW: Mobile-first UX, Fluent UI components
- VII. Testing Strategy - EXPANDED: TDD workflow, test isolation, E2E requirements

Added Sections:
- Environment & Setup (Principle II)
- Solution Structure (Principle III)
- Detailed testing requirements with viewport specifications
- Secret management (local user secrets, Azure Key Vault)
- CLI preference guidance

Removed Sections:
- Generic "Imperative Minimal Rules" (merged into other principles)

Templates Requiring Updates:
✅ plan-template.md - Updated to reflect new principles
✅ spec-template.md - Aligned with expanded testing requirements
✅ tasks-template.md - Updated for vertical slice organization
⚠ AGENTS.md - Created as comprehensive guide (docs/AGENTS.md)

Follow-up TODOs:
- Validate template files align with new constitution structure
- Ensure CI/CD scripts enforce .NET 9 SDK check from global.json
-->

# PoLearnCert Constitution

## Core Principles

### I. .NET 9.0 SDK Only (REQUIRED)

**Rule**: All projects MUST use .NET 9.0 SDK. The `global.json` file MUST be pinned to version 9.0.xxx (e.g., 9.0.100, 9.0.306). Builds MUST fail if a different SDK major version is detected.

**Rationale**: Enforcing a single SDK version eliminates version drift, ensures consistent tooling and runtime behavior across all development environments, and prevents compatibility issues. The `global.json` pinning ensures all developers and CI/CD systems use compatible SDK versions.

**Enforcement**: 
- Automated CI check validates `global.json` SDK version is 9.x.x
- Build fails on SDK version mismatch
- Local development verification via `dotnet --version`

### II. Environment & Setup (REQUIRED)

**Storage Rule**: Default to Azure Table Storage for all data persistence. For local development, MUST use Azurite.

**Local Secrets Rule**: All sensitive keys (connection strings, API keys) MUST be stored using .NET User Secrets manager (`dotnet user-secrets`) for local development. Never commit secrets to source control.

**Azure Secrets Rule**: For Azure deployments, Azure Key Vault is the primary store for all secrets. App Service Environment Variables MUST store only the Key Vault URI, not the secrets themselves.

**CLI Preference Rule**: Prefer direct, single-line CLI commands for development tasks. Reserve the `/scripts` folder exclusively for complex, multi-step automation (e.g., CI/CD pipelines, release helpers, deployment scripts).

**Rationale**: 
- Azure Table Storage with Azurite provides consistent development and production environments
- User Secrets prevent accidental secret exposure in version control
- Key Vault centralizes secret management with proper access control and auditing
- CLI-first approach reduces script maintenance overhead and improves discoverability

**Enforcement**:
- Code reviews verify no hardcoded secrets
- CI checks validate Key Vault references in Azure deployments
- Documentation MUST include Azurite setup instructions

### III. Solution Structure (REQUIRED)

**Naming Rule**: All projects, solutions, and storage tables MUST use the prefix `Po.LearnCert.*` for projects and `PoLearnCert[TableName]` for storage tables.

**Root Folder Structure** (REQUIRED):
```
/src          - Application source code
/tests        - All test projects
/docs         - README.md, PRD.md, ADR.md, AGENTS.md, Mermaid diagrams, COVERAGE reports
/scripts      - Utility scripts (.ps1, .sh) for complex automation only
/infra        - Minimal Bicep/YAML files for Azure resource deployment
```

**Source Projects** (`/src`) (REQUIRED):
- `Po.LearnCert.Api` - ASP.NET Core API (hosts Blazor WASM client). Features organized as Vertical Slices.
- `Po.LearnCert.Client` - Blazor WebAssembly project for client-side UI.
- `Po.LearnCert.Shared` - DTOs and models shared between Api and Client. No business logic.

**Test Projects** (`/tests`) (REQUIRED):
- `Po.LearnCert.UnitTests` - xUnit unit tests for business logic
- `Po.LearnCert.IntegrationTests` - xUnit integration tests for API endpoints
- `Po.LearnCert.E2ETests` - Playwright with TypeScript for end-to-end testing

**Rationale**: Consistent naming and structure improves navigation, reduces cognitive load, and enables automated tooling. Vertical slice organization in the API project aligns with feature-based development.

**Enforcement**: 
- Project templates enforce naming conventions
- CI validates folder structure compliance
- Code reviews verify proper project organization

### IV. Vertical Slice Architecture (REQUIRED)

**Rule**: Use Vertical Slice Architecture as the primary organizational pattern. Organize code by feature, not by layer. Each feature slice MUST be self-contained with all necessary components (endpoints, commands/queries, handlers, validators).

**Tooling Requirements**:
- MUST use Minimal APIs for all new endpoints
- MUST use MediatR to implement CQRS pattern within each feature slice
- MAY use Polly for resilience, Microsoft.FluentUI.AspNetCore.Components for UI, OpenTelemetry for observability, or dotnet-monitor for diagnostics when justified

**Philosophy**: Prioritize simple, well-factored code. Apply SOLID principles pragmatically. Each slice MUST be independently testable and deployable.

**Rationale**: Vertical slices reduce coupling between features, enable parallel development, and make it easier to understand the full scope of a feature. Minimal APIs reduce boilerplate. MediatR provides clean separation of concerns and request/response handling.

**Enforcement**:
- Code reviews verify feature cohesion and self-containment
- Architecture Decision Records (ADRs) document deviations
- Feature folders in `/src/Po.LearnCert.Api/Features/[FeatureName]/`

### V. Backend (API) Rules (REQUIRED)

**API Documentation Rule**: MUST enable Swagger using OpenAPI 3.0 specification for all endpoints from project start. MUST generate `.http` files for manual testing of API endpoints.

**Health Checks Rule**: MUST implement mandatory health check endpoints:
- `/health/ready` - Readiness probe (indicates app is ready to receive traffic)
- `/health/live` - Liveness probe (indicates app is running)

**Logging & Telemetry Rule**: 
- MUST use Serilog for structured logging
- Development: Write to Debug Console
- Production: Write to Application Insights
- MUST use .NET OpenTelemetry abstractions:
  - `ILogger<T>` for logging
  - `ActivitySource` for distributed tracing
  - `Meter` for custom metrics

**Error Handling Rule**: MUST implement global exception handling middleware that transforms all errors into RFC 7807 Problem Details responses. Never expose raw exceptions or stack traces to clients.

**Rationale**: 
- Swagger enables immediate API documentation and testing
- Health checks support orchestration, monitoring, and zero-downtime deployments
- Structured logging enables efficient troubleshooting and analytics
- Problem Details provide consistent, machine-readable error responses

**Enforcement**:
- CI validates presence of Swagger configuration
- Integration tests verify health endpoints respond correctly
- Code reviews verify Problem Details middleware is configured
- All exceptions MUST be logged with appropriate severity levels

### VI. Frontend (Client) Rules (REQUIRED)

**UX Design Rule**: Design MUST be mobile-first with portrait mode as primary layout. MUST also provide professional desktop layout. Layout MUST be responsive, fluid, and touch-friendly with minimum touch target size of 44x44 pixels.

**Component Selection Rule**: 
- MUST prioritize `Microsoft.FluentUI.AspNetCore.Components` for all UI components
- MUST use built-in Blazor components (`EditForm`, `InputText`, `InputNumber`) for basic forms
- MUST NOT use third-party libraries (e.g., Radzen) unless explicitly pre-approved and documented

**Accessibility Rule**: MUST follow WCAG 2.1 AA standards for accessibility.

**Rationale**: 
- Mobile-first design ensures usability on most constrained devices
- Fluent UI provides consistent, professional, accessible components
- Built-in Blazor components reduce dependencies and maintenance burden
- Touch-friendly design improves user experience across all devices

**Enforcement**:
- E2E tests MUST validate both mobile and desktop viewports
- Code reviews verify responsive design implementation
- Accessibility audits during QA phase
- No third-party component libraries without ADR approval

### VII. Testing Strategy (REQUIRED)

**TDD Workflow Rule**: MUST follow Test-Driven Development (TDD) strictly:
1. **Red**: Write a failing test
2. **Green**: Write minimal code to pass the test
3. **Refactor**: Improve code quality while keeping tests green

**Unit Testing Rule**: 
- MUST cover all new business logic with unit tests
- Framework: xUnit
- Naming: `MethodName_StateUnderTest_ExpectedBehavior`
- Isolation: Mock all external dependencies

**Integration Testing Rule**:
- MUST have a "happy path" test for every new API endpoint
- Framework: xUnit
- Database: MUST run against isolated test database (Azurite or in-memory)
- Cleanup: MUST implement full setup and teardown - no data persists between test runs

**E2E Testing Rule**:
- MUST validate responsiveness by testing both mobile and desktop viewports
- Framework: Playwright with TypeScript
- Browser: Chromium only
- Viewports: 
  - Mobile: Pixel 5 (393x851 or similar)
  - Desktop: 1920x1080
- Execution: Manual execution, NOT included in automated CI

**Test Naming Convention** (REQUIRED): All test methods MUST follow `MethodName_StateUnderTest_ExpectedBehavior` pattern.

**Rationale**:
- TDD ensures code is testable by design and serves as executable documentation
- Unit tests provide fast feedback on business logic
- Integration tests verify API contracts and data persistence
- E2E tests validate complete user workflows and responsive design
- Consistent naming improves test discoverability and maintenance

**Enforcement**:
- Code reviews verify test existence before implementation approval
- CI runs unit and integration tests on every commit
- Test coverage reports track coverage metrics
- E2E tests run manually before releases

## Technology Stack & Standards

### SDK & Ports (REQUIRED)

- **SDK**: .NET 9.0 (global.json pinned to 9.0.xxx)
- **Ports**: API MUST bind to HTTP 5000 and HTTPS 5001 only
- **Project Prefix**: `Po.LearnCert.*` for all projects
- **Table Naming**: `PoLearnCert[TableName]` for all Azure Table Storage tables

### Data Persistence (REQUIRED)

- **Default Storage**: Azure Table Storage
- **Local Development**: Azurite (MUST be configured and running)
- **Alternative Stores**: Require explicit specification and approval via Architecture Decision Record (ADR)
- **Connection Strings**: MUST be stored in User Secrets (local) or Key Vault (Azure)

### Secrets Management (REQUIRED)

- **Local Development**: .NET User Secrets (`dotnet user-secrets`)
- **Azure Production**: Azure Key Vault
- **Configuration**: App Service Environment Variables store Key Vault URI only
- **Prohibition**: Never commit secrets to source control

### Logging & Telemetry (REQUIRED)

- **Structured Logging**: Serilog
  - Development: Debug Console sink
  - Production: Application Insights sink
- **Exception Handling**: Global middleware with RFC 7807 Problem Details transformation
- **OpenTelemetry**: Use .NET built-in abstractions (`ILogger`, `ActivitySource`, `Meter`)
- **Stack Traces**: Never expose to external clients

### Formatting & Code Quality (REQUIRED)

- **Formatter**: `dotnet format` enforced in CI
- **Build Failure**: Formatting errors MUST fail builds
- **File Size Limit**: ≤500 lines per file (enforced via linters or code reviews)
- **Code Style**: Follow standard .NET conventions

## Architecture & Project Structure

### Repository Layout (REQUIRED)

All projects MUST follow this structure at repository root:

```
/src
  /Po.LearnCert.Api          - ASP.NET Core API + Blazor WASM host
    /Features                - Vertical slices organized by feature
      /Authentication
      /Quiz
      /Statistics
      /Leaderboards
  /Po.LearnCert.Client       - Blazor WebAssembly UI
  /Po.LearnCert.Shared       - Shared DTOs and models

/tests
  /Po.LearnCert.UnitTests          - xUnit unit tests
  /Po.LearnCert.IntegrationTests   - xUnit integration tests
  /Po.LearnCert.E2ETests           - Playwright TypeScript E2E tests

/docs
  README.md                  - Project overview and quickstart
  PRD.md                     - Product requirements document
  ADR.md                     - Architecture Decision Records
  AGENTS.md                  - AI agent development guidelines
  COVERAGE.md                - Test coverage reports
  [Diagrams]                 - Mermaid diagrams

/scripts
  [Complex automation only]  - CI/CD, deployment, release helpers

/infra
  [Bicep/YAML files]         - Azure resource deployment templates
```

**Documentation Constraint**: All documentation MUST be placed in `/docs` folder. Only the following files are permitted:
- README.md (project overview)
- PRD.md (product requirements)
- ADR.md (architecture decisions)
- AGENTS.md (development guidelines)
- COVERAGE.md (test coverage)
- Mermaid diagram files

**Automation Constraint**: 
- Prefer one-line CLI commands for development tasks
- Use `/scripts` only for complex multi-step automation
- Do NOT create PowerShell or markdown files during development conversations except for Azure deployment

### Code Organization (REQUIRED)

**Feature Structure** (Vertical Slices):
```
/Features/[FeatureName]/
  [FeatureName]Endpoint.cs     - Minimal API endpoint definitions
  [Action]Command.cs           - MediatR command/query
  [Action]Handler.cs           - MediatR handler implementation
  [Action]Validator.cs         - FluentValidation validators
```

**Naming Conventions**:
- Projects: `Po.LearnCert.[ProjectType]`
- Tables: `PoLearnCert[EntityName]` (e.g., `PoLearnCertUsers`)
- Features: PascalCase folder names
- Tests: `MethodName_StateUnderTest_ExpectedBehavior`

**Canonical Examples**: Provide inline code comments with examples and anti-patterns where rules are non-obvious.

## Quality & Enforcement

### Automated CI Checks (REQUIRED)

CI MUST validate:
- ✅ .NET SDK version is 9.x.x (from `global.json`)
- ✅ Required ports (5000 HTTP, 5001 HTTPS) are configured
- ✅ Project names conform to `Po.LearnCert.*` pattern
- ✅ `/health/ready` and `/health/live` endpoints exist and respond
- ✅ Problem Details middleware is configured
- ✅ `dotnet format` passes without errors
- ✅ All unit and integration tests pass
- ✅ No hardcoded secrets in code

### Testing Workflow (REQUIRED)

**TDD Cycle**:
1. Write a failing xUnit test (Red)
2. Implement minimal code to pass the test (Green)
3. Refactor while keeping tests green (Refactor)

**Test Requirements**:
- Unit tests: Cover all business logic, mock external dependencies
- Integration tests: 
  - "Happy path" test for every API endpoint
  - MUST set up and tear down cleanly (no lingering data)
  - Run against Azurite or in-memory database
- E2E tests:
  - Playwright with TypeScript
  - Test both mobile (Pixel 5: 393x851) and desktop (1920x1080) viewports
  - Executed manually, NOT in CI
  - Browser: Chromium only

**Test Naming**: `MethodName_StateUnderTest_ExpectedBehavior`

### Manual Testing (REQUIRED)

- ✅ Swagger/OpenAPI exposed from project start
- ✅ `.http` files generated for API endpoint testing
- ✅ Health check endpoints documented and accessible
- ✅ Mobile and desktop responsive layouts verified before merge

### Code Review Standards (REQUIRED)

All code reviews MUST verify:
- ✅ Compliance with all REQUIRED rules in this constitution
- ✅ Tests exist and pass before implementation approval
- ✅ No secrets in code or configuration files
- ✅ Proper error handling with Problem Details
- ✅ Feature slices are self-contained
- ✅ Mobile-first responsive design implemented
- ✅ SOLID principles applied appropriately
- ✅ Code complexity is justified when introduced
- ✅ File size ≤500 lines per file

## Governance

### Authority

This constitution supersedes all other practices and guidelines. When conflicts arise between this constitution and other documentation, the constitution takes precedence.

### Rule Classification

Rules are classified as:
- **REQUIRED**: Non-negotiable, enforced by CI/CD and code reviews
- **PREFERRED**: Strong recommendations, deviations require justification
- **INFORMATIONAL**: Guidance for context, not enforced

### Amendments

Amendments require:
1. Documentation of the change and rationale
2. Version increment following semantic versioning:
   - **MAJOR**: Backward-incompatible governance or principle removals/redefinitions
   - **MINOR**: New principle/section added or materially expanded guidance
   - **PATCH**: Clarifications, wording fixes, typo corrections, non-semantic refinements
3. Consistency propagation across all `.specify/templates/` files
4. Sync impact report documenting affected templates and follow-up actions
5. Update to LAST_AMENDED_DATE

### Versioning Policy

- **MAJOR (x.0.0)**: Breaking changes to governance, principle removals, or fundamental redefinitions
- **MINOR (x.y.0)**: New principles added, existing principles significantly expanded
- **PATCH (x.y.z)**: Clarifications, formatting improvements, typo fixes, non-semantic updates

### Compliance Review

- All code reviews MUST verify compliance with REQUIRED rules
- Complexity or deviations MUST be justified in Architecture Decision Records (ADRs)
- Use `.specify/templates/plan-template.md` Constitution Check section to validate compliance before feature development
- CI/CD pipeline enforces automated checks for all REQUIRED rules

### Runtime Development Guidance

For runtime development workflows and feature implementation:
- Consult command templates in `.specify/templates/`
- Review feature specifications in `/specs/[###-feature-name]/`
- Follow the comprehensive guidelines in `/docs/AGENTS.md`
- Document architectural decisions in `/docs/ADR.md`

---

**Version**: 1.1.0 | **Ratified**: 2025-10-26 | **Last Amended**: 2025-11-03
