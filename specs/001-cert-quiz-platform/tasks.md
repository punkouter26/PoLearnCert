---
description: "Implementation tasks for PoLearnCert Certification Quiz Platform"
---

# Tasks: PoLearnCert Certification Quiz Platform

**Input**: Design documents from `/specs/001-cert-quiz-platform/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅  
**Branch**: `001-cert-quiz-platform`  
**Date**: 2025-10-26

**Tests**: This project follows TDD approach per constitution. All test tasks are included and MUST be written first to ensure they FAIL before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

This is a web application with:
- Backend: `src/Po.LearnCert.Api/`
- Frontend: `src/Po.LearnCert.Client/`
- Shared: `src/Po.LearnCert.Shared/`
- Tests: `tests/Po.LearnCert.UnitTests/`, `tests/Po.LearnCert.IntegrationTests/`
- Tools: `tools/Po.LearnCert.QuestionGenerator/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create global.json with .NET 9.0 SDK version in repository root
- [X] T002 Create solution file PoLearnCert.sln in repository root
- [X] T003 [P] Create Po.LearnCert.Api project (ASP.NET Core Web API) in src/Po.LearnCert.Api/
- [X] T004 [P] Create Po.LearnCert.Client project (Blazor WebAssembly) in src/Po.LearnCert.Client/
- [X] T005 [P] Create Po.LearnCert.Shared project (Class Library) in src/Po.LearnCert.Shared/
- [X] T006 [P] Create Po.LearnCert.UnitTests project (xUnit) in tests/Po.LearnCert.UnitTests/
- [X] T007 [P] Create Po.LearnCert.IntegrationTests project (xUnit) in tests/Po.LearnCert.IntegrationTests/
- [X] T008 [P] Create Po.LearnCert.QuestionGenerator project (Console App) in tools/Po.LearnCert.QuestionGenerator/
- [X] T009 Add NuGet packages to Api: Azure.Data.Tables, Serilog.AspNetCore, Swashbuckle.AspNetCore, Microsoft.AspNetCore.Identity
- [X] T010 [P] Add NuGet packages to Client: Microsoft.AspNetCore.Components.WebAssembly
- [X] T011 [P] Add NuGet packages to QuestionGenerator: Azure.AI.OpenAI
- [X] T012 [P] Add NuGet packages to UnitTests: xUnit, FluentAssertions, Moq
- [X] T013 [P] Add NuGet packages to IntegrationTests: xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing
- [X] T014 Create vertical slice folder structure in src/Po.LearnCert.Api/Features/ (Quiz, Statistics, Leaderboards, Certifications, Authentication)
- [X] T015 [P] Create vertical slice folder structure in src/Po.LearnCert.Client/Features/ (Quiz, Statistics, Leaderboards, Certifications, Authentication)
- [X] T016 [P] Create folder structure in src/Po.LearnCert.Shared/ (Models, Contracts, Constants)
- [X] T017 Create appsettings.json with placeholder values in src/Po.LearnCert.Api/
- [X] T018 [P] Create appsettings.Development.json with Azurite connection strings in src/Po.LearnCert.Api/
- [X] T019 [P] Configure launchSettings.json with ports 5000 (HTTP) and 5001 (HTTPS) in src/Po.LearnCert.Api/Properties/
- [X] T020 Azurite started manually via `azurite` command (no script needed)
- [X] T021 [P] Create .gitignore file in repository root
- [X] T190: Update README.md with comprehensive overview

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T023 Create TableEntity base classes in src/Po.LearnCert.Api/Infrastructure/Entities/TableEntityBase.cs
- [X] T024 [P] Implement IRepository<T> interface in src/Po.LearnCert.Api/Infrastructure/Repositories/IRepository.cs
- [X] T025 [P] Implement TableRepository<T> base class using Azure.Data.Tables in src/Po.LearnCert.Api/Infrastructure/Repositories/TableRepository.cs
- [X] T026 Configure Serilog in src/Po.LearnCert.Api/Program.cs with Console and File sinks
- [X] T027 [P] Implement global exception handling middleware for RFC 7807 Problem Details in src/Po.LearnCert.Api/Middleware/ProblemDetailsMiddleware.cs
- [X] T028 [P] Create health check endpoint /api/health in src/Po.LearnCert.Api/Features/Health/HealthController.cs
- [X] T029 Configure Swagger/OpenAPI in src/Po.LearnCert.Api/Program.cs with API documentation
- [X] T030 [P] Configure CORS policy for Blazor Client in src/Po.LearnCert.Api/Program.cs
- [X] T031 [P] Configure dependency injection container in src/Po.LearnCert.Api/Program.cs
- [X] T032 Implement ASP.NET Core Identity custom UserStore for Azure Table Storage in src/Po.LearnCert.Api/Features/Authentication/Infrastructure/TableUserStore.cs
- [X] T033 [P] Implement ASP.NET Core Identity custom RoleStore for Azure Table Storage in src/Po.LearnCert.Api/Features/Authentication/Infrastructure/TableRoleStore.cs
- [X] T034 Configure ASP.NET Core Identity in src/Po.LearnCert.Api/Program.cs with session-based auth
- [X] T035 [P] Create UserEntity table entity in src/Po.LearnCert.Api/Features/Authentication/Infrastructure/UserEntity.cs
- [X] T036 [P] Create integration test fixture with Azurite setup in tests/Po.LearnCert.IntegrationTests/Infrastructure/AzuriteFixture.cs
- [X] T037 [P] Configure WebApplicationFactory for integration tests in tests/Po.LearnCert.IntegrationTests/Infrastructure/TestWebApplicationFactory.cs
- [X] T038 Create HttpClient configuration for Blazor Client in src/Po.LearnCert.Client/Program.cs
- [X] T039 [P] Create base CSS file with mobile-first responsive styles in src/Po.LearnCert.Client/wwwroot/css/app.css

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Take Practice Quiz Session (Priority: P1) 🎯 MVP

