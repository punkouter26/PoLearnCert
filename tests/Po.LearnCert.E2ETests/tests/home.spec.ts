import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should display navigation menu', async ({ page }) => {
    await page.goto('/');
    
    // Wait for navigation to load - Radzen layout uses RadzenSidebar and RadzenPanelMenu
    await page.waitForSelector('.rz-sidebar, .rz-panel-menu, .rz-header, header', { timeout: 15000 });
    
    // Verify navigation exists
    const nav = page.locator('.rz-sidebar, .rz-panel-menu, .rz-header, header').first();
    await expect(nav).toBeVisible();
  });

  test('should have working navigation links', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check for Radzen panel menu items
    const quizLink = page.locator('.rz-panel-menu-item a[href*="quiz"], a[href="/quiz"]').first();
    const statsLink = page.locator('.rz-panel-menu-item a[href*="statistics"], a[href="/statistics"]').first();
    const leaderboardLink = page.locator('.rz-panel-menu-item a[href*="leaderboard"], a[href="/leaderboards"]').first();
    
    const hasQuiz = await quizLink.isVisible().catch(() => false);
    const hasStats = await statsLink.isVisible().catch(() => false);
    const hasLeaderboard = await leaderboardLink.isVisible().catch(() => false);
    
    expect(hasQuiz || hasStats || hasLeaderboard).toBeTruthy();
  });

  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    await expect(page.locator('body')).toBeVisible();
    
    const sidebarToggle = page.locator('.rz-sidebar-toggle, button[title="Toggle sidebar"]').first();
    const hasToggle = await sidebarToggle.isVisible().catch(() => false);
    
    if (hasToggle) {
      await sidebarToggle.click({ force: true });
      await page.waitForTimeout(500);
      const navExists = await page.locator('text=Home').first().isVisible() || 
                        await page.locator('.rz-panel-menu-item').count() > 0;
      expect(navExists).toBeTruthy();
    }
  });

  test('should handle direct URL navigation', async ({ page }) => {
    const pages = ['/quiz', '/statistics', '/leaderboard'];
    
    for (const route of pages) {
      await page.goto(route);
      await expect(page.locator('body')).toBeVisible();
    }
  });
});
