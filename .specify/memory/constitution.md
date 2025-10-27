<!--
SYNC IMPACT REPORT
==================
Version Change: [Initial Template] → 1.0.0
Date: 2025-10-26

New Sections Added:
- All core principles defined (7 principles)
- Technology Stack & Standards section
- Architecture & Project Structure section
- Quality & Enforcement section

Templates Requiring Updates:
✅ plan-template.md - Constitution Check section aligns with principles
✅ spec-template.md - User stories and requirements align with TDD and testing principles
✅ tasks-template.md - Task organization supports vertical slice and TDD workflow

Follow-up TODOs:
- None - All placeholders filled with concrete values
-->

# PoLearnCert Constitution

## Core Principles

### I. .NET 9.0 Only (REQUIRED)

**Rule**: All projects MUST use .NET 9.0 SDK latest patch. Builds MUST fail if a different SDK major version is detected.

**Rationale**: Enforcing a single SDK version eliminates version drift, ensures consistent tooling and runtime behavior, and prevents compatibility issues. CI/CD gates verify SDK version compliance before any build proceeds.

**Enforcement**: Automated CI check validates `global.json` SDK version is 9.x.x; build fails on mismatch.

### II. Vertical Slice Architecture (PREFERRED)

**Rule**: Use Vertical Slice Architecture as the primary pattern. Apply Clean Architecture boundaries only where complexity justifies separation of concerns.

**Rationale**: Vertical slices organize code by feature rather than layer, reducing coupling and enabling independent development of user stories. Clean Architecture is reserved for domains requiring strict separation (e.g., swappable data access, third-party integration).

**Enforcement**: Code reviews verify feature cohesion and justified architectural decisions when Clean Architecture is introduced.

### III. Test-First Development (REQUIRED)

**Rule**: Follow TDD strictly. Write a failing xUnit test first, then implement code to pass it. Maintain separate unit and integration test projects.

**Rationale**: TDD ensures code is testable by design, provides immediate feedback, and serves as executable documentation. Separating test types prevents confusion and enforces proper isolation (unit vs. integration concerns).

**Enforcement**: Code reviews verify test existence before implementation; CI runs tests on every commit.

**Testing Standards**:
- xUnit for unit and integration tests
- Integration tests MUST include setup/teardown to avoid data leakage
- E2E tests use Playwright MCP with TypeScript, executed manually, NOT included in CI
- Database-using tests MUST run against Azurite or disposable test tables and clean up after execution

### IV. API Observability & Error Handling (REQUIRED)

**Rule**: All APIs MUST expose Swagger/OpenAPI from project start, provide a mandatory `/api/health` endpoint (readiness + liveness), and implement global exception handling middleware that transforms all errors into RFC 7807 Problem Details responses.

**Rationale**: Swagger enables manual testing and documentation. Health endpoints support orchestration and monitoring. Problem Details provide consistent, machine-readable error responses without exposing sensitive implementation details or stack traces.

**Enforcement**: 
- Automated CI checks verify presence of Swagger configuration, `/api/health` endpoint, and Problem Details middleware
- Serilog MUST be configured for structured logging with sensible local sinks
- Follow .NET best practices for telemetry integration

### V. Imperative Minimal Rules (INFORMATIONAL)

**Rule**: Rules are classified as REQUIRED, PREFERRED, or INFORMATIONAL. Required rules take precedence; others guide decision-making but allow flexibility for later refinement.

**Rationale**: This approach prioritizes enforceability and clarity, focusing team effort on non-negotiable standards while preserving room for context-specific judgment.

**Enforcement**: Rule classification is documented inline; tooling enforces REQUIRED rules automatically.

### VI. Mobile-First Responsive UI (REQUIRED)

**Rule**: UI MUST prioritize excellent mobile portrait UX with responsive layouts. Start with built-in Blazor components; adopt Radzen.Blazor only when advanced UX needs are explicitly justified.

**Rationale**: Mobile-first design ensures accessibility and usability on the most constrained devices. Testing on desktop and narrow-screen emulation validates layout and interaction across breakpoints.

**Enforcement**: 
- Main flows MUST be tested on desktop and mobile emulation before merge
- Code reviews verify responsive design implementation (fluid grids, touch-friendly controls, readable typography, appropriate breakpoints)

### VII. SOLID Principles & Simplicity (PREFERRED)