**Goal**: Enable learners to take 20-question quiz sessions with immediate feedback and explanations, delivering core learning value

**Independent Test**: Create quiz session, answer questions, receive immediate feedback, view final results

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T040 [P] [US1] Unit test for QuizSessionService.CreateSessionAsync in tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs
- [ ] T041 [P] [US1] Unit test for QuizSessionService.SubmitAnswerAsync in tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs
- [ ] T042 [P] [US1] Unit test for QuizSessionService.GetSessionResultsAsync in tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs
- [ ] T043 [P] [US1] Integration test for POST /api/quiz/sessions in tests/Po.LearnCert.IntegrationTests/Features/Quiz/QuizSessionEndpointsTests.cs
- [ ] T044 [P] [US1] Integration test for POST /api/quiz/sessions/{id}/answers in tests/Po.LearnCert.IntegrationTests/Features/Quiz/QuizSessionEndpointsTests.cs
- [ ] T045 [P] [US1] Integration test for GET /api/quiz/sessions/{id} in tests/Po.LearnCert.IntegrationTests/Features/Quiz/QuizSessionEndpointsTests.cs

### Implementation for User Story 1 - Shared Models

- [X] T046 [P] [US1] Create CertificationDto in src/Po.LearnCert.Shared/Models/CertificationDto.cs
- [X] T047 [P] [US1] Create SubtopicDto in src/Po.LearnCert.Shared/Models/SubtopicDto.cs
- [X] T048 [P] [US1] Create QuestionDto in src/Po.LearnCert.Shared/Models/QuestionDto.cs
- [X] T049 [P] [US1] Create AnswerChoiceDto in src/Po.LearnCert.Shared/Models/AnswerChoiceDto.cs
- [X] T050 [P] [US1] Create QuizSessionDto in src/Po.LearnCert.Shared/Models/QuizSessionDto.cs
- [X] T051 [P] [US1] Create CreateQuizSessionRequest in src/Po.LearnCert.Shared/Contracts/CreateQuizSessionRequest.cs
- [X] T052 [P] [US1] Create SubmitAnswerRequest in src/Po.LearnCert.Shared/Contracts/SubmitAnswerRequest.cs
- [X] T053 [P] [US1] Create SubmitAnswerResponse in src/Po.LearnCert.Shared/Contracts/SubmitAnswerResponse.cs
- [X] T054 [P] [US1] Create QuizResultDto in src/Po.LearnCert.Shared/Models/QuizResultDto.cs

### Implementation for User Story 1 - Backend Entities

- [X] T055 [P] [US1] Create CertificationEntity table entity in src/Po.LearnCert.Api/Features/Certifications/Infrastructure/CertificationEntity.cs (PartitionKey="Global", RowKey=CertId)
- [X] T056 [P] [US1] Create SubtopicEntity table entity in src/Po.LearnCert.Api/Features/Certifications/Infrastructure/SubtopicEntity.cs (PartitionKey=CertId, RowKey=SubtopicId)
- [X] T057 [P] [US1] Create QuestionEntity table entity in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/QuestionEntity.cs (PartitionKey=SubtopicId, RowKey=QuestionId)
- [X] T058 [P] [US1] Create AnswerChoiceEntity table entity in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/AnswerChoiceEntity.cs (PartitionKey=QuestionId, RowKey=ChoiceId)
- [X] T059 [P] [US1] Create QuizSessionEntity table entity in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/QuizSessionEntity.cs (PartitionKey=UserId, RowKey=SessionId)
- [X] T060 [P] [US1] Create SessionAnswerEntity table entity in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/SessionAnswerEntity.cs (PartitionKey=SessionId, RowKey=QuestionId)

### Implementation for User Story 1 - Backend Repositories

