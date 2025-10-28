# Phase 0: Research & Technology Decisions

**Feature**: PoLearnCert Certification Quiz Platform  
**Date**: 2025-10-26  
**Purpose**: Resolve technology choices and establish best practices for implementation

## Authentication & Identity Management

### Decision: ASP.NET Core Identity with Email/Password

**Rationale**:
- Aligns with clarification session decision (Q2) and FR-020
- Native integration with .NET 9.0 and Entity Framework Core
- Full control over user data and authentication flow
- Supports session-based authentication as specified
- Simpler than third-party OAuth providers for MVP scope

**Alternatives Considered**:
- Supabase with Google Auth: Rejected due to clarification session specifying email/password
- IdentityServer4: Overly complex for single-application scenario
- Azure AD B2C: Additional cost and complexity not justified for MVP

**Implementation Approach**:
- Use ASP.NET Core Identity for user management
- Store identity data in Azure Table Storage using custom UserStore and RoleStore implementations
- Session-based authentication with secure HTTP-only cookies
- Password hashing using ASP.NET Identity defaults (PBKDF2 with HMAC-SHA256)

**Best Practices**:
- Enforce password complexity requirements (8+ characters, mixed case, numbers, special chars)
- Implement account lockout after failed login attempts
- Use HTTPS only for all authentication endpoints
- Store session tokens in HTTP-only, secure cookies
- Implement CSRF protection for state-changing operations

---

## Azure Table Storage with .NET 9.0

### Decision: Azure.Data.Tables SDK with Repository Pattern

**Rationale**:
- Constitution mandates Azure Table Storage as default persistence
- Azure.Data.Tables is the modern, officially supported SDK for .NET
- Repository pattern provides abstraction for testability and future migration
- Table Storage is cost-effective for quiz data, user stats, and leaderboards
- Azurite provides excellent local development experience

**Alternatives Considered**:
- Microsoft.Azure.Cosmos.Table (deprecated): Superseded by Azure.Data.Tables
- Direct TableClient usage: Less testable and harder to mock
- Cosmos DB Table API: Higher cost without meaningful benefits for this use case

**Implementation Approach**:
- Create repository interfaces per entity (IUserRepository, IQuizSessionRepository, etc.)
- Implement repositories using TableClient from Azure.Data.Tables
- Use partition key strategy: certification name for questions, userId for sessions and stats
- Row keys: GUID for entities, composite keys where needed for queries

**Table Schema**:
```
PoLearnCertUsers           : PartitionKey=EmailDomain, RowKey=UserId
PoLearnCertCertifications  : PartitionKey="Global", RowKey=CertId
PoLearnCertSubtopics       : PartitionKey=CertId, RowKey=SubtopicId
PoLearnCertQuestions       : PartitionKey=SubtopicId, RowKey=QuestionId
PoLearnCertAnswerChoices   : PartitionKey=QuestionId, RowKey=ChoiceId
PoLearnCertQuizSessions    : PartitionKey=UserId, RowKey=SessionId
PoLearnCertSessionAnswers  : PartitionKey=SessionId, RowKey=QuestionId
PoLearnCertLeaderboards    : PartitionKey=CertId+TimePeriod, RowKey=UserId
```

**Best Practices**:
- Use Azurite for all local development and integration tests
- Implement connection string configuration in appsettings
- Use batch operations (TableTransactionAction) for multi-entity updates
- Implement retry policies for transient failures
- Avoid table scans; design queries to use PartitionKey and RowKey filters

---

## Blazor WebAssembly with .NET 9.0

### Decision: Blazor WASM Standalone with Built-in Components

**Rationale**:
- Constitution mandates starting with built-in Blazor components
- Blazor WASM provides SPA experience without JavaScript framework complexity
- .NET 9.0 brings performance improvements to WASM runtime
- Full integration with C# backend via HTTP clients
- Mobile-first responsive design achievable with CSS and built-in layout components

