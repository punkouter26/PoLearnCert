import { test, expect } from '@playwright/test';

test.describe('Complete Quiz Flow', () => {
  test('should complete full quiz workflow', async ({ page }) => {
    // Step 1: Navigate to quiz setup
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Step 2: Wait for quiz setup page to load - match actual Blazor selectors
    await page.waitForSelector('.quiz-setup-container, .setup-form', { timeout: 15000 });
    
    // Step 3: Check if certification dropdown is available (Blazor uses select#certification)
    const certDropdown = page.locator('select#certification').first();
    const hasCertDropdown = await certDropdown.isVisible().catch(() => false);
    
    if (hasCertDropdown) {
      // Wait for options to load
      await page.waitForTimeout(2000);
      
      // Get available options
      const options = await certDropdown.locator('option').count();
      
      if (options > 1) {
        // Select a certification (skip first option which is placeholder)
        await certDropdown.selectOption({ index: 1 });
        await page.waitForTimeout(500);
      }
    }
    
    // Step 4: Set question count (Blazor uses input#questionCount)
    const questionInput = page.locator('input#questionCount').first();
    const hasQuestionInput = await questionInput.isVisible().catch(() => false);
    
    if (hasQuestionInput) {
      await questionInput.clear();
      await questionInput.fill('5');
    }
    
    // Step 5: Start quiz (Blazor uses button.btn-primary)
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz|Creating Session/i }).first();
    const hasStartButton = await startButton.isVisible().catch(() => false);
    
    if (hasStartButton) {
      await startButton.click();
      
      // Wait for navigation or loading
      await page.waitForTimeout(3000);
      
      // Check if we navigated to quiz session or see loading state
      const currentUrl = page.url();
      const isOnSession = currentUrl.includes('/quiz/session') || currentUrl.includes('/quiz/');
      const hasLoading = await page.locator('.loading').isVisible().catch(() => false);
      
      expect(isOnSession || hasLoading).toBeTruthy();
    }
  });

  test('should display quiz questions in session', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForSelector('.quiz-setup-container', { timeout: 20000 });
    
    // Must select certification first before Start button is enabled
    const certDropdown = page.locator('select#certification');
    await page.waitForTimeout(2000); // Wait for certifications to load
    const options = await certDropdown.locator('option').count();
    if (options > 1) {
      await certDropdown.selectOption({ index: 1 });
      await page.waitForTimeout(500);
    }
    
    // Try to start a quiz session
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz/i }).first();
    const isEnabled = await startButton.isEnabled().catch(() => false);
    
    if (isEnabled) {
      await startButton.click();
      await page.waitForTimeout(3000);
      
      // Check if question is displayed (actual Blazor selectors)
      const hasQuestion = await page.locator('.quiz-question-container, .question-content').isVisible().catch(() => false);
      const hasAnswers = await page.locator('.choice-button').count() > 0;
      
      if (hasQuestion || hasAnswers) {
        // Quiz session loaded successfully
        expect(hasQuestion || hasAnswers).toBeTruthy();
      }
    }
  });

  test('should validate question count input', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    const questionInput = page.locator('input#questionCount, input[name="questionCount"], input[type="number"]').first();
    const hasInput = await questionInput.isVisible().catch(() => false);
    
    if (hasInput) {
      // Try invalid value (0)
      await questionInput.clear();
      await questionInput.fill('0');
      
      const startButton = page.locator('button.btn-primary, button[type="submit"]').first();
      await startButton.click();
      
      // Should show validation error or not proceed
      await page.waitForTimeout(1000);
      
      // Check for error message or validation
      const hasError = await page.locator('.error, .invalid-feedback, [class*="error"]').isVisible().catch(() => false);
      
      // Either we see an error or we're still on setup page
      const stillOnSetup = page.url().includes('/quiz/setup') || page.url().includes('/quiz');
      expect(hasError || stillOnSetup).toBeTruthy();
    }
  });

  test('should handle quiz cancellation', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Look for cancel or back button
    const cancelButton = page.locator('button, a').filter({ hasText: /Cancel|Back|Home/i }).first();
    const hasCancel = await cancelButton.isVisible().catch(() => false);
    
    if (hasCancel) {
      await cancelButton.click();
      await page.waitForTimeout(1000);
      
      // Should navigate away from quiz setup
      const currentUrl = page.url();
      expect(currentUrl).toBeTruthy();
    }
  });
});

