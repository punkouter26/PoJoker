import { test, expect } from '@playwright/test';

test.describe('Diagnostics Page', () => {
  test('loads diagnostics page', async ({ page }) => {
    const response = await page.goto('/diag');
    expect(response?.status()).toBeLessThan(500);
  });

  test('page renders content', async ({ page }) => {
    await page.goto('/diag');
    await page.waitForLoadState('domcontentloaded');
    
    // Page should have visible content
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });
});
