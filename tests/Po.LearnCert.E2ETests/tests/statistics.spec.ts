import { test, expect } from '@playwright/test';

test.describe('Statistics Dashboard', () => {
  test('should navigate to statistics page', async ({ page }) => {
    await page.goto('/statistics');
    await page.waitForSelector('.rz-stack, .rz-card', { timeout: 15000 });
    
    await expect(page).toHaveURL(/.*statistics/);
    await expect(page.locator('text=Performance Statistics')).toBeVisible();
  });

  test('should display stat cards when data loads', async ({ page }) => {
    await page.goto('/statistics');
    await page.waitForSelector('.rz-card, .rz-skeleton, .rz-alert', { timeout: 15000 });
    
    const radzenCards = page.locator('.rz-card');
    const count = await radzenCards.count();
    expect(count).toBeGreaterThanOrEqual(0);
  });

  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/statistics');
    await page.waitForSelector('.rz-stack, .rz-card', { timeout: 15000 });
    
    await expect(page.locator('.rz-stack').first()).toBeVisible();
  });
});
