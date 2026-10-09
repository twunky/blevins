import { defineConfig } from '@playwright/test';

// Existing documented client. Synthetic scenarios intercept reads and never write to a database.
export default defineConfig({
  testDir: '.', testMatch: 'family-homepage.ui.ts', workers: 1, retries: 0,
  timeout: 60_000, expect: { timeout: 10_000 }, reporter: 'list',
  outputDir: 'test-results/family-ui',
  use: { baseURL: 'http://localhost:4200', browserName: 'chromium', headless: true, trace: 'retain-on-failure' },
});
