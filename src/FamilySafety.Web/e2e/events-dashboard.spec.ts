import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { resolve } from 'node:path';
import {
  apiBaseUrl,
  apiJson,
  captureDirectory,
  developmentAccountHeader,
  expectHealthyApi,
  recordScenario,
  useIsolatedScenario,
} from './fixtures/invitation-scenario';

// Event discovery through Activities (REQ-UX-018), legacy redirects, Create Event and item routes.
// Family 1 ("Development Family"): A (administrator), B (joins by invitation), C. Family 2: A and B; C is not a member.
useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }
interface FamilyMember { profileId: string; displayName: string }

const captures = captureDirectory('events-dashboard');
const heading = (page: Page) => page.locator('main h1');
const mainNavigation = (page: Page) => page.getByRole('navigation', { name: 'Main navigation' });
const rows = (page: Page, type: 'Event' | 'Trip') => page.locator('.activity-row')
  .filter({ has: page.locator('.activity-badge', { hasText: new RegExp(`^${type}$`) }) });
const titles = (page: Page, type: 'Event' | 'Trip') => rows(page, type).locator('.activity-copy > strong');
const pending = (page: Page) => page.getByRole('region', { name: 'Pending RSVPs' });

function futureDate(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}

async function send(request: APIRequestContext, path: string, accountId: string, data: unknown): Promise<{ status: number; body: any }> {
  const response = await request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: accountId }, data });
  return { status: response.status(), body: await response.json().catch(() => null) };
}

interface World {
  a: DevelopmentIdentity; c: DevelopmentIdentity; family1: FamilyGroup; family2: FamilyGroup;
  aName: string; ids: Record<'lantern' | 'bay' | 'privateEvent' | 'second' | 'lanternTrip' | 'secondTrip', string>;
}
let world: World | null = null;

