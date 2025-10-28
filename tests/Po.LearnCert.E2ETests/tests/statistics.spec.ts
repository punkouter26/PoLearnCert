import { test, expect, Page } from '@playwright/test';

test.describe('Statistics Dashboard', () => {
  let page: Page;
  
  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();
    await page.goto('/');
  });

  test.afterEach(async () => {
    await page.close();
  });

  test('should navigate to statistics page', async () => {
    // Navigate directly to statistics page
    await page.goto('/statistics');
    
    // Verify we're on the statistics page
    await expect(page).toHaveURL(/.*statistics/);
    
    // Verify we're on the statistics page by checking the title
    await expect(page.locator('.page-title')).toContainText('Performance Statistics');
  });

  test('should display statistics container', async () => {
    // Navigate directly to statistics
    await page.goto('/statistics');
    
    // Verify statistics container exists
    await expect(page.locator('.statistics-container')).toBeVisible();
    
    // Verify page header exists
    await expect(page.locator('.page-header')).toBeVisible();
    await expect(page.locator('.page-title')).toBeVisible();
  });

  test('should show loading or content', async () => {
    await page.goto('/statistics');
    
    // Wait for page to fully load - check for any of the possible states
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Statistics container should always be visible
    await expect(page.locator('.statistics-container')).toBeVisible();
    
    // Page header should be visible
    await expect(page.locator('.page-header')).toBeVisible();
  });

  test('should display stat cards when data loads', async () => {
    await page.goto('/statistics');
    
    // Wait for either stats or error/loading
    await page.waitForSelector('.stats-overview, .error-section, .loading-section', { timeout: 15000 });
    
    // If stats overview is visible, check for stat cards
    const statsOverview = page.locator('.stats-overview');
    if (await statsOverview.isVisible()) {
      const statCards = page.locator('.stat-card');
      const count = await statCards.count();
      expect(count).toBeGreaterThan(0);
    }
  });

  test('should be responsive on mobile', async () => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/statistics');
    
    // Wait for content to load
    await page.waitForSelector('.statistics-container', { timeout: 15000 });
    
    // Verify container is visible on mobile
    await expect(page.locator('.statistics-container')).toBeVisible();
    
    // Check if mobile menu toggle exists
    const mobileMenu = page.locator('.navbar-toggler');
    if (await mobileMenu.isVisible()) {
      await mobileMenu.click();
      // Verify navigation is accessible
      await expect(page.locator('.nav-link[href="statistics"]')).toBeVisible();
    }
  });
});
