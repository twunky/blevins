import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });
interface Identity { accountId: string; label: string }
const component = (page: Page) => page.locator('app-sharing-sessions');
async function actAs(page: Page, actor: Identity, familyId: string) {
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption(actor.accountId);
  await page.goto('/safety');
  const selector = page.getByLabel('Current Family', { exact: true });
  if (await selector.count()) await selector.selectOption(familyId);
  await expect(component(page)).toBeVisible(); await expect(component(page)).not.toContainText('Loading sharing');
}

test('temporary sharing permission, bounded latest updates, recipient access and author stop', async ({ page, request }) => {
  test.setTimeout(180000);
  await recordScenario(test.info());
  await page.addInitScript(() => {
    let success: PositionCallback | null = null; let failure: PositionErrorCallback | null = null;
    Object.defineProperty(navigator, 'geolocation', { configurable: true, value: {
      watchPosition: (ok: PositionCallback, error: PositionErrorCallback) => { success = ok; failure = error; return 1; },
      clearWatch: () => { success = null; failure = null; },
    } });
    Object.assign(window, { sharingTestLocation: {
      point: (latitude: number) => success?.({ coords: { latitude, longitude: -72, accuracy: 12 }, timestamp: Date.now() } as GeolocationPosition),
      fail: (code: number) => failure?.({ code } as GeolocationPositionError),
    } });
  });
  const emit = (latitude: number) => page.evaluate(lat => (window as unknown as { sharingTestLocation: { point(lat: number): void } }).sharingTestLocation.point(lat), latitude);
  const fail = (code: number) => page.evaluate(value => (window as unknown as { sharingTestLocation: { fail(code: number): void } }).sharingTestLocation.fail(value), code);
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (prefix: string) => identities.find(i => i.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const family = (await apiJson<{ familyGroupId: string; name: string }[]>(request, '/api/family-groups/mine', a.accountId)).body.find(f => f.name === 'Development Family')!;
  await actAs(page, a, family.familyGroupId);
  await component(page).getByRole('button', { name: 'Start location sharing', exact: true }).click();
  await expect(component(page).getByRole('checkbox', { name: 'Development Audience Member' })).not.toBeChecked();
  await component(page).getByRole('checkbox', { name: 'Development Audience Member' }).check();
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).include('app-sharing-sessions').analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('temporary-sharing-sessions'), 'create-390.png'), fullPage: true });
  await component(page).getByRole('button', { name: 'Start sharing and request location', exact: true }).focus(); await page.keyboard.press('Enter');
  await expect(component(page)).toContainText('Sharing started');
  await fail(1); await expect(component(page)).toContainText('Location permission denied');
  await component(page).getByRole('button', { name: 'Enable location updates', exact: true }).click();
  await fail(2); await expect(component(page)).toContainText('Location unavailable');
  await emit(42); await expect(component(page)).toContainText('Latitude 42.0000');
  // Real API minute throttle remains authoritative; geolocation itself is entirely deterministic.
  await page.waitForTimeout(61000);
  await emit(43); await expect(component(page)).toContainText('Latitude 43.0000');
  await expect(component(page)).not.toContainText('Latitude 42.0000');
  const entry = (await apiJson<{ sharingSessionId: string }[]>(request, '/api/sharing-sessions/mine', a.accountId)).body[0];
  await page.goto('/home'); await expect(component(page)).toContainText('Sharing your current / last-reported location until');
  await actAs(page, c, family.familyGroupId);
  await expect(component(page).getByRole('region', { name: 'Location shared with me' })).toContainText('Latitude 43.0000');
  await expect(component(page)).toContainText('Last updated');
  await page.setViewportSize({ width: 1440, height: 1000 });
  expect((await new AxeBuilder({ page }).include('app-sharing-sessions').analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('temporary-sharing-sessions'), 'recipient-1440.png'), fullPage: true });
  const denied = await request.get(`${apiBaseUrl}/api/sharing-sessions/${entry.sharingSessionId}?selectedFamily=${family.familyGroupId}`, { headers: { [developmentAccountHeader]: b.accountId } });
  expect(denied.status()).toBe(404); expect(await denied.text()).toBe('');
  await actAs(page, a, family.familyGroupId);
  await component(page).getByRole('button', { name: 'Stop sharing', exact: true }).click(); await expect(component(page)).toContainText('Sharing stopped');
  await actAs(page, c, family.familyGroupId); await expect(component(page)).not.toContainText('Latitude 43');
  await expect(component(page)).toContainText('No active locations are shared with you');
  expect((await request.get(`${apiBaseUrl}/api/sharing-sessions/${entry.sharingSessionId}`, { headers: { [developmentAccountHeader]: c.accountId } })).status()).toBe(404);
});
