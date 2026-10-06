import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });
test.beforeEach(({ page }) => { page.setDefaultTimeout(15_000); });
interface Identity { accountId: string; label: string }
interface Family { familyGroupId: string; name: string }
interface Profile { profileId: string }
interface Circle { safetyCircleId: string; members: Profile[] }
interface CheckIn { checkInId: string; authorProfileId: string; recipientProfileIds: string[] }
interface Session { sharingSessionId: string; version: number; endsAtUtc: string; recipients: Profile[] | null; location: unknown }

async function world(request: APIRequestContext) {
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const find = (prefix: string) => identities.find(i => i.label.startsWith(prefix))!;
  const [admin, author, recipient] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const families = (await apiJson<Family[]>(request, '/api/family-groups/mine', admin.accountId)).body;
  const first = families.find(f => f.name === 'Development Family')!;
  const second = families.find(f => f.name === 'Development Second Family')!;
  if (!(await apiJson<Family[]>(request, '/api/family-groups/mine', author.accountId)).body.some(f => f.familyGroupId === first.familyGroupId)) {
    expect((await post(request, admin, '/api/membership-invitations', { familyGroupId: first.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status()).toBe(202);
    const outbox = (await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', admin.accountId)).body;
    const token = decodeURIComponent(outbox.at(-1)!.invitationUrl.split('#invite=')[1]);
    expect((await post(request, author, '/api/membership-invitations/accept', { token })).status()).toBe(200);
  }
  const authorProfile = (await apiJson<Profile>(request, '/api/profile', author.accountId)).body;
  const recipientProfile = (await apiJson<Profile>(request, '/api/profile', recipient.accountId)).body;
  return { admin, author, recipient, first, second, authorProfile, recipientProfile };
}
function post(request: APIRequestContext, actor: Identity, path: string, data: unknown) {
  return request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: actor.accountId }, data });
}
async function actAs(page: Page, actor: Identity, family: Family) {
  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption(actor.accountId);
  const selector = page.getByLabel('Current Family', { exact: true });
  if (await selector.count()) await selector.selectOption(family.familyGroupId);
  await page.getByRole('navigation', { name: 'Main navigation' }).getByRole('link', { name: 'Safety', exact: true }).click();
  await expect(page.locator('main h1')).toHaveText('Safety');
  await expect(page.locator('app-check-ins')).not.toContainText('Loading Check-Ins');
}
async function denied(request: APIRequestContext, actor: Identity, path: string) {
  const response = await request.get(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: actor.accountId } });
  expect(response.status()).toBe(404);
  expect(await response.text()).toBe('');
  if (/sharing-sessions|safety-circles|trusted-contacts/.test(path)) {
    expect(response.headers()['cache-control']).toBe('no-store');
  }
}
async function accessible(page: Page, name: string, width = 390) {
  await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('connected-safety-care'), `${name}-${width}.png`), fullPage: true });
}
async function createCircle(page: Page, request: APIRequestContext, owner: Identity, family: Family, name: string) {
  const circles = page.locator('app-safety-circles');
  await circles.getByRole('button', { name: 'Create a Safety Circle', exact: true }).click();
  await circles.getByLabel('Safety Circle name', { exact: true }).fill(name);
  await circles.getByRole('button', { name: 'Create circle', exact: true }).click();
  const row = circles.getByRole('article', { name, exact: true });
  await row.getByLabel(`Add eligible Family member to ${name}`).selectOption({ label: 'Development Audience Member' });
  await expect(row.getByRole('button', { name: `Remove Development Audience Member from ${name}` })).toBeVisible();
  const circle = (await apiJson<(Circle & { name: string })[]>(request, `/api/family-groups/${family.familyGroupId}/safety-circles`, owner.accountId)).body.find(c => c.name === name)!;
  return { circle, row };
}
async function usePreset(page: Page, selector: string, id: string) {
  const feature = page.locator(selector);
  await feature.getByRole('button', { name: 'Use a Safety Circle (optional)', exact: true }).click();
  await feature.getByLabel('Safety Circle preset', { exact: true }).selectOption(id);
  await expect(feature.getByRole('checkbox', { name: 'Development Audience Member', exact: true })).toBeChecked();
  await expect(feature).toContainText('Review and adjust the recipients below before confirming');
}

