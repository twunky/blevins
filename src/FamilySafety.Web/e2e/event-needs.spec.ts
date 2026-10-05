import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true });
interface Identity { accountId: string; label: string }
interface Profile { profileId: string; displayName: string }
const section = (page: Page) => page.locator('app-event-needs');
const row = (page: Page, title = 'Bring chairs') => section(page).getByRole('article', { name: title, exact: true });
async function post(request: APIRequestContext, actor: Identity, path: string, data: unknown) {
  return request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: actor.accountId }, data });
}
async function identity(page: Page, actor: Identity, eventId: string) {
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption({ label: actor.label });
  await page.goto(`/events/${eventId}`); await expect(section(page)).toContainText('Event Needs');
  await expect(section(page)).not.toContainText('Loading Needs');
}

test('organizer management, anonymous peer state, personal workflow, automatic release and recovery', async ({ page, request }) => {
  test.setTimeout(240_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (prefix: string) => identities.find(i => i.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const families = (await apiJson<{ familyGroupId: string; name: string }[]>(request, '/api/family-groups/mine', a.accountId)).body;
  const family = families.find(f => f.name === 'Development Family')!;
  expect((await post(request, a, '/api/membership-invitations', { familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status()).toBe(202);
  const outbox = (await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', a.accountId)).body;
  const token = decodeURIComponent(outbox.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await post(request, b, '/api/membership-invitations/accept', { token })).status()).toBe(200);
  const bp = (await apiJson<Profile>(request, '/api/profile', b.accountId)).body;
  const cp = (await apiJson<Profile>(request, '/api/profile', c.accountId)).body;
  const created = await post(request, a, `/api/family-groups/${family.familyGroupId}/events`, {
    name: 'Needs gathering', description: 'Simple Event responsibilities', date: '2026-10-04', time: '18:30', location: 'Family hall',
    audienceProfileIds: [bp.profileId, cp.profileId], editorProfileIds: [cp.profileId],
  });
  expect(created.status()).toBe(201); const eventId = (await created.json()).eventId as string;
  const base = `/api/events/${eventId}`;
  for (const profile of [bp, cp]) expect((await post(request, a, `${base}/invitations`, { inviteeProfileId: profile.profileId })).status()).toBe(200);

  await identity(page, a, eventId); await expect(section(page)).toContainText('No Needs have been added');
  await section(page).getByLabel('Need title').fill('Bring chairs');
  await section(page).getByLabel('Short detail (optional)').fill('Folding chairs');
  await section(page).getByLabel('Category (optional)').selectOption('Supplies');
  await section(page).getByRole('button', { name: 'Add Need', exact: true }).click();
  await expect(row(page)).toContainText('Open');
  await row(page).getByRole('button', { name: 'Edit Need', exact: true }).click();
  await section(page).getByLabel('Short detail (optional)').fill('Chairs for the hall');
  await section(page).getByRole('button', { name: 'Save Need', exact: true }).click();
  await expect(row(page)).toContainText('Chairs for the hall');
  await row(page).getByLabel('Ask a participant to take this Need').selectOption(bp.profileId);
  await row(page).getByRole('button', { name: 'Request assignment' }).click();
  await expect(row(page)).toContainText(`${bp.displayName} — acceptance pending`);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.waitForLoadState('networkidle');
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('event-needs'), 'organizer-assigned-1440.png'), fullPage: true });

  await identity(page, c, eventId);
  await expect(page.getByRole('button', { name: 'Edit event details' })).toBeVisible();
  await expect(row(page)).toContainText('Assigned'); await expect(row(page)).toContainText('Acceptance pending');
  await expect(section(page)).not.toContainText(bp.displayName);
  await expect(section(page).getByRole('button', { name: /Edit Need|Accept assignment|Complete Need/ })).toHaveCount(0);
  const peer = (await apiJson<any>(request, `${base}/needs`, c.accountId)).body;
  expect(peer.needs[0].responsibleProfileId).toBeNull(); expect(peer.needs[0].history).toEqual([]);
  expect(JSON.stringify(peer)).not.toContain(bp.profileId); expect(JSON.stringify(peer)).not.toContain(bp.displayName);

  await identity(page, b, eventId); await expect(row(page)).toContainText('You have a pending assignment');
  await page.setViewportSize({ width: 390, height: 844 }); await page.waitForLoadState('networkidle');
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('event-needs'), 'participant-assigned-390.png'), fullPage: true });
  await row(page).getByRole('button', { name: 'Decline assignment' }).click(); await expect(row(page)).toContainText('Open');
  // Actual keyboard claim; responsibility is personal and can be released.
  await row(page).getByRole('button', { name: 'Claim Need' }).focus(); await page.keyboard.press('Enter');
  await expect(row(page)).toContainText('You claimed this Need');
  await row(page).getByRole('button', { name: 'Release Need' }).click(); await expect(row(page)).toContainText('Open');
  await row(page).getByRole('button', { name: 'Claim Need' }).click(); await expect(row(page)).toContainText('You claimed this Need');
  const participation = page.locator('app-event-participation');
  await participation.getByRole('radio', { name: 'Not Going', exact: true }).check();
  await participation.getByRole('button', { name: 'Save RSVP' }).click();
  // The participation output refreshes Needs without page reload.
  await expect(row(page)).toContainText('Open'); await expect(row(page).getByRole('button', { name: 'Claim Need' })).toHaveCount(0);
  await row(page).getByText('Responsibility history', { exact: true }).click();
  await expect(row(page)).toContainText('Released after RSVP changed to Not Going');
  await participation.getByRole('radio', { name: 'Maybe', exact: true }).check();
  await participation.getByRole('button', { name: 'Save RSVP' }).click(); await expect(row(page)).toContainText('Open');

  await identity(page, a, eventId);
  await row(page).getByLabel('Ask a participant to take this Need').selectOption(bp.profileId);
  await row(page).getByRole('button', { name: 'Request assignment' }).click(); await expect(row(page)).toContainText('Assigned');
  await identity(page, b, eventId); await row(page).getByRole('button', { name: 'Accept assignment' }).click();
  await expect(row(page)).toContainText('You claimed this Need');
  await row(page).getByRole('button', { name: 'Complete Need', exact: true }).click();
  await row(page).getByRole('button', { name: 'Confirm complete' }).click(); await expect(row(page)).toContainText('Completed');
  await identity(page, c, eventId); await expect(row(page)).toContainText('Completed');
  await expect(section(page)).not.toContainText(bp.displayName); await expect(row(page).getByText('Responsibility history')).toHaveCount(0);

  await identity(page, a, eventId);
  await section(page).getByLabel('Need title').fill('Bring food');
  await section(page).getByRole('button', { name: 'Add Need', exact: true }).click(); await expect(row(page, 'Bring food')).toContainText('Open');
  await row(page, 'Bring food').getByLabel('Ask a participant to take this Need').selectOption(bp.profileId);
  await row(page, 'Bring food').getByRole('button', { name: 'Request assignment' }).click();
  await expect(row(page, 'Bring food')).toContainText('Assigned');
  await page.locator('app-event-participation').getByRole('button', { name: `Revoke invitation for ${bp.displayName}` }).click();
  await page.locator('app-event-participation').getByRole('button', { name: 'Confirm revoke' }).click();
  await expect(row(page, 'Bring food')).toContainText('Open'); await expect(row(page)).toContainText('Completed');
  await row(page, 'Bring food').getByText('Responsibility history', { exact: true }).click();
  await expect(row(page, 'Bring food')).toContainText('Released after invitation revocation');

  // Real stale edit: refresh recovers without replaying the rejected write.
  const before = (await apiJson<any>(request, `${base}/needs`, a.accountId)).body.needs.find((n: any) => n.title === 'Bring food');
  expect((await request.put(`${apiBaseUrl}${base}/needs/${before.needId}`, {
    headers: { [developmentAccountHeader]: a.accountId }, data: { version: before.version, title: 'Bring food', detail: 'Fresh server detail', category: null },
  })).status()).toBe(200);
  await row(page, 'Bring food').getByRole('button', { name: 'Edit Need', exact: true }).click();
  await section(page).getByRole('button', { name: 'Save Need', exact: true }).click();
  await expect(section(page)).toContainText('This Need changed');
  await expect(section(page).getByRole('button', { name: 'Save Need', exact: true })).toBeDisabled();
  await section(page).getByRole('button', { name: 'Refresh Needs' }).click(); await expect(row(page, 'Bring food')).toContainText('Fresh server detail');
  await row(page, 'Bring food').getByRole('button', { name: 'Remove Need', exact: true }).click();
  await expect(row(page, 'Bring food')).toContainText('responsibility history?');
  await row(page, 'Bring food').getByRole('button', { name: 'Confirm remove' }).click(); await expect(row(page, 'Bring food')).toHaveCount(0);

  // Intercepted read failure is a recovery check, not product evidence.
  await page.route(`**${base}/needs`, route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await section(page).getByRole('button', { name: 'Refresh Needs' }).click(); await expect(section(page)).toContainText('could not be loaded');
  await section(page).getByRole('button', { name: 'Retry loading Needs' }).click(); await expect(row(page)).toContainText('Completed');
});
