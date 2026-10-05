import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });
interface Identity { accountId: string; label: string }
const component = (page: Page) => page.locator('app-check-ins');
async function actAs(page: Page, actor: Identity, familyId: string) {
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption(actor.accountId);
  await page.goto('/safety');
  const selector = page.getByLabel('Current Family', { exact: true });
  if (await selector.count()) await selector.selectOption(familyId);
  await expect(component(page)).toBeVisible(); await expect(component(page)).not.toContainText('Loading Check-Ins');
}
test('author explicitly shares a standalone Check-In; recipient reads it; nonrecipient gets no content; narrow form and Retry', async ({ page, request }) => {
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (prefix: string) => identities.find(i => i.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const family = (await apiJson<{ familyGroupId: string; name: string }[]>(request, '/api/family-groups/mine', a.accountId)).body.find(f => f.name === 'Development Family')!;
  await actAs(page, a, family.familyGroupId);
  await component(page).getByRole('button', { name: 'Create a Check-In', exact: true }).click();
  await component(page).getByLabel('Status', { exact: true }).selectOption('Delayed');
  await component(page).getByLabel('Note (optional)', { exact: true }).fill('Taking a little longer today.');
  await component(page).getByRole('checkbox', { name: 'Development Audience Member' }).check();
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('standalone-check-ins'), 'create-390.png'), fullPage: true });
  await component(page).getByRole('button', { name: 'Create Check-In', exact: true }).focus(); await page.keyboard.press('Enter');
  await expect(component(page)).toContainText('Check-In created'); await expect(component(page)).toContainText('Taking a little longer today.');
  const entry = (await apiJson<{ checkInId: string }[]>(request, '/api/check-ins/mine', a.accountId)).body[0];
  await actAs(page, c, family.familyGroupId);
  await expect(component(page).getByRole('region', { name: 'Shared with me' })).toContainText('Taking a little longer today.');
  await expect(component(page).getByRole('button', { name: /Edit|Delete|Restore/ })).toHaveCount(0);
  await actAs(page, b, family.familyGroupId);
  await expect(component(page)).not.toContainText('Taking a little longer today.');
  const denied = await request.get(`${apiBaseUrl}/api/check-ins/${entry.checkInId}?selectedFamily=${family.familyGroupId}`, { headers: { [developmentAccountHeader]: b.accountId } });
  expect(denied.status()).toBe(404); expect(await denied.text()).toBe('');
  await page.route('**/api/check-ins/mine', route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await component(page).getByRole('button', { name: 'Refresh Check-Ins', exact: true }).click();
  await expect(component(page)).toContainText('could not be loaded');
  await component(page).getByRole('button', { name: 'Retry loading Check-Ins', exact: true }).click();
  await expect(component(page)).toContainText('No Check-Ins are shared with you');
});
