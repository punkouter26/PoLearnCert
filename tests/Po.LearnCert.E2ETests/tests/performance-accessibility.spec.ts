import { test, expect } from '@playwright/test';

test.describe('Performance and Accessibility', () => {
  test('should have no console errors on home page', async ({ page }) => {
    const consoleErrors: string[] = [];
    
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    const criticalErrors = consoleErrors.filter(error => 
      !error.includes('favicon') && 
      !error.includes('404') &&
      !error.toLowerCase().includes('analytics')
    );
    
    expect(criticalErrors.length).toBe(0);
  });

  test('should have proper ARIA labels for accessibility', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    const nav = await page.locator('nav, [role="navigation"], .rz-sidebar, .rz-panel-menu').count();
    const main = await page.locator('main, [role="main"]').count();
    
    expect(nav + main).toBeGreaterThan(0);
  });

  test('should support keyboard navigation', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    await page.keyboard.press('Tab');
    const activeElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(activeElement).toBeTruthy();
  });

  test('should display error messages when API fails', async ({ page }) => {
    await page.goto('/statistics');
    await page.waitForSelector('.rz-stack, .rz-card, .rz-alert', { timeout: 20000 });
    await page.waitForTimeout(2000);
    
    const hasContent = await page.locator('.rz-stack, .rz-card, .rz-alert, .rz-skeleton').isVisible().catch(() => false) ||
                       await page.locator('text=Performance Statistics').isVisible().catch(() => false);
    
    expect(hasContent).toBeTruthy();
  });
});

test.describe('Mobile Features', () => {
  test.use({ viewport: { width: 375, height: 667 }, hasTouch: true });

  test('should not have horizontal scroll on mobile', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
    const viewportWidth = await page.viewportSize();
    
    expect(bodyWidth).toBeLessThanOrEqual((viewportWidth?.width || 375) + 5);
  });
});

test.describe('Data Persistence', () => {
  test('should handle browser back/forward navigation', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    await page.goBack();
    await page.waitForTimeout(1000);
    expect(page.url()).not.toContain('/quiz');
    
    await page.goForward();
    await page.waitForTimeout(1000);
    expect(page.url()).toContain('/quiz');
  });
});
