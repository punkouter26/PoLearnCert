# PoLearnCert E2E Tests

This directory contains end-to-end (E2E) tests for the PoLearnCert application using TypeScript and Playwright.

## Prerequisites

1. **Node.js** (v18 or higher)
2. **npm** or **yarn**
3. **PoLearnCert API and Client** running locally

## Setup

1. Install dependencies:
   ```bash
   npm install
   ```

2. Install Playwright browsers:
   ```bash
   npx playwright install
   ```

3. Ensure the PoLearnCert application is running:
   - API server should be running on `https://localhost:7089`
   - Client should be running on `https://localhost:5173`

## Running Tests

### Run all tests
```bash
npm test
```

### Run tests in headed mode (visible browser)
```bash
npm run test:headed
```

### Run tests with UI mode (interactive)
```bash
npm run test:ui
```

### Run specific test files
```bash
# Statistics tests only
npm run test:statistics

# Quiz tests only
npm run test:quiz
```

### Debug tests
```bash
npm run test:debug
```

### View test reports
```bash
npm run test:report
```

## Test Structure

### `/tests/statistics.spec.ts`
Tests for the Statistics Dashboard functionality:
- Navigation to statistics page
- Display of statistics cards
- Loading states
- Certification performance section
- Subtopic performance section
- Responsive design
- Performance level colors
- Error handling
- Statistics updates after quiz completion
- Certification filtering

### `/tests/quiz.spec.ts`
Tests for the Quiz functionality:
- Navigation to quiz page
- Certification selection
- Starting quiz sessions
- Quiz questions and options
- Answering questions
- Progress indicators
- Quiz completion
- Navigation handling
- Responsive design
- Error handling

## Configuration

The tests are configured in `playwright.config.ts` with:
- **Base URL**: `https://localhost:7089`
- **Browsers**: Chromium, Firefox, WebKit, Mobile Chrome, Mobile Safari
- **Reporters**: HTML, JSON, JUnit
- **Screenshots**: On failure only
- **Videos**: Retained on failure
- **Traces**: On first retry

## Web Server Configuration

The tests automatically start the required web servers:
- **API Server**: `dotnet run --project ../../src/Po.LearnCert.Api` (port 7089)
- **Client Server**: `dotnet run --project ../../src/Po.LearnCert.Client` (port 5173)

## Test Features

### Cross-Browser Testing
Tests run on multiple browsers to ensure compatibility:
- Desktop: Chrome, Firefox, Safari
- Mobile: Chrome (Pixel 5), Safari (iPhone 12)

### Responsive Testing
Tests verify functionality across different screen sizes:
- Desktop (1920x1080)
- Tablet (768x1024)
- Mobile (375x667)

### Error Handling
Tests verify graceful error handling:
- Network failures
- API errors
- Invalid user input
- Missing data scenarios

### Performance Testing
Tests check for:
- Page load times
- Interactive element responsiveness
- Animation smoothness
- Memory usage (basic checks)

## Test Data

Tests use:
- **Dynamic test data**: Generated during test execution
- **Isolated test environments**: Each test runs independently
- **Cleanup procedures**: Tests clean up after themselves

## Debugging

### Visual Debugging
```bash
# Run with browser visible
npm run test:headed

# Run in debug mode with breakpoints
npm run test:debug
```

### Test Artifacts
On test failure, the following are captured:
- **Screenshots**: Saved to `test-results/`
- **Videos**: Browser recordings of failed tests
- **Traces**: Detailed execution traces
- **Console logs**: Browser console output

### Common Issues

1. **Port conflicts**: Ensure ports 7089 and 5173 are available
2. **API not running**: Start the API server before running tests
3. **Browser not installed**: Run `npx playwright install`
4. **HTTPS certificate issues**: Tests ignore HTTPS errors for local development

## CI/CD Integration

Tests are configured for CI/CD with:
- **Retry logic**: Failed tests retry 2 times in CI
- **Parallel execution**: Disabled in CI for stability
- **Artifact collection**: Screenshots, videos, and reports saved
- **JUnit output**: For CI integration

## Best Practices

1. **Test Independence**: Each test should be able to run independently
2. **Page Object Pattern**: Consider implementing for complex pages
3. **Wait Strategies**: Use explicit waits instead of fixed timeouts
4. **Cleanup**: Clean up test data after each test
5. **Assertions**: Use meaningful assertions with good error messages

## Contributing

When adding new tests:
1. Follow the existing test structure
2. Add meaningful test descriptions
3. Include both positive and negative test cases
4. Test responsive behavior
5. Handle error scenarios
6. Update this README if needed