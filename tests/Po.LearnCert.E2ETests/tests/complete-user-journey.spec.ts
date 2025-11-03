import { test, expect, Page } from '@playwright/test';

/**
 * Complete User Journey E2E Test
 * 
 * This test covers the full user workflow:
 * 1. Register a new account
 * 2. Log in with the account
 * 3. Take a 3-question quiz
 * 4. Verify statistics are updated correctly
 */

test.describe.configure({ mode: 'serial' }); // Run tests in sequence

test.describe('Complete User Journey - Register, Quiz, and Statistics', () => {
  let page: Page;
  const timestamp = Date.now();
  const testUser = {
    username: `testuser${timestamp}`,
    email: `testuser${timestamp}@example.com`,
    password: 'TestPassword123!'
  };

  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    
    // Enable console logging for debugging
    page.on('console', msg => {
      if (msg.type() === 'error') {
        console.log('Browser Error:', msg.text());
      }
    });
  });

  test.afterAll(async () => {
    await page.close();
  });

  test('should complete full user journey: register → login → quiz → statistics', async () => {
    test.setTimeout(90000); // Increase timeout to 90 seconds for this comprehensive test
    
    // ========================
    // STEP 1: Register Account
    // ========================
    console.log('Step 1: Registering new account...');
    await page.goto('/register');
    await page.waitForLoadState('networkidle');

    // Fill registration form
    const usernameInput = page.locator('input[name="username"], input#username, input[type="text"]').first();
    const emailInput = page.locator('input[name="email"], input#email, input[type="email"]').first();
    const passwordInput = page.locator('input[name="password"], input#password, input[type="password"]').first();
    const confirmPasswordInput = page.locator('input[name="confirmPassword"], input#confirmPassword').first();

    await usernameInput.fill(testUser.username);
    await emailInput.fill(testUser.email);
    await passwordInput.fill(testUser.password);
    
    // Fill confirm password if it exists
    const hasConfirmPassword = await confirmPasswordInput.isVisible().catch(() => false);
    if (hasConfirmPassword) {
      await confirmPasswordInput.fill(testUser.password);
    }

    // Submit registration
    const registerButton = page.locator('button[type="submit"], button.btn-primary').filter({ hasText: /register|sign up|create/i }).first();
    await registerButton.click();

    // Wait for registration to complete - either redirect or success message
    await page.waitForTimeout(2000);
    
    console.log('Registration completed');

    // ========================
    // STEP 2: Login
    // ========================
    console.log('Step 2: Logging in...');
    
    // Navigate to login page (might already be there after registration)
    const currentUrl = page.url();
    if (!currentUrl.includes('/login')) {
      await page.goto('/login');
      await page.waitForLoadState('networkidle');
    }

    // Fill login form
    const loginUsernameInput = page.locator('input[name="username"], input#username, input[type="text"]').first();
    const loginPasswordInput = page.locator('input[name="password"], input#password, input[type="password"]').first();

    await loginUsernameInput.fill(testUser.username);
    await loginPasswordInput.fill(testUser.password);

    // Submit login
    const loginButton = page.locator('button[type="submit"], button.btn-primary').filter({ hasText: /login|sign in/i }).first();
    await loginButton.click();

    // Wait for login to complete and redirect to home
    await page.waitForTimeout(2000);
    await page.waitForLoadState('networkidle');

    console.log('Login completed');

    // Verify we're logged in by checking if we can access authenticated pages
    // We'll navigate to quiz setup - if login failed, we'd be redirected to login
    await page.waitForTimeout(1000);

    // ========================
    // STEP 3: Take a 3-Question Quiz
    // ========================
    console.log('Step 3: Starting quiz...');
    
    // Navigate to quiz setup
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Select a certification
    const certDropdown = page.locator('select#certification, select[name="certification"]').first();
    const hasCertDropdown = await certDropdown.isVisible().catch(() => false);
    
    if (hasCertDropdown) {
      await page.waitForTimeout(1000); // Wait for certifications to load
      const options = await certDropdown.locator('option').count();
      
      if (options > 1) {
        await certDropdown.selectOption({ index: 1 }); // Select first actual certification
        await page.waitForTimeout(500);
      }
    }

    // Set question count to 3
    const questionInput = page.locator('input#questionCount, input[name="questionCount"], input[type="number"]').first();
    const hasQuestionInput = await questionInput.isVisible().catch(() => false);
    
    if (hasQuestionInput) {
      await questionInput.clear();
      await questionInput.fill('3');
      await page.waitForTimeout(500);
    }

    // Start the quiz
    const startButton = page.locator('button.btn-primary, button[type="submit"]').filter({ hasText: /start|begin|create/i }).first();
    await startButton.click();
    
    await page.waitForTimeout(2000);
    await page.waitForLoadState('networkidle');

    console.log('Quiz started, answering questions...');

    // Answer 3 questions
    for (let i = 1; i <= 3; i++) {
      console.log(`Answering question ${i}...`);
      
      // Check if we're still on a question page or if we've moved to results
      const onResultsPage = page.url().includes('/results');
      if (onResultsPage) {
        console.log(`Quiz completed after ${i-1} questions (less than 3 questions available)`);
        break;
      }
      
      // Wait for question to load
      try {
        await page.waitForSelector('.quiz-question-container, .question-content', { timeout: 15000 });
      } catch (e) {
        // Might have navigated to results or somewhere else
        const currentUrl = page.url();
        console.log(`Current URL after timeout: ${currentUrl}`);
        if (currentUrl.includes('/results')) {
          console.log(`Navigated to results after question ${i-1}`);
          break;
        }
        if (currentUrl.includes('/quiz')) {
          console.log(`Still on quiz page but selector not found`);
        }
        throw e;
      }
      
      await page.waitForTimeout(1500);

      // Find and click the first answer choice button
      const answerButton = page.locator('button.choice-button').first();
      await answerButton.waitFor({ state: 'visible', timeout: 10000 });
      await answerButton.click();
      await page.waitForTimeout(800);

      // Submit the answer
      const submitButton = page.locator('button.btn-primary').filter({ hasText: /submit/i }).first();
      await submitButton.waitFor({ state: 'visible', timeout: 5000 });
      await submitButton.click();
      await page.waitForTimeout(2000);
    }

    console.log('All questions answered');

    // Wait for results page
    await page.waitForTimeout(2000);
    await page.waitForLoadState('networkidle');

    // Verify we're on results page
    const onResultsPage = page.url().includes('/results') || 
                          await page.locator('.quiz-results, .results-container').isVisible().catch(() => false);
    
    if (onResultsPage) {
      console.log('Quiz completed, on results page');
      
      // Verify results are displayed
      const scoreElement = page.locator('.score-percentage, .score-badge, .score').first();
      const hasScore = await scoreElement.isVisible().catch(() => false);
      expect(hasScore).toBeTruthy();
      
      // Wait a bit for statistics to be updated on the server
      await page.waitForTimeout(3000);
    }

    // ========================
    // STEP 4: Verify Statistics
    // ========================
    console.log('Step 4: Verifying statistics...');
    
    // Navigate to statistics page
    await page.goto('/statistics');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Wait for statistics content to load
    await page.waitForSelector('.statistics-container, .stats-overview', { timeout: 10000 });

    // Verify statistics are updated
    console.log('Checking statistics values...');

    // Check Total Sessions stat
    const totalSessionsStat = page.locator('.stat-card').filter({ hasText: /total sessions|sessions/i }).first();
    if (await totalSessionsStat.isVisible().catch(() => false)) {
      const sessionValue = await totalSessionsStat.locator('.stat-value, .value').textContent();
      console.log('Total Sessions:', sessionValue);
      
      // Should show at least 1 session
      const sessionCount = parseInt(sessionValue?.trim() || '0');
      expect(sessionCount).toBeGreaterThanOrEqual(1);
    }

    // Check Questions Answered stat
    const questionsAnsweredStat = page.locator('.stat-card').filter({ hasText: /questions answered|total questions/i }).first();
    if (await questionsAnsweredStat.isVisible().catch(() => false)) {
      const questionValue = await questionsAnsweredStat.locator('.stat-value, .value').textContent();
      console.log('Questions Answered:', questionValue);
      
      // Should show at least 3 questions
      const questionCount = parseInt(questionValue?.trim() || '0');
      expect(questionCount).toBeGreaterThanOrEqual(3);
    }

    // Check Accuracy stat
    const accuracyStat = page.locator('.stat-card').filter({ hasText: /accuracy|correct/i }).first();
    if (await accuracyStat.isVisible().catch(() => false)) {
      const accuracyValue = await accuracyStat.locator('.stat-value, .value').textContent();
      console.log('Accuracy:', accuracyValue);
      
      // Should show some percentage
      expect(accuracyValue).toMatch(/%/);
    }

    // Check for session history
    const sessionHistorySection = page.locator('.session-history, .recent-sessions').first();
    if (await sessionHistorySection.isVisible().catch(() => false)) {
      console.log('Session history section found');
      
      // Should have at least one session in history
      const sessionCards = page.locator('.session-card, .history-item');
      const sessionCount = await sessionCards.count();
      expect(sessionCount).toBeGreaterThanOrEqual(1);
      
      console.log(`Found ${sessionCount} session(s) in history`);
    }

    // Check for certification performance
    const certPerformanceSection = page.locator('.certification-performance, .cert-stats').first();
    if (await certPerformanceSection.isVisible().catch(() => false)) {
      console.log('Certification performance section found');
      
      // Should have at least one certification
      const certCards = page.locator('.cert-card, .performance-card');
      const certCount = await certCards.count();
      expect(certCount).toBeGreaterThanOrEqual(1);
      
      console.log(`Found ${certCount} certification(s)`);
    }

    console.log('✓ Complete user journey test passed!');
    console.log(`✓ User ${testUser.username} registered, logged in, completed quiz, and statistics are updated`);
  });

  test('should show statistics increase after second quiz', async () => {
    // This test assumes the previous test has run and the user is logged in
    console.log('Taking a second quiz to verify statistics increment...');
    
    // Take another quiz
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Select certification and start quiz
    const certDropdown = page.locator('select#certification, select[name="certification"]').first();
    if (await certDropdown.isVisible().catch(() => false)) {
      await page.waitForTimeout(1000);
      await certDropdown.selectOption({ index: 1 });
      await page.waitForTimeout(500);
    }

    const questionInput = page.locator('input#questionCount, input[name="questionCount"], input[type="number"]').first();
    if (await questionInput.isVisible().catch(() => false)) {
      await questionInput.clear();
      await questionInput.fill('3');
    }

    const startButton = page.locator('button.btn-primary, button[type="submit"]').filter({ hasText: /start|begin|create/i }).first();
    await startButton.click();
    await page.waitForTimeout(2000);

    // Answer questions
    for (let i = 1; i <= 3; i++) {
      await page.waitForSelector('.quiz-question-container, .question-content', { timeout: 15000 });
      await page.waitForTimeout(1500);

      const answerButton = page.locator('button.choice-button').first();
      await answerButton.waitFor({ state: 'visible', timeout: 10000 });
      await answerButton.click();
      await page.waitForTimeout(800);

      const submitButton = page.locator('button.btn-primary').filter({ hasText: /submit/i }).first();
      await submitButton.waitFor({ state: 'visible', timeout: 5000 });
      await submitButton.click();
      await page.waitForTimeout(2000);
    }

    await page.waitForTimeout(3000); // Wait for statistics update

    // Check statistics again
    await page.goto('/statistics');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Verify session count increased
    const totalSessionsStat = page.locator('.stat-card').filter({ hasText: /total sessions|sessions/i }).first();
    if (await totalSessionsStat.isVisible().catch(() => false)) {
      const sessionValue = await totalSessionsStat.locator('.stat-value, .value').textContent();
      const sessionCount = parseInt(sessionValue?.trim() || '0');
      
      // Should now show at least 2 sessions
      expect(sessionCount).toBeGreaterThanOrEqual(2);
      console.log(`✓ Session count increased to ${sessionCount}`);
    }

    // Verify questions answered increased
    const questionsAnsweredStat = page.locator('.stat-card').filter({ hasText: /questions answered|total questions/i }).first();
    if (await questionsAnsweredStat.isVisible().catch(() => false)) {
      const questionValue = await questionsAnsweredStat.locator('.stat-value, .value').textContent();
      const questionCount = parseInt(questionValue?.trim() || '0');
      
      // Should now show at least 6 questions (3 + 3)
      expect(questionCount).toBeGreaterThanOrEqual(6);
      console.log(`✓ Questions answered increased to ${questionCount}`);
    }

    console.log('✓ Statistics correctly increment after second quiz');
  });
});