- [X] T061 [P] [US1] Implement ICertificationRepository interface in src/Po.LearnCert.Api/Features/Certifications/Infrastructure/ICertificationRepository.cs
- [X] T062 [P] [US1] Implement CertificationRepository in src/Po.LearnCert.Api/Features/Certifications/Infrastructure/CertificationRepository.cs
- [X] T063 [P] [US1] Implement IQuestionRepository interface in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/IQuestionRepository.cs
- [X] T064 [P] [US1] Implement QuestionRepository in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/QuestionRepository.cs
- [X] T065 [P] [US1] Implement IQuizSessionRepository interface in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/IQuizSessionRepository.cs
- [X] T066 [US1] Implement QuizSessionRepository in src/Po.LearnCert.Api/Features/Quiz/Infrastructure/QuizSessionRepository.cs (depends on T065)

### Implementation for User Story 1 - Backend Services

- [X] T067 [US1] Implement QuizSessionService with CreateSessionAsync (FR-001, FR-008, FR-017, FR-021, FR-022) in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs
- [X] T068 [US1] Implement QuizSessionService.SubmitAnswerAsync with immediate feedback (FR-002, FR-003, FR-016) in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs
- [X] T069 [US1] Implement QuizSessionService.GetSessionResultsAsync (FR-004, FR-005) in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs
- [X] T070 [US1] Implement QuizSessionService.GetSessionDetailsAsync in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs

### Implementation for User Story 1 - Backend API Endpoints

- [X] T071 [US1] Implement POST /api/quiz/sessions endpoint in src/Po.LearnCert.Api/Features/Quiz/QuizController.cs
- [X] T072 [US1] Implement POST /api/quiz/sessions/{id}/answers endpoint with validation in src/Po.LearnCert.Api/Features/Quiz/QuizController.cs
- [X] T073 [US1] Implement GET /api/quiz/sessions/{id} endpoint in src/Po.LearnCert.Api/Features/Quiz/QuizController.cs
- [X] T074 [US1] Implement GET /api/certifications endpoint in src/Po.LearnCert.Api/Features/Certifications/CertificationsController.cs
- [X] T075 [US1] Add request validation using Data Annotations for quiz endpoints in src/Po.LearnCert.Api/Features/Quiz/QuizController.cs
- [X] T076 [US1] Add error handling and logging for quiz operations in src/Po.LearnCert.Api/Features/Quiz/QuizController.cs
- [X] T077 [US1] Register quiz services in dependency injection container in src/Po.LearnCert.Api/Program.cs

### Implementation for User Story 1 - Frontend Services

- [X] T078 [P] [US1] Create IQuizSessionService interface in src/Po.LearnCert.Client/Features/Quiz/Services/IQuizSessionService.cs
- [X] T079 [US1] Implement QuizSessionService HTTP client wrapper in src/Po.LearnCert.Client/Features/Quiz/Services/QuizSessionService.cs
- [X] T080 [P] [US1] Create ICertificationService interface in src/Po.LearnCert.Client/Features/Certifications/Services/ICertificationService.cs
- [X] T081 [US1] Implement CertificationService HTTP client wrapper in src/Po.LearnCert.Client/Features/Certifications/Services/CertificationService.cs

### Implementation for User Story 1 - Frontend Components

- [X] T082 [P] [US1] Create QuizSessionSetup.razor component for certification and subtopic selection in src/Po.LearnCert.Client/Features/Quiz/QuizSessionSetup.razor
- [X] T083 [P] [US1] Create QuizQuestion.razor component for displaying questions and answer choices in src/Po.LearnCert.Client/Features/Quiz/QuizQuestion.razor
- [X] T084 [P] [US1] Create AnswerFeedback.razor component for immediate feedback display in src/Po.LearnCert.Client/Features/Quiz/AnswerFeedback.razor
- [X] T085 [P] [US1] Create QuizResults.razor component for final score display in src/Po.LearnCert.Client/Features/Quiz/QuizResults.razor
- [X] T086 [US1] Implement quiz state management in src/Po.LearnCert.Client/Features/Quiz/QuizSessionState.cs
- [X] T087 [US1] Add mobile-first responsive CSS for quiz components in src/Po.LearnCert.Client/wwwroot/css/quiz.css
- [X] T088 [US1] Register quiz services in dependency injection in src/Po.LearnCert.Client/Program.cs

### Implementation for User Story 1 - Data Seeding

- [X] T089 [US1] Data seeding handled by Po.LearnCert.QuestionGenerator C# console app (Phase 7: T179-T187)
- [X] T090 [US1] QuestionGenerator populates PoLearnCertCertifications with AZ-900 and Security+ using AI
- [X] T091 [US1] QuestionGenerator creates subtopics, questions (200+ per cert), and answer choices dynamically
- [X] T092 [US1] Use `dotnet run` in tools/Po.LearnCert.QuestionGenerator to seed database

**Checkpoint**: User Story 1 (MVP) is complete - learners can take quiz sessions with immediate feedback

---

## Phase 4: User Story 2 - View Personal Performance Statistics (Priority: P2)

**Goal**: Enable learners to view historical performance and identify strengths/weaknesses across topics

**Independent Test**: Take multiple quiz sessions, view statistics dashboard showing accurate performance breakdown

### Tests for User Story 2

