# PoLearnCert - Certification Quiz Platform

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-blue)](https://blazor.net/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

> An intelligent online learning platform for technical certification exam preparation through AI-powered quizzes with immediate feedback.

---

## 🎯 Overview

PoLearnCert is a comprehensive web-based learning platform designed to help IT professionals master technical certifications through:

- **📝 Interactive Quiz Sessions**: 20-question rapid-fire quizzes with instant feedback
- **📊 Performance Analytics**: Detailed statistics showing strengths and weaknesses
- **🏆 Competitive Leaderboards**: Time-based rankings to motivate learners
- **🤖 AI-Powered Questions**: Azure OpenAI-generated questions with quality validation

### Supported Certifications
- ✅ Microsoft Azure Fundamentals (AZ-900)
- ✅ CompTIA Security+ (SY0-701)

---

## ✨ Key Features

### For Learners
- **Immediate Feedback**: Get explanations for every answer in <1 second
- **Adaptive Learning**: Focus on weak areas with subtopic performance tracking
- **Mobile-First Design**: Study anywhere on any device
- **Progress Tracking**: View detailed historical performance and trends

### For Administrators
- **AI Question Generation**: Generate hundreds of questions using Azure OpenAI
- **Quality Control**: Automated validation ensures 4 choices and 1 correct answer
- **Scalable Architecture**: Support 100+ concurrent users
- **Health Monitoring**: Built-in health checks and structured logging

---

## 🏗️ Architecture

### Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | Blazor WebAssembly (.NET 9.0) | Single-page application |
| **Backend** | ASP.NET Core Web API (.NET 9.0) | RESTful API |
| **Authentication** | ASP.NET Core Identity | User management & sessions |
| **Database** | Azure Table Storage | NoSQL data persistence |
| **Caching** | In-memory | Performance optimization |
| **Logging** | Serilog | Structured logging |
| **Testing** | xUnit, Playwright | Unit, integration, E2E tests |
| **AI** | Azure OpenAI (GPT-4o) | Question generation |

### System Architecture

```
┌─────────────────────────────────────────────┐
│           Blazor WASM Client                │
│  (Quiz UI, Statistics, Leaderboards)        │
└──────────────────┬──────────────────────────┘
                   │ HTTPS (5001)
                   ▼
┌─────────────────────────────────────────────┐
│         ASP.NET Core Web API                │
│  ┌──────────┬──────────┬──────────────┐    │
│  │  Quiz    │  Stats   │ Leaderboards │    │
│  │ Sessions │  Service │   Service    │    │
│  └──────────┴──────────┴──────────────┘    │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│        Azure Table Storage                  │
│  (Questions, Sessions, Stats, Leaderboards) │
└─────────────────────────────────────────────┘

External Tool:
┌─────────────────────────────────────────────┐
│     Question Generator Console App          │
│   (Azure OpenAI GPT-4o Integration)         │
└─────────────────────────────────────────────┘
```

### Project Structure

```
PoLearnCert/
├── src/
│   ├── Po.LearnCert.Api/               # Web API backend
│   │   ├── Features/
│   │   │   ├── Quiz/                   # Quiz sessions
│   │   │   ├── Statistics/             # Performance tracking
│   │   │   ├── Leaderboards/           # Rankings
│   │   │   ├── Certifications/         # Cert metadata
│   │   │   └── Authentication/         # User auth
│   │   └── Program.cs
│   ├── Po.LearnCert.Client/            # Blazor WASM frontend
│   │   ├── Features/
│   │   │   ├── Quiz/                   # Quiz components
│   │   │   ├── Statistics/             # Stats dashboard
│   │   │   └── Leaderboards/           # Leaderboard views
│   │   └── Program.cs
│   └── Po.LearnCert.Shared/            # Shared models & DTOs
├── tests/
│   ├── Po.LearnCert.UnitTests/         # Unit tests (xUnit)
│   ├── Po.LearnCert.IntegrationTests/  # Integration tests
│   └── Po.LearnCert.E2ETests/          # E2E tests (Playwright)
├── tools/
│   └── Po.LearnCert.QuestionGenerator/ # AI question generator
├── docs/
│   ├── PRD.md                          # Product requirements
│   └── STEPS.md                        # Implementation guide
├── specs/
│   └── 001-cert-quiz-platform/         # Feature specifications
└── .github/
    └── workflows/
        └── ci.yml                      # CI/CD pipeline
```

---

## 🚀 Quick Start

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0+ | Build & run application |
| [Node.js](https://nodejs.org/) | 20+ | E2E testing (optional) |
| [Azurite](https://github.com/Azure/Azurite) | Latest | Local storage emulator |

### Installation

#### 1. Clone the repository

```bash
git clone https://github.com/your-org/PoLearnCert.git
cd PoLearnCert
```

#### 2. Start Azurite (Azure Storage Emulator)

```bash
# Install Azurite globally (one-time setup)
npm install -g azurite

# Start Azurite
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

**Tip**: Keep Azurite running in a separate terminal window while developing.

#### 3. Configure API settings (optional)

The API uses `appsettings.Development.json` by default with Azurite connection strings. For production, update `appsettings.json`:

```json
{
  "AzureTableStorage": {
    "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING"
  }
}
```

#### 4. Build the solution

```bash
dotnet build
```

#### 5. Seed questions with AI-generated content

The Question Generator uses Azure OpenAI to create realistic certification questions:

```bash
cd tools/Po.LearnCert.QuestionGenerator

# Configure Azure OpenAI settings in appsettings.json first
# Then generate questions (e.g., 10 per subtopic)
dotnet run -- --count 10
```

**Note**: Requires Azure OpenAI resource. See [Question Generator README](tools/Po.LearnCert.QuestionGenerator/README.md) for setup.

#### 6. Run the application

**Option A: Visual Studio**
1. Open `PoLearnCert.sln`
2. Set `Po.LearnCert.Api` as startup project
3. Press F5 to run

**Option B: Command Line**
```bash
cd src/Po.LearnCert.Api
dotnet run
```

The application will start at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001

---

## 📖 Usage

### Taking a Quiz

1. Navigate to http://localhost:5000
2. Click **"Start Quiz"**
3. Select a certification (AZ-900 or Security+)
4. Choose subtopics (minimum 1)
5. Set number of questions per subtopic (1-50)
6. Answer questions and receive instant feedback
7. View your results at the end

### Viewing Statistics

1. Click **"Statistics"** in the navigation menu
2. View metrics:
   - **Overall accuracy** (percentage of correct answers)
   - **Total sessions completed**
   - **Average score** across all quizzes
   - **Performance by certification** (AZ-900, Security+)
   - **Performance by subtopic** (8 subtopics available)

### Checking Leaderboards

1. Click **"Leaderboards"** in the navigation menu
2. Select a certification tab (AZ-900 or Security+)
3. View top performers ranked by:
   - **Total score** (primary sort)
   - **Accuracy percentage** (secondary sort)
4. See your rank highlighted if you've completed sessions

### Generating Questions (Administrator Tool)

**Interactive Mode:**
```bash
cd tools/Po.LearnCert.QuestionGenerator
dotnet run
# Follow prompts to enter question count and confirmation
```

**CLI Mode (Automated):**
```bash
dotnet run -- 50        # Generate 50 questions per subtopic with confirmation
dotnet run -- 25 -y     # Generate 25 questions per subtopic, skip confirmation
dotnet run -- --help    # Display usage information
```

**Features:**
- AI-powered question generation using Azure OpenAI GPT-4o
- Automatic retry with exponential backoff (handles rate limits)
- Validates 4 choices per question with 1 correct answer
- Seeds questions directly to Azure Table Storage
- Progress indicators and generation metrics

---

## 🧪 Testing

### Run Unit Tests

```bash
cd tests/Po.LearnCert.UnitTests
dotnet test
```

### Run Integration Tests

```bash
cd tests/Po.LearnCert.IntegrationTests
dotnet test
```

### Run E2E Tests (Playwright)

```bash
cd tests/Po.LearnCert.E2ETests
npx playwright test
```

For detailed testing strategy, see [STEPS.md](docs/STEPS.md#testing-strategy).

---

## 🏗️ Development

### Project Conventions

- **Vertical Slice Architecture**: Organize by feature, not layer
- **Minimal APIs**: Use endpoint routing for clean, focused APIs
- **CQRS**: Separate read and write operations where beneficial
- **TDD**: Write tests first, then implementation

### Adding a New Feature

1. Create feature folder: `src/Po.LearnCert.Api/Features/YourFeature/`
2. Add endpoint: `YourFeatureEndpoint.cs`
3. Add models: `YourFeatureModels.cs`
4. Add service: `YourFeatureService.cs`
5. Write tests: `tests/Po.LearnCert.UnitTests/Features/YourFeature/`
6. Register services in `Program.cs`

For detailed workflow, see [STEPS.md](docs/STEPS.md#development-workflow).

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

### Code Quality Standards

- ✅ Write unit tests for all new features
- ✅ Maintain >80% code coverage
- ✅ Follow C# naming conventions
- ✅ Use async/await for I/O operations
- ✅ Add XML documentation for public APIs

### Pull Request Process

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature-name`
3. **Commit** changes: `git commit -m 'Add: Description of feature'`
4. **Push** to branch: `git push origin feature/your-feature-name`
5. **Open** a pull request with:
   - Clear description of changes
   - Reference to related issues
   - Test results and coverage

### Commit Message Format

Use conventional commits:
```
<type>: <description>

[optional body]
[optional footer]
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Example**:
```
feat: Add performance tracking by certification

- Track overall accuracy per certification
- Display stats in dashboard
- Add API endpoint /api/statistics/by-certification

Closes #42
```

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **[PRD.md](docs/PRD.md)** - Product Requirements Document
- **[STEPS.md](docs/STEPS.md)** - Implementation Guide & Developer Onboarding
- **[Specifications](specs/001-cert-quiz-platform/)** - Detailed feature specifications

---

## 🙏 Acknowledgments

- **Azure OpenAI (GPT-4o)** - AI-powered question generation
- **Blazor WebAssembly** - Modern web UI framework
- **Playwright** - Reliable end-to-end testing
- **Azurite** - Local Azure Storage emulation

---

## 🐛 Troubleshooting

### Common Issues

**Problem**: `Azurite is not running`
```bash
# Solution: Start Azurite manually
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

**Problem**: `Port 5000/5001 already in use`
```bash
# Solution: Kill the process using the port (PowerShell)
Get-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess | Stop-Process
```

**Problem**: `No questions found in database`
```bash
# Solution: Seed questions using the generator tool
cd tools/Po.LearnCert.QuestionGenerator
dotnet run -- 10 -y
```

For more troubleshooting, see [STEPS.md](docs/STEPS.md#troubleshooting).

---

## 🚀 Deployment

### Deploy to Azure App Service

```bash
# 1. Build for production
dotnet publish src/Po.LearnCert.Api -c Release -o ./publish

# 2. Deploy to Azure (requires Azure CLI)
az webapp up --name your-app-name --resource-group your-rg --runtime "DOTNET|9.0"
```

For detailed deployment instructions, see [STEPS.md](docs/STEPS.md#deployment).

---

**Built with ❤️ using .NET 9.0 and Blazor WebAssembly**

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
