import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  timeout: 30000,
  
  use: {
    baseURL: process.env.BASE_URL || 'https://localhost:5001',
    headless: true,
    ignoreHTTPSErrors: true,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'dotnet run --project ../../src/Po.Joker/Po.Joker.csproj --urls https://localhost:5001 --environment Development',
    url: 'https://localhost:5001/health',
    reuseExistingServer: true,
    ignoreHTTPSErrors: true,
    timeout: 120000,
    env: {
      POJOKER_USE_MOCK_AI: 'true',
      POJOKER_DISABLE_KEYVAULT: 'true',
    },
  },
});
