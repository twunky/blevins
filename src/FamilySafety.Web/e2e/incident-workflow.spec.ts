import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { randomUUID } from 'node:crypto';
import { mkdirSync } from 'node:fs';
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

// Fresh database and API for this file (both journeys share it); Adult C is needed for explicit Incident access.
useIsolatedScenario({ tripAudienceAdult: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }
interface FamilyMember { profileId: string; displayName: string }
interface IncidentView {
  incidentId: string;
  state: 'Open' | 'Frozen' | 'Resolved';
  coordinatorProfileId: string;
  originatingConcernId: string | null;
  participants: { profileId: string; isCoordinator: boolean }[];
  entries: { entryId: string; kind: string; text: string | null; contributorProfileId: string }[];
  tasks: { taskId: string; status: string }[];
  timeline: { type: string }[];
  expiresAt: string | null;
  frozenAt: string | null;
  freezeReason: string | null;
}
interface Actors {
  a: DevelopmentIdentity; b: DevelopmentIdentity; c: DevelopmentIdentity;
  family: FamilyGroup; aProfile: string; bProfile: FamilyMember; cProfile: FamilyMember;
}

const desktopViewport = { width: 1440, height: 1000 };
const narrowViewport = { width: 390, height: 844 };
// Transient captures (WI-009-002): routine runs never write canonical evidence under ai-context/.
const evidenceDirectory = captureDirectory('incident-workflow');

async function capture(page: Page, name: string): Promise<void> {
  mkdirSync(evidenceDirectory, { recursive: true });
  await page.setViewportSize(desktopViewport);
  await page.screenshot({ path: resolve(evidenceDirectory, `${name}-1440.png`), fullPage: true });
  await page.setViewportSize(narrowViewport);
  expect(await page.evaluate(() => document.documentElement.scrollWidth), `${name} must not overflow at 390 px`)
    .toBeLessThanOrEqual(narrowViewport.width);
  await page.screenshot({ path: resolve(evidenceDirectory, `${name}-390.png`), fullPage: true });
  await page.setViewportSize(desktopViewport);
}

async function send(request: APIRequestContext, method: 'post' | 'put', path: string, accountId: string, data: unknown) {
  const response = await request[method](`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: accountId }, data });
  return { status: response.status(), body: await response.json().catch(() => null) };
}

/** Status only: a nondisclosing 404 has no JSON body. */
async function statusOf(request: APIRequestContext, path: string, accountId: string) {
  const response = await request.get(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: accountId } });
  return { status: response.status() };
}

async function actAs(page: Page, identity: DevelopmentIdentity, path: string): Promise<void> {
  const select = page.getByLabel('Active test identity');
  if (await select.inputValue() !== identity.accountId) {
    await select.selectOption(identity.accountId);
    await page.waitForURL('**/home');
  }
  await page.goto(path);
  await expect(page.getByLabel('Active test identity')).toHaveValue(identity.accountId);
}

async function setUp(page: Page, request: APIRequestContext): Promise<Actors> {
  await recordScenario(test.info());
  await expectHealthyApi(request);
  const identityResponse = await request.get(`${apiBaseUrl}/api/development/identities`);
  const identities = (await identityResponse.json() as { identities: DevelopmentIdentity[] }).identities;
  const find = (prefix: string) => identities.find(identity => identity.label.startsWith(prefix))!;
  const [a, b, c] = [find('Adult A'), find('Adult B'), find('Adult C')];
  expect(a && b && c).toBeTruthy();

  const family = (await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', a.accountId)).body[0];
  const bGroups = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', b.accountId);
  if (!bGroups.body.some(group => group.familyGroupId === family.familyGroupId)) {
    // Same invitation setup as concern-workflow.spec.ts: Adult B joins the Family so they can be a subject.
    expect((await send(request, 'post', '/api/membership-invitations', a.accountId,
      { familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status).toBe(202);
    const outbox = await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', a.accountId);
    const token = decodeURIComponent(outbox.body.at(-1)!.invitationUrl.split('#invite=')[1]);
    expect((await send(request, 'post', '/api/membership-invitations/accept', b.accountId, { token })).status).toBe(200);
  }
  const members = (await apiJson<FamilyMember[]>(request, `/api/family-groups/${family.familyGroupId}/members`, a.accountId)).body;
  const aProfile = (await apiJson<{ profileId: string }>(request, '/api/profile', a.accountId)).body.profileId;
  const bProfile = members.find(member => member.displayName === 'Development Recipient')!;
  const cProfile = members.find(member => member.displayName === 'Development Audience Member')!;
  expect(bProfile && cProfile).toBeTruthy();
  await page.setViewportSize(desktopViewport);
  await page.goto('/home');
  await expect(page.getByLabel('Active test identity')).toBeVisible();
  return { a, b, c, family, aProfile, bProfile, cProfile };
}

const entryRequest = (text: string) => ({
  kind: 'Observation', category: 'Reported', text, sourceDescription: 'Development Adult', sourceUnknown: false,
  eventAt: null, relatedEntryId: null, explanation: null, isOutdated: false, originalCategory: null,
});

test('Journey 1 — direct Incident: explicit access, contributions, tasks, handoff, resolution, and unavailable', async ({ page, request }) => {
  test.setTimeout(420_000);
  const { a, b, c, family, cProfile } = await setUp(page, request);

  // 1. Adult A creates an Incident about B with nobody else selected.
  await actAs(page, a, '/incidents');
  await expect(page.getByRole('heading', { level: 1, name: 'Safety Incidents' })).toBeVisible();
  await expect(page.getByText('No Incidents are currently shared with you.')).toBeVisible();
  await capture(page, 'incident-list-empty');
  await page.getByRole('button', { name: 'Start an Incident' }).click();
  await expect(page).toHaveURL(/\/incidents\/new$/);
  await page.getByLabel('Who is this Incident about?').selectOption({ label: 'Development Recipient' });
  await page.getByLabel('Why are you starting this Incident?').fill('No reply since the expected arrival this evening.');
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult A');
  const people = page.getByRole('group', { name: 'People with access' });
  await expect(people.getByText('Choosing someone as the subject does not share the Incident with them.')).toBeVisible();
  await expect(people.getByRole('checkbox', { name: /Development Recipient/ })).not.toBeChecked();
  await expect(people.getByText('(subject, not added)')).toBeVisible();
  for (const box of await people.getByRole('checkbox').all()) await expect(box).not.toBeChecked();
  await capture(page, 'incident-create-direct');
  const createResponse = page.waitForResponse(response => response.url().endsWith(`/api/family-groups/${family.familyGroupId}/incidents`)
    && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Incident', exact: true }).click();
  expect((await createResponse).status()).toBe(201);
  const created = await (await createResponse).json() as IncidentView;
  const incidentPath = `/incidents/${created.incidentId}`;
  await expect(page).toHaveURL(new RegExp(`${incidentPath}$`));
  await expect(page.getByRole('heading', { name: 'Incident about Development Recipient', exact: true })).toBeVisible();
  await expect(page.getByRole('status').filter({ hasText: 'Incident created.' })).toBeVisible();
  await expect(page.getByText('Only you have access. Add people when you are ready.')).toBeVisible();
  await expect(page.getByText('Development Recipient is the subject of this Incident and does not have access unless added.')).toBeVisible();
  expect(created.participants.map(person => person.profileId)).toEqual([created.coordinatorProfileId]);

  // 2. The subject gets no automatic access: server-enforced 404 and an empty list.
  expect((await statusOf(request, `/api/incidents/${created.incidentId}`, b.accountId)).status).toBe(404);
  expect((await apiJson<IncidentView[]>(request, '/api/incidents', b.accountId)).body).toHaveLength(0);
  await actAs(page, b, '/incidents');
  await expect(page.getByText('No Incidents are currently shared with you.')).toBeVisible();
  await page.goto(incidentPath);
  await expect(page.getByRole('heading', { name: 'This Incident is unavailable' })).toBeVisible();
  await capture(page, 'incident-unavailable');

  // 3. A adds C under People with access; C can now list and open it.
  await actAs(page, a, incidentPath);
  await page.getByRole('button', { name: 'Manage people with access' }).click();
  const editor = page.getByRole('group', { name: 'People with access' });
  await editor.getByRole('checkbox', { name: 'Development Audience Member' }).check();
  await expect(editor.getByText("The coordinator can't be removed.")).toBeVisible();
  await capture(page, 'incident-people-editor');
  await page.getByRole('button', { name: 'Save people with access' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'People with access saved.' })).toBeVisible();
  expect((await statusOf(request, `/api/incidents/${created.incidentId}`, c.accountId)).status).toBe(200);

  // 4. C adds a finding.
  await actAs(page, c, '/incidents');
  const row = page.getByRole('link', { name: 'Open Incident about Development Recipient, Open' });
  await expect(row).toBeVisible();
  await expect(row).toContainText('Coordinated by');
  await capture(page, 'incident-list-open');
  await row.click();
  await page.getByRole('button', { name: 'Add a finding' }).click();
  await page.getByRole('textbox', { name: 'Information' }).fill('Adult C saw a message from the subject at noon.');
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult C');
  await capture(page, 'incident-finding-composer');
  await page.getByRole('button', { name: 'Add finding' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Finding added.' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Coordination', exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Post a summary update' })).toHaveCount(0);
  await capture(page, 'incident-detail-participant');

  // 4b. A records a Reached contact attempt with Time unknown; the state stays Open.
  await actAs(page, a, incidentPath);
  await page.getByRole('button', { name: 'Record a contact attempt' }).click();
  await expect(page.getByText('Do not enter phone numbers or addresses.')).toBeVisible();
  await page.getByLabel('Person or place contacted').fill('Development Recipient');
  await page.getByLabel('Method').selectOption({ label: 'Call' });
  await page.getByLabel('Outcome').selectOption({ label: 'Reached' });
  await page.getByRole('checkbox', { name: 'Time unknown' }).check();
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult A');
  await page.getByRole('textbox', { name: 'Notes (optional)' }).fill('Brief call; will call back later.');
  await capture(page, 'incident-contact-composer');
  await page.getByRole('button', { name: 'Record contact attempt' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Contact attempt recorded.' })).toBeVisible();
  await expect(page.getByText('Development Adult tried to reach Development Recipient · Call · Reached')).toBeVisible();
  await expect(page.getByText('Open', { exact: true }).first()).toBeVisible();
  expect((await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, a.accountId)).body.state).toBe('Open');
  await expect(page.getByRole('link', { name: /^(Call|Text|Email)/ })).toHaveCount(0);

  // An Unknown for timeline variety.
  await page.getByRole('button', { name: 'Record an Unknown' }).click();
  await page.getByRole('textbox', { name: 'What is unknown' }).fill('Whether the subject reached the hotel.');
  await page.getByRole('checkbox', { name: 'Source is unknown' }).check();
  await page.getByRole('button', { name: 'Record Unknown' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Unknown recorded.' })).toBeVisible();

  // 5. A creates tasks: one for C (completed by C), one for C left open, one for A (cancelled).
  for (const [description, assignee] of [['Call the hotel front desk', 'Development Audience Member'], ['Ask the neighbor', 'Development Audience Member'], ['Check the bus schedule', 'Development Adult (you)']]) {
    await page.getByRole('button', { name: 'Add a task' }).click();
    await page.getByLabel('Task description').fill(description);
    await page.getByLabel('Assign to').selectOption({ label: assignee });
    if (description === 'Call the hotel front desk') await capture(page, 'incident-task-create');
    await page.getByRole('button', { name: 'Add task' }).click();
    await expect(page.getByRole('status').filter({ hasText: 'Task added.' })).toBeVisible();
  }
  await page.getByRole('button', { name: 'Cancel task… Check the bus schedule' }).click();
  await page.getByLabel('Reason for cancelling').fill('Another family member already checked.');
  await page.getByRole('button', { name: 'Cancel task', exact: true }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Task cancelled.' })).toBeVisible();

  await actAs(page, c, incidentPath);
  await expect(page.getByRole('button', { name: /^Cancel task…/ })).toHaveCount(0);
  await page.getByRole('button', { name: 'Complete… Call the hotel front desk' }).click();
  await page.getByLabel('Result').fill('The front desk confirmed a check-in this evening.');
  await page.getByRole('radio', { name: /^Confirmed/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Hotel front desk');
  await page.getByRole('button', { name: 'Complete task' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Task completed.' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'View result for “Call the hotel front desk”' })).toBeVisible();

  // 6. A corrects A's own contact attempt; C adds another report on A's correction. Originals stay.
  await actAs(page, a, incidentPath);
  await page.getByRole('listitem').filter({ hasText: 'tried to reach Development Recipient' })
    .getByRole('button', { name: 'Add a correction' }).click();
  await page.getByRole('textbox', { name: 'Corrected information' }).fill('The call connected only briefly.');
  await page.getByLabel('Explanation').fill('I checked my call log.');
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult A');
  await page.getByRole('button', { name: 'Add correction' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Correction added.' })).toBeVisible();
  await expect(page.getByRole('link', { name: /^Correction to Development Adult's entry from .*: view original$/ })).toBeVisible();

  await actAs(page, c, incidentPath);
  await page.getByRole('listitem').filter({ hasText: 'The call connected only briefly.' })
    .getByRole('button', { name: 'Add another report' }).click();
  await page.getByRole('textbox', { name: 'Your report' }).fill('The subject also texted the family group.');
  await page.getByLabel('Explanation').fill('Adding what I saw separately.');
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult C');
  await capture(page, 'incident-report-composer');
  await page.getByRole('button', { name: 'Add report' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Report added.' })).toBeVisible();
  const afterReports = (await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, a.accountId)).body;
  expect(afterReports.entries.map(entry => entry.text)).toEqual(expect.arrayContaining([
    'Adult C saw a message from the subject at noon.', 'Brief call; will call back later.', 'The call connected only briefly.',
    'The subject also texted the family group.']));
  expect(afterReports.entries.map(entry => entry.kind)).toEqual(expect.arrayContaining(['Correction', 'Challenge']));

  // 7. A (coordinator) view with the full timeline, then hands off to C with explicit confirmation.
  await actAs(page, a, incidentPath);
  await expect(page.getByRole('list', { name: 'Incident timeline, oldest first' })).toBeVisible();
  await capture(page, 'incident-detail-coordinator');
  await page.getByRole('button', { name: 'Hand off coordination…' }).click();
  await page.getByRole('group', { name: 'Hand off coordination to' }).getByRole('radio', { name: 'Development Audience Member' }).check();
  await capture(page, 'incident-handoff-choose');
  await page.getByRole('button', { name: 'Continue' }).click();
  await expect(page.getByRole('heading', { name: 'Hand off to Development Audience Member?' })).toBeFocused();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('group', { name: 'Hand off coordination to' })).toBeVisible();
  await page.getByRole('button', { name: 'Continue' }).click();
  await capture(page, 'incident-handoff-confirm');
  await page.getByRole('button', { name: 'Hand off coordination', exact: true }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Development Audience Member now coordinates this Incident.' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Coordination', exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Manage people with access' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Add a task for yourself' })).toBeVisible();
  const handedOff = (await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, a.accountId)).body;
  expect(handedOff.coordinatorProfileId).toBe(cProfile.profileId);

  // 8. C resolves with an Unknown contact-restored time.
  await actAs(page, c, incidentPath);
  await expect(page.getByRole('heading', { name: 'Coordination', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Resolve this Incident…' }).click();
  await page.getByLabel('Resolution summary').fill('The family coordinated contact and the subject checked in at the hotel.');
  await page.getByRole('textbox', { name: 'Outcome', exact: true }).fill('The subject called the family from the hotel lobby.');
  await page.getByRole('radio', { name: /^Reported/ }).check();
  await page.getByRole('textbox', { name: 'Source description' }).fill('Adult C');
  await page.getByRole('checkbox', { name: 'Unknown', exact: true }).check();
  await page.getByLabel(/^Lessons/).fill('Share the travel itinerary before leaving.');
  await expect(page.getByText("It can't be reopened.", { exact: false })).toBeVisible();
  await capture(page, 'incident-resolve-form');
  await page.getByRole('button', { name: 'Resolve Incident', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Resolution', exact: true })).toBeVisible();
  await expect(page.getByRole('status').filter({ hasText: 'Incident resolved.' })).toBeVisible();

  // 9. Resolved is read-only for both, with no reopen action.
  for (const identity of [c, a]) {
    await actAs(page, identity, incidentPath);
    const resolution = page.getByRole('heading', { name: 'Resolution', exact: true }).locator('xpath=ancestor::section[1]');
    await expect(resolution).toContainText('Contact restored or resolution time Unknown');
    await expect(resolution).toContainText('Resolution means coordination ended.');
    await expect(page.getByText('Not completed', { exact: true })).toBeVisible();
    for (const name of [/Add a/, /Record/, /Post a summary/, /Manage people/, /Hand off/, /Resolve/, /Reopen/, /Complete…/, /Cancel task/, /another report/, /correction/])
      await expect(page.getByRole('button', { name })).toHaveCount(0);
  }
  await capture(page, 'incident-resolved');
  expect((await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, a.accountId)).body.state).toBe('Resolved');

  // 10. A random Incident ID gives the same generic unavailable state as B saw.
  await page.goto(`/incidents/${randomUUID()}`);
  await expect(page.getByRole('heading', { name: 'This Incident is unavailable' })).toBeVisible();
  await expect(page.getByText('Only Incidents currently shared with you can be opened.')).toBeVisible();

  await page.goto('/incidents');
  await expect(page.getByRole('heading', { name: 'Ended coordination (read-only)' })).toBeVisible();
  await capture(page, 'incident-list-ended');

  // Frozen cannot be produced in E2E (30-day inactivity or coordinator loss). Render it from an
  // intercepted copy of this real detail response; labeled "mocked" in VER-WI-008-004.
  await page.route(`**/api/incidents/${created.incidentId}`, async route => {
    const response = await route.fetch();
    const frozen = await response.json() as IncidentView & { resolution: unknown };
    frozen.state = 'Frozen';
    frozen.resolution = null;
    frozen.frozenAt = new Date().toISOString();
    frozen.freezeReason = 'Inactivity';
    frozen.timeline = frozen.timeline.map(item => item.type === 'Resolved' ? { ...item, type: 'Frozen' } : item);
    await route.fulfill({ response, json: frozen });
  });
  await page.goto(incidentPath);
  await expect(page.getByText('Coordination in this Incident is no longer editable.', { exact: false })).toBeVisible();
  await expect(page.getByText('Inactivity')).toHaveCount(0);
  await capture(page, 'incident-frozen-mocked');
  await page.unrouteAll();

  await page.goto('/home');
  await expect(page.getByRole('heading', { name: 'Safety coordination' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Open Incidents' })).toBeVisible();
  await capture(page, 'home-safety-coordination');
});

test('Journey 2 — Start an Incident from a Concern: nothing transferred, Concern unchanged, lineage only when authorized to both', async ({ page, request }) => {
  test.setTimeout(300_000);
  const { a, b, c, family, bProfile, cProfile } = await setUp(page, request);

  // 1. A Concern by A about B, with C selected and one entry. Snapshot its API view.
  const concernReason = `Concern-only reason ${Date.now()}`;
  const concern = await send(request, 'post', `/api/family-groups/${family.familyGroupId}/concerns`, a.accountId,
    { subjectProfileId: bProfile.profileId, reason: concernReason, participantProfileIds: [cProfile.profileId] });
  expect(concern.status).toBe(201);
  const concernId = (concern.body as { concernId: string }).concernId;
  expect((await send(request, 'post', `/api/concerns/${concernId}/entries`, a.accountId, entryRequest('Concern-only observation text'))).status).toBe(201);
  const before = (await apiJson<object>(request, `/api/concerns/${concernId}`, a.accountId)).body;

  // 2. A opens the Concern and chooses Start an Incident….
  await actAs(page, a, '/concerns');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Open/ }).filter({ hasText: concernReason }).click();
  const separate = page.getByRole('heading', { name: 'Separate Incident' }).locator('xpath=ancestor::section[1]');
  await expect(separate).toContainText('Nothing here is copied or shared.');
  await capture(page, 'concern-separate-incident-card');
  await separate.getByRole('button', { name: 'Start an Incident…' }).click();
  await expect(page).toHaveURL(/\/incidents\/new$/);
  expect(page.url()).not.toContain(concernId);
  expect(page.url()).not.toContain('?');
  await expect(page.getByRole('heading', { name: 'Starting a separate Incident' })).toBeVisible();
  const subject = page.getByLabel('Who is this Incident about?');
  await expect(subject).toHaveValue(bProfile.profileId);
  await subject.selectOption(cProfile.profileId);
  await subject.selectOption(bProfile.profileId);
  await expect(page.getByLabel('Why are you starting this Incident?')).toHaveValue('');
  const people = page.getByRole('group', { name: 'People with access' });
  for (const box of await people.getByRole('checkbox').all()) await expect(box).not.toBeChecked();
  await expect(people.getByRole('checkbox', { name: 'Development Audience Member' })).not.toBeChecked();
  await expect(page.getByText(concernReason)).toHaveCount(0);
  await capture(page, 'incident-create-from-concern');
  const incidentReason = 'Coordinating a separate family follow-up.';
  await page.getByLabel('Why are you starting this Incident?').fill(incidentReason);
  await page.getByRole('radio', { name: /^Assumption/ }).check();
  await page.getByRole('checkbox', { name: 'Source is unknown' }).check();
  const createResponse = page.waitForResponse(response => response.url().endsWith(`/api/family-groups/${family.familyGroupId}/incidents`)
    && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Incident', exact: true }).click();
  const createdResponse = await createResponse;
  expect(createdResponse.status()).toBe(201);
  expect(createdResponse.request().postDataJSON()).toEqual({
    subjectProfileId: bProfile.profileId, reason: incidentReason, reasonCategory: 'Assumption', reasonSourceDescription: null,
    reasonSourceUnknown: true, participantProfileIds: [], originatingConcernId: concernId,
  });
  const created = await createdResponse.json() as IncidentView;
  const incidentPath = `/incidents/${created.incidentId}`;
  await expect(page.getByRole('heading', { name: 'Incident about Development Recipient', exact: true })).toBeVisible();
  await expect(page.getByText('Started from a Concern')).toBeVisible();
  await capture(page, 'incident-detail-lineage');

  // 3. No audience transferred, nothing copied, Concern unchanged.
  expect((await statusOf(request, `/api/incidents/${created.incidentId}`, c.accountId)).status).toBe(404);
  expect((await apiJson<IncidentView[]>(request, '/api/incidents', c.accountId)).body.map(row => row.incidentId)).not.toContain(created.incidentId);
  await actAs(page, c, incidentPath);
  await expect(page.getByRole('heading', { name: 'This Incident is unavailable' })).toBeVisible();
  const view = (await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, a.accountId)).body;
  expect(view.entries.map(entry => [entry.kind, entry.text])).toEqual([['Summary', incidentReason]]);
  expect(view.timeline.map(item => item.type)).toEqual(['IncidentCreated', 'Entry']);
  expect(view.participants).toHaveLength(1);
  expect((await apiJson<object>(request, `/api/concerns/${concernId}`, a.accountId)).body).toEqual(before);

  // 4. A opens the Concern from the lineage link.
  await actAs(page, a, incidentPath);
  await page.getByRole('link', { name: 'Open the Concern' }).click();
  // WI-009-005 §8: the lineage link is the Concern's own URL (it was /concerns plus navigation state).
  await expect(page).toHaveURL(new RegExp(`/concerns/${concernId}$`));
  await expect(page.getByRole('heading', { name: 'Concern about Development Recipient' })).toBeVisible();
  await expect(page.getByText(concernReason)).toBeVisible();

  // 5. A adds C and B. C (both accesses) sees lineage; B (Incident only) sees none, and the API returns null.
  expect((await send(request, 'put', `/api/incidents/${created.incidentId}/participants`, a.accountId,
    { participantProfileIds: [cProfile.profileId, bProfile.profileId] })).status).toBe(200);
  expect((await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, c.accountId)).body.originatingConcernId).toBe(concernId);
  await actAs(page, c, incidentPath);
  await expect(page.getByText('Started from a Concern')).toBeVisible();
  await expect(page.getByRole('link', { name: 'Open the Concern' })).toBeVisible();
  expect((await apiJson<IncidentView>(request, `/api/incidents/${created.incidentId}`, b.accountId)).body.originatingConcernId).toBeNull();
  await actAs(page, b, incidentPath);
  await expect(page.getByRole('heading', { name: 'Incident about Development Recipient', exact: true })).toBeVisible();
  await expect(page.getByText('Started from a Concern')).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Open the Concern' })).toHaveCount(0);
  await expect(page.getByText(/Concern/)).toHaveCount(0);
  await capture(page, 'incident-detail-no-lineage');
  expect((await apiJson<object>(request, `/api/concerns/${concernId}`, a.accountId)).body).toEqual(before);

  // Loading /incidents/new without navigation state is plain direct creation.
  await actAs(page, a, '/incidents/new');
  await expect(page.getByRole('heading', { name: 'Start an Incident' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Starting a separate Incident' })).toHaveCount(0);
  await expect(page.getByLabel('Who is this Incident about?')).toHaveValue('');
});
