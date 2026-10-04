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

// Fresh database and API for this file: Adults A and B only, with B not yet a member.
useIsolatedScenario({ tripAudienceAdult: false });

interface DevelopmentIdentity {
  accountId: string;
  label: string;
}

interface FamilyGroup {
  familyGroupId: string;
  name: string;
}

// WI-009-010 (DEC-080): the approved first-use wording and the copy it replaced, which must not reappear beneath it.
const homeNoFamily = 'You’re not part of a Family yet. Ask a Family administrator to send you an invitation. '
  + 'While you wait, you can review your profile and sharing choices.';
const sharedNoFamily = 'This area needs an active Family. Ask a Family administrator to send you an invitation.';
const innerNoFamily = /Family access begins|do not currently have an active family group/i;
const selfServiceFamily = /Create (a )?Family|Find Family|Join code|Request (to join|access)|Search Families/i;
const captures = captureDirectory('first-use');

async function expectNoOverflow(page: Page, where: string): Promise<void> {
  const width = page.viewportSize()!.width;
  expect(await page.evaluate(() => document.documentElement.scrollWidth), where).toBeLessThanOrEqual(width);
}

/** Full-page captures of the current page at each width, checked for horizontal overflow. Ends at 1440. */
async function capture(page: Page, name: string, widths: number[]): Promise<void> {
  for (const width of widths) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    await expectNoOverflow(page, `${name} ${width}`);
    await page.screenshot({ path: resolve(captures, `${name}-${width}.png`), fullPage: true });
  }
  await page.setViewportSize({ width: 1440, height: 1000 });
}

async function preview(request: APIRequestContext, token: string, accountId: string): Promise<{ status: number; body: string }> {
  const response = await request.post(`${apiBaseUrl}/api/membership-invitations/preview`, {
    headers: { [developmentAccountHeader]: accountId }, data: { token },
  });
  return { status: response.status(), body: await response.text() };
}

