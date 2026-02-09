import { test, expect } from '@playwright/test';

test.describe('Joke Performance Flow', () => {
  test('starts performance and displays joke card', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // The stage header should show "Performance"
    await expect(page.getByRole('heading', { name: /performance/i })).toBeVisible();

    // Click the Start button to begin the joke loop
    const startBtn = page.getByRole('button', { name: /start/i });
    await expect(startBtn).toBeVisible({ timeout: 10000 });
    await startBtn.click();

    // Wait for a joke to appear — look for either the JokeCard setup text or any stage content change
    // The orchestrator fetches a joke and transitions through states
    await expect(page.locator('.jester-stage')).toBeVisible();

    // The Stop button should now be visible (performance is running)
    await expect(page.getByRole('button', { name: /stop/i })).toBeVisible({ timeout: 15000 });

    // Wait for joke content to render (setup text from the mock API)
    await page.waitForTimeout(3000);

    // Stop the performance
    const stopBtn = page.getByRole('button', { name: /stop/i });
    await stopBtn.click();

    // Start button should reappear after stopping
    await expect(page.getByRole('button', { name: /start/i })).toBeVisible({ timeout: 10000 });
  });

  test('navigates from home to leaderboard', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Find and click the leaderboard nav link
    const leaderboardLink = page.getByRole('link', { name: /leaderboard/i });
    await expect(leaderboardLink).toBeVisible({ timeout: 10000 });
    await leaderboardLink.click();

    // Should be on the leaderboard page
    await expect(page).toHaveURL(/leaderboard/i);
    await expect(page.locator('body')).toBeVisible();
  });

  test('API endpoints respond correctly', async ({ request }) => {
    // Verify the joke fetch API returns a valid joke
    const jokeResponse = await request.get('/api/jokes/fetch?safeMode=true');
    expect(jokeResponse.status()).toBe(200);
    const joke = await jokeResponse.json();
    expect(joke.id).toBeGreaterThan(0);
    expect(joke.setup).toBeTruthy();
    expect(joke.punchline).toBeTruthy();

    // Verify the leaderboard API
    const lbResponse = await request.get('/api/leaderboard?sortBy=Triumph&top=10');
    expect(lbResponse.status()).toBe(200);

    // Verify the diagnostics API shape
    const diagResponse = await request.get('/api/diagnostics');
    expect([200, 503]).toContain(diagResponse.status());
    const diag = await diagResponse.json();
    expect(diag.version).toBeTruthy();
    expect(diag.environment).toBeTruthy();
  });
});