**Alternatives Considered**:
- Blazor Server: Requires persistent SignalR connection; less scalable for 100+ concurrent users
- Radzen.Blazor components: Constitution requires justification; start with built-in first
- React/Vue/Angular: Violates .NET 9.0 only principle; introduces separate tech stack

**Implementation Approach**:
- Use Blazor WASM hosted model (served by API project)
- HttpClient for API communication with JSON serialization
- Component-based architecture matching vertical slices (Quiz, Statistics, Leaderboards, Certifications)
- CSS Grid and Flexbox for responsive layouts (mobile-first)
- Local storage for quiz session state (cleared on interruption per FR-019)

**Mobile-First Responsive Design**:
- Start with mobile portrait layout (320px-768px)
- Use fluid grids and relative units (rem, %, vw/vh)
- Touch-friendly buttons (minimum 44x44px tap targets)
- Test on Chrome DevTools mobile emulation and real devices
- Breakpoints: Mobile (<768px), Tablet (768px-1024px), Desktop (>1024px)

**Best Practices**:
- Separate presentation components from logic (code-behind pattern)
- Use scoped CSS for component isolation
- Implement loading states and error boundaries
- Lazy load routes for performance
- Minimize bundle size (tree-shaking, link trimming)

---

## API Design & REST Conventions

### Decision: RESTful API with OpenAPI 3.0 Documentation

**Rationale**:
- Constitution mandates Swagger/OpenAPI from project start
- RESTful conventions provide predictable, intuitive endpoints
- ASP.NET Core provides excellent built-in support for OpenAPI
- Manual testing via Swagger UI required per constitution

**Resource-Based Endpoint Design**:
```
POST   /api/auth/register           # User registration
POST   /api/auth/login              # User login
POST   /api/auth/logout             # User logout
GET    /api/health                  # Health check (required)

GET    /api/certifications          # List all certifications
GET    /api/certifications/{id}     # Get certification details

POST   /api/quiz/sessions           # Start new quiz session
GET    /api/quiz/sessions/{id}      # Get session details
POST   /api/quiz/sessions/{id}/answers  # Submit answer

GET    /api/statistics              # Get user's statistics
GET    /api/statistics/subtopics    # Get subtopic performance

GET    /api/leaderboards/{certId}   # Get leaderboard for certification
```

**Best Practices**:
- Use HTTP status codes correctly (200, 201, 400, 401, 404, 500)
- Implement RFC 7807 Problem Details for all errors
- Version API in URL if breaking changes needed (e.g., /api/v1/)
- Use DTOs for request/response bodies (never expose entities directly)
- Implement pagination for list endpoints (skip/take query parameters)
- Add request validation using FluentValidation or Data Annotations

---

## Error Handling & Observability

### Decision: Serilog + RFC 7807 Problem Details + Health Checks

**Rationale**:
- Constitution mandates Serilog, Problem Details, and /api/health endpoint
- Serilog provides structured logging with minimal performance overhead
- Problem Details (RFC 7807) standardizes error responses
- Health checks support container orchestration and monitoring

**Serilog Configuration**:
- Sinks: Console (structured JSON for production), File (local development)
- Enrichers: Thread, Environment, Machine name
- Minimum level: Information (Warning for Microsoft.* namespaces)
- Log request/response for API calls (exclude sensitive data)

