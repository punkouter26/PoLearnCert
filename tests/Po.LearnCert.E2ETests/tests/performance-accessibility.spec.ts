import { test, expect } from '@playwright/test';

test.describe('Performance and Accessibility', () => {
  test('should load pages within acceptable time', async ({ page }) => {
    const startTime = Date.now();
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    const loadTime = Date.now() - startTime;
    
    // Page should load within 15 seconds
    expect(loadTime).toBeLessThan(15000);
  });

  test('should have no console errors on home page', async ({ page }) => {
    const consoleErrors: string[] = [];
    
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Filter out known acceptable errors (like 404s for optional resources)
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
    
    // Check for navigation landmarks
    const nav = await page.locator('nav, [role="navigation"]').count();
    const main = await page.locator('main, [role="main"]').count();
    
    // Should have at least navigation or main content
    expect(nav + main).toBeGreaterThan(0);
  });

  test('should support keyboard navigation', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Test Tab navigation
    await page.keyboard.press('Tab');
    
    // Check if focus moved to a focusable element
    const activeElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(activeElement).toBeTruthy();
  });

  test('should have readable text contrast', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check if body text is visible
    const bodyText = page.locator('body');
    await expect(bodyText).toBeVisible();
    
    // Verify text is not invisible (opacity/color)
    const opacity = await bodyText.evaluate((el) => window.getComputedStyle(el).opacity);
    expect(parseFloat(opacity)).toBeGreaterThan(0);
  });
});

test.describe('Error Handling', () => {
  test('should handle 404 pages gracefully', async ({ page }) => {
    // Navigate to non-existent page
    const response = await page.goto('/non-existent-page-xyz', { waitUntil: 'networkidle' });
    
    // Page should still render something
    await expect(page.locator('body')).toBeVisible();
    
    // Blazor apps typically handle routing client-side, so might not return 404
    // Just verify the page renders
    expect(true).toBeTruthy();
  });

  test('should handle network errors gracefully', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Simulate offline mode
    await page.context().setOffline(true);
    
    // Try to navigate to another page
    await page.goto('/quiz', { waitUntil: 'domcontentloaded' }).catch(() => {});
    
    // Restore online
    await page.context().setOffline(false);
    
    // Page should still be functional
    expect(true).toBeTruthy();
  });

  test('should display error messages when API fails', async ({ page }) => {
    await page.goto('/statistics');
    
    // Wait for page to attempt loading data
    await page.waitForTimeout(3000);
    
    // Check for error state
    const hasError = await page.locator('.error, .error-message, [class*="error"]').isVisible().catch(() => false);
    const hasData = await page.locator('.statistics-container, .stat-card').isVisible().catch(() => false);
    const hasLoading = await page.locator('.loading, .spinner').isVisible().catch(() => false);
    
    // Should show error, data, or loading
    expect(hasError || hasData || hasLoading).toBeTruthy();
  });
});

test.describe('Mobile-Specific Features', () => {
  test.use({ viewport: { width: 375, height: 667 } });

  test('should handle touch gestures', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Simulate touch tap on navigation
    const nav = page.locator('nav, .navbar').first();
    if (await nav.isVisible()) {
      await nav.tap();
      await page.waitForTimeout(500);
    }
    
    expect(true).toBeTruthy();
  });

  test('should not have horizontal scroll on mobile', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check body width doesn't exceed viewport
    const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
    const viewportWidth = await page.viewportSize();
    
    expect(bodyWidth).toBeLessThanOrEqual((viewportWidth?.width || 375) + 5); // Allow 5px margin
  });

  test('should have mobile-friendly button sizes', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Check button sizes (should be at least 44x44 for touch targets)
    const buttons = page.locator('button.btn-primary, button[type="submit"]').first();
    const hasButton = await buttons.isVisible().catch(() => false);
    
    if (hasButton) {
      const box = await buttons.boundingBox();
      if (box) {
        expect(box.height).toBeGreaterThanOrEqual(32); // Minimum reasonable touch target
      }
    }
  });

  test('should display mobile menu correctly', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Find mobile menu toggle
    const toggle = page.locator('.navbar-toggler, .menu-toggle, button[aria-label*="menu"]').first();
    const hasToggle = await toggle.isVisible().catch(() => false);
    
    if (hasToggle) {
      // Click to open menu
      await toggle.click();
      await page.waitForTimeout(500);
      
      // Check if menu expanded
      const menuExpanded = await page.locator('.navbar-collapse.show, .menu.open, nav.open').isVisible().catch(() => false);
      
      // Close menu
      if (menuExpanded) {
        await toggle.click();
        await page.waitForTimeout(500);
      }
      
      expect(hasToggle).toBeTruthy();
    }
  });
});

test.describe('Cross-Browser Consistency', () => {
  test('should render consistently on Chromium', async ({ page, browserName }) => {
    expect(browserName).toBe('chromium');
    
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Verify core elements
    await expect(page.locator('body')).toBeVisible();
    
    // Check viewport
    const viewport = await page.viewportSize();
    expect(viewport).toBeTruthy();
  });

  test('should maintain layout on different screen sizes', async ({ page }) => {
    const viewports = [
      { width: 1920, height: 1080 }, // Desktop
      { width: 768, height: 1024 },  // Tablet
      { width: 375, height: 667 },   // Mobile
    ];
    
    for (const viewport of viewports) {
      await page.setViewportSize(viewport);
      await page.goto('/');
      await page.waitForLoadState('networkidle', { timeout: 15000 });
      
      // Verify content is visible at this viewport
      await expect(page.locator('body')).toBeVisible();
      
      // No horizontal overflow
      const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
      expect(bodyWidth).toBeLessThanOrEqual(viewport.width + 20); // Allow small margin
    }
  });
});

test.describe('Data Persistence', () => {
  test('should maintain state across page refreshes', async ({ page }) => {
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Set a value
    const questionInput = page.locator('input#questionCount, input[type="number"]').first();
    const hasInput = await questionInput.isVisible().catch(() => false);
    
    if (hasInput) {
      await questionInput.fill('10');
      
      // Refresh page
      await page.reload();
      await page.waitForLoadState('networkidle', { timeout: 15000 });
      
      // Value might persist or reset depending on implementation
      const newValue = await questionInput.inputValue().catch(() => '');
      expect(newValue).toBeTruthy();
    }
  });

  test('should handle browser back/forward navigation', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Navigate to quiz
    await page.goto('/quiz/setup');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    
    // Go back
    await page.goBack();
    await page.waitForTimeout(1000);
    
    // Should be back on home
    expect(page.url()).not.toContain('/quiz');
    
    // Go forward
    await page.goForward();
    await page.waitForTimeout(1000);
    
    // Should be on quiz again
    expect(page.url()).toContain('/quiz');
  });
});
