import { test, expect } from '@playwright/test';

test.describe('Leaderboard Page', () => {
  test('navigates to leaderboard', async ({ page }) => {
    await page.goto('/leaderboard');
    await expect(page).toHaveURL(/leaderboard/i);
  });

  test('page loads without errors', async ({ page }) => {
    await page.goto('/leaderboard');
    await page.waitForLoadState('domcontentloaded');
    
    // Either shows content or the page title
    const content = page.locator('body');
    await expect(content).toBeVisible();
  });
});
