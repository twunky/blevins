import { defineConfig } from '@playwright/test';
import { resolve } from 'node:path';

// Frontend-only scenarios intercept every API request; no database or running API is used.
export default defineConfig({
  testDir: '.', testMatch: 'activities.ui.ts', workers: 1, retries: 0,
  timeout: 60_000, expect: { timeout: 10_000 }, reporter: 'list',
  outputDir: 'test-results/activities-ui',
  use: { baseURL: 'http://localhost:4201', browserName: 'chromium', headless: true, trace: 'retain-on-failure' },
  webServer: { command: 'npm start -- --host localhost --port 4201', cwd: resolve(__dirname, '..'), url: 'http://localhost:4201', reuseExistingServer: false, timeout: 120_000 },
});
