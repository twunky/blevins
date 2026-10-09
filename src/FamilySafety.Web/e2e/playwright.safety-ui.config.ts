import { defineConfig } from '@playwright/test';
import { resolve } from 'node:path';

// Every API read is intercepted; no backend or database is used.
export default defineConfig({
  testDir: '.', testMatch: 'safety-homepage.ui.ts', workers: 1, retries: 0,
  timeout: 60_000, reporter: 'list', outputDir: 'test-results/safety-ui',
  use: { baseURL: 'http://localhost:4203', browserName: 'chromium', headless: true },
  webServer: { command: 'npm start -- --host localhost --port 4203', cwd: resolve(__dirname, '..'), url: 'http://localhost:4203', reuseExistingServer: false, timeout: 120_000 },
});
