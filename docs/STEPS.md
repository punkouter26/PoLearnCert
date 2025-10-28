# Implementation Steps Guide

## PoLearnCert - Development Workflow

This document provides step-by-step instructions for setting up, developing, and deploying the PoLearnCert platform.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Running the Application](#running-the-application)
4. [Development Workflow](#development-workflow)
5. [Testing Strategy](#testing-strategy)
6. [Question Generation](#question-generation)
7. [Deployment](#deployment)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software
- **.NET 9.0 SDK** (latest patch)
  - Download: https://dotnet.microsoft.com/download/dotnet/9.0
  - Verify: `dotnet --version` should show 9.x.x

- **Azurite** (Azure Storage Emulator)
  - Install via npm: `npm install -g azurite`
  - Or use Visual Studio Azurite extension

- **Node.js 20+** (for E2E tests)
  - Download: https://nodejs.org/
  - Verify: `node --version`

### Optional but Recommended
- **Visual Studio 2022** (v17.8+) or **Visual Studio Code**
- **Azure Storage Explorer** (for viewing table data)
- **Postman** or **Thunder Client** (for API testing)

### Azure Resources (Optional for AI Features)
- Azure OpenAI Service endpoint
- API key for GPT-4o deployment

---

## Local Development Setup

### Step 1: Clone Repository
```bash
git clone <repository-url>
cd PoLearnCert
```

### Step 2: Start Azurite
Open a terminal and run:
```bash
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

Or start via Visual Studio:
- View → Other Windows → Azurite

### Step 3: Verify Configuration

**API Configuration** (`src/Po.LearnCert.Api/appsettings.Development.json`):
```json
{
  "AzureTableStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Question Generator Configuration** (`tools/Po.LearnCert.QuestionGenerator/appsettings.json`):
```json
{
  "AzureTableStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  },
  "AzureOpenAI": {
    "Endpoint": "<your-endpoint>",
    "Key": "<your-key>",
    "DeploymentName": "gpt-4o"
  },
  "SeedData": {
    "QuestionsPerSubtopic": 25
  }
}
```

### Step 4: Build Solution
```bash
dotnet build
```

Expected output: All projects build successfully

### Step 5: Seed Database (Optional)
Generate questions using AI:
```bash
cd tools/Po.LearnCert.QuestionGenerator
dotnet run -- --count 5  # Generate 5 questions per subtopic
```

**Note**: This uses Azure OpenAI to create realistic certification questions. Configure your Azure OpenAI endpoint in `appsettings.json` first.

---

## Running the Application

### Option 1: Visual Studio
1. Set `Po.LearnCert.Api` as startup project
2. Press F5 to run with debugging
3. Browser opens to https://localhost:5001

### Option 2: Command Line

**Terminal 1 - API Server:**
```bash
cd src/Po.LearnCert.Api
dotnet run
```

**Terminal 2 - Watch Mode (optional):**
```bash
cd src/Po.LearnCert.Client
dotnet watch
```

Access the application:
- **API**: http://localhost:5000 or https://localhost:5001
- **Swagger**: https://localhost:5001/swagger
- **Health Check**: https://localhost:5001/api/health

---

## Development Workflow

### Feature Development Process

#### 1. Create Feature Branch
```bash
git checkout -b feature/your-feature-name
```

#### 2. Follow Vertical Slice Architecture
Organize code by feature, not layer:

```
src/Po.LearnCert.Api/Features/
├── Quiz/
│   ├── QuizController.cs
│   ├── Services/
│   │   ├── IQuizSessionService.cs
│   │   └── QuizSessionService.cs
│   └── Infrastructure/
│       ├── QuizSessionEntity.cs
│       ├── IQuizSessionRepository.cs
│       └── QuizSessionRepository.cs
```

#### 3. Write Tests First (TDD)
```bash
# Create test file
tests/Po.LearnCert.UnitTests/Features/Quiz/QuizSessionServiceTests.cs

# Run tests
dotnet test
```

#### 4. Implement Feature
- Add models to `Po.LearnCert.Shared`
- Implement backend logic in `Po.LearnCert.Api`
- Create frontend components in `Po.LearnCert.Client`

#### 5. Register Services
**Backend** (`Po.LearnCert.Api/Program.cs`):
```csharp
builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();
builder.Services.AddScoped<IQuizSessionRepository, QuizSessionRepository>();
```

**Frontend** (`Po.LearnCert.Client/Program.cs`):
```csharp
builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();
```

#### 6. Test Locally
```bash
# Build
dotnet build

# Run unit tests
dotnet test tests/Po.LearnCert.UnitTests

# Run integration tests
dotnet test tests/Po.LearnCert.IntegrationTests

# Run E2E tests
cd tests/Po.LearnCert.E2ETests
npm test
```

#### 7. Commit and Push
```bash
git add .
git commit -m "feat: implement quiz session feature"
git push origin feature/your-feature-name
```

---

## Testing Strategy

### Unit Tests (xUnit)
```bash
# Run all unit tests
dotnet test tests/Po.LearnCert.UnitTests

# Run specific test
dotnet test --filter "FullyQualifiedName~QuizSessionServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

**Example Test:**
```csharp
[Fact]
public async Task CreateSessionAsync_ValidRequest_ReturnsSession()
{
    // Arrange
    var service = new QuizSessionService(mockRepo);
    var request = new CreateSessionRequest { CertificationId = "AZ900" };

    // Act
    var result = await service.CreateSessionAsync(request);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("AZ900", result.CertificationId);
}
```

### Integration Tests (xUnit + TestWebApplicationFactory)
```bash
# Run integration tests
dotnet test tests/Po.LearnCert.IntegrationTests

# Run specific endpoint test
dotnet test --filter "StatisticsEndpointsTests"
```

**Example Test:**
```csharp
[Fact]
public async Task GetStatistics_ReturnsOk()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/statistics?userId=test");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadFromJsonAsync<UserStatisticsDto>();
    Assert.NotNull(content);
}
```

### E2E Tests (Playwright)
```bash
cd tests/Po.LearnCert.E2ETests

# Install dependencies (first time only)
npm install

# Run tests
npm test

# Run tests with UI mode
npx playwright test --ui

# Generate report
npx playwright show-report
```

---

## Question Generation

### Using the Question Generator Tool

#### Interactive Mode
```bash
cd tools/Po.LearnCert.QuestionGenerator
dotnet run
```

Follow prompts:
1. Enter number of questions per subtopic (default: 25)
2. Confirm generation (Y/n)

#### Command-Line Mode
```bash
# Generate 50 questions per subtopic (400 total)
dotnet run -- 50 -y

# Generate 10 questions (interactive confirmation)
dotnet run -- 10

# View help
dotnet run -- --help
```

#### Output
- Generates questions for all 8 subtopics (4 per certification)
- Validates 4 choices per question, 1 correct answer
- Stores in Azure Table Storage
- Displays progress: ● (success) × (failure)

#### Configuration
Edit `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "Key": "your-api-key",
    "DeploymentName": "gpt-4o"
  },
  "SeedData": {
    "QuestionsPerSubtopic": 25
  }
}
```

---

## Deployment

### Prerequisites
- Azure subscription
- Azure Table Storage account
- Azure OpenAI Service (optional)

### Deployment Steps

#### 1. Create Azure Resources
```bash
# Resource group
az group create --name rg-polearncert --location eastus

# Storage account
az storage account create \
  --name stpolearncert \
  --resource-group rg-polearncert \
  --location eastus \
  --sku Standard_LRS

# Get connection string
az storage account show-connection-string \
  --name stpolearncert \
  --resource-group rg-polearncert
```

#### 2. Update Configuration
Update `appsettings.Production.json`:
```json
{
  "AzureTableStorage": {
    "ConnectionString": "<production-connection-string>"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

#### 3. Publish Application
```bash
# Publish API
dotnet publish src/Po.LearnCert.Api -c Release -o publish/api

# Publish includes Blazor WASM client automatically
```

#### 4. Deploy to Azure App Service
```bash
# Create App Service plan
az appservice plan create \
  --name asp-polearncert \
  --resource-group rg-polearncert \
  --sku B1

# Create web app
az webapp create \
  --name app-polearncert \
  --resource-group rg-polearncert \
  --plan asp-polearncert \
  --runtime "DOTNET|9.0"

# Deploy
az webapp deployment source config-zip \
  --resource-group rg-polearncert \
  --name app-polearncert \
  --src publish/api.zip
```

---

## Troubleshooting

### Common Issues

#### Azurite Not Running
**Symptom**: `No connection could be made`

**Solution**:
```bash
# Start Azurite
azurite --silent --location c:\azurite

# Or restart Visual Studio Azurite
```

#### Port Already in Use
**Symptom**: `Address already in use: 5000`

**Solution**:
```bash
# Windows - Find and kill process
netstat -ano | findstr :5000
taskkill /PID <process-id> /F

# Or change port in launchSettings.json
```

#### Blazor Client Not Loading
**Symptom**: Blank page or 404 errors

**Solution**:
```bash
# Rebuild solution
dotnet clean
dotnet build

# Ensure API project references Client project
```

#### Questions Not Generating
**Symptom**: All questions fail with × marker

**Solution**:
1. Check Azure OpenAI configuration
2. Verify API key is valid
3. Check deployment name matches
4. Review console error messages

#### E2E Tests Failing
**Symptom**: Tests can't find elements

**Solution**:
```bash
# Ensure API is running
cd src/Po.LearnCert.Api
dotnet run

# In another terminal, run tests
cd tests/Po.LearnCert.E2ETests
npm test
```

### Getting Help

- **Documentation**: See `README.md` and `PRD.md`
- **API Reference**: Visit `/swagger` endpoint
- **Health Check**: Check `/api/health` endpoint
- **Logs**: Review `logs/` directory for Serilog output

---

## Best Practices

### Code Quality
- Follow SOLID principles
- Use dependency injection
- Write descriptive variable names
- Add XML documentation comments
- Keep methods focused (<50 lines)

### Performance
- Use async/await for I/O operations
- Implement pagination for large datasets
- Cache frequently accessed data
- Optimize Azure Table Storage queries with proper partition keys

### Security
- Never commit secrets to git
- Use User Secrets for development
- Use Azure Key Vault for production
- Validate all user inputs
- Implement rate limiting

### Testing
- Maintain >80% code coverage
- Test edge cases and error scenarios
- Use meaningful test names
- Mock external dependencies
- Keep tests fast (<100ms per test)

---

## Quick Reference

### Common Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Run API
cd src/Po.LearnCert.Api && dotnet run

# Generate questions
cd tools/Po.LearnCert.QuestionGenerator && dotnet run -- 10 -y

# Run E2E tests
cd tests/Po.LearnCert.E2ETests && npm test

# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore
```

### File Locations
- **API**: `src/Po.LearnCert.Api/`
- **Client**: `src/Po.LearnCert.Client/`
- **Shared Models**: `src/Po.LearnCert.Shared/`
- **Unit Tests**: `tests/Po.LearnCert.UnitTests/`
- **Integration Tests**: `tests/Po.LearnCert.IntegrationTests/`
- **E2E Tests**: `tests/Po.LearnCert.E2ETests/`
- **Question Generator**: `tools/Po.LearnCert.QuestionGenerator/`
- **Documentation**: `docs/`
- **Specifications**: `specs/001-cert-quiz-platform/`

---

**Last Updated**: October 27, 2025  
**Version**: 1.0
