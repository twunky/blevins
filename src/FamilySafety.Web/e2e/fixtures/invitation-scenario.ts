import { expect, type APIRequestContext, type TestInfo } from '@playwright/test';
import { currentScenarioDatabase, scenarioDatabasePattern } from './isolated-scenario';

export { captureDirectory, useIsolatedScenario } from './isolated-scenario';
export const apiBaseUrl = 'http://localhost:5080';
export const developmentAccountHeader = 'X-Development-Account';

/** The current spec file's isolated database, created by useIsolatedScenario(...). */
export function scenarioDatabase(): string {
  const database = currentScenarioDatabase();
  if (!scenarioDatabasePattern.test(database) || database === 'FamilySafety') {
    throw new Error(`Expected a unique FamilySafety_Invitation_E2E_* database, received: ${database || '(empty)'}`);
  }
  return database;
}

export async function recordScenario(testInfo: TestInfo): Promise<void> {
  const database = scenarioDatabase();
  testInfo.annotations.push({ type: 'scenario-database', description: database });
  console.log(`E2E scenario database: ${database}`);
  await testInfo.attach('scenario-database.txt', {
    body: database,
    contentType: 'text/plain',
  });
}

export async function apiJson<T>(
  request: APIRequestContext,
  path: string,
  accountId: string,
): Promise<{ status: number; body: T }> {
  const response = await request.get(`${apiBaseUrl}${path}`, {
    headers: { [developmentAccountHeader]: accountId },
  });
  return { status: response.status(), body: await response.json() as T };
}

export async function expectHealthyApi(request: APIRequestContext): Promise<void> {
  const response = await request.get(`${apiBaseUrl}/health`);
  expect(response.ok(), 'API /health should return a healthy response').toBeTruthy();
  expect((await response.text()).trim()).toBe('Healthy');
}
