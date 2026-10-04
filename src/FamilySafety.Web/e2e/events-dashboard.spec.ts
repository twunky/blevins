import { expect, test, type APIRequestContext, type Locator, type Page } from '@playwright/test';
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

// WI-009-008: the Events dashboard (/events), Create event (/events/new), All events (/events/all), and the item page.
// Family 1 ("Development Family"): A (administrator), B (joins by invitation), C. Family 2: A and B; C is not a member.
useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }
interface FamilyMember { profileId: string; displayName: string }

const captures = captureDirectory('events-dashboard');
const heading = (page: Page) => page.locator('main h1');
const mainNavigation = (page: Page) => page.getByRole('navigation', { name: 'Main navigation' });
const card = (page: Page, name: 'upcoming' | 'trips' | 'links' | 'rsvps') => page.locator(`.events-dash--${name}`);
const rows = (locator: Locator) => locator.locator('li a.item-link');

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

async function expectEventsCurrent(page: Page, value: 'page' | 'true'): Promise<void> {
  const current = mainNavigation(page).locator('a[aria-current]');
  await expect(current).toHaveCount(1);
  await expect(current).toHaveText('Events');
  await expect(current).toHaveAttribute('aria-current', value);
}

async function expectNoOverflow(page: Page): Promise<void> {
  expect(await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth)).toBe(0);
}

