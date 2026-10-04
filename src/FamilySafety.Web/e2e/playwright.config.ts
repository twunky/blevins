import { resolve } from 'node:path';
import { defineConfig, type Project } from '@playwright/test';

const webRoot = resolve(__dirname, '..');

// Each spec file declares its own scenario with useIsolatedScenario(...) (e2e/fixtures/isolated-scenario.ts),
// which gives it a fresh database and its own API on port 5080. Only the Angular dev server is shared.
//
// Optional explicit file order, for order-independence checks only; it does not affect any scenario:
//   $env:FAMILY_SAFETY_E2E_SPEC_ORDER = 'trip-audience-removal,invitation-handoff,incident-workflow,concern-workflow'
// Each listed file becomes a project that depends on the previous one. Unset, files run in the default order.
const specOrder = (process.env.FAMILY_SAFETY_E2E_SPEC_ORDER ?? '')
  .split(',')
  .map(spec => spec.trim().replace(/\.spec\.ts$/, ''))
  .filter(Boolean);
const orderedProjects: Project[] | undefined = specOrder.length
  ? specOrder.map((spec, index) => ({
    name: `${index + 1}-${spec}`,
    testMatch: `${spec}.spec.ts`,
    dependencies: index ? [`${index}-${specOrder[index - 1]}`] : [],
  }))
  : undefined;

export default defineConfig({
  testDir: '.',
  testMatch: '*.spec.ts',
  projects: orderedProjects,
  fullyParallel: false,
  // One worker: every scenario API uses the fixed port 5080 that the Angular proxy targets.
  workers: 1,
  retries: 0,
  timeout: 120_000,
  expect: { timeout: 15_000 },
  reporter: 'list',
  outputDir: resolve(__dirname, 'test-results'),
  globalSetup: resolve(__dirname, 'global-setup.ts'),
  globalTeardown: resolve(__dirname, 'global-teardown.ts'),
  use: {
    baseURL: 'http://localhost:4200',
    browserName: 'chromium',
    headless: true,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command: 'npm start -- --host localhost --port 4200',
      cwd: webRoot,
      url: 'http://localhost:4200',
      timeout: 180_000,
      reuseExistingServer: false,
      stdout: 'ignore',
      stderr: 'pipe',
    },
  ],
});