/** Built once per file through the existing APIs: the audiences decide who may see each item, never the dashboard. */
async function setUpWorld(request: APIRequestContext): Promise<World> {
  if (world) return world;
  await recordScenario(test.info());
  await expectHealthyApi(request);
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json() as { identities: DevelopmentIdentity[] }).identities;
  const find = (prefix: string) => identities.find(identity => identity.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const aFamilies = (await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', a.accountId)).body;
  const family1 = aFamilies.find(group => group.name === 'Development Family')!;
  const family2 = aFamilies.find(group => group.name === 'Development Second Family')!;

  // B joins Family 1 through the existing invitation API, so Family 1 has three adults.
  expect((await send(request, '/api/membership-invitations', a.accountId,
    { familyGroupId: family1.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status).toBe(202);
  const outbox = await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', a.accountId);
  const token = decodeURIComponent(outbox.body.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await send(request, '/api/membership-invitations/accept', b.accountId, { token })).status).toBe(200);

  const members = (await apiJson<FamilyMember[]>(request, `/api/family-groups/${family1.familyGroupId}/members`, a.accountId)).body;
  const profileOf = (account: DevelopmentIdentity) => apiJson<FamilyMember>(request, '/api/profile', account.accountId);
  const [aProfile, bProfile, cProfile] = await Promise.all([a, b, c].map(async account => (await profileOf(account)).body.profileId));
  const aName = members.find(member => member.profileId === aProfile)!.displayName;

  const event = async (account: DevelopmentIdentity, family: FamilyGroup, name: string, days: number, audience: string[]) => {
    const created = await send(request, `/api/family-groups/${family.familyGroupId}/events`, account.accountId, {
      name, description: `${name} description`, date: futureDate(days), time: '18:30', location: `${name} Hall`,
      audienceProfileIds: audience, editorProfileIds: [],
    });
    expect(created.status, `${name} create`).toBe(201);
    return created.body.eventId as string;
  };
  const trip = async (family: FamilyGroup, title: string, days: number, audience: string[]) => {
    const created = await send(request, `/api/family-groups/${family.familyGroupId}/trips`, a.accountId, {
      title, primaryDestination: `${title} Coast`, startDate: futureDate(days), endDate: futureDate(days + 2),
      description: null, transportationNotes: null, accommodationNotes: null, expectedArrivalDate: null, expectedArrivalTime: null,
      expectedArrivalTimeZone: null, travelerProfileIds: [aProfile], contacts: [], audienceType: 'SelectedPeople', audienceProfileIds: audience,
    });
    expect(created.status, `${title} create`).toBe(201);
    return created.body.tripId as string;
  };
  world = {
    a, c, family1, family2, aName,
    ids: {
      lantern: await event(a, family1, 'Lantern Walk', 10, [cProfile]),
      bay: await event(b, family1, 'Bay Picnic', 12, [cProfile]),
      // Shared with B only: C is a Family 1 member outside its audience.
      privateEvent: await event(a, family1, 'Private Supper', 8, [bProfile]),
      second: await event(a, family2, 'Second Family Dinner', 9, [bProfile]),
      lanternTrip: await trip(family1, 'Lantern Trip', 14, [cProfile]),
      secondTrip: await trip(family2, 'Second Family Trip', 15, [bProfile]),
    },
  };
  return world;
}

async function useIdentity(page: Page, identity: DevelopmentIdentity, path: string): Promise<void> {
  await page.goto('/home');
  const selector = page.getByLabel('Active test identity');
  if (await selector.inputValue() !== identity.accountId) await selector.selectOption({ label: identity.label });
  await expect(selector).toHaveValue(identity.accountId);
  await page.goto(path);
  await expect(selector).toHaveValue(identity.accountId);
}

async function expectActivitiesCurrent(page: Page, value: 'page' | 'true'): Promise<void> {
  const current = mainNavigation(page).locator('a[aria-current]');
  await expect(current).toHaveCount(1);
  await expect(current).toHaveText('Activities');
  await expect(current).toHaveAttribute('aria-current', value);
}

async function filterType(page: Page, type: 'All' | 'Events' | 'Trips'): Promise<void> {
  const button = page.getByRole('group', { name: 'Activity type' })
    .getByRole('button', { name: new RegExp(`^${type} \\d+$`) });
  await button.click();
  await expect(button).toHaveAttribute('aria-pressed', 'true');
}

async function expectNoOverflow(page: Page): Promise<void> {
  expect(await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth)).toBe(0);
}

test('Activities discovery, privacy, item links, date filters, RSVP navigation, create → item, and responsive layouts', async ({ page, request }) => {
  test.setTimeout(180_000);
  const w = await setUpWorld(request);
  const apiPaths: string[] = [];
  page.on('request', sent => { const url = new URL(sent.url()); if (url.pathname.startsWith('/api/')) apiPaths.push(url.pathname); });
  await page.setViewportSize({ width: 1440, height: 1000 });

  // Direct entry as C (single Family; not an administrator).
  await useIdentity(page, w.c, '/home');
  await page.waitForLoadState('networkidle');
  apiPaths.length = 0;
  await page.goto('/events');
  await expect(page).toHaveURL(/\/activities\?type=Event$/);
  await expect(heading(page)).toHaveText('Activities');
  await expect(page.locator('.page-title')).toContainText(w.family1.name);
  await expectActivitiesCurrent(page, 'page');
  await expect(page.getByRole('group', { name: 'Activity type' }).getByRole('button', { name: /^Events / })).toHaveAttribute('aria-pressed', 'true');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await expect(rows(page, 'Trip')).toHaveCount(0);
  await filterType(page, 'All');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await expect(titles(page, 'Trip')).toHaveText(['Lantern Trip']);
  // Privacy: an Event outside C's audience and items of a Family C does not belong to never appear.
  await expect(page.locator('main')).not.toContainText(/Private Supper|Second Family/);
  // Audience creation issues zero invitations; the protected own-Pending query is empty.
  await expect(pending(page)).toContainText('You have no pending event invitations in this family.');
  await expect(pending(page).locator('button, a, input, select, img')).toHaveCount(0);
  await expect(pending(page).locator('.activity-card-heading > span')).toHaveText('0');
  await expect(page.locator('main')).not.toContainText(/Going|Maybe|Respond/);
  await page.waitForLoadState('networkidle');
  expect(apiPaths.filter(path => /event-invitations/i.test(path)),
    'the dashboard uses the protected selected-Family own-Pending read').toEqual([`/api/family-groups/${w.family1.familyGroupId}/event-invitations/mine`]);
  // Type/search controls replace the former dashboard cards and Quick Links.
  await filterType(page, 'Trips');
  await expect(titles(page, 'Trip')).toHaveText(['Lantern Trip']);
  await expect(rows(page, 'Event')).toHaveCount(0);
  await filterType(page, 'Events');
  await page.getByRole('searchbox', { name: 'Search activities' }).fill('Lantern');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk']);
  await page.getByRole('searchbox', { name: 'Search activities' }).fill('');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await filterType(page, 'All');
  await page.screenshot({ path: resolve(captures, 'events-1440.png'), fullPage: true });
  for (const viewport of [{ width: 1024, height: 900 }, { width: 768, height: 1000 }, { width: 390, height: 844 }]) {
    await page.setViewportSize(viewport);
    await expectNoOverflow(page);
    await page.screenshot({ path: resolve(captures, `events-${viewport.width}.png`), fullPage: true });
  }
  await page.setViewportSize({ width: 1440, height: 1000 });

  // Activities → Event item → Back to Activities.
  await rows(page, 'Event').filter({ hasText: 'Lantern Walk' }).click();
  await expect(page).toHaveURL(new RegExp(`/events/${w.ids.lantern}$`));
  await expect(heading(page)).toHaveText('Events');
  await expect(heading(page)).toBeFocused();
  await expectActivitiesCurrent(page, 'true');
  await expect(page.getByRole('heading', { level: 2, name: 'Lantern Walk' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Edit event details', exact: true })).toHaveCount(0);
  await page.getByRole('link', { name: 'Back to Activities', exact: true }).click();
  await expect(page).toHaveURL(/\/activities$/);

  // Activities → authorized Trip item, still a distinct domain route.
  await rows(page, 'Trip').filter({ hasText: 'Lantern Trip' }).click();
  await expect(page).toHaveURL(new RegExp(`/trips/${w.ids.lanternTrip}$`));
  await expect(heading(page)).toHaveText('Trips');
  await expectActivitiesCurrent(page, 'true');
  await page.getByRole('link', { name: 'Back to Activities', exact: true }).click();
  await expect(page).toHaveURL(/\/activities$/);

  // Main navigation → profile, then back; type/date controls retain all-Event listing coverage.
  await mainNavigation(page).getByRole('link', { name: 'Me / Settings', exact: true }).click();
  await expect(page).toHaveURL(/\/me$/);
  await expect(heading(page)).toHaveText('Your profile');
  await page.goBack();
  await expect(page).toHaveURL(/\/activities$/);
  await filterType(page, 'Events');
  await page.getByLabel('Activity dates').selectOption('all');
  await expect(page.getByRole('heading', { name: 'All Activities', exact: true })).toBeVisible();
  await expectActivitiesCurrent(page, 'page');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await expect(page.locator('main')).not.toContainText('Private Supper');
  await expect(page.getByRole('link', { name: 'Edit event details', exact: true })).toHaveCount(0);

  // Direct entry: /events/all and /events/new are pages, never item reads.
  apiPaths.length = 0;
  await page.goto('/events/all');
  await expect(page).toHaveURL(/\/activities\?type=Event&period=all$/);
  await expect(heading(page)).toHaveText('Activities');
  await expect(page.getByLabel('Activity dates')).toHaveValue('all');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await page.reload();
  await expect(page).toHaveURL(/\/activities\?type=Event&period=all$/);
  await expect(page.getByLabel('Activity dates')).toHaveValue('all');
  await expect(titles(page, 'Event')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await page.screenshot({ path: resolve(captures, 'events-all-1440.png'), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  await expectNoOverflow(page);
  await page.screenshot({ path: resolve(captures, 'events-all-390.png'), fullPage: true });
  await page.goto('/events/new');
  await expect(heading(page)).toHaveText('Create Activity');
  await expectNoOverflow(page);
  await page.screenshot({ path: resolve(captures, 'events-new-390.png'), fullPage: true });
  await page.setViewportSize({ width: 1440, height: 1000 });
  await expect(page.locator('.page-title')).toContainText('does not send an invitation or start location sharing');
  await expectActivitiesCurrent(page, 'true');
  await page.screenshot({ path: resolve(captures, 'events-new-1440.png'), fullPage: true });
  await page.waitForLoadState('networkidle');
  expect(apiPaths.filter(path => /^\/api\/events\/(new|all)$/.test(path))).toEqual([]);

  // Activities → type chooser → Event create → item URL with a transient success message.
  await page.goto('/activities');
  await page.getByRole('main').getByRole('link', { name: 'Create Activity', exact: true }).click();
  await expect(page).toHaveURL(/\/activities\/new$/);
  await expect(heading(page)).toBeFocused();
  await page.getByRole('navigation', { name: 'Choose activity type' }).getByRole('link', { name: /^Event / }).click();
  await expect(page).toHaveURL(/\/events\/new$/);
  await expect(heading(page)).toBeFocused();
  await page.locator('#event-name').fill('Harvest Supper');
  await page.locator('#event-location').fill('Barn');
  await page.locator('#event-description').fill('Bring a dish.');
  await page.locator('#event-date').fill(futureDate(18));
  await page.locator('#event-time').fill('17:00');
  await page.locator('.event-access-list').getByLabel(w.aName, { exact: true }).selectOption('view');
  await page.locator('.events-create-form').getByRole('button', { name: 'Create event' }).click();
  await expect(page).toHaveURL(/\/events\/[0-9a-f-]{36}$/);
  await expect(page.getByRole('heading', { level: 2, name: 'Harvest Supper' })).toBeVisible();
  await expect(page.getByRole('status').filter({ hasText: 'Event created and shared with the selected family members.' })).toBeVisible();
  await page.reload();
  await expect(page.getByRole('heading', { level: 2, name: 'Harvest Supper' })).toBeVisible();
  await expect(page.getByText('Event created and shared')).toHaveCount(0);

  // Only an explicit invitation adds an RSVP link; audience sharing alone did not.
  const createdUrl = page.url();
  const participation = page.locator('app-event-participation');
  await expect(participation).toContainText('No invitations have been issued');
  await participation.getByRole('button', { name: `Invite ${w.aName}`, exact: true }).click();
  await expect(participation).toContainText('Invitation issued');
  await useIdentity(page, w.a, '/home');
  await page.getByLabel('Current Family').selectOption({ label: w.family1.name });
  await mainNavigation(page).getByRole('link', { name: 'Activities', exact: true }).click();
  await expect(pending(page).getByRole('link', { name: 'Harvest Supper', exact: true }))
    .toHaveAttribute('href', new URL(createdUrl).pathname);
  await pending(page).getByRole('link', { name: 'Harvest Supper', exact: true }).click();
  await expect(page).toHaveURL(createdUrl);
  await expect(participation.getByRole('heading', { name: 'Your invitation' })).toBeVisible();
  await participation.getByRole('radio', { name: 'Going', exact: true }).check();
  await participation.getByRole('button', { name: 'Save RSVP', exact: true }).click();
  await expect(participation).toContainText('Current RSVP: Going');
  await page.getByRole('link', { name: 'Back to Activities', exact: true }).click();
  await expect(pending(page)).toContainText('You have no pending event invitations in this family.');
  await expect(pending(page).getByRole('link', { name: 'Harvest Supper', exact: true })).toHaveCount(0);
});

test('Family switching clears and reloads Activities, and administrator status adds no visibility', async ({ page, request }) => {
  test.setTimeout(120_000);
  const w = await setUpWorld(request);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await useIdentity(page, w.a, '/home');
  await page.getByLabel('Current Family').selectOption({ label: w.family1.name });
  await mainNavigation(page).getByRole('link', { name: 'Activities', exact: true }).click();
  await expect(page).toHaveURL(/\/activities$/);
  // A administers Family 1 but is outside the audience of B's Bay Picnic: it never appears.
  // The first test's created "Harvest Supper" (C → A) is also visible to A when both tests run; it is set aside here.
  await expect(titles(page, 'Event').first()).toHaveText('Private Supper');
  expect((await titles(page, 'Event').allTextContents()).map(name => name.trim()).filter(name => name !== 'Harvest Supper'))
    .toEqual(['Private Supper', 'Lantern Walk']);
  await expect(titles(page, 'Trip')).toHaveText(['Lantern Trip']);
  await expect(page.locator('main')).not.toContainText(/Bay Picnic|Second Family/);

  await page.getByLabel('Current Family').selectOption({ label: w.family2.name });
  await expect(page).toHaveURL(/\/activities$/);
  await expect(page.locator('.page-title')).toContainText(w.family2.name);
  await expect(titles(page, 'Event')).toHaveText(['Second Family Dinner']);
  await expect(titles(page, 'Trip')).toHaveText(['Second Family Trip']);
  await expect(page.locator('main')).not.toContainText(/Lantern|Private Supper|Bay Picnic/);
});
