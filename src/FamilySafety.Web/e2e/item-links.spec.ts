import { expect, test, type APIRequestContext, type Locator, type Page } from '@playwright/test';
import { randomUUID } from 'node:crypto';
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

// WI-009-005: refresh-safe item URLs and authorize-first deep links (REQ-UX-014, REQ-PRIV-027).
// The OD-E second Family makes Adult A a member of two Families; Adult C is a member of the first Family only.
useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }
interface FamilyMember { profileId: string; displayName: string }

type ItemType = 'event' | 'plan' | 'trip' | 'concern' | 'incident';
const itemTypes: readonly ItemType[] = ['event', 'plan', 'trip', 'concern', 'incident'];
const readPath: Record<ItemType, string> = {
  event: '/api/events/', plan: '/api/plans/', trip: '/api/trips/', concern: '/api/concerns/', incident: '/api/incidents/',
};
const urlPath: Record<ItemType, string> = {
  event: '/events/', plan: '/plans/', trip: '/trips/', concern: '/concerns/', incident: '/incidents/',
};
/** What each item page shows: its area (main navigation), page h1, the item h2 for a label, and its unavailable card. */
const page_: Record<ItemType, { area: string; h1: string; heading: (label: string) => string; noun: string; plural: string }> = {
  event: { area: 'Events', h1: 'Events', heading: label => `${label} Event`, noun: 'Event', plural: 'Events' },
  plan: { area: 'Plans & Trips', h1: 'Plans', heading: label => `${label} Plan`, noun: 'Plan', plural: 'Plans' },
  trip: { area: 'Plans & Trips', h1: 'Trips', heading: label => `${label} Trip`, noun: 'Trip', plural: 'Trips' },
  concern: { area: 'Safety', h1: 'Concerns', heading: () => 'Concern about Development Recipient', noun: 'Concern', plural: 'Concerns' },
  incident: { area: 'Safety', h1: 'Safety Incidents', heading: () => 'Incident about Development Recipient', noun: 'Incident', plural: 'Incidents' },
};
const widths = [
  { name: '1440', viewport: { width: 1440, height: 1000 } },
  { name: '390', viewport: { width: 390, height: 844 } },
] as const;
// Transient captures (WI-009-002): routine runs never write canonical evidence under ai-context/.
const captures = captureDirectory('item-links');

/** One set of items created through the existing APIs as Adult A, all shared with Adult B only. */
type ItemSet = Record<ItemType, string>;

interface World {
  a: DevelopmentIdentity; b: DevelopmentIdentity; c: DevelopmentIdentity;
  family1: FamilyGroup; family2: FamilyGroup;
  aProfile: string; bProfile: string;
  /** Family 1 items shared with B; C is a Family 1 member outside the audience. */
  family1Items: ItemSet;
  /** Family 2 items shared with B; C is not a Family 2 member. */
  family2Items: ItemSet;
}

let world: World | null = null;

async function send(request: APIRequestContext, path: string, accountId: string, data: unknown): Promise<{ status: number; body: any }> {
  const response = await request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: accountId }, data });
  return { status: response.status(), body: await response.json().catch(() => null) };
}

function futureDate(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}

async function createItems(request: APIRequestContext, a: string, family: FamilyGroup, aProfile: string, bProfile: string, label: string): Promise<ItemSet> {
  const base = `/api/family-groups/${family.familyGroupId}`;
  const event = await send(request, `${base}/events`, a, {
    name: `${label} Event`, description: `${label} Event description`, date: futureDate(20), time: '18:30',
    location: `${label} Hall`, audienceProfileIds: [bProfile], editorProfileIds: [],
  });
  expect(event.status, `${label} Event create`).toBe(201);
  const plan = await send(request, `${base}/plans`, a, {
    activityOrDestination: `${label} Plan`, date: futureDate(21), time: '09:00', expectedReturnDate: null, expectedReturnTime: null,
    expectedCheckInDate: null, expectedCheckInTime: null, notes: null, audienceType: 'SelectedPeople', audienceProfileIds: [bProfile],
  });
  expect(plan.status, `${label} Plan create`).toBe(201);
  const trip = await send(request, `${base}/trips`, a, {
    title: `${label} Trip`, primaryDestination: `${label} Coast`, startDate: futureDate(22), endDate: futureDate(24),
    description: null, transportationNotes: null, accommodationNotes: null, expectedArrivalDate: null, expectedArrivalTime: null,
    expectedArrivalTimeZone: null, travelerProfileIds: [aProfile], contacts: [], audienceType: 'SelectedPeople', audienceProfileIds: [bProfile],
  });
  expect(trip.status, `${label} Trip create`).toBe(201);
  const concern = await send(request, `${base}/concerns`, a, {
    subjectProfileId: bProfile, reason: `${label} Concern reason`, participantProfileIds: [bProfile],
  });
  expect(concern.status, `${label} Concern create`).toBe(201);
  const incident = await send(request, `${base}/incidents`, a, {
    subjectProfileId: bProfile, reason: `${label} Incident reason`, reasonCategory: 'Reported', reasonSourceDescription: 'Adult A',
    reasonSourceUnknown: false, participantProfileIds: [bProfile], originatingConcernId: null,
  });
  expect(incident.status, `${label} Incident create`).toBe(201);
  return {
    event: event.body.eventId, plan: plan.body.planId, trip: trip.body.tripId,
    concern: concern.body.concernId, incident: incident.body.incidentId,
  };
}

