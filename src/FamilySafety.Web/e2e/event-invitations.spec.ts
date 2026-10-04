import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });
const captures = captureDirectory('event-invitations');
interface Identity { accountId: string; label: string }
interface Family { familyGroupId: string; name: string }
interface Profile { profileId: string; displayName: string }
interface World { a: Identity; b: Identity; c: Identity; family: Family; second: Family; bProfile: Profile; cProfile: Profile }
let world: World | null = null;
const section = (page: Page) => page.locator('app-event-participation');
const discovery = (page: Page) => page.locator('.events-dash--rsvps');

async function post(request: APIRequestContext, account: Identity, path: string, data: unknown) {
  return request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: account.accountId }, data });
}
async function setup(request: APIRequestContext): Promise<World> {
  if (world) return world;
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (label: string) => identities.find(identity => identity.label.startsWith(label))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const families = (await apiJson<Family[]>(request, '/api/family-groups/mine', a.accountId)).body;
  const family = families.find(f => f.name === 'Development Family')!;
  const second = families.find(f => f.name === 'Development Second Family')!;
  expect((await post(request, a, '/api/membership-invitations', { familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status()).toBe(202);
  const outbox = (await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', a.accountId)).body;
  const token = decodeURIComponent(outbox.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await post(request, b, '/api/membership-invitations/accept', { token })).status()).toBe(200);
  world = { a, b, c, family, second,
    bProfile: (await apiJson<Profile>(request, '/api/profile', b.accountId)).body,
    cProfile: (await apiJson<Profile>(request, '/api/profile', c.accountId)).body };
  return world;
}
async function createEvent(request: APIRequestContext, w: World, name: string, second = false) {
  const response = await post(request, w.a, `/api/family-groups/${(second ? w.second : w.family).familyGroupId}/events`, {
    name, description: 'Invitation verification gathering', date: '2020-01-01', time: '18:30', location: 'Family hall',
    audienceProfileIds: second ? [w.bProfile.profileId] : [w.bProfile.profileId, w.cProfile.profileId],
    editorProfileIds: second ? [] : [w.cProfile.profileId],
  });
  expect(response.status()).toBe(201);
  return (await response.json()).eventId as string;
}
async function identity(page: Page, account: Identity, path: string) {
  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption({ label: account.label });
  await page.goto(path);
  await expect(page.getByLabel('Active test identity')).toHaveValue(account.accountId);
}
async function capture(page: Page, state: string, axe = true) {
  await page.waitForLoadState('networkidle');
  for (const width of [1440, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    expect(await page.evaluate(() => document.documentElement.scrollWidth), `${state} overflow`).toBeLessThanOrEqual(width);
    if (axe) {
      const result = await new AxeBuilder({ page }).analyze();
      expect(result.violations, `${state} ${width} accessibility`).toEqual([]);
    }
    await page.screenshot({ path: resolve(captures, `${state}-${width}.png`), fullPage: true });
  }
}
async function chooseFamily(page: Page, family: Family) {
  await page.getByLabel('Current Family', { exact: true }).selectOption({ label: family.name });
}

test('organizer issue, own Pending discovery, every RSVP, revoke, re-invite and nondisclosure', async ({ page, request }) => {
  test.setTimeout(240_000);
  const w = await setup(request);
  const id = await createEvent(request, w, 'Invitation gathering');
  await identity(page, w.a, `/events/${id}`);
  await expect(section(page)).toContainText('No invitations have been issued');
  await expect(section(page).getByRole('button', { name: `Invite ${w.bProfile.displayName}`, exact: true })).toBeVisible();
  await expect(section(page).getByRole('button', { name: /Invite Development Adult/ })).toHaveCount(0);
  await capture(page, 'organizer-empty');
  // Actual keyboard activation of the new organizer action.
  await section(page).getByRole('button', { name: `Invite ${w.bProfile.displayName}`, exact: true }).focus();
  await page.keyboard.press('Enter');
  await expect(section(page)).toContainText('Invitation issued');
  const first = (await apiJson<any>(request, `/api/events/${id}/invitations`, w.a.accountId)).body.invitations[0].invitation;
  expect(first.rsvpStatus).toBe('Pending'); expect(first.rsvpUpdatedAtUtc).toBeNull();
  await capture(page, 'organizer-pending');

  await identity(page, w.b, '/events'); await chooseFamily(page, w.family);
  await expect(discovery(page).getByRole('link', { name: 'Invitation gathering' })).toHaveAttribute('href', `/events/${id}`);
  await capture(page, 'pending-dashboard');
  await chooseFamily(page, w.second);
  await expect(discovery(page)).toContainText('no pending Event invitations');
  await expect(discovery(page)).not.toContainText('Invitation gathering');
  await chooseFamily(page, w.family);
  await discovery(page).getByRole('link', { name: 'Invitation gathering' }).click();
  await expect(section(page).getByRole('heading', { name: 'Your invitation' })).toBeVisible();
  await expect(section(page)).toContainText('not submitted a response');
  await expect(section(page).getByRole('button', { name: /^Invite |Revoke/ })).toHaveCount(0);
  await capture(page, 'invitee-pending');
  // Keyboard-only choice and submit. Focus remains visibly on an actual control.
  await section(page).getByRole('radio', { name: 'Going', exact: true }).focus();
  await page.keyboard.press('Space');
  await section(page).getByRole('button', { name: 'Save RSVP' }).focus(); await page.keyboard.press('Enter');
  await expect(section(page)).toContainText('Your RSVP was saved');
  await expect(section(page)).toContainText('Current RSVP: Going');
  await capture(page, 'invitee-going');
  await page.getByRole('link', { name: 'Back to Events' }).click();
  await expect(discovery(page)).toContainText('no pending Event invitations');

  await identity(page, w.a, `/events/${id}`);
  await expect(section(page)).toContainText('Active · Going'); await capture(page, 'organizer-going');
  await identity(page, w.b, `/events/${id}`);
  for (const response of ['Maybe', 'Not Going', 'Pending']) {
    await section(page).getByRole('radio', { name: response, exact: true }).check();
    await section(page).getByRole('button', { name: 'Save RSVP' }).click();
    await expect(section(page)).toContainText(`Current RSVP: ${response}`);
  }
  await identity(page, w.c, `/events/${id}`);
  await expect(page.getByRole('button', { name: 'Edit event details' })).toBeVisible();
  await expect(section(page)).toContainText('no active invitation');
  await expect(section(page)).not.toContainText(w.bProfile.displayName);
  await expect(section(page).getByRole('button')).toHaveCount(0);
  await capture(page, 'editor-no-invitation');

  await identity(page, w.a, `/events/${id}`);
  await section(page).getByRole('button', { name: `Revoke invitation for ${w.bProfile.displayName}` }).click();
  await expect(section(page)).toContainText('final RSVP stays'); await capture(page, 'revoke-confirmation');
  await section(page).getByRole('button', { name: 'Confirm revoke' }).focus(); await page.keyboard.press('Enter');
  await expect(section(page)).toContainText('Revoked · Pending'); await capture(page, 'organizer-revoked');
  await identity(page, w.b, `/events/${id}`);
  await expect(section(page)).toContainText('no active invitation');
  await expect(section(page).getByRole('button', { name: 'Save RSVP' })).toHaveCount(0); await capture(page, 'invitee-revoked');
  await page.getByRole('link', { name: 'Back to Events' }).click(); await expect(discovery(page)).toContainText('no pending');
  await identity(page, w.a, `/events/${id}`);
  await section(page).getByRole('button', { name: `Invite ${w.bProfile.displayName}`, exact: true }).click();
  await expect(section(page)).toContainText('Invitation issued');
  const history = (await apiJson<any>(request, `/api/events/${id}/invitations`, w.a.accountId)).body.invitations;
  expect(history).toHaveLength(2);
  const current = history.find((row: any) => !row.invitation.revokedAtUtc).invitation;
  expect(current.invitationId).not.toBe(first.invitationId); expect(current.rsvpUpdatedAtUtc).toBeNull();
  await identity(page, w.b, '/events'); await chooseFamily(page, w.family);
  await discovery(page).getByRole('link', { name: 'Invitation gathering' }).click();
  await expect(section(page)).toContainText('Current RSVP: Pending');

  const second = await createEvent(request, w, 'Private second-Family gathering', true);
  for (const path of [`/api/events/${second}/invitations`, `/api/events/${second}/invitation/mine`]) {
    const response = await request.get(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: w.c.accountId } });
    expect(response.status()).toBe(404); expect(await response.text()).toBe('');
  }
  await identity(page, w.c, `/events/${second}`);
  await expect(page.locator('main')).toContainText('unavailable');
  await expect(section(page)).toHaveCount(0);
  await expect(page.locator('main')).not.toContainText('Private second-Family gathering');
  await expect(page.locator('.current-family strong')).toHaveText(w.family.name);
  await capture(page, 'unauthorized-event');
});

test('protected read loading, failure Retry and stale-write refresh recovery', async ({ page, request }) => {
  test.setTimeout(180_000);
  const w = await setup(request);
  const id = await createEvent(request, w, 'Recovery gathering');
  expect((await post(request, w.a, `/api/events/${id}/invitations`, { inviteeProfileId: w.bProfile.profileId })).status()).toBe(200);
  await page.route(`**/api/events/${id}/invitations`, route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await identity(page, w.a, `/events/${id}`);
  await expect(section(page)).toContainText('could not be loaded'); await capture(page, 'organizer-read-error');
  await section(page).getByRole('button', { name: 'Retry loading invitations' }).click();
  await expect(section(page)).toContainText('Active · Pending');

  await identity(page, w.b, '/events'); await chooseFamily(page, w.family);
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  await page.route(`**/api/events/${id}/invitation/mine`, async route => { await gate; await route.fulfill({ status: 500, body: '{}' }); }, { times: 1 });
  await discovery(page).getByRole('link', { name: 'Recovery gathering' }).click();
  await expect(section(page)).toContainText('Loading invitations');
  // Held read: no network-idle/axe wait. This capture is an explicitly intercepted read.
  await page.screenshot({ path: resolve(captures, 'invitee-loading-390.png'), fullPage: true });
  release();
  await expect(section(page)).toContainText('could not be loaded'); await capture(page, 'invitee-read-error');
  await section(page).getByRole('button', { name: 'Retry loading invitations' }).click();
  await expect(section(page)).toContainText('Current RSVP: Pending');
  // Change the real server version independently of this rendered client.
  const own = (await apiJson<any>(request, `/api/events/${id}/invitation/mine`, w.b.accountId)).body.invitation;
  const update = await request.put(`${apiBaseUrl}/api/events/${id}/invitation/mine/rsvp`, {
    headers: { [developmentAccountHeader]: w.b.accountId }, data: { invitationId: own.invitationId, version: own.version, rsvpStatus: 'Maybe' },
  });
  expect(update.status()).toBe(200);
  await section(page).getByRole('radio', { name: 'Going', exact: true }).check();
  await section(page).getByRole('button', { name: 'Save RSVP' }).click();
  await expect(section(page)).toContainText('This invitation changed');
  await expect(section(page).getByRole('button', { name: 'Save RSVP' })).toBeDisabled(); await capture(page, 'invitee-conflict');
  await section(page).getByRole('button', { name: 'Refresh invitations' }).click();
  await expect(section(page)).toContainText('Current RSVP: Maybe');
  await section(page).getByRole('radio', { name: 'Going', exact: true }).check();
  await section(page).getByRole('button', { name: 'Save RSVP' }).click();
  await expect(section(page)).toContainText('Current RSVP: Going');
});
