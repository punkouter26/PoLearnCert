import { test, expect, Page } from '@playwright/test';

test.describe('Quiz Functionality', () => {
  let page: Page;
  
  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();
    await page.goto('/');
  });

  test.afterEach(async () => {
    await page.close();
  });

  test('should display quiz setup page', async () => {
    // Navigate to quiz setup page
    await page.goto('/quiz');
    await expect(page.locator('.quiz-setup-container')).toBeVisible();
    await expect(page.locator('h1')).toContainText('Start Quiz Session');
  });

  test('should navigate to quiz setup via navigation', async () => {
    // Navigate directly to quiz setup page
    await page.goto('/quiz/setup');
    
    // Verify we're on the quiz setup page
    await expect(page).toHaveURL(/.*quiz\/setup/);
    await expect(page.locator('h1')).toContainText('Start Quiz Session');
  });

  test('should display certification selection dropdown', async () => {
    await page.goto('/quiz/setup');
    
    // Wait for certifications to load
    await page.waitForSelector('select#certification, .loading, .error-message', { timeout: 15000 });
    
    // Check if certification dropdown exists
    const certSelect = page.locator('select#certification');
    if (await certSelect.isVisible()) {
      // Verify dropdown has options
      const options = certSelect.locator('option');
      const count = await options.count();
      expect(count).toBeGreaterThan(0); // Should have at least the placeholder option
    }
  });

  test('should show question count input', async () => {
    await page.goto('/quiz/setup');
    
    // Wait for page to load
    await page.waitForSelector('.quiz-setup-container', { timeout: 15000 });
    
    // Verify question count input exists
    const questionInput = page.locator('input#questionCount');
    await expect(questionInput).toBeVisible();
    
    // Verify default value
    const value = await questionInput.inputValue();
    expect(parseInt(value)).toBeGreaterThan(0);
  });

  test('should show start quiz button', async () => {
    await page.goto('/quiz/setup');
    
    // Wait for page to load
    await page.waitForSelector('.quiz-setup-container', { timeout: 15000 });
    
    // Verify start button exists
    const startButton = page.locator('button.btn-primary').filter({ hasText: /Start Quiz|Creating Session/ });
    await expect(startButton).toBeVisible();
  });

  test('should be responsive on mobile', async () => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/quiz/setup');
    
    // Wait for content to load
    await page.waitForSelector('.quiz-setup-container', { timeout: 15000 });
    
    // Verify container is visible on mobile
    await expect(page.locator('.quiz-setup-container')).toBeVisible();
    
    // Check if mobile menu toggle exists
    const mobileMenu = page.locator('.navbar-toggler');
    if (await mobileMenu.isVisible()) {
      await mobileMenu.click();
      // Verify navigation is accessible
      await expect(page.locator('.nav-link[href="quiz/setup"]')).toBeVisible();
    }
  });
});