test('Safety preset → explicit own Check-In → recipient, admin denial and immutable grants across Family switching', async ({ page, request }) => {
  const w = await world(request);
  await actAs(page, w.author, w.first);
  await expect(page.locator('main h1')).toBeFocused();
  const { circle, row } = await createCircle(page, request, w.author, w.first, 'Status recipients');
  const check = page.locator('app-check-ins');
  await page.setViewportSize({ width: 390, height: 844 });
  await page.getByRole('link', { name: 'Open Check-Ins', exact: true }).focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/safety#check-ins$/);
  await expect(page.getByLabel('Current Family', { exact: true })).toHaveValue(w.first.familyGroupId);
  await expect(check.getByRole('button', { name: 'Create a Check-In', exact: true })).toBeInViewport();
  await check.getByRole('button', { name: 'Create a Check-In', exact: true }).click();
  await check.getByLabel('Status', { exact: true }).selectOption('Delayed');
  await check.getByLabel('Note (optional)', { exact: true }).fill('Synthetic delay, no automatic escalation.');
  for (const choice of await check.getByRole('checkbox').all()) await expect(choice).not.toBeChecked();
  await usePreset(page, 'app-check-ins', circle.safetyCircleId);
  await expect(check.getByRole('checkbox', { name: 'Development Adult', exact: true })).not.toBeChecked();
  await expect(check).toContainText('Status, note and recipients are fixed after creation');
  await accessible(page, 'check-in-review');
  await check.getByRole('button', { name: 'Create Check-In', exact: true }).focus();
  await page.keyboard.press('Enter');
  await expect(check).toContainText('Check-In created');
  const entry = (await apiJson<CheckIn[]>(request, '/api/check-ins/mine', w.author.accountId)).body[0];
  expect(entry.authorProfileId).toBe(w.authorProfile.profileId);
  expect(entry.recipientProfileIds).toEqual([w.recipientProfile.profileId]);
  // Add the administrator to the preset AFTER confirmation: circle membership supplies no grant.
  await row.getByLabel('Add eligible Family member to Status recipients').selectOption({ label: 'Development Adult' });
  await row.getByRole('button', { name: 'Remove Development Audience Member from Status recipients' }).click();
  await expect(row.getByRole('button', { name: 'Remove Development Audience Member from Status recipients' })).toHaveCount(0);
  expect((await apiJson<CheckIn>(request, `/api/check-ins/${entry.checkInId}`, w.author.accountId)).body.recipientProfileIds).toEqual(entry.recipientProfileIds);
  await denied(request, w.admin, `/api/family-groups/${w.first.familyGroupId}/safety-circles/${circle.safetyCircleId}`);
  await denied(request, w.admin, `/api/check-ins/${entry.checkInId}?selectedFamily=${w.first.familyGroupId}`);
  await actAs(page, w.recipient, w.first);
  await expect(check.getByRole('region', { name: 'Shared with me', exact: true })).toContainText('Synthetic delay');
  const recipientView = (await apiJson<CheckIn>(request, `/api/check-ins/${entry.checkInId}`, w.recipient.accountId)).body;
  expect(recipientView.recipientProfileIds).toBeNull();
  await actAs(page, w.admin, w.first);
  await expect(check).not.toContainText('Synthetic delay');
  await page.getByLabel('Current Family', { exact: true }).selectOption(w.second.familyGroupId);
  await expect(check).not.toContainText('Synthetic delay');
  await actAs(page, w.author, w.first);
  await check.getByRole('button', { name: 'Create a Check-In', exact: true }).click();
  await check.getByRole('button', { name: 'Use a Safety Circle (optional)', exact: true }).click();
  await check.getByLabel('Safety Circle preset', { exact: true }).selectOption(circle.safetyCircleId);
  await expect(check.getByRole('checkbox', { name: 'Development Adult', exact: true })).toBeChecked();
  await expect(check.getByRole('checkbox', { name: 'Development Audience Member', exact: true })).not.toBeChecked();
  await page.getByLabel('Current Family', { exact: true }).selectOption(w.second.familyGroupId);
  await expect(check.locator('form')).toHaveCount(0);
  await expect(page.locator('app-safety-circles')).not.toContainText('Status recipients');
  // Authorized history is intentionally independent of the selected creation Family.
  await expect(check).toContainText('Synthetic delay');
  await check.getByRole('button', { name: 'Create a Check-In', exact: true }).click();
  await check.getByRole('button', { name: 'Use a Safety Circle (optional)', exact: true }).click();
  await expect(check.getByLabel('Safety Circle preset')).not.toContainText('Status recipients');
  for (const choice of await check.getByRole('checkbox').all()) await expect(choice).not.toBeChecked();
});

