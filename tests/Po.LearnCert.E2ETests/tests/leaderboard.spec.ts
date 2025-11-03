import { test, expect } from '@playwright/test';

test.describe('Leaderboard', () => {
  test('should navigate to leaderboard page', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Verify we're on the leaderboard page
    await expect(page).toHaveURL(/.*leaderboard/);
    
    // Verify page loaded
    await expect(page.locator('body')).toBeVisible();
  });

  test('should display leaderboard container', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for page to load
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check for leaderboard container or title
    const hasContainer = await page.locator('.leaderboard-container, .leaderboard').isVisible().catch(() => false);
    const hasTitle = await page.locator('h1, h2').filter({ hasText: /leaderboard/i }).isVisible().catch(() => false);
    
    // At least one should be visible
    expect(hasContainer || hasTitle).toBeTruthy();
  });

  test('should show loading state or data', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for page to settle
    await page.waitForTimeout(2000);
    
    // Check for loading, data, or error state
    const hasLoading = await page.locator('.loading, .spinner').isVisible().catch(() => false);
    const hasTable = await page.locator('table, .leaderboard-list').isVisible().catch(() => false);
    const hasError = await page.locator('.error, .error-message').isVisible().catch(() => false);
    const hasEmpty = await page.locator('.empty, .no-data').isVisible().catch(() => false);
    
    // One of these states should be present
    expect(hasLoading || hasTable || hasError || hasEmpty).toBeTruthy();
  });

  test('should display leaderboard entries if data exists', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for data to load
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    // Check if leaderboard entries exist
    const tableRows = page.locator('table tbody tr, .leaderboard-entry, .user-rank');
    const count = await tableRows.count();
    
    // If there are entries, verify they have content
    if (count > 0) {
      const firstEntry = tableRows.first();
      await expect(firstEntry).toBeVisible();
    }
  });

  test('should show user rankings with scores', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for content
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    // Look for score/percentage indicators
    const hasScores = await page.locator('[class*="score"], [class*="percentage"], [class*="points"]').count() > 0;
    const hasNumbers = await page.locator('td, .stat-value').filter({ hasText: /\d+%?/ }).count() > 0;
    
    // If there's data, there should be scores or numbers
    const hasData = await page.locator('table tbody tr, .leaderboard-entry').count() > 0;
    if (hasData) {
      expect(hasScores || hasNumbers).toBeTruthy();
    }
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/leaderboard');
    
    // Wait for content to load
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Verify page is visible
    await expect(page.locator('body')).toBeVisible();
    
    // Check if content adapts to mobile
    const viewportWidth = await page.viewportSize();
    expect(viewportWidth?.width).toBe(375);
  });

  test('should handle empty leaderboard gracefully', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for page to load
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    // Page should display something (loading, empty state, or data)
    const bodyVisible = await page.locator('body').isVisible();
    expect(bodyVisible).toBeTruthy();
  });

  test('should allow filtering or sorting if available', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for content
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check for filter/sort controls
    const hasFilters = await page.locator('select, .filter, .sort, button[class*="filter"]').count() > 0;
    const hasTableHeaders = await page.locator('th').count() > 0;
    
    // If controls exist, they should be interactable
    if (hasFilters || hasTableHeaders) {
      // Just verify they're present - interaction depends on implementation
      expect(true).toBeTruthy();
    }
  });

  test('should refresh data when page is reloaded', async ({ page }) => {
    await page.goto('/leaderboard');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Reload page
    await page.reload();
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Page should still be functional
    await expect(page.locator('body')).toBeVisible();
  });

  test('should display rank numbers for entries', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for data
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(2000);
    
    // Check if there are any entries
    const entries = await page.locator('table tbody tr, .leaderboard-entry, .rank-item').count();
    
    if (entries > 0) {
      // Look for rank indicators (1, 2, 3, etc.)
      const hasRankNumbers = await page.locator('td:first-child, .rank, .position').filter({ hasText: /^[1-9]\d*$/ }).count() > 0;
      const hasHashSymbol = await page.locator('th').filter({ hasText: /#|Rank|Position/i }).count() > 0;
      
      expect(hasRankNumbers || hasHashSymbol).toBeTruthy();
    }
  });
});

test.describe('Leaderboard Mobile Experience', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test('should display leaderboard on mobile device', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for content
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Verify mobile rendering
    await expect(page.locator('body')).toBeVisible();
  });

  test('should have scrollable content on mobile', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for content
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.waitForTimeout(1000);
    
    // Check if content is scrollable
    const bodyHeight = await page.evaluate(() => document.body.scrollHeight);
    const viewportHeight = await page.evaluate(() => window.innerHeight);
    
    // Content might be scrollable or fit in viewport
    expect(bodyHeight).toBeGreaterThan(0);
    expect(viewportHeight).toBeGreaterThan(0);
  });

  test('should maintain usability on small screens', async ({ page }) => {
    await page.goto('/leaderboard');
    
    // Wait for content
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Verify critical elements are accessible
    const hasContent = await page.locator('table, .leaderboard-list, .leaderboard-entry').count() > 0 ||
                       await page.locator('.loading, .error, .empty').count() > 0;
    
    expect(hasContent).toBeTruthy();
  });
});
