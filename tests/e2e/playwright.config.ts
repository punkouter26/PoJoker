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
    baseURL: process.env.BASE_URL || 'http://localhost:5123',
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
    command: 'dotnet run --project ../../src/Po.Joker/Po.Joker.csproj --configuration Release --urls http://localhost:5123',
    url: 'http://localhost:5123',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