test('private manual contacts → Safety discovery → deliberate Concern/Incident with independent audiences', async ({ page, request }) => {
  const w = await world(request);
  await actAs(page, w.author, w.first);
  const contacts = page.locator('app-trusted-contacts');
  await expect(contacts).toContainText('Listing someone here does not give them access');
  await contacts.getByRole('button', { name: 'Add a Trusted Contact', exact: true }).click();
  await contacts.getByLabel('Display Name', { exact: true }).fill('Development Audience Member');
  await contacts.getByLabel('Email Address', { exact: true }).fill('manual-only@example.invalid');
  await contacts.getByRole('button', { name: 'Create Trusted Contact', exact: true }).click();
  await expect(contacts).toContainText('Trusted Contact added');
  const contact = (await apiJson<{ trustedContactId: string }[]>(request, '/api/trusted-contacts', w.author.accountId)).body[0];
  await page.getByLabel('Current Family', { exact: true }).selectOption(w.second.familyGroupId);
  await expect(contacts).toContainText('manual-only@example.invalid');
  await contacts.getByRole('button', { name: 'Edit contact', exact: true }).click();
  await contacts.getByLabel('Relationship / Label (optional)').fill('Private manual entry');
  await contacts.getByRole('button', { name: 'Save Trusted Contact', exact: true }).click();
  await expect(contacts).toContainText('Trusted Contact updated');
  await page.getByLabel('Current Family', { exact: true }).selectOption(w.first.familyGroupId);
  const check = page.locator('app-check-ins');
  await check.getByRole('button', { name: 'Create a Check-In', exact: true }).click();
  for (const choice of await check.getByRole('checkbox').all()) await expect(choice).not.toBeChecked();
  await expect(check).not.toContainText('manual-only@example.invalid');
  await check.getByRole('button', { name: 'Cancel', exact: true }).click();
  const sharing = page.locator('app-sharing-sessions');
  await sharing.getByRole('button', { name: 'Start location sharing', exact: true }).click();
  for (const choice of await sharing.getByRole('checkbox').all()) await expect(choice).not.toBeChecked();
  await expect(sharing).not.toContainText('manual-only@example.invalid');
  await sharing.getByRole('button', { name: 'Cancel sharing draft', exact: true }).click();
  // Status and contact activity have not created coordination records.
  expect((await apiJson<unknown[]>(request, '/api/concerns', w.author.accountId)).body).toEqual([]);
  expect((await apiJson<unknown[]>(request, '/api/incidents', w.author.accountId)).body).toEqual([]);
  await page.getByRole('link', { name: 'View all Concerns', exact: true }).focus();
  await page.keyboard.press('Enter');
  await expect(page.locator('main h1')).toBeFocused();
  await page.getByRole('button', { name: 'Create a Concern', exact: true }).click();
  await page.getByLabel('Who is this Concern about?').selectOption(w.recipientProfile.profileId);
  await page.getByLabel('Reason or context').fill('Manual uncertainty review only.');
  const people = page.getByRole('group', { name: 'Who should have access?' });
  await expect(people.getByRole('checkbox', { name: /Development Audience Member/ })).not.toBeChecked();
  await people.getByRole('checkbox', { name: /Development Adult/ }).check();
  const creation = page.waitForResponse(r => r.url().endsWith('/concerns') && r.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Concern', exact: true }).click();
  const concern = await (await creation).json() as { concernId: string };
  await expect(page).toHaveURL(new RegExp(`/concerns/${concern.concernId}$`));
  const before = (await apiJson<unknown>(request, `/api/concerns/${concern.concernId}`, w.author.accountId)).body;
  await expect(page.getByText('Nothing here is copied or shared.', { exact: false })).toBeVisible();
  await page.getByRole('button', { name: 'Start an Incident…', exact: true }).click();
  await expect(page.getByLabel('Who is this Incident about?')).toHaveValue(w.recipientProfile.profileId);
  await expect(page.getByLabel('Why are you starting this Incident?')).toHaveValue('');
  for (const choice of await page.getByRole('group', { name: 'People with access' }).getByRole('checkbox').all()) await expect(choice).not.toBeChecked();
  await page.getByLabel('Why are you starting this Incident?').fill('Deliberate separate coordination.');
  await page.getByRole('radio', { name: /^Assumption/ }).check();
  await page.getByRole('checkbox', { name: 'Source is unknown' }).check();
  await accessible(page, 'separate-incident');
  const incidentCreation = page.waitForResponse(r => r.url().endsWith('/incidents') && r.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Incident', exact: true }).click();
  const incident = await (await incidentCreation).json() as { incidentId: string; participants: Profile[] };
  expect(incident.participants.map(p => p.profileId)).toEqual([w.authorProfile.profileId]);
  expect((await apiJson<unknown>(request, `/api/concerns/${concern.concernId}`, w.author.accountId)).body).toEqual(before);
  await denied(request, w.recipient, `/api/concerns/${concern.concernId}`);
  await denied(request, w.recipient, `/api/incidents/${incident.incidentId}`);
  await denied(request, w.admin, `/api/incidents/${incident.incidentId}`);
  await denied(request, w.admin, `/api/trusted-contacts/${contact.trustedContactId}`);
  await actAs(page, w.admin, w.second);
  await expect(contacts).not.toContainText('manual-only@example.invalid');
  await page.evaluate(id => {
    history.pushState({}, '', `/incidents/${id}`);
    dispatchEvent(new PopStateEvent('popstate', { state: {} }));
  }, incident.incidentId);
  await expect(page.getByRole('heading', { name: 'This Incident is unavailable' })).toBeVisible();
  await expect(page.getByLabel('Current Family', { exact: true })).toHaveValue(w.second.familyGroupId);
  await expect(page.locator('main')).not.toContainText('Deliberate separate coordination.');
  await actAs(page, w.recipient, w.first);
  await page.goto(`/concerns/${concern.concernId}`);
  await expect(page.getByRole('heading', { name: 'This Concern is unavailable' })).toBeVisible();
  await expect(page.locator('main')).not.toContainText('Manual uncertainty review only.');
  await actAs(page, w.author, w.first);
  await page.getByRole('link', { name: 'View all Concerns', exact: true }).click();
  await page.getByRole('link', { name: /Open Concern about Development Audience Member, Open/ }).click();
  await page.getByRole('button', { name: 'Resolve this Concern…', exact: true }).click();
  await page.getByLabel('Resolution note').fill('Review ended; no safety verdict.');
  await page.getByRole('button', { name: 'Resolve Concern', exact: true }).click();
  await expect(page.getByText('Review ended; no safety verdict.', { exact: true })).toBeVisible();
  expect((await apiJson<{ state: string }>(request, `/api/incidents/${incident.incidentId}`, w.author.accountId)).body.state).toBe('Open');
  await page.getByRole('link', { name: 'Back to Concerns', exact: true }).click();
  await page.getByRole('link', { name: 'Back to Safety', exact: true }).click();
  await expect(page.locator('main h1')).toBeFocused();
  await contacts.getByRole('button', { name: 'Remove contact', exact: true }).click();
  await contacts.getByRole('button', { name: 'Confirm removal', exact: true }).click();
  await expect(contacts).toContainText('permanently removed');
  await denied(request, w.author, `/api/trusted-contacts/${contact.trustedContactId}`);
  await page.getByRole('link', { name: 'View all Safety Incidents', exact: true }).click();
  await page.getByRole('link', { name: 'Back to Safety', exact: true }).click();
  await accessible(page, 'safety-discovery', 1440);
  await page.getByRole('navigation', { name: 'Main navigation' }).getByRole('link', { name: 'Home', exact: true }).click();
  await expect(page).toHaveURL(/\/home$/);
  await expect(page.locator('main h1')).toContainText('Development Recipient');
});

test('preset → bounded sharing → permission recovery, latest/freshness, Home, stop and real server expiry', async ({ page, request }) => {
  test.setTimeout(180_000);
  const w = await world(request);
  // Browser callbacks only are mocked; all grants, points and end checks use the real API/SQL.
  await page.addInitScript(() => {
    let success: PositionCallback | null = null;
    let failure: PositionErrorCallback | null = null;
    Object.defineProperty(navigator, 'geolocation', { configurable: true, value: {
      watchPosition: (ok: PositionCallback, error: PositionErrorCallback) => { success = ok; failure = error; return 1; },
      clearWatch: () => { success = null; failure = null; },
    } });
    Object.assign(window, { careLocation: {
      point: () => success?.({ coords: { latitude: 42, longitude: -72, accuracy: 12 }, timestamp: Date.now() } as GeolocationPosition),
      fail: (code: number) => failure?.({ code } as GeolocationPositionError),
    } });
  });
  const fail = (code: number) => page.evaluate(value => (window as unknown as { careLocation: { fail(code: number): void } }).careLocation.fail(value), code);
  await actAs(page, w.author, w.first);
  const { circle } = await createCircle(page, request, w.author, w.first, 'Location recipients');
  const sharing = page.locator('app-sharing-sessions');
  await sharing.getByRole('button', { name: 'Start location sharing', exact: true }).click();
  await usePreset(page, 'app-sharing-sessions', circle.safetyCircleId);
  // Deliberately choose a custom bounded end, rather than the one-hour default.
  const end = new Date(Date.now() + 10 * 60000);
  const localEnd = new Date(end.getTime() - end.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  await sharing.getByLabel('Sharing end time').fill(localEnd);
  await expect(sharing).toContainText('Audience stays fixed');
  await expect(sharing).toContainText('background updates are not guaranteed');
  await accessible(page, 'sharing-review');
  const creation = page.waitForResponse(r => r.url().endsWith('/api/sharing-sessions') && r.request().method() === 'POST');
  await sharing.getByRole('button', { name: 'Start sharing and request location', exact: true }).focus();
  await page.keyboard.press('Enter');
  const session = await (await creation).json() as Session;
  expect(Date.parse(session.endsAtUtc)).toBe(Date.parse(localEnd));
  expect(session.recipients!.map(p => p.profileId)).toEqual([w.recipientProfile.profileId]);
  await fail(1);
  await expect(sharing).toContainText('Location permission denied');
  await expect(sharing).toContainText('Location not yet available');
  await sharing.getByRole('button', { name: 'Enable location updates', exact: true }).click();
  await fail(2);
  await expect(sharing).toContainText('Location unavailable');
  await fail(3);
  await expect(sharing).toContainText('Location update temporarily failed');
  await page.evaluate(() => (window as unknown as { careLocation: { point(): void } }).careLocation.point());
  await expect(sharing).toContainText('Latitude 42.0000');
  await expect(sharing).toContainText('Last reported point; location may have changed');
  await denied(request, w.admin, `/api/sharing-sessions/${session.sharingSessionId}?selectedFamily=${w.first.familyGroupId}`);
  await page.getByRole('navigation', { name: 'Main navigation' }).getByRole('link', { name: 'Home', exact: true }).click();
  await expect(sharing).toContainText('Sharing your current / last-reported location until');
  await sharing.getByRole('link', { name: 'Manage sharing in Safety', exact: true }).click();
  await expect(sharing.getByRole('button', { name: 'Enable location updates', exact: true })).toBeVisible();
  await actAs(page, w.recipient, w.first);
  await expect(sharing.getByRole('region', { name: 'Location shared with me' })).toContainText('Latitude 42.0000');
  expect((await apiJson<Session>(request, `/api/sharing-sessions/${session.sharingSessionId}`, w.recipient.accountId)).body.recipients).toBeNull();
  // Accelerate only browser wall time to verify stale presentation; server session stays real and active.
  await page.clock.setSystemTime(new Date(Date.now() + 130000));
  await expect(sharing).toContainText('Location has not updated recently');
  await page.clock.setSystemTime(new Date());
  await page.route('**/api/sharing-sessions/received', route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await sharing.getByRole('button', { name: 'Refresh sharing', exact: true }).click();
  await expect(sharing).toContainText('Sharing could not be loaded');
  await expect(sharing).not.toContainText('Latitude 42');
  await sharing.getByRole('button', { name: 'Refresh sharing', exact: true }).click();
  await expect(sharing).toContainText('Latitude 42.0000');
  await actAs(page, w.admin, w.first);
  await expect(sharing).not.toContainText('Latitude 42');
  await actAs(page, w.author, w.first);
  await sharing.getByRole('button', { name: 'Stop sharing', exact: true }).click();
  await expect(sharing).toContainText('Sharing stopped');
  await denied(request, w.recipient, `/api/sharing-sessions/${session.sharingSessionId}`);
  await actAs(page, w.recipient, w.first);
  await expect(sharing).not.toContainText('Latitude 42');
  // Short-duration API fixture uses the same supported explicit-end contract. No intercepted expiry.
  const short = await post(request, w.author, '/api/sharing-sessions', { familyGroupId: w.first.familyGroupId,
    recipientProfileIds: [w.recipientProfile.profileId], endsAtUtc: new Date(Date.now() + 15000).toISOString() });
  expect(short.status()).toBe(200);
  const expiring = await short.json() as Session;
  const point = await request.put(`${apiBaseUrl}/api/sharing-sessions/${expiring.sharingSessionId}/location`, {
    headers: { [developmentAccountHeader]: w.author.accountId }, data: { latitude: 44, longitude: -72, accuracy: 12,
      capturedAtUtc: new Date().toISOString(), version: expiring.version },
  });
  expect(point.status()).toBe(200);
  await sharing.getByRole('button', { name: 'Refresh sharing', exact: true }).click();
  await expect(sharing).toContainText('Latitude 44.0000');
  await expect(sharing).not.toContainText('Latitude 44', { timeout: 25000 });
  await denied(request, w.recipient, `/api/sharing-sessions/${expiring.sharingSessionId}`);
  await expect(sharing).toContainText('No active locations are shared with you');
  // Location updates did not create an additional Concern or Incident.
  expect((await apiJson<unknown[]>(request, '/api/concerns', w.recipient.accountId)).body).toEqual([]);
  expect((await apiJson<unknown[]>(request, '/api/incidents', w.recipient.accountId)).body).toEqual([]);
});