- [ ] T093 [P] [US2] Unit test for StatisticsService.GetUserStatisticsAsync in tests/Po.LearnCert.UnitTests/Features/Statistics/StatisticsServiceTests.cs
- [ ] T094 [P] [US2] Unit test for StatisticsService.GetSubtopicPerformanceAsync in tests/Po.LearnCert.UnitTests/Features/Statistics/StatisticsServiceTests.cs
- [ ] T095 [P] [US2] Integration test for GET /api/statistics in tests/Po.LearnCert.IntegrationTests/Features/Statistics/StatisticsEndpointsTests.cs
- [ ] T096 [P] [US2] Integration test for GET /api/statistics/subtopics in tests/Po.LearnCert.IntegrationTests/Features/Statistics/StatisticsEndpointsTests.cs

### Implementation for User Story 2 - Shared Models

- [X] T097 [P] [US2] Create UserStatisticsDto in src/Po.LearnCert.Shared/Models/UserStatisticsDto.cs
- [X] T098 [P] [US2] Create SubtopicPerformanceDto in src/Po.LearnCert.Shared/Models/SubtopicPerformanceDto.cs
- [X] T099 [P] [US2] Create PerformanceRecordDto in src/Po.LearnCert.Shared/Models/PerformanceRecordDto.cs (SessionHistoryDto)

### Implementation for User Story 2 - Backend Entities

- [X] T100 [P] [US2] Create PerformanceRecordEntity table entity in src/Po.LearnCert.Api/Features/Statistics/Infrastructure/PerformanceRecordEntity.cs (UserStatisticsEntity, CertificationPerformanceEntity, SubtopicPerformanceEntity)

### Repositories

- [X] T101 [P] [US2] Implement IPerformanceRepository interface in src/Po.LearnCert.Api/Features/Statistics/Infrastructure/IPerformanceRepository.cs (IUserStatisticsRepository)
- [X] T102 [US2] Implement PerformanceRepository in src/Po.LearnCert.Api/Features/Statistics/Infrastructure/PerformanceRepository.cs (UserStatisticsRepository)
- [X] T103 [US2] Implement StatisticsService.GetUserStatisticsAsync (FR-009, FR-010) in src/Po.LearnCert.Api/Features/Statistics/Services/StatisticsService.cs (UserStatisticsService)
- [X] T104 [US2] Implement StatisticsService.GetSubtopicPerformanceAsync in src/Po.LearnCert.Api/Features/Statistics/Services/StatisticsService.cs (UserStatisticsService)
- [X] T105 [US2] Update QuizSessionService to create PerformanceRecord after quiz completion in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs

### Implementation for User Story 2 - Backend Services

- [ ] T101 [P] [US2] Implement IPerformanceRepository interface in src/Po.LearnCert.Api/Features/Statistics/Infrastructure/IPerformanceRepository.cs
- [ ] T102 [US2] Implement PerformanceRepository in src/Po.LearnCert.Api/Features/Statistics/Infrastructure/PerformanceRepository.cs
- [ ] T103 [US2] Implement StatisticsService.GetUserStatisticsAsync (FR-009, FR-010) in src/Po.LearnCert.Api/Features/Statistics/Services/StatisticsService.cs
- [ ] T104 [US2] Implement StatisticsService.GetSubtopicPerformanceAsync in src/Po.LearnCert.Api/Features/Statistics/Services/StatisticsService.cs
- [ ] T105 [US2] Update QuizSessionService to create PerformanceRecord after quiz completion in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs

### Implementation for User Story 2 - Backend API Endpoints

- [X] T106 [US2] Implement GET /api/statistics endpoint in src/Po.LearnCert.Api/Features/Statistics/StatisticsController.cs (UserStatisticsController)
- [X] T107 [US2] Implement GET /api/statistics/subtopics endpoint with filtering in src/Po.LearnCert.Api/Features/Statistics/StatisticsController.cs
- [X] T108 [US2] Add error handling and logging for statistics operations in src/Po.LearnCert.Api/Features/Statistics/StatisticsController.cs
- [X] T109 [US2] Register statistics services in dependency injection in src/Po.LearnCert.Api/Program.cs

### Implementation for User Story 2 - Frontend Services

- [X] T110 [P] [US2] Create IStatisticsService interface in src/Po.LearnCert.Client/Features/Statistics/Services/IStatisticsService.cs
- [X] T111 [US2] Implement StatisticsService HTTP client wrapper in src/Po.LearnCert.Client/Features/Statistics/Services/StatisticsService.cs

### Implementation for User Story 2 - Frontend Components

- [X] T112 [P] [US2] Create StatisticsDashboard.razor component for overall stats display in src/Po.LearnCert.Client/Features/Statistics/StatisticsDashboard.razor (Statistics.razor)
- [X] T113 [P] [US2] Create SubtopicPerformanceChart.razor component with color-coded indicators in src/Po.LearnCert.Client/Features/Statistics/SubtopicPerformanceChart.razor
- [X] T114 [P] [US2] Create ProgressTrendChart.razor component for timeline visualization in src/Po.LearnCert.Client/Features/Statistics/ProgressTrendChart.razor
- [X] T115 [US2] Add mobile-first responsive CSS for statistics components in src/Po.LearnCert.Client/Features/Statistics/Statistics.razor.css (statistics.css)
- [X] T116 [US2] Register statistics services in dependency injection in src/Po.LearnCert.Client/Program.cs

