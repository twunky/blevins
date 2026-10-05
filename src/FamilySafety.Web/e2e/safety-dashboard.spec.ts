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

// WI-009-009 / DEC-079: the read-only Safety dashboard at /safety. Every row comes from the existing authorized list
// reads; the dashboard adds no access. The OD-E second Family makes Adult A a member of two Families (A administers
// the first); Adult C is a member of the first Family only; Adult B joins the first Family during setup.
useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }
interface FamilyMember { profileId: string; displayName: string }

const sentinel = 'SENTINEL-SAFETY-REASON';
const forbiddenWording = /all clear|everyone is safe|nothing to worry|is safe\b|unsafe|in danger|monitoring|emergency|overdue|missed check/i;
// Transient captures (WI-009-002); canonical evidence is copied into ai-context/ only during verification.
const captures = captureDirectory('safety-dashboard');

interface World {
  a: DevelopmentIdentity; b: DevelopmentIdentity; c: DevelopmentIdentity;
  family1: FamilyGroup; family2: FamilyGroup;
  aName: string; bName: string; cName: string;
  /** A creates; subject C; participant B. Visible to A and B, never to C (the subject). */
  concernBySubjectC: string;
  /** C creates; subject A (A administers Family 1); participant B. Never visible to A. */
  concernAboutA: string;
  /** Family 2: A creates; subject and participant B. Visible to A with the Family 2 label. */
  concernFamily2: string;
  /** A creates and resolves. Never on any dashboard. */
  concernResolved: string;
  incidentBySubjectC: string; incidentAboutA: string; incidentFamily2: string; incidentResolved: string;
}

let world: World | null = null;

async function send(request: APIRequestContext, path: string, accountId: string, data: unknown): Promise<{ status: number; body: any }> {
  const response = await request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: accountId }, data });
  return { status: response.status(), body: await response.json().catch(() => null) };
}

async function createConcern(request: APIRequestContext, creator: string, family: FamilyGroup, subject: string, participants: string[], label: string) {
  const created = await send(request, `/api/family-groups/${family.familyGroupId}/concerns`, creator,
    { subjectProfileId: subject, reason: `${sentinel} ${label}`, participantProfileIds: participants });
  expect(created.status, `${label} Concern create`).toBe(201);
  return created.body.concernId as string;
}

async function createIncident(request: APIRequestContext, creator: string, family: FamilyGroup, subject: string, participants: string[], label: string) {
  const created = await send(request, `/api/family-groups/${family.familyGroupId}/incidents`, creator, {
    subjectProfileId: subject, reason: `${sentinel} ${label}`, reasonCategory: 'Reported', reasonSourceDescription: 'Adult A',
    reasonSourceUnknown: false, participantProfileIds: participants, originatingConcernId: null,
  });
  expect(created.status, `${label} Incident create`).toBe(201);
  return created.body.incidentId as string;
}

