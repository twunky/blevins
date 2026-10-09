import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });
interface Identity { accountId: string; label: string }
interface DateEntry { dateId: string; title: string; description: string; date: string; version: string; canDelete: boolean }
const section = (page: Page) => page.locator('app-shared-family-dates');
async function identity(page: Page, actor: Identity, familyId: string) {
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption(actor.accountId);
  await page.goto('/family/settings');
  const familySelector = page.getByLabel('Current Family', { exact: true });
  if (await familySelector.count()) await familySelector.selectOption(familyId);
  await expect(section(page)).toBeVisible(); await expect(section(page)).not.toContainText('Loading shared dates');
}

test('creator and associated viewer create/edit/delete a date; unauthorized identity is denied; narrow UI and Retry work', async ({ page, request }) => {
  test.setTimeout(240_000); await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (prefix: string) => identities.find(i => i.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const family = (await apiJson<{ familyGroupId: string; name: string }[]>(request, '/api/family-groups/mine', a.accountId)).body.find(f => f.name === 'Development Family')!;
  const path = `/api/family-groups/${family.familyGroupId}/dates`;
  await identity(page, a, family.familyGroupId);
  await expect(section(page)).toContainText('No shared dates are available to you');
  await section(page).getByRole('button', { name: 'Add a date', exact: true }).click();
  await section(page).getByLabel('Title', { exact: true }).fill('Family milestone');
  await section(page).getByLabel('Date including year').fill('2028-02-29');
  await section(page).getByLabel('Description', { exact: true }).fill('A specific date to remember together.');
  await section(page).getByRole('checkbox', { name: 'Development Audience Member' }).check();
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('shared-family-dates'), 'create-390.png'), fullPage: true });
  await section(page).getByRole('button', { name: 'Create date', exact: true }).focus(); await page.keyboard.press('Enter');
  await expect(section(page)).toContainText('Date created'); await expect(section(page)).toContainText('Feb 29, 2028');
  const entry = (await apiJson<{ dates: DateEntry[] }>(request, path, a.accountId)).body.dates[0];
  expect(entry.date).toBe('2028-02-29');

  // A selected/forged Family and known ID do not grant B membership or date access.
  const denied = await request.put(`${apiBaseUrl}${path}/${entry.dateId}`, {
    headers: { [developmentAccountHeader]: b.accountId },
    data: { version: entry.version, title: 'Denied', description: 'Forged Family', date: entry.date, familyGroupId: family.familyGroupId },
  });
  expect(denied.status()).toBe(404); expect(await denied.text()).toBe('');
  expect((await request.get(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: b.accountId } })).status()).toBe(404);

  await identity(page, c, family.familyGroupId);
  await expect(section(page)).toContainText('Family milestone'); await expect(section(page)).toContainText('Development Audience Member');
  await expect(section(page).getByRole('button', { name: 'Delete date', exact: true })).toHaveCount(0);
  await section(page).getByRole('button', { name: 'Edit date', exact: true }).click();
  await expect(section(page).locator('fieldset')).toHaveCount(0);
  await section(page).getByLabel('Date including year').fill('2028-03-01');
  await section(page).getByLabel('Description', { exact: true }).fill('Updated by an associated member.');
  await section(page).getByRole('button', { name: 'Save date', exact: true }).click();
  await expect(section(page)).toContainText('Date saved'); await expect(section(page)).toContainText('Mar 1, 2028');
  await identity(page, a, family.familyGroupId);
  await expect(section(page)).toContainText('Updated by an associated member.');
  await page.setViewportSize({ width: 390, height: 844 }); await page.waitForLoadState('networkidle');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await section(page).getByRole('button', { name: 'Delete date', exact: true }).click();
  await expect(section(page)).toContainText('disappear immediately for everyone'); await expect(section(page)).toContainText('cannot restore');
  await page.screenshot({ path: resolve(captureDirectory('shared-family-dates'), 'delete-confirmation-390.png'), fullPage: true });
  await section(page).getByRole('button', { name: 'Confirm delete', exact: true }).click();
  await expect(section(page)).toContainText('Date deleted'); await expect(section(page)).not.toContainText('Family milestone');
  await identity(page, c, family.familyGroupId); await expect(section(page)).toContainText('No shared dates are available to you');
  expect((await apiJson<{ dates: DateEntry[] }>(request, path, a.accountId)).body.dates).toEqual([]);
  expect((await apiJson<{ dates: DateEntry[] }>(request, path, c.accountId)).body.dates).toEqual([]);

  // One transient, explicitly intercepted read failure exercises Retry after the real journey.
  await page.route(`**${path}`, route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await section(page).getByRole('button', { name: 'Refresh dates', exact: true }).click();
  await expect(section(page)).toContainText('could not be loaded');
  await section(page).getByRole('button', { name: 'Retry loading dates' }).click();
  await expect(section(page)).toContainText('No shared dates are available to you');
});