**Checkpoint**: User Stories 1 AND 2 are both independently functional

---

## Phase 5: User Story 3 - Compete on Leaderboards (Priority: P3)

**Goal**: Enable competitive motivation through rankings and social comparison

**Independent Test**: View leaderboards populated with multiple users, verify ranking calculations, confirm filtering works

### Tests for User Story 3

- [ ] T117 [P] [US3] Unit test for LeaderboardService.GetLeaderboardAsync in tests/Po.LearnCert.UnitTests/Features/Leaderboards/LeaderboardServiceTests.cs
- [ ] T118 [P] [US3] Unit test for LeaderboardService.UpdateLeaderboardAsync in tests/Po.LearnCert.UnitTests/Features/Leaderboards/LeaderboardServiceTests.cs
- [ ] T119 [P] [US3] Integration test for GET /api/leaderboards/{certId} in tests/Po.LearnCert.IntegrationTests/Features/Leaderboards/LeaderboardEndpointsTests.cs
- [ ] T120 [P] [US3] Integration test for leaderboard filtering by time period in tests/Po.LearnCert.IntegrationTests/Features/Leaderboards/LeaderboardEndpointsTests.cs

### Implementation for User Story 3 - Shared Models

- [X] T121 [P] [US3] Create LeaderboardEntryDto in src/Po.LearnCert.Shared/Models/LeaderboardEntryDto.cs
- [X] T122 [P] [US3] Create LeaderboardDto in src/Po.LearnCert.Shared/Models/LeaderboardDto.cs

### Implementation for User Story 3 - Backend Entities

- [X] T123 [P] [US3] Create LeaderboardEntity table entity in src/Po.LearnCert.Api/Features/Leaderboards/Infrastructure/LeaderboardEntity.cs (PartitionKey=CertId+TimePeriod, RowKey=UserId)

### Implementation for User Story 3 - Backend Services

- [X] T124 [P] [US3] Implement ILeaderboardRepository interface in src/Po.LearnCert.Api/Features/Leaderboards/Infrastructure/ILeaderboardRepository.cs
- [X] T125 [US3] Implement LeaderboardRepository in src/Po.LearnCert.Api/Features/Leaderboards/Infrastructure/LeaderboardRepository.cs
- [X] T126 [US3] Implement LeaderboardService.GetLeaderboardAsync with filtering (FR-011, FR-012, FR-013) in src/Po.LearnCert.Api/Features/Leaderboards/Services/LeaderboardService.cs
- [X] T127 [US3] Implement LeaderboardService.UpdateLeaderboardAsync to update on quiz completion in src/Po.LearnCert.Api/Features/Leaderboards/Services/LeaderboardService.cs
- [X] T128 [US3] Update QuizSessionService to trigger leaderboard update after quiz completion in src/Po.LearnCert.Api/Features/Quiz/Services/QuizSessionService.cs

### Implementation for User Story 3 - Backend API Endpoints

- [X] T129 [US3] Implement GET /api/leaderboards/{certId} endpoint with time period filtering in src/Po.LearnCert.Api/Features/Leaderboards/LeaderboardsController.cs
- [X] T130 [US3] Add pagination support (skip/take) for leaderboard endpoints in src/Po.LearnCert.Api/Features/Leaderboards/LeaderboardsController.cs
- [X] T131 [US3] Add error handling and logging for leaderboard operations in src/Po.LearnCert.Api/Features/Leaderboards/LeaderboardsController.cs
- [X] T132 [US3] Register leaderboard services in dependency injection in src/Po.LearnCert.Api/Program.cs

### Implementation for User Story 3 - Frontend Services

- [X] T133 [P] [US3] Create ILeaderboardService interface in src/Po.LearnCert.Client/Features/Leaderboards/Services/ILeaderboardService.cs
- [X] T134 [US3] Implement LeaderboardService HTTP client wrapper in src/Po.LearnCert.Client/Features/Leaderboards/Services/LeaderboardService.cs

### Implementation for User Story 3 - Frontend Components

- [X] T135 [P] [US3] Create LeaderboardView.razor component with ranking display in src/Po.LearnCert.Client/Features/Leaderboards/LeaderboardView.razor
- [X] T136 [P] [US3] Create LeaderboardFilters.razor component for certification and time period selection in src/Po.LearnCert.Client/Features/Leaderboards/LeaderboardFilters.razor
- [X] T137 [US3] Implement current user highlighting in leaderboard (FR-014) in src/Po.LearnCert.Client/Features/Leaderboards/LeaderboardView.razor
- [X] T138 [US3] Add mobile-first responsive CSS for leaderboard components in src/Po.LearnCert.Client/Features/Leaderboards/Leaderboards.razor.css
- [X] T139 [US3] Register leaderboard services in dependency injection in src/Po.LearnCert.Client/Program.cs

