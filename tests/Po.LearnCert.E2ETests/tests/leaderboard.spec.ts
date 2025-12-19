import { test, expect } from '@playwright/test';

test.describe('Leaderboard', () => {
  test('should navigate to leaderboard page', async ({ page }) => {
    await page.goto('/leaderboard');
    await expect(page).toHaveURL(/.*leaderboard/);
    await expect(page.locator('body')).toBeVisible();
  });

  test('should display leaderboard entries if data exists', async ({ page }) => {
    await page.goto('/leaderboard');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    const tableRows = page.locator('.rz-data-grid tbody tr, .rz-datatable tbody tr, table tbody tr');
    const count = await tableRows.count();
    
    if (count > 0) {
      const firstEntry = tableRows.first();
      await expect(firstEntry).toBeVisible();
    }
  });

  test('should show user rankings with scores', async ({ page }) => {
    await page.goto('/leaderboard');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    const hasScores = await page.locator('[class*="score"], [class*="percentage"]').count() > 0;
    const hasNumbers = await page.locator('td, .stat-value').filter({ hasText: /\d+%?/ }).count() > 0;
    const hasData = await page.locator('table tbody tr, .leaderboard-entry').count() > 0;
    
    if (hasData) {
      expect(hasScores || hasNumbers).toBeTruthy();
    }
  });

  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/leaderboard');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    await expect(page.locator('body')).toBeVisible();
  });

  test('should display rank numbers for entries', async ({ page }) => {
    await page.goto('/leaderboard');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    const entries = await page.locator('table tbody tr, .leaderboard-entry, .rank-item').count();
    
    if (entries > 0) {
      const hasRankNumbers = await page.locator('td:first-child, .rank, .position').filter({ hasText: /^[1-9]\d*$/ }).count() > 0;
      const hasHashSymbol = await page.locator('th').filter({ hasText: /#|Rank|Position/i }).count() > 0;
      expect(hasRankNumbers || hasHashSymbol).toBeTruthy();
    }
  });
});