**Rule**: Enforce SOLID principles and appropriate Gang of Four (GoF) design patterns. Prefer simple, small, well-factored code. Apply YAGNI (You Aren't Gonna Need It).

**Rationale**: SOLID promotes maintainable, extensible code. GoF patterns solve recurring design problems. Simplicity reduces cognitive load and prevents over-engineering.

**Enforcement**: Code reviews verify adherence; complexity MUST be justified when introduced.

## Technology Stack & Standards

### SDK & Ports (REQUIRED)

- **SDK**: .NET 9.0 latest patch
- **Ports**: API MUST bind to HTTP 5000 and HTTPS 5001 only
- **Project Prefix**: All projects MUST follow `Po.AppName.*` naming pattern
- **Table Naming**: `PoAppName[TableName]` for all storage tables

### Data Persistence (REQUIRED)

- **Default Storage**: Azure Table Storage using Azurite for local development
- **Alternative Stores**: Require explicit specification and approval in feature spec
- **Local Dev**: Azurite MUST be configured and documented in quickstart

### Logging & Telemetry (REQUIRED)

- **Structured Logging**: Serilog with sensible local sinks
- **Exception Handling**: Global middleware transforms all exceptions to RFC 7807 Problem Details; never expose raw exceptions or stack traces
- **Telemetry**: Follow .NET best practices for Application Insights or equivalent observability platform

### Formatting & Linting (REQUIRED)

- **Formatter**: `dotnet format` enforced as pre-commit or CI gate
- **Build Failure**: Formatting errors MUST fail builds
- **File Size Limit**: ≤500 lines per file; enforce via linters or pre-commit checks

## Architecture & Project Structure

### Repository Layout (REQUIRED)

All projects MUST follow this structure at repository root:

```
/src
  /Po.AppName.Api
  /Po.AppName.Client (Blazor WebAssembly)
  /Po.AppName.Shared
/tests
  /Po.AppName.UnitTests
  /Po.AppName.IntegrationTests
/docs
  PRD.MD
  STEPS.MD
  README.MD
/scripts
  (CLI helpers only)
```

**Documentation Constraint**: Only PRD.MD, STEPS.MD, and README.MD are permitted in `/docs`. All other doc files created during development MUST be placed in `/docs` folder.

**Automation Constraint**: All operations MUST use CLI commands only. Provide one-line commands at a time for human execution. Do NOT create extra markdown or PowerShell files during conversations, except for Azure deployment files.

### Code Organization (PREFERRED)

- **Canonical Examples**: Provide small inline code comments with examples and one-line anti-patterns where rules are non-obvious
- **Naming**: Project and table names MUST follow `Po.AppName` pattern exactly

## Quality & Enforcement

### Automated CI Checks (REQUIRED)

CI MUST validate:
- .NET SDK major version is 9
- Required ports (5000 HTTP, 5001 HTTPS) are configured
- Project prefix conforms to `Po.AppName.*`
- `/api/health` endpoint exists
- Problem Details middleware is present
- `dotnet format` passes without errors

### Testing Workflow (REQUIRED)

1. Write a failing xUnit test
2. Implement code to pass the test
3. Refactor as needed
4. Integration tests MUST set up and tear down cleanly (no lingering data)
5. E2E tests executed manually via Playwright MCP (TypeScript)
6. API methods surfaced in Swagger for manual verification during dev/QA

### Manual Testing (REQUIRED)

- Expose Swagger/OpenAPI from project start
- Document endpoints used for manual testing
- Create minimal, easy-to-invoke API methods for dev/QA verification

## Governance

### Authority

This constitution supersedes all other practices and guidelines. When conflicts arise, the constitution takes precedence.

### Amendments

Amendments require:
1. Documentation of the change and rationale
2. Version increment following semantic versioning (MAJOR: breaking governance changes; MINOR: new principles/sections; PATCH: clarifications/wording)
3. Consistency propagation across all `.specify/templates/` files
4. Sync impact report documenting affected templates and follow-up actions

### Versioning Policy

- **MAJOR**: Backward-incompatible governance or principle removals/redefinitions
- **MINOR**: New principle/section added or materially expanded guidance
- **PATCH**: Clarifications, wording fixes, typo corrections, non-semantic refinements

### Compliance Review

- All code reviews MUST verify compliance with REQUIRED rules
- Complexity MUST be justified when Clean Architecture or alternative patterns are introduced
- Use `.specify/templates/plan-template.md` Constitution Check section to validate compliance before Phase 0 research

### Runtime Development Guidance

For runtime development workflows, consult the command templates in `.specify/templates/` and the feature specification documents in `/specs/[###-feature-name]/`.

**Version**: 1.0.0 | **Ratified**: 2025-10-26 | **Last Amended**: 2025-10-26
