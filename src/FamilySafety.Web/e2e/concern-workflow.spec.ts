import { expect, test } from '@playwright/test';
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

// Fresh database and API for this file; Adult C is the selected Concern participant.
useIsolatedScenario({ tripAudienceAdult: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }
interface FamilyMember { profileId: string; displayName: string }
interface ConcernView {
  concernId: string;
  creatorProfileId: string;
  subjectProfileId: string;
  state: 'Open' | 'Frozen' | 'Resolved';
  participants: FamilyMember[];
  entries: { text: string; contributorProfileId: string }[];
  resolutionNote: string | null;
}

test('Adult A creates a selected-audience Concern and Adult C contributes to resolved history', async ({ page, request }) => {
  await recordScenario(test.info());
  await expectHealthyApi(request);

  const identityResponse = await request.get(`${apiBaseUrl}/api/development/identities`);
  expect(identityResponse.ok()).toBeTruthy();
  const identityData = await identityResponse.json() as { defaultAccountId: string; identities: DevelopmentIdentity[] };
  const adultA = identityData.identities.find(identity => identity.label.startsWith('Adult A'));
  const adultB = identityData.identities.find(identity => identity.label.startsWith('Adult B'));
  const adultC = identityData.identities.find(identity => identity.label.startsWith('Adult C'));
  expect(adultA).toBeDefined();
  expect(adultB).toBeDefined();
  expect(adultC).toBeDefined();
  expect(identityData.defaultAccountId).toBe(adultA!.accountId);
  // Transient captures (WI-009-002): routine runs never write canonical evidence under ai-context/.
  const evidenceDirectory = captureDirectory('concern-workflow');
  mkdirSync(evidenceDirectory, { recursive: true });
  await page.setViewportSize(desktopViewport);
  const capture = async (name: string) => {
    await page.screenshot({ path: resolve(evidenceDirectory, `${name}-desktop.png`), fullPage: true });
    await page.setViewportSize(narrowViewport);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(narrowViewport.width);
    await page.screenshot({ path: resolve(evidenceDirectory, `${name}-narrow.png`), fullPage: true });
    await page.setViewportSize(desktopViewport);
  };

  const aGroups = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultA!.accountId);
  expect(aGroups.status).toBe(200);
  expect(aGroups.body).toHaveLength(1);
  const family = aGroups.body[0];
  const cGroups = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultC!.accountId);
  expect(cGroups.body.map(group => group.familyGroupId)).toContain(family.familyGroupId);

  const bGroupsBefore = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId);
  if (!bGroupsBefore.body.some(group => group.familyGroupId === family.familyGroupId)) {
    const issue = await request.post(`${apiBaseUrl}/api/membership-invitations`, {
      headers: { [developmentAccountHeader]: adultA!.accountId },
      data: { familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid' },
    });
    expect(issue.status()).toBe(202);
    const outboxResponse = await request.get(`${apiBaseUrl}/api/development/invitation-outbox`, {
      headers: { [developmentAccountHeader]: adultA!.accountId },
    });
    expect(outboxResponse.ok()).toBeTruthy();
    const outbox = await outboxResponse.json() as { invitationUrl: string }[];
    const token = uriToken(outbox.at(-1)!.invitationUrl);
    const accepted = await request.post(`${apiBaseUrl}/api/membership-invitations/accept`, {
      headers: { [developmentAccountHeader]: adultB!.accountId },
      data: { token },
    });
    expect(accepted.status()).toBe(200);
  }

  const aMembersResponse = await request.get(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/members`, {
    headers: { [developmentAccountHeader]: adultA!.accountId },
  });
  expect(aMembersResponse.ok()).toBeTruthy();
  const familyMembers = await aMembersResponse.json() as FamilyMember[];
  const bProfile = familyMembers.find(member => member.displayName === 'Development Recipient');
  const cProfile = familyMembers.find(member => member.displayName === 'Development Audience Member');
  expect(bProfile).toBeDefined();
  expect(cProfile).toBeDefined();

  await page.goto('/concerns');
  const activeIdentity = page.getByLabel('Active test identity');
  await expect(activeIdentity).toHaveValue(adultA!.accountId);
  await expect(page.getByRole('heading', { name: 'Concerns', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Create a Concern' }).click();
  await page.getByLabel('Who is this Concern about?').selectOption(bProfile!.profileId);
  const reason = `Coordination note ${Date.now()}`;
  await page.getByLabel('Reason or context').fill(reason);
  const audience = page.getByRole('group', { name: 'Who should have access?' });
  const subjectChoice = audience.getByRole('checkbox', { name: /Development Recipient/ });
  const cChoice = audience.getByRole('checkbox', { name: /Development Audience Member/ });
  await expect(subjectChoice).not.toBeChecked();
  await cChoice.check();
  await expect(subjectChoice).not.toBeChecked();
  await expect(audience).toContainText('The subject is not included unless you select them');
  await capture('concern-create');

  const creationResponsePromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/family-groups/${family.familyGroupId}/concerns`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create Concern', exact: true }).click();
  const creationResponse = await creationResponsePromise;
  expect(creationResponse.status()).toBe(201);
  const created = await creationResponse.json() as ConcernView;
  expect(created.subjectProfileId).toBe(bProfile!.profileId);
  expect(created.participants.map(person => person.profileId)).toContain(cProfile!.profileId);
  expect(created.participants.map(person => person.profileId)).not.toContain(bProfile!.profileId);
  await expect(page.getByRole('heading', { name: 'Concern about Development Recipient' })).toBeVisible();
  await capture('concern-open-new');

  const bDetail = await request.get(`${apiBaseUrl}/api/concerns/${created.concernId}`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  expect(bDetail.status()).toBe(404);
  await activeIdentity.selectOption(adultB!.accountId);
  await page.goto('/concerns');
  await expect(page.getByText('No Concerns are currently available to you')).toBeVisible();
  await expect(page.getByText(reason, { exact: true })).toHaveCount(0);
  await capture('concern-list-empty');

  await activeIdentity.selectOption(adultC!.accountId);
  await page.goto('/concerns');
  await expect(page.getByRole('link', { name: /Open Concern about Development Recipient, Open/ })).toBeVisible();
  await capture('concern-list-populated');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Open/ }).click();
  await expect(page.getByRole('heading', { name: 'Concern about Development Recipient' })).toBeVisible();
  await page.getByRole('button', { name: 'Add an observation' }).click();
  await page.getByRole('textbox', { name: 'Information' }).fill('Adult C spoke with the subject this afternoon.');
  await page.getByLabel('Source description').fill('Adult C');
  await capture('concern-observation-composer');
  const contributionPromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/concerns/${created.concernId}/entries`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Add contribution' }).click();
  const contribution = await contributionPromise;
  expect(contribution.status()).toBe(201);
  await page.getByRole('button', { name: 'Add a correction' }).click();
  await page.getByRole('textbox', { name: 'Information' }).fill('Adult C spoke with the subject this morning, not this afternoon.');
  await page.getByLabel('Source description').fill('Adult C');
  await page.getByLabel('Explanation').fill('I checked my call log.');
  await capture('concern-correction-composer');
  const correctionPromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/concerns/${created.concernId}/entries`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Add contribution' }).click();
  expect((await correctionPromise).status()).toBe(201);
  await expect(page.getByText('Correction to original entry')).toBeVisible();

  // Render the Frozen read-only state from the authorized response shape without changing the server record.
  await page.route(`**/api/concerns/${created.concernId}`, async route => {
    const response = await route.fetch();
    const frozen = await response.json() as ConcernView & { frozenAt: string | null; freezeReason: string | null };
    frozen.state = 'Frozen';
    frozen.frozenAt = new Date().toISOString();
    frozen.freezeReason = 'Inactivity';
    await route.fulfill({ response, json: frozen });
  });
  await page.route('**/api/concerns', async route => {
    const response = await route.fetch();
    const rows = await response.json() as (ConcernView & { frozenAt: string | null; freezeReason: string | null })[];
    for (const row of rows) {
      if (row.concernId === created.concernId) {
        row.state = 'Frozen';
        row.frozenAt = new Date().toISOString();
        row.freezeReason = 'Inactivity';
      }
    }
    await route.fulfill({ response, json: rows });
  });
  await page.goto('/concerns');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Frozen/ }).click();
  await expect(page.getByText('Start a new Concern if coordination needs to continue.')).toBeVisible();
  await capture('concern-frozen');
  await page.unrouteAll();

  await activeIdentity.selectOption(adultA!.accountId);
  await page.goto('/concerns');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Open/ }).click();
  await page.getByRole('button', { name: 'Add another report' }).first().click();
  await page.getByRole('textbox', { name: 'Information' }).fill('The subject also replied to a family message.');
  await page.getByLabel('Source description').fill('Adult A');
  await page.getByLabel('Explanation').fill('Adding what I saw separately.');
  await capture('concern-challenge-composer');
  const challengePromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/concerns/${created.concernId}/entries`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Add contribution' }).click();
  expect((await challengePromise).status()).toBe(201);
  await expect(page.getByText('Another report about this entry')).toBeVisible();
  await capture('concern-open-history');
  await page.getByRole('button', { name: 'Manage participants' }).click();
  await expect(page.getByRole('group', { name: 'Selected participants' })).toBeVisible();
  await capture('concern-participants-manager');
  await page.getByRole('button', { name: 'Cancel' }).click();
  await page.getByRole('button', { name: 'Resolve this Concern…' }).click();
  await page.getByLabel('Resolution note').fill('The planned coordination is complete.');
  await capture('concern-resolve-form');
  const resolutionPromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/concerns/${created.concernId}/resolve`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Resolve Concern' }).click();
  const resolution = await resolutionPromise;
  expect(resolution.status()).toBe(200);

  await activeIdentity.selectOption(adultC!.accountId);
  await page.goto('/concerns');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Resolved/ }).click();
  await expect(page.getByText('Resolved', { exact: true })).toBeVisible();
  await expect(page.getByText('Adult C spoke with the subject this afternoon.', { exact: true })).toBeVisible();
  await expect(page.getByText(/Resolution means coordination on this record ended/)).toBeVisible();
  await capture('concern-resolved');
  await expect(page.getByRole('button', { name: 'Add an observation' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Manage participants' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Resolve Concern' })).toHaveCount(0);

  const cDetail = await request.get(`${apiBaseUrl}/api/concerns/${created.concernId}`, {
    headers: { [developmentAccountHeader]: adultC!.accountId },
  });
  expect(cDetail.status()).toBe(200);
  const resolved = await cDetail.json() as ConcernView;
  expect(resolved.state).toBe('Resolved');
  expect(resolved.entries.map(entry => entry.contributorProfileId)).toContain(cProfile!.profileId);
  expect(resolved.resolutionNote).toBe('The planned coordination is complete.');

  // Render the generic unavailable state from an intercepted 404; the UI must not explain why.
  await page.route(`**/api/concerns/${created.concernId}`, route => route.fulfill({ status: 404, json: {} }));
  await page.goto('/concerns');
  await page.getByRole('link', { name: /Open Concern about Development Recipient, Resolved/ }).click();
  await expect(page.getByRole('heading', { name: 'This Concern is unavailable' })).toBeVisible();
  await capture('concern-unavailable');
  await page.unrouteAll();

  await page.route('**/api/concerns', route => route.fulfill({ status: 503, json: { message: 'internal test-only failure payload' } }));
  await page.goto('/concerns');
  await expect(page.getByText('Concerns could not be loaded. Please try again.')).toBeVisible();
  await expect(page.getByText('internal test-only failure payload', { exact: true })).toHaveCount(0);
  await capture('concern-list-error');
  await page.unrouteAll();

  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Safety coordination' })).toBeVisible();
  await capture('home-concern-entry');
});

const desktopViewport = { width: 1440, height: 1000 };
const narrowViewport = { width: 390, height: 844 };

function uriToken(invitationUrl: string): string {
  const token = invitationUrl.split('#invite=')[1];
  expect(token).toBeTruthy();
  return decodeURIComponent(token!);
}