async function setUpWorld(request: APIRequestContext): Promise<World> {
  if (world) return world;
  await recordScenario(test.info());
  await expectHealthyApi(request);
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json() as
    { identities: DevelopmentIdentity[] }).identities;
  const find = (prefix: string) => identities.find(identity => identity.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  const aFamilies = (await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', a.accountId)).body;
  const family1 = aFamilies.find(group => group.name === 'Development Family')!;
  const family2 = aFamilies.find(group => group.name === 'Development Second Family')!;
  expect(aFamilies).toHaveLength(2);
  // A administers Family 1: administrator status must add no Safety visibility.
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/administered', a.accountId)).body.map(group => group.familyGroupId))
    .toContain(family1.familyGroupId);

  // B joins Family 1 through the existing invitation API, as in item-links.spec.ts.
  expect((await send(request, '/api/membership-invitations', a.accountId,
    { familyGroupId: family1.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status).toBe(202);
  const outbox = await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', a.accountId);
  const token = decodeURIComponent(outbox.body.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await send(request, '/api/membership-invitations/accept', b.accountId, { token })).status).toBe(200);

  const members = (await apiJson<FamilyMember[]>(request, `/api/family-groups/${family1.familyGroupId}/members`, a.accountId)).body;
  const aProfile = (await apiJson<{ profileId: string; displayName: string }>(request, '/api/profile', a.accountId)).body;
  const bMember = members.find(member => member.displayName === 'Development Recipient')!;
  const cMember = members.find(member => member.profileId !== aProfile.profileId && member.profileId !== bMember.profileId)!;

  const concernResolved = await createConcern(request, a.accountId, family1, cMember.profileId, [bMember.profileId], 'resolved concern');
  expect((await send(request, `/api/concerns/${concernResolved}/resolve`, a.accountId, { note: 'Coordination ended.' })).status).toBe(200);
  const incidentResolved = await createIncident(request, a.accountId, family1, cMember.profileId, [bMember.profileId], 'resolved incident');
  expect((await send(request, `/api/incidents/${incidentResolved}/resolve`, a.accountId, {
    summary: 'Coordination ended.', outcomeText: 'Contact was restored.', outcomeCategory: 'Reported', outcomeSourceDescription: 'Adult A',
    outcomeSourceUnknown: false, contactRestoredAt: null, contactRestoredTimeUnknown: true, assistanceProvided: null,
    unresolvedFollowUp: null, lessons: null,
  })).status).toBe(200);

  world = {
    a, b, c, family1, family2, aName: aProfile.displayName, bName: bMember.displayName, cName: cMember.displayName,
    concernBySubjectC: await createConcern(request, a.accountId, family1, cMember.profileId, [bMember.profileId], 'subject C'),
    concernAboutA: await createConcern(request, c.accountId, family1, aProfile.profileId, [bMember.profileId], 'about A'),
    concernFamily2: await createConcern(request, a.accountId, family2, bMember.profileId, [bMember.profileId], 'family 2'),
    concernResolved,
    incidentBySubjectC: await createIncident(request, a.accountId, family1, cMember.profileId, [bMember.profileId], 'subject C'),
    incidentAboutA: await createIncident(request, c.accountId, family1, aProfile.profileId, [bMember.profileId], 'about A'),
    incidentFamily2: await createIncident(request, a.accountId, family2, bMember.profileId, [bMember.profileId], 'family 2'),
    incidentResolved,
  };
  return world;
}

// ---- Browser helpers ------------------------------------------------------------------------------------------

const identitySelect = (page: Page) => page.getByLabel('Active test identity');
const dashboard = (page: Page) => page.locator('app-safety-dashboard');
const card = (page: Page, heading: string) => dashboard(page).locator('section.card').filter({ has: page.getByRole('heading', { name: heading, exact: true }) });
const rowLinks = (page: Page, heading: string) => card(page, heading).locator('.safety-rows a.item-link');

async function actAs(page: Page, identity: DevelopmentIdentity): Promise<void> {
  if (await identitySelect(page).count() === 0) await page.goto('/home');
  if (await identitySelect(page).inputValue() !== identity.accountId) {
    await identitySelect(page).selectOption(identity.accountId);
    await page.waitForURL('**/home');
  }
  await expect(identitySelect(page)).toHaveValue(identity.accountId);
}

/** Opens /safety directly and records every API path it requests after the page starts loading. */
async function openSafety(page: Page): Promise<string[]> {
  const requested: string[] = [];
  const onRequest = (sent: { url(): string }) => { if (sent.url().includes('/api/')) requested.push(new URL(sent.url()).pathname); };
  page.on('request', onRequest);
  await page.goto('/safety');
  await expect(page.locator('main h1')).toHaveText('Safety');
  await expect(card(page, 'Open Concerns').locator('.safety-rows, .safety-empty')).toBeVisible();
  await expect(card(page, 'Open Safety Incidents').locator('.safety-rows, .safety-empty')).toBeVisible();
  await page.waitForLoadState('networkidle');
  page.off('request', onRequest);
  return requested;
}

async function hrefs(page: Page, heading: string): Promise<string[]> {
  return rowLinks(page, heading).evaluateAll(links => links.map(link => link.getAttribute('href') ?? ''));
}

// ---- Journeys -------------------------------------------------------------------------------------------------

test('Adult A sees only authorized Open items, with Family context, and nothing private or alarming', async ({ page, request }) => {
  test.setTimeout(180_000);
  const w = await setUpWorld(request);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await actAs(page, w.a);

  const requested = await openSafety(page);
  // Direct entry with two Families and none selected: the dashboard renders without the Family prompt.
  await expect(page.locator('#current-family')).toHaveValue('');
  await expect(page.locator('[aria-label="Family context required"]')).toHaveCount(0);
  await expect(page.getByRole('navigation', { name: 'Main navigation' }).locator('a[aria-current="page"]')).toHaveText('Safety');

  // Only the two authorized Safety lists are read; no Check-In, Trip, or item-detail request.
  const safetyReads = [...new Set(requested.filter(path => /\/api\/(concerns|incidents|check-ins|trips)/i.test(path)))].sort();
  expect(safetyReads).toEqual(['/api/check-ins/mine', '/api/check-ins/received', '/api/concerns', '/api/incidents']);

  // Concerns: A's own Family 1 Concern and the Family 2 Concern; never the Concern about A (subject + administrator),
  // never the resolved one.
  expect((await hrefs(page, 'Open Concerns')).sort()).toEqual([`/concerns/${w.concernBySubjectC}`, `/concerns/${w.concernFamily2}`].sort());
  expect((await hrefs(page, 'Open Safety Incidents')).sort()).toEqual([`/incidents/${w.incidentBySubjectC}`, `/incidents/${w.incidentFamily2}`].sort());
  await expect(dashboard(page)).not.toContainText(`Concern about ${w.aName}`);
  await expect(dashboard(page)).not.toContainText(`Incident about ${w.aName}`);
  await expect(dashboard(page).locator('.safety-state')).toHaveText(['Open', 'Open', 'Open', 'Open']);

  // Family context from the authorized rows, because A belongs to two Families.
  await expect(card(page, 'Open Concerns')).toContainText(w.family1.name);
  await expect(card(page, 'Open Concerns')).toContainText(w.family2.name);
  await expect(card(page, 'Open Safety Incidents')).toContainText(w.family2.name);

  // Minimal disclosure and calm wording.
  await expect(page.locator('main')).not.toContainText(sentinel);
  await expect(page.locator('main')).not.toContainText(/Frozen|Resolved/);
  expect(await dashboard(page).innerText()).not.toMatch(forbiddenWording);
  await expect(dashboard(page).locator('.safety-more')).toHaveCount(0);
  await expect(dashboard(page).getByRole('button')).toHaveCount(0);
  await expect(dashboard(page).getByRole('link')).toHaveCount(7);

  // Check-Ins placeholder: its sentence only.
  const checkIns = card(page, 'Recent Check-Ins');
  await expect(checkIns.locator('.safety-future-note')).toHaveText('View personal status reports sent by you or explicitly shared with you.');
  await expect(checkIns.locator('button, input, li, time, img')).toHaveCount(0);
  await expect(card(page, 'About Concerns and Incidents')).toContainText('Neither one proves danger or safety.');

  // 44 px targets on row and View all links.
  for (const box of await dashboard(page).locator('.safety-rows a.item-link, a.safety-view-all').evaluateAll(
    links => links.map(link => link.getBoundingClientRect().height))) expect(box).toBeGreaterThanOrEqual(44);

  // Canonical evidence source (copied during verification): the populated dashboard at every review width.
  for (const width of [1440, 1024, 768, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    await page.waitForTimeout(200);
    expect(await page.evaluate(() => document.documentElement.scrollWidth), `/safety at ${width}`).toBeLessThanOrEqual(width);
    await page.screenshot({ path: resolve(captures, `safety-populated-multi-family-${width}.png`), fullPage: true });
  }
  await page.setViewportSize({ width: 1440, height: 1000 });

  // A Family 2 row opens its item URL; authorize-first switching then selects Family 2.
  await rowLinks(page, 'Open Concerns').and(page.locator(`[href="/concerns/${w.concernFamily2}"]`)).click();
  await expect(page).toHaveURL(new RegExp(`/concerns/${w.concernFamily2}$`));
  await expect(page.getByRole('heading', { level: 2, name: `Concern about ${w.bName}` })).toBeVisible();
  await expect(page.locator('#current-family')).toHaveValue(w.family2.familyGroupId);
  await expect(page.getByRole('navigation', { name: 'Main navigation' }).locator('a[aria-current="true"]')).toHaveText('Safety');

  await page.goto('/safety');
  await rowLinks(page, 'Open Safety Incidents').and(page.locator(`[href="/incidents/${w.incidentBySubjectC}"]`)).click();
  await expect(page).toHaveURL(new RegExp(`/incidents/${w.incidentBySubjectC}$`));
  await expect(page.locator('main h1')).toHaveText('Safety Incidents');
  await expect(page.locator('#current-family')).toHaveValue(w.family1.familyGroupId);
});

test('Adult C sees only what C is authorized for: never a Concern or Incident where C is only the subject', async ({ page, request }) => {
  test.setTimeout(120_000);
  const w = await setUpWorld(request);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await actAs(page, w.c);
  await openSafety(page);
  // C created the items about A, and is the subject (without access) of A's items.
  expect(await hrefs(page, 'Open Concerns')).toEqual([`/concerns/${w.concernAboutA}`]);
  expect(await hrefs(page, 'Open Safety Incidents')).toEqual([`/incidents/${w.incidentAboutA}`]);
  await expect(dashboard(page)).not.toContainText(`about ${w.cName}`);
  // One Family: no Family label on rows.
  expect((await dashboard(page).locator('.safety-rows').allInnerTexts()).join(' ')).not.toContain(w.family1.name);
  await expect(page.locator('main')).not.toContainText(sentinel);

  // URLs are not authorization: C's direct URL to A's Concern about C shows the generic unavailable state.
  await page.goto(`/concerns/${w.concernBySubjectC}`);
  await expect(page.getByRole('heading', { name: 'This Concern is unavailable' })).toBeVisible();
  await expect(page.locator('main')).not.toContainText(sentinel);
});

test('Adult B (two Families) sees exactly the Open subset of B\'s authorized lists at 390 px, with no hidden count', async ({ page, request }) => {
  test.setTimeout(120_000);
  const w = await setUpWorld(request);
  // The server lists are the authority; the dashboard may only show their Open rows.
  const bConcerns = (await apiJson<{ concernId: string; state: string }[]>(request, '/api/concerns', w.b.accountId)).body;
  const bIncidents = (await apiJson<{ incidentId: string; state: string }[]>(request, '/api/incidents', w.b.accountId)).body;
  await page.setViewportSize({ width: 390, height: 844 });
  await actAs(page, w.b);
  await openSafety(page);
  const shownConcerns = await hrefs(page, 'Open Concerns');
  const shownIncidents = await hrefs(page, 'Open Safety Incidents');
  // The dashboard is exactly the Open subset of B's authorized lists, at most three rows each.
  const openConcerns = bConcerns.filter(row => row.state === 'Open').map(row => `/concerns/${row.concernId}`);
  const openIncidents = bIncidents.filter(row => row.state === 'Open').map(row => `/incidents/${row.incidentId}`);
  for (const href of shownConcerns) expect(openConcerns).toContain(href);
  for (const href of shownIncidents) expect(openIncidents).toContain(href);
  expect(shownConcerns.length).toBe(Math.min(3, openConcerns.length));
  expect(shownIncidents.length).toBe(Math.min(3, openIncidents.length));
  expect(shownConcerns).not.toContain(`/concerns/${w.concernResolved}`);
  expect(shownIncidents).not.toContain(`/incidents/${w.incidentResolved}`);
  // Additional open items are announced without a count.
  if (openConcerns.length > 3) {
    await expect(card(page, 'Open Concerns').locator('.safety-more')).toHaveText('Additional open Concerns are available.');
  }
  expect(await dashboard(page).innerText()).not.toMatch(/\b\d+ (more|additional|open)\b/i);

  // 390: no overflow; full-width View all and row links are 44 px targets.
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  for (const height of await dashboard(page).locator('.safety-rows a.item-link, a.safety-view-all').evaluateAll(
    links => links.map(link => link.getBoundingClientRect().height))) expect(height).toBeGreaterThanOrEqual(44);
  await page.screenshot({ path: resolve(captures, 'safety-participant-390.png'), fullPage: true });
});

test('Back to Safety returns from Concerns and from Safety Incidents', async ({ page, request }) => {
  test.setTimeout(120_000);
  const w = await setUpWorld(request);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await actAs(page, w.c);
  for (const [path, h1, capture] of [['/concerns', 'Concerns', 'concerns-back-to-safety'], ['/incidents', 'Safety Incidents', 'incidents-back-to-safety']] as const) {
    await page.goto(path);
    await expect(page.locator('main h1')).toHaveText(h1);
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: resolve(captures, `${capture}-1440.png`), fullPage: true });
    await page.getByRole('link', { name: 'Back to Safety', exact: true }).click();
    await expect(page).toHaveURL(/\/safety$/);
    await expect(page.locator('main h1')).toHaveText('Safety');
    await expect(page.locator('main h1')).toBeFocused();
  }
});