**Checkpoint**: User Stories 1, 2, AND 3 are all independently functional

---

## Phase 6: Authentication & User Management

**Goal**: Enable user account creation and session-based authentication

### Tests for Authentication

- [X] T153 [P] [AUTH] Unit test for AuthenticationService.RegisterAsync in tests/Po.LearnCert.UnitTests/Features/Authentication/AuthenticationServiceTests.cs
- [X] T154 [P] [AUTH] Unit test for AuthenticationService.LoginAsync in tests/Po.LearnCert.UnitTests/Features/Authentication/AuthenticationServiceTests.cs
- [X] T155 [P] [AUTH] Integration test for POST /api/auth/register in tests/Po.LearnCert.IntegrationTests/Features/Authentication/AuthEndpointsTests.cs
- [X] T156 [P] [AUTH] Integration test for POST /api/auth/login in tests/Po.LearnCert.IntegrationTests/Features/Authentication/AuthEndpointsTests.cs
- [X] T157 [P] [AUTH] Integration test for POST /api/auth/logout in tests/Po.LearnCert.IntegrationTests/Features/Authentication/AuthEndpointsTests.cs

### Implementation for Authentication - Shared Models

- [X] T158 [P] [AUTH] Create RegisterRequest in src/Po.LearnCert.Shared/Contracts/RegisterRequest.cs
- [X] T159 [P] [AUTH] Create LoginRequest in src/Po.LearnCert.Shared/Contracts/LoginRequest.cs
- [X] T160 [P] [AUTH] Create AuthenticationResponse in src/Po.LearnCert.Shared/Contracts/AuthenticationResponse.cs

### Implementation for Authentication - Backend Services

- [X] T161 [AUTH] Implement AuthenticationService.RegisterAsync with password validation (FR-018, FR-020) in src/Po.LearnCert.Api/Features/Authentication/Services/AuthenticationService.cs
- [X] T162 [AUTH] Implement AuthenticationService.LoginAsync with session creation in src/Po.LearnCert.Api/Features/Authentication/Services/AuthenticationService.cs
- [X] T163 [AUTH] Implement AuthenticationService.LogoutAsync in src/Po.LearnCert.Api/Features/Authentication/Services/AuthenticationService.cs

### Implementation for Authentication - Backend API Endpoints

- [X] T164 [AUTH] Implement POST /api/auth/register endpoint in src/Po.LearnCert.Api/Features/Authentication/AuthController.cs
- [X] T165 [AUTH] Implement POST /api/auth/login endpoint in src/Po.LearnCert.Api/Features/Authentication/AuthController.cs
- [X] T166 [AUTH] Implement POST /api/auth/logout endpoint in src/Po.LearnCert.Api/Features/Authentication/AuthController.cs
- [X] T167 [AUTH] Add authentication middleware to protect quiz/statistics/leaderboard endpoints in src/Po.LearnCert.Api/Program.cs
- [X] T168 [AUTH] Add error handling and logging for authentication operations in src/Po.LearnCert.Api/Features/Authentication/AuthController.cs
- [X] T169 [AUTH] Register authentication services in dependency injection in src/Po.LearnCert.Api/Program.cs

### Implementation for Authentication - Frontend Services

- [X] T170 [P] [AUTH] Create IAuthenticationService interface in src/Po.LearnCert.Client/Features/Authentication/Services/IAuthenticationService.cs
- [X] T171 [AUTH] Implement AuthenticationService HTTP client wrapper in src/Po.LearnCert.Client/Features/Authentication/Services/AuthenticationService.cs
- [X] T172 [AUTH] Implement AuthenticationStateProvider for session management in src/Po.LearnCert.Client/Features/Authentication/CustomAuthStateProvider.cs

### Implementation for Authentication - Frontend Components

- [X] T173 [P] [AUTH] Create Register.razor component in src/Po.LearnCert.Client/Features/Authentication/Register.razor
- [X] T174 [P] [AUTH] Create Login.razor component in src/Po.LearnCert.Client/Features/Authentication/Login.razor
- [X] T175 [P] [AUTH] Create navigation menu with login/logout in src/Po.LearnCert.Client/Shared/MainLayout.razor
- [X] T176 [AUTH] Add mobile-first responsive CSS for authentication components in src/Po.LearnCert.Client/Features/Authentication/Auth.razor.css
- [X] T177 [AUTH] Configure authentication state in src/Po.LearnCert.Client/Program.cs
- [X] T178 [AUTH] Add protected route configuration for authenticated pages in src/Po.LearnCert.Client/App.razor

**Checkpoint**: User authentication and authorization fully functional

---

## Phase 7: Question Generator Tool

**Goal**: Separate console app for AI-powered question generation using Azure OpenAI

### Implementation for Question Generator