test('Adult B accepts an invitation without inheriting Adult A state or access', async ({ page, request }) => {
  test.setTimeout(120_000);
  await recordScenario(test.info());
  await expectHealthyApi(request);
  console.log('API /health: Healthy');

  const identitiesResponse = await request.get(`${apiBaseUrl}/api/development/identities`);
  expect(identitiesResponse.ok()).toBeTruthy();
  const identityOptions = await identitiesResponse.json() as {
    defaultAccountId: string;
    identities: DevelopmentIdentity[];
  };
  const adultA = identityOptions.identities.find(identity => identity.label.startsWith('Adult A'));
  const adultB = identityOptions.identities.find(identity => identity.label.startsWith('Adult B'));
  expect(adultA).toBeDefined();
  expect(adultB).toBeDefined();
  expect(identityOptions.defaultAccountId).toBe(adultA!.accountId);

  const aMemberships = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultA!.accountId);
  expect(aMemberships.status).toBe(200);
  expect(aMemberships.body).toHaveLength(1);
  const family = aMemberships.body[0];
  expect(family.name).toBe('Development Family');

  const aAdministered = await apiJson<FamilyGroup[]>(request, '/api/family-groups/administered', adultA!.accountId);
  expect(aAdministered.status).toBe(200);
  expect(aAdministered.body.map(group => group.familyGroupId)).toContain(family.familyGroupId);
  const aProfile = await apiJson<{ profileId: string; displayName: string }>(request, '/api/profile', adultA!.accountId);
  expect(aProfile.status).toBe(200);
  expect(aProfile.body.displayName).toBe('Development Adult');

  const [events, plans, trips] = await Promise.all([
    apiJson<unknown[]>(request, `/api/family-groups/${family.familyGroupId}/events`, adultA!.accountId),
    apiJson<unknown[]>(request, `/api/family-groups/${family.familyGroupId}/plans`, adultA!.accountId),
    apiJson<unknown[]>(request, '/api/trips', adultA!.accountId),
  ]);
  expect(events.status).toBe(200);
  expect(events.body).toEqual([]);
  expect(plans.status).toBe(200);
  expect(plans.body).toEqual([]);
  expect(trips.status).toBe(200);
  expect(trips.body).toEqual([]);

  const bMembershipsBeforeAccept = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId);
  expect(bMembershipsBeforeAccept.status).toBe(200);
  expect(bMembershipsBeforeAccept.body).toEqual([]);
  const bAdministeredBeforeAccept = await apiJson<FamilyGroup[]>(request, '/api/family-groups/administered', adultB!.accountId);
  expect(bAdministeredBeforeAccept.status).toBe(200);
  expect(bAdministeredBeforeAccept.body).toEqual([]);
  const bProfileBeforeAccept = await apiJson<{
    displayName: string;
    contactEmail: string | null;
    contactEmailVisibility: string;
  }>(request, '/api/profile', adultB!.accountId);
  expect(bProfileBeforeAccept.status).toBe(200);
  expect(bProfileBeforeAccept.body.displayName).toBe('Development Recipient');
  expect(bProfileBeforeAccept.body.contactEmail).toBeNull();
  expect(bProfileBeforeAccept.body.contactEmailVisibility).toBe('OnlyMe');
  const bMembersBeforeAccept = await request.get(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/members`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  expect(bMembersBeforeAccept.status()).toBe(404);
  const bAProfileBeforeAccept = await request.get(`${apiBaseUrl}/api/family-members/${aProfile.body.profileId}/profile`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  expect(bAProfileBeforeAccept.status()).toBe(404);

  await page.goto('/');
  const activeIdentity = page.getByLabel('Active test identity');
  await expect(activeIdentity).toBeVisible();
  await expect(activeIdentity).toHaveValue(adultA!.accountId);
  console.log('Angular ready: http://localhost:4200');
  await expect(page.getByRole('link', { name: 'Family', exact: true })).toBeVisible();

  await page.getByRole('link', { name: 'Family', exact: true }).click();
  // WI-009-007: invitation administration is in Family settings, reached by the gear every member sees.
  await page.getByRole('link', { name: 'Family settings', exact: true }).click();
  await expect(page).toHaveURL(/\/family\/settings$/);
  await expect(page.getByRole('heading', { name: 'Invite an adult family member' })).toBeVisible();
  await page.getByLabel('Recipient email').fill('development-recipient@example.invalid');
  await page.getByRole('button', { name: 'Send or resend invitation' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Invitation sent.' })).toBeVisible();
  // WI-009-010 O7: administrator feedback sits in the Invite card, once.
  await expect(page.locator('form.family-admin-panel .invite-feedback')).toHaveText('Invitation sent.');
  await expect(page.getByText('Invitation sent.')).toHaveCount(1);

  await page.getByRole('button', { name: 'Show Development invitation links' }).click();
  const initialOutboxLink = page.getByRole('link', { name: 'Open invitation' });
  await expect(initialOutboxLink).toBeVisible();
  await expect(page.getByText('development-recipient@example.invalid')).toBeVisible();
  const firstInvitationUrl = await initialOutboxLink.getAttribute('href');
  expect(firstInvitationUrl).toContain('#invite=');

  const browserBMembershipResponse = page.waitForResponse(response =>
    response.url().endsWith('/api/family-groups/mine') && response.request().method() === 'GET');
  await activeIdentity.selectOption({ label: adultB!.label });
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  // WI-009-010 O4: the approved no-Family Home, with one real action and no Family creation.
  await expect(page).toHaveURL(/\/home$/);
  const noFamilyHome = page.locator('section.home-no-family');
  await expect(noFamilyHome.locator('p')).toHaveText(homeNoFamily);
  await expect(noFamilyHome.locator('a, button')).toHaveText(['Review your profile']);
  await expect(page.getByRole('main')).not.toContainText(selfServiceFamily);
  await expect(page.getByText(family.name, { exact: true })).toHaveCount(0);
  const browserBMembership = await browserBMembershipResponse;
  expect(browserBMembership.request().headers()[developmentAccountHeader.toLowerCase()]).toBe(adultB!.accountId);
  expect(await browserBMembership.json()).toEqual([]);
  await capture(page, 'home-no-family', [1440, 390]);

  // WI-009-010 O11: one Family-required message on each page that needs a Family; no inner no-Family copy beneath it.
  for (const path of ['/family', '/events', '/plans']) {
    await page.goto(path);
    await expect(activeIdentity).toHaveValue(adultB!.accountId);
    await expect(page.locator('section.context-prompt').filter({ visible: true })).toHaveCount(1);
    await expect(page.locator('section.context-prompt p')).toHaveText(sharedNoFamily);
    await expect(page.locator('section.context-prompt a')).toHaveText(['Review your profile']);
    await expect(page.getByRole('main').locator('p').filter({ visible: true }).filter({ hasText: innerNoFamily })).toHaveCount(0);
    await expect(page.getByRole('main').getByText('needs an active Family')).toHaveCount(1);
    await expect(page.getByRole('main')).not.toContainText(selfServiceFamily);
  }
  await page.goto('/family');
  await expect(page.locator('section.context-prompt p')).toHaveText(sharedNoFamily);
  await expect(page.getByRole('heading', { name: 'Family members', exact: true })).toBeHidden();
  await capture(page, 'family-required-single-message', [1440, 390]);
  await page.locator('section.context-prompt').getByRole('link', { name: 'Review your profile' }).click();
  await expect(page).toHaveURL(/\/me$/);
  await expect(page.getByRole('heading', { level: 1, name: 'Your profile' })).toBeFocused();
  await page.goto('/home');
  await noFamilyHome.getByRole('link', { name: 'Review your profile' }).click();
  await expect(page).toHaveURL(/\/me$/);
  await expect(page.locator('#display-name')).toHaveValue('Development Recipient');

  // D. Decline; then the same link is unavailable (a real server outcome). Both offer Go to Home (O5).
  await page.getByRole('button', { name: 'Show Development invitation links' }).click();
  const invitationLink = page.getByRole('link', { name: 'Open invitation' });
  await expect(invitationLink).toBeVisible();
  const invitationNavigation = page.waitForEvent('load');
  await invitationLink.click();
  await invitationNavigation;
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  await expect(page.getByRole('heading', { name: 'Family invitation' })).toBeVisible();
  const invitationResponse = page.getByRole('region', { name: 'Invitation response' });
  // Privacy: the preview shows only the inviter's display name and the Family name.
  await expect(invitationResponse.locator('p').first()).toHaveText(`Development Adult invited you to join ${family.name}.`);
  await expect(invitationResponse).not.toContainText(/@|Development Recipient|members?\b|capacity|administrator/i);
  await expect(page.getByRole('button', { name: 'Accept invitation' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Decline invitation' })).toBeVisible();
  await expect(page.getByRole('navigation', { name: 'Main navigation' })).toHaveCount(0);
  await expect(page.getByText(`Current Family: ${family.name}`, { exact: false })).toHaveCount(0);
  await page.setViewportSize({ width: 390, height: 844 });
  await expectNoOverflow(page, 'invitation preview 390');
  await page.getByRole('button', { name: 'Decline invitation' }).click();
  await expect(invitationResponse.getByRole('status')).toHaveText('Invitation declined.');
  await expect(invitationResponse.getByRole('button')).toHaveText(['Go to Home']);
  await expectNoOverflow(page, 'declined 390');
  await page.screenshot({ path: resolve(captures, 'invitation-declined-390.png'), fullPage: true });
  await invitationResponse.getByRole('button', { name: 'Go to Home' }).click();
  await expect(page).toHaveURL(/\/home$/);
  await expect(noFamilyHome).toBeVisible();
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId)).body).toEqual([]);

  // C. Unavailable: the declined link and a made-up token look the same, and neither says why.
  await page.goto(firstInvitationUrl!);
  await expect(invitationResponse.getByRole('alert')).toHaveText('This invitation is unavailable.');
  await expect(invitationResponse.getByRole('button')).toHaveText(['Go to Home']);
  const unavailableText = await invitationResponse.innerText();
  await capture(page, 'invitation-unavailable', [390, 1440]);
  await page.goto('/#invite=not-a-real-invitation');
  await expect(invitationResponse.getByRole('alert')).toHaveText('This invitation is unavailable.');
  expect(await invitationResponse.innerText()).toBe(unavailableText);
  await expect(invitationResponse).not.toContainText(/declined|expired|revoked|already|member|used/i);
  await invitationResponse.getByRole('button', { name: 'Go to Home' }).click();
  await expect(page).toHaveURL(/\/home$/);
  await expect(noFamilyHome).toBeVisible();
  const firstToken = decodeURIComponent(firstInvitationUrl!.split('#invite=')[1]);
  const declinedPreview = await preview(request, firstToken, adultB!.accountId);
  expect(declinedPreview.status).toBe(404);
  expect(declinedPreview).toEqual(await preview(request, 'not-a-real-invitation', adultB!.accountId));

  // A resends; B opens the new invitation.
  await activeIdentity.selectOption({ label: adultA!.label });
  await expect(activeIdentity).toHaveValue(adultA!.accountId);
  await page.goto('/family/settings');
  await page.getByLabel('Recipient email').fill('development-recipient@example.invalid');
  await page.getByRole('button', { name: 'Send or resend invitation' }).click();
  await expect(page.locator('form.family-admin-panel .invite-feedback')).toHaveText('Invitation sent.');
  const outbox = await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', adultA!.accountId);
  const encodedToken = outbox.body.at(-1)!.invitationUrl.split('#invite=')[1];
  const token = decodeURIComponent(encodedToken);
  expect(token).not.toBe(firstToken);
  await activeIdentity.selectOption({ label: adultB!.label });
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  await page.goto(`/#invite=${encodedToken}`);
  await expect(page.getByRole('heading', { name: 'Family invitation' })).toBeVisible();
  await expect(invitationResponse).toContainText(family.name);

  // E. Capacity: no scenario fixture fills a Family, so the server's documented 409 is answered in the browser only.
  await page.route('**/api/membership-invitations/accept', route => route.fulfill({
    status: 409, json: { message: 'The family group has reached its capacity.' } }));
  await page.getByRole('button', { name: 'Accept invitation' }).click();
  await expect(invitationResponse.getByRole('alert')).toHaveText('This family group has reached its capacity.');
  await expect(invitationResponse.getByRole('button')).toHaveText(['Accept invitation', 'Decline invitation', 'Go to Home']);
  await expectNoOverflow(page, 'capacity 390');
  await page.unroute('**/api/membership-invitations/accept');
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId)).body).toEqual([]);
  await page.setViewportSize({ width: 1440, height: 1000 });

  // A. Accept → the confirmed membership → profile review → Continue to Home without changes → /home, announced once.
  let postAcceptDocumentLoads = 0;
  page.on('load', () => { postAcceptDocumentLoads++; });
  const tokenUses: string[] = [];
  page.on('request', sent => {
    if (sent.url().includes(encodedToken) || (sent.postData() ?? '').includes(token)) tokenUses.push(sent.url());
  });
  const acceptanceResponsePromise = page.waitForResponse(response =>
    response.url().endsWith('/api/membership-invitations/accept'));
  const membershipRefreshPromise = page.waitForResponse(response =>
    response.url().endsWith('/api/family-groups/mine') && response.request().method() === 'GET');
  await page.getByRole('button', { name: 'Accept invitation' }).click();
  const acceptanceResponse = await acceptanceResponsePromise;
  expect(acceptanceResponse.status()).toBe(200);
  const membershipRefreshResponse = await membershipRefreshPromise;
  expect(membershipRefreshResponse.status()).toBe(200);
  expect((await membershipRefreshResponse.json() as FamilyGroup[])
    .map(group => group.familyGroupId)).toContain(family.familyGroupId);
  await expect(page.getByRole('heading', { name: 'Review your profile' })).toBeVisible();
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  expect(postAcceptDocumentLoads).toBe(0);
  await expect(page.getByRole('heading', { name: 'Family members', exact: true })).toBeHidden();

  const bMembershipsAfterAccept = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId);
  expect(bMembershipsAfterAccept.status).toBe(200);
  expect(bMembershipsAfterAccept.body.map(group => group.familyGroupId)).toContain(family.familyGroupId);
  // Acceptance grants membership only: B administers nothing.
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/administered', adultB!.accountId)).body).toEqual([]);
  await expect(page.getByRole('button', { name: 'Continue to Home without changes' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Continue to Home', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Continue to Home without changes' }).click();
  await expect(page).toHaveURL(/\/home$/);
  await expect(page.locator('p.family-switch-status')).toHaveText(`You joined ${family.name}.`);
  await expect(page.getByText('You joined')).toHaveCount(1);
  await expect(page.getByRole('heading', { level: 1, name: /^Good (morning|afternoon|evening)/ })).toBeFocused();
  await expect(page.locator('.home-title')).toContainText(`happening with ${family.name} today`);
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  await expectNoOverflow(page, 'home arrival 1440');
  await page.screenshot({ path: resolve(captures, 'home-arrival-1440.png'), fullPage: true });
  expect(postAcceptDocumentLoads).toBe(0);
  // The invitation token is used for acceptance only, never after it.
  expect(tokenUses).toHaveLength(1);
  expect(tokenUses[0]).toMatch(/\/api\/membership-invitations\/accept$/);
  expect(page.url()).not.toContain('invite=');

  // The announcement is one-time: a refresh does not repeat it.
  await page.reload();
  await expect(page.locator('.home-title')).toContainText(`happening with ${family.name} today`);
  await expect(page.locator('p.family-switch-status')).toHaveText('');
  await expect(page.getByText('You joined')).toHaveCount(0);

  // WI-009-007: B is an active member but not an administrator: Settings shows membership, never Invite.
  await page.getByRole('link', { name: 'Family', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Family members', exact: true })).toBeVisible();
  await page.getByRole('link', { name: 'Family settings', exact: true }).click();
  await expect(page.getByRole('heading', { level: 1, name: 'Family settings' })).toBeVisible();
  await expect(page.getByText('You are an active member.', { exact: false })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Family relationships' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Invite an adult family member' })).toHaveCount(0);
  await expect(page.getByLabel('Recipient email')).toHaveCount(0);
  await page.getByRole('link', { name: 'Back to Family' }).click();
  await expect(page).toHaveURL(/\/family$/);
  await expect(activeIdentity).toHaveValue(adultB!.accountId);

  await page.getByRole('link', { name: 'Home', exact: true }).click();
  await expect(page.getByRole('heading', { level: 1, name: /^Good (morning|afternoon|evening)/ })).toBeVisible();
  await expect(page.locator('.home-title')).toContainText(`happening with ${family.name} today`);
  await expect(page.getByText('You joined')).toHaveCount(0);
  await expect(activeIdentity).toHaveValue(adultB!.accountId);

  // Already a member: the used link is unavailable exactly like any other unavailable invitation.
  await page.goto(`/#invite=${encodedToken}`);
  await expect(invitationResponse.getByRole('alert')).toHaveText('This invitation is unavailable.');
  expect(await invitationResponse.innerText()).toBe(unavailableText);
  expect(await preview(request, token, adultB!.accountId)).toEqual(declinedPreview);

  const bMembersAfterAccept = await request.get(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/members`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  expect(bMembersAfterAccept.status()).toBe(200);
  expect(await bMembersAfterAccept.json()).toEqual(expect.arrayContaining([
    expect.objectContaining({ displayName: 'Development Recipient' }),
  ]));
  const sharedAProfileAfterAccept = await request.get(`${apiBaseUrl}/api/family-members/${aProfile.body.profileId}/profile`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  expect(sharedAProfileAfterAccept.status()).toBe(200);
  const sharedAProfile = await sharedAProfileAfterAccept.json() as {
    displayName: string;
    contactEmail: string | null;
    contactPhone: string | null;
    birthdayMonth: number | null;
    birthdayDay: number | null;
  };
  expect(sharedAProfile.displayName).toBe('Development Adult');
  expect(sharedAProfile.contactEmail).toBeNull();
  expect(sharedAProfile.contactPhone).toBeNull();
  expect(sharedAProfile.birthdayMonth).toBeNull();
  expect(sharedAProfile.birthdayDay).toBeNull();
  await expect(page.getByText(/Private A|Stale A/)).toHaveCount(0);
});
