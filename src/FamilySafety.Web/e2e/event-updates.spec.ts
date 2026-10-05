import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });
interface Identity { accountId: string; label: string }
interface Update { updateId: string; text: string; version: string }
const section = (page: Page) => page.locator('app-event-updates');
async function post(request: APIRequestContext, actor: Identity, path: string, data: unknown) {
  return request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: actor.accountId }, data });
}
async function identity(page: Page, actor: Identity, eventId: string) {
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption({ label: actor.label });
  await page.goto(`/events/${eventId}`); await expect(section(page)).toContainText('Event Updates');
  await expect(section(page)).not.toContainText('Loading Updates');
}

test('organizer posts, edits and hides; current viewer reads only visible text; stale and unauthorized writes fail', async ({ page, request }) => {
  test.setTimeout(240_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (prefix: string) => identities.find(i => i.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const families = (await apiJson<{ familyGroupId: string; name: string }[]>(request, '/api/family-groups/mine', a.accountId)).body;
  const family = families.find(f => f.name === 'Development Family')!;
  const cp = (await apiJson<{ profileId: string }>(request, '/api/profile', c.accountId)).body;
  const created = await post(request, a, `/api/family-groups/${family.familyGroupId}/events`, {
    name: 'Updates gathering', description: 'Simple organizer coordination', date: '2026-10-04', time: '18:30', location: 'Family hall',
    audienceProfileIds: [cp.profileId], editorProfileIds: [cp.profileId],
  });
  expect(created.status()).toBe(201); const eventId = (await created.json()).eventId as string;
  const base = `/api/events/${eventId}/updates`;
  await identity(page, a, eventId); await expect(section(page)).toContainText('No Updates have been posted');
  await section(page).getByLabel('Update text').fill('Meet at the front entrance.');
  await section(page).getByRole('button', { name: 'Post Update', exact: true }).focus(); await page.keyboard.press('Enter');
  await expect(section(page)).toContainText('Meet at the front entrance.');
  const original = (await apiJson<{ updates: Update[] }>(request, base, a.accountId)).body.updates[0];

  await identity(page, c, eventId);
  await expect(page.getByRole('button', { name: 'Edit event details' })).toBeVisible();
  await expect(section(page)).toContainText('Meet at the front entrance.');
  await expect(section(page).getByRole('button', { name: /Post Update|Edit Update|Hide Update/ })).toHaveCount(0);
  await expect(section(page).getByText('Posted', { exact: false })).toBeVisible();
  await page.setViewportSize({ width: 390, height: 844 }); await page.waitForLoadState('networkidle');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('event-updates'), 'viewer-390.png'), fullPage: true });
  expect((await post(request, c, base, { text: 'Editor cannot post' })).status()).toBe(404);
  expect((await request.put(`${apiBaseUrl}${base}/${original.updateId}`, {
    headers: { [developmentAccountHeader]: c.accountId }, data: { version: original.version, text: 'Denied edit' },
  })).status()).toBe(404);
  expect((await post(request, c, `${base}/${original.updateId}/hide`, { version: original.version })).status()).toBe(404);

  await identity(page, a, eventId);
  await section(page).getByRole('button', { name: 'Edit Update', exact: true }).click();
  await section(page).getByLabel('Update text').fill('Use the side entrance instead.');
  await section(page).getByRole('button', { name: 'Save Update', exact: true }).click();
  await expect(section(page)).toContainText('Use the side entrance instead.');
  await expect(section(page)).toContainText('Edited');
  await page.setViewportSize({ width: 1440, height: 1000 }); await page.waitForLoadState('networkidle');
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('event-updates'), 'organizer-edited-1440.png'), fullPage: true });
  await identity(page, c, eventId); await expect(section(page)).toContainText('Use the side entrance instead.');
  await expect(section(page)).not.toContainText('Meet at the front entrance.');
  const visible = (await apiJson<{ updates: Update[] }>(request, base, c.accountId)).body;
  expect(JSON.stringify(visible)).not.toContain('Meet at the front entrance.');
  expect(JSON.stringify(visible)).not.toMatch(/revision|hiddenAt/i);

  await identity(page, a, eventId);
  const current = visible.updates[0];
  expect((await request.put(`${apiBaseUrl}${base}/${current.updateId}`, {
    headers: { [developmentAccountHeader]: a.accountId }, data: { version: current.version, text: 'Final entrance instructions.' },
  })).status()).toBe(200);
  await section(page).getByRole('button', { name: 'Hide Update', exact: true }).click();
  await section(page).getByRole('button', { name: 'Confirm hide' }).click();
  await expect(section(page)).toContainText('This Update changed');
  await expect(section(page).getByRole('button', { name: 'Edit Update', exact: true })).toBeDisabled();
  await section(page).getByRole('button', { name: 'Refresh Updates' }).click();
  await expect(section(page)).toContainText('Final entrance instructions.');
  await section(page).getByRole('button', { name: 'Hide Update', exact: true }).click();
  await expect(section(page)).toContainText('cannot be restored here');
  await section(page).getByRole('button', { name: 'Confirm hide' }).click();
  await expect(section(page)).toContainText('Update hidden'); await expect(section(page)).toContainText('No Updates have been posted');
  await identity(page, c, eventId); await expect(section(page)).toContainText('No Updates have been posted');
  await expect(section(page)).not.toContainText('Final entrance instructions.');
  expect((await apiJson<{ updates: Update[] }>(request, base, c.accountId)).body.updates).toEqual([]);
  expect((await apiJson<{ updates: Update[] }>(request, base, a.accountId)).body.updates).toEqual([]);

  // B is a member of another Family; a selected/forged Family and known IDs grant nothing.
  const denied = await request.get(`${apiBaseUrl}${base}?familyGroupId=${family.familyGroupId}`, { headers: { [developmentAccountHeader]: b.accountId } });
  expect(denied.status()).toBe(404); expect(await denied.text()).toBe('');
  expect((await post(request, b, `${base}/${original.updateId}/hide`, { version: original.version, familyGroupId: family.familyGroupId })).status()).toBe(404);
  await identity(page, a, eventId);
  // Intercepted transient read failure exercises Retry, separate from real lifecycle evidence.
  await page.route(`**${base}`, route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await section(page).getByRole('button', { name: 'Refresh Updates' }).click(); await expect(section(page)).toContainText('could not be loaded');
  await section(page).getByRole('button', { name: 'Retry loading Updates' }).click(); await expect(section(page)).toContainText('No Updates have been posted');
});