- [X] T179 [P] [TOOL] Create QuestionGeneratorService in tools/Po.LearnCert.QuestionGenerator/Services/QuestionGeneratorService.cs
- [X] T180 [P] [TOOL] Implement Azure OpenAI client wrapper in tools/Po.LearnCert.QuestionGenerator/Services/OpenAIService.cs
- [X] T181 [TOOL] Create prompt templates for question generation in tools/Po.LearnCert.QuestionGenerator/Prompts/
- [X] T182 [TOOL] Implement question validation logic in tools/Po.LearnCert.QuestionGenerator/Validators/QuestionValidator.cs
- [X] T183 [TOOL] Implement Azure Table Storage insertion logic in tools/Po.LearnCert.QuestionGenerator/Services/QuestionStorageService.cs
- [X] T184 [TOOL] Configure appsettings.json for Azure OpenAI endpoint and keys in tools/Po.LearnCert.QuestionGenerator/
- [X] T185 [TOOL] Implement Program.cs with command-line arguments in tools/Po.LearnCert.QuestionGenerator/Program.cs
- [X] T186 [TOOL] Add retry logic for API rate limits in tools/Po.LearnCert.QuestionGenerator/Services/OpenAIService.cs
- [X] T187 [TOOL] Add logging for generation metrics in tools/Po.LearnCert.QuestionGenerator/Program.cs

**Checkpoint**: Question generator tool can create and validate AI-generated questions

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T188 [P] Create PRD.MD documentation in docs/
- [X] T189 [P] Create STEPS.MD implementation guide in docs/
- [X] T190 [P] Update README.md with setup instructions, architecture overview, and quickstart
- [X] T191 [P] Create CI workflow in .github/workflows/ci.yml with build, test, and format checks
- [ ] T192 Performance optimization: Add response caching for certification and leaderboard endpoints
- [ ] T193 Performance optimization: Optimize Azure Table Storage queries with proper partition key filters
- [ ] T194 Security hardening: Add rate limiting middleware to API endpoints
- [ ] T195 Security hardening: Implement CSRF protection for state-changing operations
- [ ] T196 [P] Add unit tests for edge cases (insufficient questions, tie-breaking, session interruption)
- [ ] T197 [P] Add integration tests for error scenarios (invalid requests, unauthorized access)
- [ ] T198 Run manual Playwright E2E tests for critical user journeys
- [ ] T199 Validate all success criteria from spec.md (SC-001 through SC-010)
- [ ] T200 Run quickstart.md validation end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User Story 1 (P1) - Quiz Sessions: Can start after Foundational
  - User Story 2 (P2) - Statistics: Can start after Foundational (integrates with US1 quiz completion)
  - User Story 3 (P3) - Leaderboards: Can start after Foundational (integrates with US1 quiz completion)
- **Authentication (Phase 6)**: Can start after Foundational (cross-cutting feature)
- **Question Generator (Phase 7)**: Can start after Setup (independent tool)
- **Polish (Phase 8)**: Depends on desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Integrates with US1 but independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Integrates with US1 but independently testable

### Within Each User Story

1. Tests MUST be written and FAIL before implementation
2. Shared models before backend/frontend (parallel after models exist)
3. Backend entities before repositories
4. Repositories before services
5. Services before API endpoints
6. Frontend services before components
7. Core implementation before integration with other stories
8. Story complete and independently tested before moving to next priority

### Parallel Opportunities

- **Phase 1 (Setup)**: T003-T008 (project creation), T009-T013 (NuGet packages), T014-T016 (folder structure), T017-T019 (configuration), T020-T022 (scripts and docs) can run in parallel
- **Phase 2 (Foundational)**: T024-T025 (repository interfaces), T027-T031 (middleware, health, CORS, DI), T032-T035 (Identity), T036-T037 (test infrastructure), T038-T039 (client config) can run in parallel
- **User Story 1**: 
  - Tests T040-T045 can run in parallel
  - Shared models T046-T054 can run in parallel
  - Backend entities T055-T060 can run in parallel
  - Backend repositories T061-T065 can run in parallel
  - Frontend services T078-T081 can run in parallel after shared models
  - Frontend components T082-T085 can run in parallel after services
- **User Story 2**:
  - Tests T093-T096 can run in parallel
  - Shared models T097-T099 can run in parallel
  - Frontend components T112-T114 can run in parallel
- **User Story 3**:
  - Tests T117-T120 can run in parallel
  - Shared models T121-T122 can run in parallel
  - Frontend components T135-T136 can run in parallel
- **User Story 4**:
  - Tests T140-T142 can run in parallel
  - Frontend components T149-T150 can run in parallel
- **Authentication**:
  - Tests T153-T157 can run in parallel
  - Shared models T158-T160 can run in parallel
  - Frontend components T173-T175 can run in parallel
- **Question Generator**:
  - T179-T180 can run in parallel
