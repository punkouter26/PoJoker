import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('returns response', async ({ page }) => {
    const response = await page.goto('/');
    // Accept any response that isn't a server error
    expect(response?.status()).toBeLessThan(500);
  });

  test('page has body element', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('body')).toBeVisible();
  });

  test('no critical JavaScript errors', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => errors.push(error.message));
    
    await page.goto('/');
    await page.waitForLoadState('domcontentloaded');
    
    // Filter known acceptable errors
    const criticalErrors = errors.filter(e => 
      !e.includes('ResizeObserver') && 
      !e.includes('blazor') &&
      !e.includes('Blazor')
    );
    
    expect(criticalErrors).toHaveLength(0);
  });
});