/** Builds the shared scenario once per file: identities, both Families, B joined to Family 1, and two item sets. */
async function setUpWorld(request: APIRequestContext): Promise<World> {
  if (world) return world;
  await recordScenario(test.info());
  await expectHealthyApi(request);
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json() as
    { identities: DevelopmentIdentity[] }).identities;
  const find = (prefix: string) => identities.find(identity => identity.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  expect(a && b && c).toBeTruthy();

  // AC-34: the opt-in fixture seeded the second Family with fixed IDs; A and B are members, C is not.
  const aFamilies = (await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', a.accountId)).body;
  const family1 = aFamilies.find(group => group.name === 'Development Family')!;
  const family2 = aFamilies.find(group => group.name === 'Development Second Family')!;
  expect(aFamilies).toHaveLength(2);
  expect(family2.familyGroupId).toBe('cccccccc-cccc-cccc-cccc-cccccccccccc');
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', b.accountId)).body.map(group => group.familyGroupId))
    .toEqual([family2.familyGroupId]);
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', c.accountId)).body.map(group => group.familyGroupId))
    .toEqual([family1.familyGroupId]);

  // B joins Family 1 through the existing invitation API, as in concern-workflow.spec.ts, so Family 1 has three adults.
  expect((await send(request, '/api/membership-invitations', a.accountId,
    { familyGroupId: family1.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status).toBe(202);
  const outbox = await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', a.accountId);
  const token = decodeURIComponent(outbox.body.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await send(request, '/api/membership-invitations/accept', b.accountId, { token })).status).toBe(200);

  const aProfile = (await apiJson<{ profileId: string }>(request, '/api/profile', a.accountId)).body.profileId;
  const members = (await apiJson<FamilyMember[]>(request, `/api/family-groups/${family1.familyGroupId}/members`, a.accountId)).body;
  const bProfile = members.find(member => member.displayName === 'Development Recipient')!.profileId;

  world = {
    a, b, c, family1, family2, aProfile, bProfile,
    family1Items: await createItems(request, a.accountId, family1, aProfile, bProfile, 'Harbor'),
    family2Items: await createItems(request, a.accountId, family2, aProfile, bProfile, 'Second'),
  };
  return world;
}

interface ReadShape { status: number; body: string; contentType: string | null; contentLength: string | null; headerNames: string }

async function readShape(request: APIRequestContext, type: ItemType, id: string, accountId: string): Promise<ReadShape> {
  const response = await request.get(`${apiBaseUrl}${readPath[type]}${id}`, { headers: { [developmentAccountHeader]: accountId } });
  const headers = response.headers();
  return {
    status: response.status(),
    body: await response.text(),
    contentType: headers['content-type'] ?? null,
    contentLength: headers['content-length'] ?? null,
    // Date varies per response; every other header name must match.
    headerNames: Object.keys(headers).filter(name => name !== 'date').sort().join(','),
  };
}

test('AC-31 — live server-response precheck: missing and unauthorized item reads are indistinguishable', async ({ request }) => {
  const w = await setUpWorld(request);
  const results: Record<string, unknown> = {};
  for (const type of itemTypes) {
    // Positive control: the audience member reads each item, and the response names its Family by ID.
    const authorized = await request.get(`${apiBaseUrl}${readPath[type]}${w.family2Items[type]}`,
      { headers: { [developmentAccountHeader]: w.b.accountId } });
    expect(authorized.status(), `${type}: B is in the audience`).toBe(200);
    expect((await authorized.json()).familyGroupId).toBe(w.family2.familyGroupId);

    const missing = await readShape(request, type, randomUUID(), w.c.accountId);
    const otherFamily = await readShape(request, type, w.family2Items[type], w.c.accountId);
    const notInAudience = await readShape(request, type, w.family1Items[type], w.c.accountId);
    results[type] = { missing, otherFamily, notInAudience };
    for (const [name, shape] of Object.entries({ otherFamily, notInAudience })) {
      expect(shape, `${type}: ${name} must match a missing item exactly`).toEqual(missing);
    }
    expect(missing.status, `${type}: nondisclosing status`).toBe(404);
    expect(missing.body, `${type}: empty body`).toBe('');
  }
  console.log(`[AC-31] ${JSON.stringify(results)}`);
  await test.info().attach('ac-31-precheck.json', { body: JSON.stringify(results, null, 2), contentType: 'application/json' });
});

// ---- Browser journeys -----------------------------------------------------------------------------------------

const identitySelect = (page: Page) => page.getByLabel('Active test identity');
const familySelect = (page: Page) => page.locator('#current-family');
const topFamily = (page: Page) => page.locator('.current-family strong');
const switchStatus = (page: Page) => page.locator('p.family-switch-status');
const mainNav = (page: Page) => page.getByRole('navigation', { name: 'Main navigation' });

async function actAs(page: Page, identity: DevelopmentIdentity, path = '/home'): Promise<void> {
  if (await identitySelect(page).count() === 0) await page.goto('/home');
  if (await identitySelect(page).inputValue() !== identity.accountId) {
    await identitySelect(page).selectOption(identity.accountId);
    await page.waitForURL('**/home');
  }
  await page.goto(path);
  await expect(identitySelect(page)).toHaveValue(identity.accountId);
}

/** In-app navigation to a URL without a reload, so the session-only Family selection is kept (REQ-UX-011). */
async function goInApp(page: Page, url: string): Promise<void> {
  await page.evaluate(target => {
    history.pushState({}, '', target);
    dispatchEvent(new PopStateEvent('popstate', { state: {} }));
  }, url);
  await page.waitForURL(`**${url}`);
}

/** Selects a Family in the top bar on Home, as a person would. */
async function chooseFamily(page: Page, family: FamilyGroup): Promise<void> {
  if (!page.url().endsWith('/home')) await page.goto('/home');
  await familySelect(page).selectOption(family.familyGroupId);
  await expect(topFamily(page)).toHaveText(family.name);
}

async function capture(page: Page, name: string): Promise<void> {
  for (const { name: width, viewport } of widths) {
    await page.setViewportSize(viewport);
    expect(await page.evaluate(() => document.documentElement.scrollWidth), `${name} must not overflow at ${width} px`)
      .toBeLessThanOrEqual(viewport.width);
    await page.screenshot({ path: resolve(captures, `${name}-${width}.png`), fullPage: true });
  }
  await page.setViewportSize(widths[0].viewport);
}

/** The visible item heading (the shell keeps other areas in the DOM, hidden). */
const itemHeading = (page: Page) => page.locator('main h2:visible').first();

async function expectItemPage(page: Page, type: ItemType, label: string): Promise<void> {
  await expect(itemHeading(page)).toHaveText(page_[type].heading(label));
  await expect(page.locator('main h1')).toHaveCount(1);
  await expect(page.locator('main h1')).toHaveText(page_[type].h1);
  await expect(mainNav(page).locator('a[aria-current]')).toHaveCount(1);
  await expect(mainNav(page).locator('a[aria-current]')).toHaveText(page_[type].area);
  await expect(mainNav(page).locator('a[aria-current]')).toHaveAttribute('aria-current', 'true');
  await expect(page.getByText('Choose your current Family')).toHaveCount(0);
}

/** The one unavailable card for a type: the shared card for Event, Plan, Trip; the existing card for Concern and Incident. */
function unavailableCard(page: Page, type: ItemType): Locator {
  return page.locator('main section:visible').filter({ has: page.getByRole('heading', { name: `This ${page_[type].noun} is unavailable` }) }).last();
}

async function expectUnavailable(page: Page, type: ItemType): Promise<string> {
  const card = unavailableCard(page, type);
  const heading = card.getByRole('heading', { name: `This ${page_[type].noun} is unavailable` });
  await expect(heading).toBeVisible();
  await expect(heading).toBeFocused();
  await expect(card).toContainText(`Only ${page_[type].plural} currently shared with you can be opened.`);
  await expect(card.getByText(`Return to ${page_[type].plural}`)).toBeVisible();
  await expect(switchStatus(page)).toHaveText('');
  return card.innerHTML();
}

test('AC-01 to AC-10, AC-29, AC-41 — each item URL renders on direct entry and after refresh, at 1440 and 390 px', async ({ page, request }) => {
  test.setTimeout(300_000);
  const w = await setUpWorld(request);
  for (const { name: width, viewport } of widths) {
    await page.setViewportSize(viewport);
    for (const type of itemTypes) {
      // A fresh tab: A belongs to two Families and nothing is selected yet.
      await page.goto(`${urlPath[type]}${w.family1Items[type]}`);
      await expect(identitySelect(page)).toHaveValue(w.a.accountId);
      await expectItemPage(page, type, 'Harbor');
      // Authorized: the item's Family is selected from A's own list (REQ-UX-014), and announced.
      await expect(topFamily(page)).toHaveText(w.family1.name);
      await expect(switchStatus(page)).toHaveText(`Switched to ${w.family1.name}.`);
      await expect(switchStatus(page)).toHaveAttribute('role', 'status');
      expect(await page.evaluate(() => document.documentElement.scrollWidth), `${type} at ${width} px`).toBeLessThanOrEqual(viewport.width);
      if (width === '1440') await capture(page, `${type}-detail`);
      await page.reload();
      await expectItemPage(page, type, 'Harbor');
    }
  }

  // Item content per type (desktop).
  await page.setViewportSize(widths[0].viewport);
  await page.goto(`/events/${w.family1Items.event}`);
  const event = page.getByRole('article', { name: 'Event details' });
  await expect(event).toContainText('Harbor Hall');
  await expect(event).toContainText('Harbor Event description');
  await expect(event).toContainText('Organized by Development Adult');
  await expect(event.getByRole('button', { name: 'Edit event details' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Back to Events' })).toHaveAttribute('href', '/events');

  await page.goto(`/plans/${w.family1Items.plan}`);
  const plan = page.getByRole('article', { name: 'Plan details' });
  await expect(plan).toContainText('Planned by Development Adult');
  await expect(plan.getByRole('button', { name: 'Add note' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Back to Plans' })).toHaveAttribute('href', '/plans');

  await page.goto(`/trips/${w.family1Items.trip}`);
  const trip = page.getByRole('article', { name: 'Trip details' });
  await expect(trip).toContainText('Harbor Coast');
  await expect(trip.getByRole('heading', { name: 'Trip Notes' })).toBeVisible();
  await expect(trip.getByRole('button', { name: 'Change audience' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Back to Trips' })).toHaveAttribute('href', '/trips');

  await page.goto(`/concerns/${w.family1Items.concern}`);
  await expect(page.getByText('Harbor Concern reason')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'History' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'People with Concern access' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Back to Concerns' })).toHaveAttribute('href', '/concerns');

  // AC-10: a member of two Families refreshes an Incident URL with nothing selected; no Family prompt, ever.
  const sawPrompt: boolean[] = [];
  await page.goto(`/incidents/${w.family1Items.incident}`);
  sawPrompt.push(await page.getByText('Choose your current Family').isVisible());
  await expect(page.getByText('Harbor Incident reason').first()).toBeVisible();
  await page.reload();
  sawPrompt.push(await page.getByText('Choose your current Family').isVisible());
  await expect(itemHeading(page)).toHaveText('Incident about Development Recipient');
  expect(sawPrompt).toEqual([false, false]);
});

test('Plan creator can add, edit, and clear the existing note on the item page', async ({ page, request }) => {
  const world = await setUpWorld(request);
  const planId = world.family1Items.plan;
  await page.goto(`/plans/${planId}`);
  const detail = page.getByRole('article', { name: 'Plan details' });
  await detail.getByRole('button', { name: 'Add note' }).click();
  await detail.getByRole('textbox', { name: 'Plan note' }).fill('Meet at the north entrance.');
  await detail.getByRole('button', { name: 'Save note' }).click();
  await expect(detail).toContainText('Meet at the north entrance.');
  await page.reload();
  await expect(detail).toContainText('Meet at the north entrance.');
  await detail.getByRole('button', { name: 'Edit note' }).click();
  await detail.getByRole('textbox', { name: 'Plan note' }).fill('Bring water.');
  await detail.getByRole('button', { name: 'Save note' }).click();
  await expect(detail).toContainText('Bring water.');
  await detail.getByRole('button', { name: 'Clear note' }).click();
  await expect(detail.getByRole('button', { name: 'Add note' })).toBeVisible();
  await page.reload();
  await expect(detail).not.toContainText('Bring water.');
});

test('A transient Event or Trip item read failure offers Retry, which repeats the authorized read and renders the item', async ({ page, request }) => {
  const world = await setUpWorld(request);
  const eventId = world.family1Items.event;
  // Only the first item read fails; the Retry goes to the real server, which authorizes it as before.
  await page.route(`**/api/events/${eventId}`, route => route.fulfill({ status: 503, body: '' }), { times: 1 });
  await page.goto(`/events/${eventId}`);
  await expect(page.getByRole('alert').filter({ hasText: 'This Event could not be opened. Please try again.' })).toBeVisible();
  const read = page.waitForResponse(response => response.url().endsWith(`/api/events/${eventId}`));
  await page.getByRole('button', { name: 'Retry loading Event' }).click();
  expect((await read).status()).toBe(200);
  await expect(page.getByRole('article', { name: 'Event details' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Retry loading Event' })).toHaveCount(0);

  // A Trip read failure is a read error with Retry, not the generic unavailable card.
  const tripId = world.family1Items.trip;
  await page.route(`**/api/trips/${tripId}`, route => route.fulfill({ status: 503, body: '' }), { times: 1 });
  await page.goto(`/trips/${tripId}`);
  await expect(page.getByRole('alert').filter({ hasText: 'This Trip could not be opened. Please try again.' })).toBeVisible();
  await expect(page.getByText('This Trip is unavailable')).toHaveCount(0);
  const tripRead = page.waitForResponse(response => response.url().endsWith(`/api/trips/${tripId}`));
  await page.getByRole('button', { name: 'Retry loading Trip' }).click();
  expect((await tripRead).status()).toBe(200);
  await expect(page.getByRole('heading', { name: 'Harbor Trip' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Retry loading Trip' })).toHaveCount(0);
});

test('AC-17 to AC-21, AC-32, AC-33, AC-41 — authorized cross-Family links switch only after the item read, and announce once', async ({ page, request }) => {
  test.setTimeout(300_000);
  const w = await setUpWorld(request);
  await page.setViewportSize(widths[0].viewport);
  await actAs(page, w.a, '/home');

  for (const type of itemTypes) {
    await chooseFamily(page, w.family1);
    await expect(switchStatus(page)).toHaveText('');
    // Hold the item response; nothing about Family 2 may happen before it returns.
    const log: string[] = [];
    const onRequest = (r: { url(): string }) => log.push(r.url());
    page.on('request', onRequest);
    let release!: () => void;
    const gate = new Promise<void>(resolveGate => release = resolveGate);
    const itemUrl = `**${readPath[type]}${w.family2Items[type]}`;
    await page.route(itemUrl, async route => { await gate; await route.continue(); });
    const itemResponse = page.waitForResponse(response => response.url().endsWith(`${readPath[type]}${w.family2Items[type]}`));

    await goInApp(page, `${urlPath[type]}${w.family2Items[type]}`);
    await page.waitForTimeout(500);
    await expect(topFamily(page)).toHaveText(w.family1.name);
    await expect(familySelect(page)).toHaveValue(w.family1.familyGroupId);
    await expect(switchStatus(page)).toHaveText('');
    await expect(page.getByText(page_[type].heading('Second'), { exact: true })).toHaveCount(0);
    expect(log.filter(url => url.includes(`/api/family-groups/${w.family2.familyGroupId}/`)), `${type}: Family 2 reads before 200`).toEqual([]);
    const beforeCount = log.length;

    release();
    expect((await itemResponse).status()).toBe(200);
    await expectItemPage(page, type, 'Second');
    await expect(topFamily(page)).toHaveText(w.family2.name);
    await expect(familySelect(page)).toHaveValue(w.family2.familyGroupId);
    await expect(switchStatus(page)).toHaveText(`Switched to ${w.family2.name}.`);
    await expect(page.getByRole('status').filter({ hasText: 'Switched to' })).toHaveCount(1);
    // Family 2's scoped data is loaded after the 200, and Family 1's is gone.
    await expect.poll(() => log.slice(beforeCount).some(url => url.includes(`/api/family-groups/${w.family2.familyGroupId}/members`))).toBe(true);
    if (type === 'event') await capture(page, 'cross-family-switch-announcement');
    await page.unroute(itemUrl);
    page.off('request', onRequest);

    // AC-32: Back after a switch keeps the new selection; nothing switches back.
    await page.goBack();
    await expect(page).toHaveURL(/\/home$/);
    await expect(topFamily(page)).toHaveText(w.family2.name);
    await expect(switchStatus(page)).toHaveText('');
  }

  // OD-F: Concern and Incident opened from their lists (which span A's Families) also switch after authorization.
  await chooseFamily(page, w.family1);
  await goInApp(page, '/concerns');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Open/ }).filter({ hasText: 'Second Concern reason' }).click();
  await expect(page).toHaveURL(new RegExp(`/concerns/${w.family2Items.concern}$`));
  await expect(page.getByText('Second Concern reason')).toBeVisible();
  await expect(topFamily(page)).toHaveText(w.family2.name);
  await expect(switchStatus(page)).toHaveText(`Switched to ${w.family2.name}.`);

  await chooseFamily(page, w.family1);
  await goInApp(page, '/incidents');
  await page.getByRole('link', { name: 'Open Incident about Development Recipient, Open' }).filter({ hasText: w.family2.name }).click();
  await expect(page).toHaveURL(new RegExp(`/incidents/${w.family2Items.incident}$`));
  await expect(page.getByText('Second Incident reason').first()).toBeVisible();
  await expect(topFamily(page)).toHaveText(w.family2.name);
  await expect(switchStatus(page)).toHaveText(`Switched to ${w.family2.name}.`);

  // AC-21: an item of the already selected Family opens with no switch, no Family reload, and no announcement.
  const sameLog: string[] = [];
  page.on('request', r => sameLog.push(r.url()));
  await goInApp(page, `/events/${w.family2Items.event}`);
  await expectItemPage(page, 'event', 'Second');
  await page.waitForTimeout(500);
  await expect(topFamily(page)).toHaveText(w.family2.name);
  await expect(switchStatus(page)).toHaveText('');
  // The shell's own load reads Family 2 once on creation; the item adds no second Family read.
  expect(sameLog.filter(url => url.endsWith(`/api/family-groups/${w.family2.familyGroupId}/members`))).toHaveLength(1);
});

test('AC-23 to AC-25, AC-41 — missing, unauthorized, and invalid item URLs show one generic card and never switch Family', async ({ page, request }) => {
  test.setTimeout(300_000);
  const w = await setUpWorld(request);
  // C belongs to Family 1 only. C may not read A's Family 2 items (other Family) or Family 1 items (not in audience).
  await actAs(page, w.c, '/home');
  for (const { name: width, viewport } of widths) {
    await page.setViewportSize(viewport);
    for (const type of itemTypes) {
      const itemRequests: string[] = [];
      const onRequest = (r: { url(): string }) => { if (r.url().includes(readPath[type])) itemRequests.push(r.url()); };
      page.on('request', onRequest);
      const cases = [
        { name: 'missing', url: `${urlPath[type]}${randomUUID()}` },
        { name: 'other Family', url: `${urlPath[type]}${w.family2Items[type]}` },
        { name: 'not in audience', url: `${urlPath[type]}${w.family1Items[type]}` },
        { name: 'invalid ID', url: `${urlPath[type]}not-a-guid` },
      ];
      const markup: string[] = [];
      for (const item of cases) {
        await page.goto(item.url);
        markup.push(await expectUnavailable(page, type));
        await expect(topFamily(page), `${type} ${item.name}`).toHaveText(w.family1.name);
        await expect(page.locator('main h1')).toHaveText(page_[type].h1);
        const text = await page.locator('body').innerText();
        expect(text, `${type} ${item.name}: no other Family is named`).not.toContain(w.family2.name);
        expect(text, `${type} ${item.name}: no item name`).not.toMatch(/Second (Event|Plan|Trip|Coast|Concern reason|Incident reason)|Harbor (Event|Plan|Trip|Coast|Concern reason|Incident reason)/);
        expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(viewport.width);
        if (width === '1440' && item.name === 'missing') await capture(page, `${type}-unavailable`);
      }
      // AC-25: the same card whatever the reason; the DOM differs only in the URL.
      expect(new Set(markup).size, `${type}: one card for every case`).toBe(1);
      // An invalid ID is never sent to the server.
      expect(itemRequests.filter(url => url.endsWith('not-a-guid'))).toEqual([]);
      page.off('request', onRequest);
    }
  }

  // AC-23 with no selection: A has two Families and a fresh tab; a missing item leaves "none" selected.
  await actAs(page, w.a, `/events/${randomUUID()}`);
  await expectUnavailable(page, 'event');
  await expect(topFamily(page)).toHaveCount(0);
  await expect(familySelect(page)).toHaveValue('');
});

test('AC-11 to AC-16, AC-32, AC-36, AC-41 — list and Home rows are links; back and forward restore list and item', async ({ page, request }) => {
  test.setTimeout(300_000);
  const w = await setUpWorld(request);
  await page.setViewportSize(widths[0].viewport);
  await actAs(page, w.a, '/home');
  await chooseFamily(page, w.family1);

  // AC-16: Home rows link to their items; captured with the links visible.
  const home = (heading: string) => page.locator(`section[aria-labelledby="${heading}"]`);
  await expect(home('home-events-heading').getByRole('link', { name: 'Harbor Event' })).toHaveAttribute('href', `/events/${w.family1Items.event}`);
  await expect(home('home-plans-heading').getByRole('link', { name: 'Harbor Plan' })).toHaveAttribute('href', `/plans/${w.family1Items.plan}`);
  await expect(home('home-trips-heading').getByRole('link', { name: 'Harbor Trip' })).toHaveAttribute('href', `/trips/${w.family1Items.trip}`);
  await capture(page, 'home-row-links');
  for (const [heading, type, name] of [['home-events-heading', 'event', 'Harbor Event'], ['home-plans-heading', 'plan', 'Harbor Plan'], ['home-trips-heading', 'trip', 'Harbor Trip']] as const) {
    await home(heading).getByRole('link', { name }).click();
    await expectItemPage(page, type, 'Harbor');
    await expect(page.locator('main h1')).toBeFocused();
    await page.goBack();
    await expect(page).toHaveURL(/\/home$/);
    await expect(home(heading).getByRole('link', { name })).toBeVisible();
  }

  // AC-11 to AC-15: list rows are semantic links with the existing names; list → item → Back → Forward.
  const lists: [ItemType, string, Locator][] = [
    // WI-009-008: the complete Event list (with inline edit) is /events/all.
    ['event', '/events/all', page.locator('.events-overview').getByRole('link', { name: 'Harbor Event' })],
    ['plan', '/plans', page.locator('.plans-overview').getByRole('link', { name: 'Harbor Plan' })],
    ['trip', '/trips', page.getByRole('link', { name: /Harbor Trip/ })],
    ['concern', '/concerns', page.getByRole('link', { name: /Open Concern about Development Recipient, Open/ }).filter({ hasText: 'Harbor Concern reason' })],
    ['incident', '/incidents', page.getByRole('link', { name: 'Open Incident about Development Recipient, Open' }).filter({ hasText: w.family1.name })],
  ];
  for (const [type, list, link] of lists) {
    await goInApp(page, list);
    await expect(link).toHaveAttribute('href', `${urlPath[type]}${w.family1Items[type]}`);
    if (type === 'event') await expect(page.locator('.events-overview').getByRole('button', { name: 'Edit event details' }).first()).toBeVisible();
    // Keyboard: the row link is reachable and opens with Enter.
    await link.focus();
    await expect(link).toBeFocused();
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(new RegExp(`${urlPath[type]}${w.family1Items[type]}$`));
    await expectItemPage(page, type, 'Harbor');
    await page.goBack();
    await expect(page).toHaveURL(new RegExp(`${list}$`));
    await expect(link).toBeVisible();
    // No focus jump on popstate (WI-009-003 D4).
    await expect(page.locator('main h1')).not.toBeFocused();
    await page.goForward();
    await expectItemPage(page, type, 'Harbor');
    await expect(page.locator('main h1')).not.toBeFocused();
    await expect(topFamily(page)).toHaveText(w.family1.name);
  }

  // AC-36 at 390 px: a row link is reachable by keyboard with a visible focus outline.
  await page.setViewportSize(widths[1].viewport);
  await goInApp(page, '/trips');
  const row = page.getByRole('link', { name: /Harbor Trip/ });
  await row.focus();
  await page.keyboard.press('Shift+Tab');
  await page.keyboard.press('Tab');
  await expect(row).toBeFocused();
  expect(await row.evaluate(element => getComputedStyle(element).outlineStyle)).toBe('solid');
  await page.keyboard.press('Enter');
  await expectItemPage(page, 'trip', 'Harbor');
});

test('AC-27 — Concern creation ends at its own URL, keeping the recovery and ambiguous-create checks', async ({ page, request }) => {
  test.setTimeout(240_000);
  const w = await setUpWorld(request);
  await page.setViewportSize(widths[0].viewport);
  await actAs(page, w.a, '/home');
  await chooseFamily(page, w.family1);

  async function fillCreate(reason: string, initial = false) {
    await goInApp(page, '/concerns');
    await page.getByRole('button', { name: 'Create a Concern' }).click();
    await page.getByLabel('Who is this Concern about?').selectOption({ label: 'Development Recipient' });
    await page.getByLabel('Reason or context').fill(reason);
    if (initial) {
      await page.getByLabel('Add an Observation').check();
      await page.getByRole('textbox', { name: 'Information' }).fill('A call was expected at noon.');
      await page.getByLabel('Source description').fill('Development Adult');
    }
  }

  // 1. A fully successful create.
  await fillCreate('Created to test its own URL');
  const created = page.waitForResponse(response => response.url().endsWith(`/api/family-groups/${w.family1.familyGroupId}/concerns`)
    && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Concern', exact: true }).click();
  const concernId = (await (await created).json()).concernId as string;
  await expect(page).toHaveURL(new RegExp(`/concerns/${concernId}$`));
  await expect(page.getByText('Created to test its own URL')).toBeVisible();
  await page.reload();
  await expect(page.getByText('Created to test its own URL')).toBeVisible();
  await expect(itemHeading(page)).toHaveText('Concern about Development Recipient');

  // 2. The initial entry fails: the recovery panel stays on /concerns until "Continue to Concern".
  await chooseFamily(page, w.family1);
  await fillCreate('Created with a failing initial entry', true);
  await page.route('**/api/concerns/*/entries', route => route.fulfill({ status: 503, body: '' }));
  const recoveryCreate = page.waitForResponse(response => response.url().endsWith('/concerns') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Concern', exact: true }).click();
  const recoveryId = (await (await recoveryCreate).json()).concernId as string;
  await expect(page.getByText('The Concern was created, but the initial entry was not saved.')).toBeVisible();
  await expect(page).toHaveURL(/\/concerns$/);
  await page.unroute('**/api/concerns/*/entries');
  await page.getByRole('button', { name: 'Continue to Concern' }).click();
  await expect(page).toHaveURL(new RegExp(`/concerns/${recoveryId}$`));
  await expect(page.getByText('Created with a failing initial entry')).toBeVisible();

  // 3. Ambiguous create: the server creates it but the response is lost; the existing check finds it, then it opens.
  await chooseFamily(page, w.family1);
  await fillCreate('Created behind a lost response');
  await page.route(`**/api/family-groups/${w.family1.familyGroupId}/concerns`, async route => {
    if (route.request().method() !== 'POST') return route.continue();
    await route.fetch();
    await route.fulfill({ status: 502, body: '' });
  });
  await page.getByRole('button', { name: 'Create Concern', exact: true }).click();
  await expect(page.getByText('We could not confirm whether the Concern was created.').first()).toBeVisible();
  await expect(page).toHaveURL(/\/concerns$/);
  await page.unroute(`**/api/family-groups/${w.family1.familyGroupId}/concerns`);
  await page.getByRole('button', { name: 'Refresh authorized list' }).click();
  await expect(page).toHaveURL(/\/concerns\/[0-9a-f-]{36}$/);
  await expect(page.getByText('A matching Concern was found in your authorized list.')).toBeVisible();
  await expect(page.getByText('Created behind a lost response')).toBeVisible();
});

test('AC-28 — Start an Incident from a Concern URL, and its lineage link is the Concern URL, authorized on its own', async ({ page, request }) => {
  test.setTimeout(240_000);
  const w = await setUpWorld(request);
  await page.setViewportSize(widths[0].viewport);
  await actAs(page, w.a, `/concerns/${w.family1Items.concern}`);
  await expect(page.getByText('Harbor Concern reason')).toBeVisible();
  await page.getByRole('button', { name: 'Start an Incident…' }).click();
  await expect(page).toHaveURL(/\/incidents\/new$/);
  // Navigation state carries the three IDs only.
  const stateKeys = await page.evaluate(() => Object.keys(history.state ?? {}).filter(key => !['navigationId', 'ɵrouterPageId'].includes(key)).sort());
  expect(stateKeys).toEqual(['familyGroupId', 'originatingConcernId', 'subjectProfileId']);
  await expect(page.getByRole('heading', { name: 'Starting a separate Incident' })).toBeVisible();
  await expect(page.getByLabel('Who is this Incident about?')).toHaveValue(w.bProfile);
  await expect(page.getByText('Harbor Concern reason')).toHaveCount(0);
  await page.getByLabel('Why are you starting this Incident?').fill('Started from the Harbor Concern.');
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult A');
  const created = page.waitForResponse(response => response.url().endsWith(`/api/family-groups/${w.family1.familyGroupId}/incidents`)
    && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Incident', exact: true }).click();
  const incident = await (await created).json() as { incidentId: string; participants: { profileId: string }[] };
  await expect(page).toHaveURL(new RegExp(`/incidents/${incident.incidentId}$`));
  expect(incident.participants.map(person => person.profileId)).toEqual([w.aProfile]);
  const lineage = page.getByRole('link', { name: 'Open the Concern' });
  await expect(lineage).toHaveAttribute('href', `/concerns/${w.family1Items.concern}`);
  await lineage.click();
  await expect(page).toHaveURL(new RegExp(`/concerns/${w.family1Items.concern}$`));
  await expect(page.getByText('Harbor Concern reason')).toBeVisible();

  // C joins the Incident only. The Concern URL is authorized on its own: C gets the Concern unavailable card.
  const members = (await apiJson<FamilyMember[]>(request, `/api/family-groups/${w.family1.familyGroupId}/members`, w.a.accountId)).body;
  const cProfile = members.find(member => member.displayName === 'Development Audience Member')!.profileId;
  const add = await request.put(`${apiBaseUrl}/api/incidents/${incident.incidentId}/participants`, {
    headers: { [developmentAccountHeader]: w.a.accountId }, data: { participantProfileIds: [cProfile] },
  });
  expect(add.status()).toBe(200);
  await actAs(page, w.c, `/incidents/${incident.incidentId}`);
  await expect(itemHeading(page)).toHaveText('Incident about Development Recipient');
  await expect(page.getByRole('link', { name: 'Open the Concern' })).toHaveCount(0);
  await page.goto(`/concerns/${w.family1Items.concern}`);
  await expectUnavailable(page, 'concern');
});

test('AC-01 — editing an Event from its own page uses the existing form and PUT', async ({ page, request }) => {
  test.setTimeout(180_000);
  const w = await setUpWorld(request);
  const created = await send(request, `/api/family-groups/${w.family1.familyGroupId}/events`, w.a.accountId, {
    name: 'Editable Event', description: 'Before the edit', date: futureDate(30), time: '17:00', location: 'Community room',
    audienceProfileIds: [w.bProfile], editorProfileIds: [],
  });
  expect(created.status).toBe(201);
  await page.setViewportSize(widths[0].viewport);
  await actAs(page, w.a, `/events/${created.body.eventId}`);
  const article = page.getByRole('article', { name: 'Event details' });
  await article.getByRole('button', { name: 'Edit event details' }).click();
  await article.getByLabel('Event name').fill('Edited Event');
  const put = page.waitForResponse(response => response.request().method() === 'PUT'
    && response.url().endsWith(`/api/family-groups/${w.family1.familyGroupId}/events/${created.body.eventId}`));
  await article.getByRole('button', { name: 'Save event details' }).click();
  expect((await put).status()).toBe(200);
  await expect(itemHeading(page)).toHaveText('Edited Event');
  await expect(page.getByRole('article', { name: 'Event details' }).getByText('Event details updated.')).toBeVisible();
  await page.reload();
  await expect(itemHeading(page)).toHaveText('Edited Event');

  // B is in the audience without edit access: the same page has no edit control.
  await actAs(page, w.b, `/events/${created.body.eventId}`);
  await expect(itemHeading(page)).toHaveText('Edited Event');
  await expect(page.getByRole('button', { name: 'Edit event details' })).toHaveCount(0);
});
