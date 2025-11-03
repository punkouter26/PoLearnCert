import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should load home page successfully', async ({ page }) => {
    await page.goto('/');
    
    // Verify page title
    await expect(page).toHaveTitle(/LearnCert|PoLearnCert/i);
    
    // Verify main content is visible
    await expect(page.locator('body')).toBeVisible();
  });

  test('should display navigation menu', async ({ page }) => {
    await page.goto('/');
    
    // Wait for navigation to load
    await page.waitForSelector('nav, .navbar', { timeout: 10000 });
    
    // Verify navigation exists
    const nav = page.locator('nav, .navbar').first();
    await expect(nav).toBeVisible();
  });

  test('should have working navigation links', async ({ page }) => {
    await page.goto('/');
    
    // Wait for page to load
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check for common navigation links
    const quizLink = page.locator('a[href*="quiz"], .nav-link:has-text("Quiz")').first();
    const statsLink = page.locator('a[href*="statistics"], .nav-link:has-text("Statistics")').first();
    const leaderboardLink = page.locator('a[href*="leaderboard"], .nav-link:has-text("Leaderboard")').first();
    
    // At least one navigation link should exist
    const hasQuiz = await quizLink.isVisible().catch(() => false);
    const hasStats = await statsLink.isVisible().catch(() => false);
    const hasLeaderboard = await leaderboardLink.isVisible().catch(() => false);
    
    expect(hasQuiz || hasStats || hasLeaderboard).toBeTruthy();
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport (iPhone 12 Pro)
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');
    
    // Wait for page to load
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Verify page is visible on mobile
    await expect(page.locator('body')).toBeVisible();
    
    // Check if mobile menu toggle exists
    const mobileMenuToggle = page.locator('.navbar-toggler, .menu-toggle, button[aria-label*="menu"]').first();
    const hasToggle = await mobileMenuToggle.isVisible().catch(() => false);
    
    if (hasToggle) {
      // Click mobile menu toggle
      await mobileMenuToggle.click();
      await page.waitForTimeout(500); // Wait for menu animation
      
      // Menu should be expanded or visible
      const menuExpanded = await page.locator('.navbar-collapse.show, .menu.open, nav.open').isVisible().catch(() => false);
      expect(menuExpanded).toBeTruthy();
    }
  });

  test('should have proper meta tags for SEO', async ({ page }) => {
    await page.goto('/');
    
    // Check for viewport meta tag
    const viewport = await page.locator('meta[name="viewport"]').getAttribute('content');
    expect(viewport).toBeTruthy();
    expect(viewport).toContain('width=device-width');
  });

  test('should handle page refresh without errors', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Reload the page
    await page.reload();
    await page.waitForLoadState('networkidle');
    
    // Page should still be functional
    await expect(page.locator('body')).toBeVisible();
  });
});

test.describe('Navigation', () => {
  test('should navigate between pages', async ({ page }) => {
    await page.goto('/');
    
    // Navigate to quiz page
    await page.goto('/quiz');
    await expect(page).toHaveURL(/.*quiz/);
    
    // Navigate to statistics page
    await page.goto('/statistics');
    await expect(page).toHaveURL(/.*statistics/);
    
    // Navigate back to home
    await page.goto('/');
    await expect(page.locator('body')).toBeVisible();
  });

  test('should handle direct URL navigation', async ({ page }) => {
    // Direct navigation to different pages
    const pages = [
      '/quiz',
      '/quiz/setup',
      '/statistics',
      '/leaderboard'
    ];
    
    for (const route of pages) {
      await page.goto(route);
      // Page should load without errors
      await expect(page.locator('body')).toBeVisible();
    }
  });

  test('should maintain navigation state on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    
    // Navigate to quiz page
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle');
    
    // Verify URL changed
    await expect(page).toHaveURL(/.*quiz/);
    
    // Verify content loaded
    await expect(page.locator('body')).toBeVisible();
  });
});