test('direct entry, privacy, item links, Quick Links, All events, create → item, and responsive layouts', async ({ page, request }) => {
  test.setTimeout(180_000);
  const w = await setUpWorld(request);
  const apiPaths: string[] = [];
  page.on('request', sent => { const url = new URL(sent.url()); if (url.pathname.startsWith('/api/')) apiPaths.push(url.pathname); });
  await page.setViewportSize({ width: 1440, height: 1000 });

  // Direct entry as C (single Family; not an administrator).
  await useIdentity(page, w.c, '/events');
  await expect(heading(page)).toHaveText('Events');
  await expect(page.locator('.page-title')).toContainText(w.family1.name);
  await expectEventsCurrent(page, 'page');
  await expect(page.locator('.events-grid h2')).toHaveText(['Upcoming Events', 'Trips', 'Quick Links', 'RSVPs Needed']);
  await expect(rows(card(page, 'upcoming'))).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await expect(rows(card(page, 'trips'))).toHaveText(['Lantern Trip']);
  // Privacy: an Event outside C's audience and items of a Family C does not belong to never appear.
  await expect(page.locator('main')).not.toContainText(/Private Supper|Second Family/);
  // RSVPs Needed: text only; no invitation or RSVP implied, requested, or derived from audience.
  await expect(card(page, 'rsvps').locator('p')).toHaveText('Event invitations that need your response will appear here.');
  await expect(card(page, 'rsvps').locator('button, a, input, select, img, li')).toHaveCount(0);
  await expect(card(page, 'rsvps')).not.toContainText(/\d/);
  await expect(page.locator('main')).not.toContainText(/Going|Maybe|Respond/);
  await page.waitForLoadState('networkidle');
  expect(apiPaths.filter(path => /rsvp|invitation(?!s\/accept)/i.test(path) && !path.startsWith('/api/development/')),
    'the dashboard requests no RSVP or invitation data').toEqual([]);
  // Quick Links: six real destinations.
  const quick = card(page, 'links').getByRole('link');
  await expect(quick).toHaveCount(6);
  expect(await quick.evaluateAll(links => links.map(link => link.getAttribute('href'))))
    .toEqual(['/events/new', '/events/all', '/trips', '/plans', '/family', '/me']);
  await page.screenshot({ path: resolve(captures, 'events-1440.png'), fullPage: true });
  for (const viewport of [{ width: 1024, height: 900 }, { width: 768, height: 1000 }, { width: 390, height: 844 }]) {
    await page.setViewportSize(viewport);
    await expectNoOverflow(page);
    await page.screenshot({ path: resolve(captures, `events-${viewport.width}.png`), fullPage: true });
  }
  await page.setViewportSize({ width: 1440, height: 1000 });

  // Dashboard → Event item → Back to Events.
  await rows(card(page, 'upcoming')).filter({ hasText: 'Lantern Walk' }).click();
  await expect(page).toHaveURL(new RegExp(`/events/${w.ids.lantern}$`));
  await expect(heading(page)).toHaveText('Events');
  await expect(heading(page)).toBeFocused();
  await expectEventsCurrent(page, 'true');
  await expect(page.getByRole('heading', { level: 2, name: 'Lantern Walk' })).toBeVisible();
  await page.getByRole('link', { name: 'Back to Events' }).click();
  await expect(page).toHaveURL(/\/events$/);

  // Dashboard → Trip item, in the Plans & Trips area.
  await rows(card(page, 'trips')).filter({ hasText: 'Lantern Trip' }).click();
  await expect(page).toHaveURL(new RegExp(`/trips/${w.ids.lanternTrip}$`));
  await expect(heading(page)).toHaveText('Trips');
  await expect(mainNavigation(page).locator('a[aria-current]')).toHaveText('Plans & Trips');
  await page.goBack();
  await expect(page).toHaveURL(/\/events$/);

  // Quick Link → Your profile, then back; View all → All events.
  await card(page, 'links').getByRole('link', { name: /Your profile/ }).click();
  await expect(page).toHaveURL(/\/me$/);
  await expect(heading(page)).toHaveText('Your profile');
  await page.goBack();
  await card(page, 'upcoming').getByRole('link', { name: 'View all Events' }).click();
  await expect(page).toHaveURL(/\/events\/all$/);
  await expect(heading(page)).toHaveText('All events');
  await expect(heading(page)).toBeFocused();
  await expectEventsCurrent(page, 'true');
  await expect(page.locator('.events-overview li a.item-link')).toHaveText(['Lantern Walk', 'Bay Picnic']);
  await expect(page.locator('main')).not.toContainText('Private Supper');
  await expect(page.getByRole('button', { name: 'Edit event details' })).toHaveCount(0);

  // Direct entry: /events/all and /events/new are pages, never item reads.
  apiPaths.length = 0;
  await page.goto('/events/all');
  await expect(heading(page)).toHaveText('All events');
  await page.screenshot({ path: resolve(captures, 'events-all-1440.png'), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  await expectNoOverflow(page);
  await page.screenshot({ path: resolve(captures, 'events-all-390.png'), fullPage: true });
  await page.goto('/events/new');
  await expect(heading(page)).toHaveText('Create event');
  await expectNoOverflow(page);
  await page.screenshot({ path: resolve(captures, 'events-new-390.png'), fullPage: true });
  await page.setViewportSize({ width: 1440, height: 1000 });
  await expect(page.locator('.page-title')).toContainText('does not send an invitation or start location sharing');
  await expectEventsCurrent(page, 'true');
  await page.screenshot({ path: resolve(captures, 'events-new-1440.png'), fullPage: true });
  expect(apiPaths.filter(path => /^\/api\/events\/(new|all)$/.test(path))).toEqual([]);

  // Dashboard → Create event → the created Event's item URL with the success message.
  await page.goto('/events');
  await page.getByRole('main').getByRole('link', { name: 'Create event', exact: true }).click();
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
});

test('Family switching clears and reloads the summaries, and administrator status adds no visibility', async ({ page, request }) => {
  test.setTimeout(120_000);
  const w = await setUpWorld(request);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await useIdentity(page, w.a, '/home');
  await page.getByLabel('Current Family').selectOption({ label: w.family1.name });
  await mainNavigation(page).getByRole('link', { name: 'Events' }).click();
  await expect(page).toHaveURL(/\/events$/);
  // A administers Family 1 but is outside the audience of B's Bay Picnic: it never appears.
  // The first test's created "Harvest Supper" (C → A) is also visible to A when both tests run; it is set aside here.
  await expect(rows(card(page, 'upcoming')).first()).toHaveText('Private Supper');
  expect((await rows(card(page, 'upcoming')).allTextContents()).map(name => name.trim()).filter(name => name !== 'Harvest Supper'))
    .toEqual(['Private Supper', 'Lantern Walk']);
  await expect(rows(card(page, 'trips'))).toHaveText(['Lantern Trip']);
  await expect(page.locator('main')).not.toContainText(/Bay Picnic|Second Family/);

  await page.getByLabel('Current Family').selectOption({ label: w.family2.name });
  await expect(page).toHaveURL(/\/events$/);
  await expect(page.locator('.page-title')).toContainText(w.family2.name);
  await expect(rows(card(page, 'upcoming'))).toHaveText(['Second Family Dinner']);
  await expect(rows(card(page, 'trips'))).toHaveText(['Second Family Trip']);
  await expect(page.locator('main')).not.toContainText(/Lantern|Private Supper|Bay Picnic/);
});