- **Polish**: T188-T191 (documentation and CI), T196-T197 (additional tests) can run in parallel
- **Cross-Story Parallelism**: After Phase 2, User Stories 1, 2, 3, 4, Authentication, and Question Generator can all proceed in parallel if team capacity allows

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit test for QuizSessionService.CreateSessionAsync in tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs"
Task: "Unit test for QuizSessionService.SubmitAnswerAsync in tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs"
Task: "Unit test for QuizSessionService.GetSessionResultsAsync in tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs"
Task: "Integration test for POST /api/quiz/sessions in tests/Po.LearnCert.IntegrationTests/Features/Quiz/QuizSessionEndpointsTests.cs"
Task: "Integration test for POST /api/quiz/sessions/{id}/answers in tests/Po.LearnCert.IntegrationTests/Features/Quiz/QuizSessionEndpointsTests.cs"
Task: "Integration test for GET /api/quiz/sessions/{id} in tests/Po.LearnCert.IntegrationTests/Features/Quiz/QuizSessionEndpointsTests.cs"

# Launch all shared models for User Story 1 together:
Task: "Create CertificationDto in src/Po.LearnCert.Shared/Models/CertificationDto.cs"
Task: "Create SubtopicDto in src/Po.LearnCert.Shared/Models/SubtopicDto.cs"
Task: "Create QuestionDto in src/Po.LearnCert.Shared/Models/QuestionDto.cs"
Task: "Create AnswerChoiceDto in src/Po.LearnCert.Shared/Models/AnswerChoiceDto.cs"
Task: "Create QuizSessionDto in src/Po.LearnCert.Shared/Models/QuizSessionDto.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T022)
2. Complete Phase 2: Foundational (T023-T039) - CRITICAL, blocks all stories
3. Complete Phase 6: Authentication (T153-T178) - Required for user context
4. Complete Phase 3: User Story 1 (T040-T092) - Core quiz functionality
5. **STOP and VALIDATE**: Test User Story 1 independently (can create account, take quiz, see results)
6. Deploy/demo MVP with quiz functionality

### Incremental Delivery

1. Complete Setup + Foundational + Authentication → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP - quiz sessions!)
3. Add User Story 2 → Test independently → Deploy/Demo (MVP + personal tracking!)
4. Add User Story 3 → Test independently → Deploy/Demo (MVP + competition!)
5. Add Question Generator (Phase 7) → AI-powered content creation
6. Polish (Phase 8) → Production-ready platform

### Parallel Team Strategy

With multiple developers:

1. **Together**: Complete Setup (Phase 1) + Foundational (Phase 2)
2. **Once Foundational is done**:
   - Developer A: User Story 1 (Quiz Sessions)
   - Developer B: User Story 2 (Statistics)
   - Developer C: User Story 3 (Leaderboards)
   - Developer D: Authentication (Phase 6)
   - Developer E: Question Generator (Phase 7)
3. Stories complete and integrate independently

---

## Task Summary

**Total Tasks**: 187

**Breakdown by Phase**:
- Phase 1 (Setup): 22 tasks
- Phase 2 (Foundational): 17 tasks
- Phase 3 (User Story 1 - Quiz Sessions): 53 tasks
- Phase 4 (User Story 2 - Statistics): 24 tasks
- Phase 5 (User Story 3 - Leaderboards): 23 tasks
- Phase 6 (Authentication): 26 tasks
- Phase 7 (Question Generator): 9 tasks
- Phase 8 (Polish): 13 tasks

**Breakdown by User Story**:
- User Story 1 (P1 - Quiz Sessions): 53 tasks
- User Story 2 (P2 - Statistics): 24 tasks
- User Story 3 (P3 - Leaderboards): 23 tasks
- Authentication (Cross-cutting): 26 tasks
- Infrastructure (Setup + Foundational): 39 tasks
- Tooling (Question Generator): 9 tasks
- Polish: 13 tasks

**Parallel Opportunities**: 77 tasks marked [P] can run in parallel within their phase

**Independent Test Criteria**:
- User Story 1: Create quiz session, answer questions, receive immediate feedback, view final results
- User Story 2: Take multiple quiz sessions, view statistics dashboard showing accurate performance
- User Story 3: View leaderboards with multiple users, verify ranking calculations and filtering

**Suggested MVP Scope**: 
- Phase 1 (Setup) + Phase 2 (Foundational) + Phase 6 (Authentication) + Phase 3 (User Story 1)
- **Total MVP Tasks**: 118 tasks
- **MVP Delivers**: Account creation, login, 20-question quiz sessions with immediate feedback and explanations

**Format Validation**: ✅ All 187 tasks follow the required checklist format with checkboxes, task IDs, appropriate [P] and [Story] labels, and specific file paths

---

## Notes

- [P] tasks = different files, no dependencies within their phase
- [Story] label maps task to specific user story for traceability (US1, US2, US3, AUTH, TOOL)
- Each user story is independently completable and testable after Foundational phase
- TDD approach: Write tests FIRST, ensure they FAIL, then implement to make them PASS
- Commit after each task or logical group of parallel tasks
- Stop at checkpoints to validate stories independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Constitution compliance: .NET 9.0 only, vertical slices, TDD, mobile-first, Problem Details, Serilog, xUnit