**Problem Details Structure**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Quiz session with ID 'abc123' was not found",
  "instance": "/api/quiz/sessions/abc123",
  "traceId": "00-abc123-def456-00"
}
```

**Health Check Implementation**:
- `/api/health` endpoint returns 200 OK when healthy
- Check: Azure Table Storage connectivity (query PoLearnCertCertifications table)
- Check: ASP.NET Core Identity connectivity
- Response includes status for each dependency

**Best Practices**:
- Never log sensitive data (passwords, tokens, PII)
- Include correlation IDs in all log entries
- Use structured logging (avoid string concatenation)
- Implement global exception handling middleware
- Log errors at appropriate levels (Warning for expected, Error for unexpected)

---

## Testing Strategy

### Decision: xUnit + FluentAssertions + Testcontainers (Azurite)

**Rationale**:
- Constitution mandates xUnit for unit and integration tests
- FluentAssertions provides readable test assertions
- Testcontainers or direct Azurite usage for isolated integration tests
- Playwright MCP TypeScript for manual E2E tests per constitution

**Unit Testing Approach**:
- Test services and repositories in isolation using mocks (Moq or NSubstitute)
- Aim for >80% code coverage on business logic
- Use AAA pattern (Arrange, Act, Assert)
- One assertion per test method when possible
- Descriptive test names: `MethodName_Scenario_ExpectedBehavior`

**Integration Testing Approach**:
- Spin up Azurite in test fixture or use Testcontainers
- Create test tables with known data
- Execute real queries against Azurite
- Clean up test data in teardown (avoid leakage)
- Test API endpoints end-to-end with WebApplicationFactory

**E2E Testing Approach**:
- Playwright MCP with TypeScript executed manually (not in CI)
- Test critical user journeys (quiz session, statistics view, leaderboard)
- Run against local environment or staging
- Focus on happy paths and critical edge cases

**Best Practices**:
- Separate unit and integration test projects
- Use test fixtures for shared setup/teardown
- Avoid test interdependencies (each test independent)
- Use realistic test data that mirrors production scenarios
- Mock external dependencies (Azure OpenAI) in tests

---

## Question Generation Tool

### Decision: Separate C# Console App with Azure OpenAI SDK

**Rationale**:
- User specified separate C# console app for question generation
- Azure OpenAI provides GPT-4 for high-quality question generation
- Separation of concerns: quiz platform doesn't need AI dependencies
- Batch question generation can run offline

**Implementation Approach**:
- Create Po.LearnCert.QuestionGenerator console app in /tools directory
- Use Azure.AI.OpenAI SDK for GPT-4 integration
- Generate questions with prompt engineering (certification topic, difficulty, format)
- Validate generated questions (correct answer exists, plausible distractors)
- Output to JSON or directly insert into Azure Table Storage

**Best Practices**:
- Store API keys in user secrets or environment variables (never commit)
- Implement retry logic for API rate limits
- Validate AI-generated content before persisting
- Log generation metrics (questions created, API costs, errors)
- Support batch generation to minimize API calls

---

## Summary of Technology Stack

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| SDK | .NET 9.0 | Constitution requirement |
| Frontend | Blazor WebAssembly | Native .NET, SPA experience |
| Backend API | ASP.NET Core Web API | Native .NET, RESTful conventions |
| Authentication | ASP.NET Core Identity | Email/password session-based auth |
| Data Storage | Azure Table Storage | Constitution default, cost-effective |
| Local Storage Emulator | Azurite | Official Azure emulator |
| Logging | Serilog | Structured logging, constitution requirement |
| Error Handling | RFC 7807 Problem Details | Standardized error format |
| API Documentation | Swagger/OpenAPI 3.0 | Manual testing, constitution requirement |
| Unit Testing | xUnit + FluentAssertions | Constitution requirement |
| Integration Testing | xUnit + Azurite | Isolated tests against emulator |
| E2E Testing | Playwright MCP TypeScript | Manual execution per constitution |
| Question Generation | Azure OpenAI SDK (GPT-4) | AI-powered content creation |
| Dependency Injection | Built-in ASP.NET Core DI | Native .NET support |
| Configuration | appsettings.json + User Secrets | Standard .NET configuration |

---

## Next Steps

1. Proceed to Phase 1: Design (data-model.md, contracts/, quickstart.md)
2. Create global.json with .NET 9.0 SDK version
3. Initialize project structure per plan.md
4. Configure Azurite for local development
5. Set up CI pipeline for automated checks