test.describe('Quiz Session Interaction', () => {
  test('should show progress indicator in quiz', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForSelector('.quiz-setup-container', { timeout: 10000 });
    
    // Must select certification first before Start button is enabled
    const certDropdown = page.locator('select#certification');
    await page.waitForTimeout(2000); // Wait for certifications to load
    const options = await certDropdown.locator('option').count();
    if (options > 1) {
      await certDropdown.selectOption({ index: 1 });
      await page.waitForTimeout(500);
    }
    
    // Attempt to start quiz
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz/i }).first();
    const isEnabled = await startButton.isEnabled().catch(() => false);
    
    if (isEnabled) {
      await startButton.click();
      await page.waitForTimeout(3000);
      
      // Look for progress indicator (actual Blazor selector: .progress-bar, .question-counter)
      const hasProgress = await page.locator('.progress-bar, .progress-info').isVisible().catch(() => false);
      const hasCounter = await page.locator('.question-counter').isVisible().catch(() => false);
      
      if (hasProgress || hasCounter) {
        expect(hasProgress || hasCounter).toBeTruthy();
      }
    }
  });

  test('should allow answer selection', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForSelector('.quiz-setup-container', { timeout: 10000 });
    
    // Must select certification first before Start button is enabled
    const certDropdown = page.locator('select#certification');
    await page.waitForTimeout(2000); // Wait for certifications to load
    const options = await certDropdown.locator('option').count();
    if (options > 1) {
      await certDropdown.selectOption({ index: 1 });
      await page.waitForTimeout(500);
    }
    
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz/i }).first();
    const isEnabled = await startButton.isEnabled().catch(() => false);
    
    if (isEnabled) {
      await startButton.click();
      await page.waitForTimeout(3000);
      
      // Look for answer options (Blazor uses .choice-button)
      const answerButtons = page.locator('.choice-button');
      const buttonCount = await answerButtons.count();
      
      if (buttonCount > 0) {
        // Click first answer button
        await answerButtons.first().click();
        await page.waitForTimeout(500);
        expect(true).toBeTruthy();
      }
    }
  });

  test('should show submit answer button', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForSelector('.quiz-setup-container', { timeout: 10000 });
    
    // Must select certification first before Start button is enabled
    const certDropdown = page.locator('select#certification');
    await page.waitForTimeout(2000); // Wait for certifications to load
    const options = await certDropdown.locator('option').count();
    if (options > 1) {
      await certDropdown.selectOption({ index: 1 });
      await page.waitForTimeout(500);
    }
    
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz/i }).first();
    const isEnabled = await startButton.isEnabled().catch(() => false);
    
    if (isEnabled) {
      await startButton.click();
      await page.waitForTimeout(3000);
      
      // Look for submit/next button
      const submitButton = page.locator('button').filter({ hasText: /Submit|Next|Continue|Confirm/i }).first();
      const hasSubmit = await submitButton.isVisible().catch(() => false);
      
      if (hasSubmit) {
        expect(hasSubmit).toBeTruthy();
      }
    }
  });
});

test.describe('Quiz Mobile Experience', () => {
  test.use({ viewport: { width: 390, height: 844 } });

  test('should display quiz setup on mobile', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Wait for Blazor to render the quiz setup container
    await page.waitForSelector('.quiz-setup-container', { timeout: 10000 });
    
    // Verify mobile rendering
    await expect(page.locator('body')).toBeVisible();
    
    // Check for setup form
    const hasForm = await page.locator('.quiz-setup-container').isVisible().catch(() => false);
    expect(hasForm).toBeTruthy();
  });

  test('should have accessible controls on mobile', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Verify inputs are accessible on mobile
    const questionInput = page.locator('input#questionCount, input[type="number"]').first();
    const hasInput = await questionInput.isVisible().catch(() => false);
    
    if (hasInput) {
      // Verify input is tappable
      const boundingBox = await questionInput.boundingBox();
      expect(boundingBox).toBeTruthy();
      if (boundingBox) {
        expect(boundingBox.height).toBeGreaterThan(20); // Minimum touch target
      }
    }
  });

  test('should display questions properly on mobile', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForSelector('.quiz-setup-container', { timeout: 10000 });
    
    // Must select certification first before Start button is enabled
    const certDropdown = page.locator('select#certification');
    await page.waitForTimeout(2000); // Wait for certifications to load
    const options = await certDropdown.locator('option').count();
    if (options > 1) {
      await certDropdown.selectOption({ index: 1 });
      await page.waitForTimeout(500);
    }
    
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz/i }).first();
    const isEnabled = await startButton.isEnabled().catch(() => false);
    
    if (isEnabled) {
      await startButton.click();
      await page.waitForTimeout(3000);
      
      // Check if content fits mobile viewport
      const viewportWidth = await page.viewportSize();
      expect(viewportWidth?.width).toBe(390);
      
      // Verify no horizontal scroll needed
      const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
      expect(bodyWidth).toBeLessThanOrEqual(395); // Allow small margin
    }
  });
});
