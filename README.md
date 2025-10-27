# PoLearnCert - Certification Quiz Platform

An online learning platform for technical certification exam preparation through rapid-fire quizzes.

## Overview

PoLearnCert helps individuals prepare for technical certification exams through 20-question quiz sessions with immediate feedback. The platform tracks performance statistics, provides competitive leaderboards, and supports Microsoft Azure Fundamentals (AZ-900) and CompTIA Security+ certifications.

## Technology Stack

- **.NET 9.0**: Latest .NET SDK
- **Blazor WebAssembly**: Frontend SPA framework
- **ASP.NET Core Web API**: Backend REST API
- **Azure Table Storage**: Primary data persistence (Azurite for local development)
- **ASP.NET Core Identity**: Email/password session-based authentication
- **Serilog**: Structured logging
- **xUnit**: Unit and integration testing
- **Azure OpenAI**: Question generation tool

## Project Structure

```
/src
  /Po.LearnCert.Api          # ASP.NET Core Web API backend
  /Po.LearnCert.Client       # Blazor WebAssembly frontend
  /Po.LearnCert.Shared       # Shared DTOs and contracts

/tests
  /Po.LearnCert.UnitTests
  /Po.LearnCert.IntegrationTests

/tools
  /Po.LearnCert.QuestionGenerator  # Azure OpenAI question generator

/docs                       # Project documentation
/scripts                    # PowerShell scripts (Azurite, seeding)
```

## Prerequisites

- .NET 9.0 SDK or later
- Node.js (for Azurite)
- Azurite (`npm install -g azurite`)

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd PoLearnCert
```

### 2. Start Azurite (Azure Storage Emulator)

```powershell
.\scripts\start-azurite.ps1
```

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Run the API

```bash
cd src/Po.LearnCert.Api
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

### 5. Run the Blazor Client

```bash
cd src/Po.LearnCert.Client
dotnet run
```

### 6. Run tests

```bash
# Unit tests
dotnet test tests/Po.LearnCert.UnitTests

# Integration tests (requires Azurite running)
dotnet test tests/Po.LearnCert.IntegrationTests
```

## Features

### Phase 1: Core Quiz Functionality (MVP)
- ✅ User authentication (email/password)
- ✅ 20-question quiz sessions
- ✅ Immediate answer feedback with explanations
- ✅ Final quiz results (score, time, accuracy)

### Phase 2: Performance Tracking
- ⏳ Personal statistics dashboard
- ⏳ Performance by certification and subtopic
- ⏳ Progress trends over time

### Phase 3: Competitive Features
- ⏳ Global leaderboards
- ⏳ Filtering by certification and time period
- ⏳ User ranking highlights

### Phase 4: Certification Information
- ⏳ Certification browsing
- ⏳ Detailed certification descriptions
- ⏳ Subtopic coverage information

## Architecture

### Vertical Slice Architecture

The application is organized by features (vertical slices) rather than technical layers:

- **Quiz**: Session management, question delivery, answer submission
- **Statistics**: Performance tracking and analytics
- **Leaderboards**: Rankings and competition
- **Certifications**: Certification information and browsing
- **Authentication**: User management and session-based auth

### API Endpoints

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `GET /api/health` - Health check
- `GET /api/certifications` - List certifications
- `GET /api/certifications/{id}` - Get certification details
- `POST /api/quiz/sessions` - Start quiz session
- `POST /api/quiz/sessions/{id}/answers` - Submit answer
- `GET /api/quiz/sessions/{id}` - Get session details
- `GET /api/statistics` - Get user statistics
- `GET /api/leaderboards/{certId}` - Get leaderboard

## Contributing

1. Follow TDD approach: write tests first
2. Use vertical slice organization
3. Follow .NET coding conventions
4. All API errors must use RFC 7807 Problem Details
5. Log structured events with Serilog

## License

[License TBD]

## Support

For issues and questions, please file an issue in the repository.
