import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';
useIsolatedScenario({ tripAudienceAdult: true });
interface Circle { safetyCircleId: string; version: string; name: string; members: { profileId: string; displayName: string }[] }
test('owner manages a private circle, confirms both explicit audiences and deletes the preset without changing target grants', async ({ page, request }) => {
  await recordScenario(test.info());
  await page.setViewportSize({ width: 1440, height: 1000 });
  const identities = (await (await request.get(apiBaseUrl + '/api/development/identities')).json()).identities as { accountId: string; label: string }[];
  const a = identities.find(i => i.label.startsWith('Adult A'))!, c = identities.find(i => i.label.startsWith('Adult C'))!;
  const family = (await apiJson<{ familyGroupId: string }[]>(request, '/api/family-groups/mine', a.accountId)).body[0];
  const url = '/api/family-groups/' + family.familyGroupId + '/safety-circles';
  await page.addInitScript(() => Object.defineProperty(navigator, 'geolocation', { configurable: true, value: {
    watchPosition: (_success: unknown, error: (failure: { code: number }) => void) => { setTimeout(() => error({ code: 1 }), 0); return 1; }, clearWatch: () => {},
  } }));
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption(a.accountId); await page.goto('/safety');
  const circles = page.locator('app-safety-circles'); await expect(circles).toContainText('You have no Safety Circles');
  await circles.getByRole('button', { name: 'Create a Safety Circle', exact: true }).click();
  await circles.getByLabel('Safety Circle name', { exact: true }).fill('Close family'); await circles.getByRole('button', { name: 'Create circle', exact: true }).click();
  await expect(circles).toContainText('Safety Circle created');
  const circleRow = circles.getByRole('article', { name: 'Close family', exact: true });
  await circleRow.getByLabel('Add eligible Family member to Close family').selectOption({ label: 'Development Audience Member' });
  await expect(circleRow.getByRole('button', { name: 'Remove Development Audience Member from Close family' })).toBeVisible();
  await page.screenshot({ path: resolve(captureDirectory('safety-circles'), 'manage-1440.png'), fullPage: true });
  const circle = (await apiJson<Circle[]>(request, url, a.accountId)).body[0];
  const check = page.locator('app-check-ins');
  await check.getByRole('button', { name: 'Create a Check-In', exact: true }).click(); await check.getByLabel('Status', { exact: true }).selectOption('Arrived');
  await check.getByLabel('Note (optional)', { exact: true }).fill('Circle journey status.');
  await check.getByRole('button', { name: 'Use a Safety Circle (optional)', exact: true }).click();
  await check.getByLabel('Safety Circle preset', { exact: true }).selectOption(circle.safetyCircleId);
  await expect(check.getByRole('checkbox', { name: 'Development Audience Member' })).toBeChecked();
  await expect(check.getByLabel('Safety Circle preset', { exact: true })).toHaveValue(circle.safetyCircleId);
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('safety-circles'), 'check-in-preset-390.png'), fullPage: true });
  await check.getByRole('button', { name: 'Create Check-In', exact: true }).focus(); await page.keyboard.press('Enter'); await expect(check).toContainText('Check-In created');
  const sharing = page.locator('app-sharing-sessions'); await sharing.getByRole('button', { name: 'Start location sharing', exact: true }).click();
  await sharing.getByRole('button', { name: 'Use a Safety Circle (optional)', exact: true }).click(); await sharing.getByLabel('Safety Circle preset', { exact: true }).selectOption(circle.safetyCircleId);
  await expect(sharing.getByRole('checkbox', { name: 'Development Audience Member' })).toBeChecked();
  await sharing.getByRole('button', { name: 'Start sharing and request location', exact: true }).click(); await expect(sharing).toContainText('Sharing started');
  const beforeChecks = (await apiJson<{ checkInId: string; recipientProfileIds: string[] }[]>(request, '/api/check-ins/mine', a.accountId)).body;
  const beforeSessions = (await apiJson<{ sharingSessionId: string; recipients: unknown[] }[]>(request, '/api/sharing-sessions/mine', a.accountId)).body;
  await circleRow.getByRole('button', { name: 'Rename circle', exact: true }).click(); await circles.getByLabel('Safety Circle name', { exact: true }).fill('Travel people'); await circles.getByRole('button', { name: 'Save circle name', exact: true }).click();
  const renamed = circles.getByRole('article', { name: 'Travel people', exact: true });
  await renamed.getByRole('button', { name: 'Remove Development Audience Member from Travel people' }).click(); await expect(renamed).toContainText('No members selected yet');
  await renamed.getByRole('button', { name: 'Delete circle', exact: true }).click(); await expect(renamed).toContainText('Existing Check-Ins and Sharing Sessions keep their own recipients');
  await renamed.getByRole('button', { name: 'Confirm circle deletion', exact: true }).click(); await expect(circles).toContainText('Safety Circle permanently deleted');
  expect((await apiJson<Circle[]>(request, url, a.accountId)).body).toEqual([]);
  expect((await apiJson<typeof beforeChecks>(request, '/api/check-ins/mine', a.accountId)).body[0].recipientProfileIds).toEqual(beforeChecks[0].recipientProfileIds);
  expect((await apiJson<typeof beforeSessions>(request, '/api/sharing-sessions/mine', a.accountId)).body[0].recipients).toEqual(beforeSessions[0].recipients);
  for (const target of ['/api/check-ins/' + beforeChecks[0].checkInId, '/api/sharing-sessions/' + beforeSessions[0].sharingSessionId]) {
    const response = await request.get(apiBaseUrl + target, { headers: { [developmentAccountHeader]: c.accountId } }); expect(response.status()).toBe(200);
  }
});
