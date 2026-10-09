import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: false });

test('invited adult reaches Home, discovers RSVP, coordinates Needs and Updates, and returns to resolved attention', async ({ page, request }) => {
  test.setTimeout(240_000);
  page.setDefaultTimeout(15_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as { accountId: string; label: string }[];
  const admin = identities.find(i => i.label.startsWith('Adult A'))!;
  const member = identities.find(i => i.label.startsWith('Adult B'))!;
  const family = (await apiJson<{ familyGroupId: string }[]>(request, '/api/family-groups/mine', admin.accountId)).body[0];
  const post = (path: string, data: unknown) => request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: admin.accountId }, data });
  const nav = page.getByRole('navigation', { name: 'Main navigation' });
  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption(admin.accountId);

  await test.step('existing invitation entry and optional profile review', async () => {
    await nav.getByRole('link', { name: 'Family', exact: true }).click();
    await page.getByRole('link', { name: 'Family settings', exact: true }).click();
    await page.getByLabel('Recipient email').fill('development-recipient@example.invalid');
    await page.getByRole('button', { name: 'Send or resend invitation' }).click();
    await expect(page.locator('.invite-feedback')).toHaveText('Invitation sent.');
    await page.getByLabel('Active test identity').selectOption(member.accountId);
    await expect(page.locator('.home-no-family')).toBeVisible();
    expect((await apiJson<unknown[]>(request, '/api/family-groups/mine', member.accountId)).body).toEqual([]);
    await page.getByRole('button', { name: 'Show Development invitation links' }).click();
    const loaded = page.waitForEvent('load');
    await page.getByRole('link', { name: 'Open invitation', exact: true }).click();
    await loaded;
    await page.getByRole('button', { name: 'Accept invitation', exact: true }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Review your profile' })).toBeVisible();
    await page.getByRole('button', { name: 'Continue to Home without changes' }).focus();
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(/\/home$/);
    await expect(page.locator('.current-family strong')).toHaveText('Development Family');
    await nav.getByRole('link', { name: 'Me / Settings', exact: true }).click();
    await expect(page.locator('#display-name')).toHaveValue('Development Recipient');
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
  });

  // Minimum organizer data; browser coverage verifies the connected invitee workflow.
  const profile = (await apiJson<{ profileId: string }>(request, '/api/profile', member.accountId)).body;
  const created = await post(`/api/family-groups/${family.familyGroupId}/events`, {
    name: 'Connected Family gathering', description: 'Bring supplies and read the organizer update.',
    date: '2030-06-15', time: '18:30', location: 'Synthetic Family hall', audienceProfileIds: [profile.profileId], editorProfileIds: [],
  });
  expect(created.status()).toBe(201);
  const eventId = (await created.json()).eventId as string;
  const base = `/api/events/${eventId}`;
  expect((await post(`${base}/invitations`, { inviteeProfileId: profile.profileId })).status()).toBe(200);
  expect((await post(`${base}/needs`, { title: 'Bring paper plates', detail: 'For the gathering', category: 'Supplies' })).status()).toBe(200);
  expect((await post(`${base}/updates`, { text: 'Use the side entrance.' })).status()).toBe(200);

  await test.step('Home and Activities discover the same actionable invitation', async () => {
    await page.reload();
    const attention = page.locator('.home-attention');
    await expect(attention).toContainText('RSVP needed');
    await expect(attention.getByRole('link', { name: 'Connected Family gathering', exact: true })).toHaveAttribute('href', `/events/${eventId}`);
    await nav.getByRole('link', { name: 'Activities', exact: true }).click();
    await expect(page).toHaveURL(/\/activities$/);
    await expect(page.getByRole('region', { name: 'Pending RSVPs' }).getByRole('link', { name: 'Connected Family gathering', exact: true }))
      .toHaveAttribute('href', `/events/${eventId}`);
    await expect(page.locator('.activity-row').filter({ hasText: 'Connected Family gathering' })).toHaveAttribute('href', `/events/${eventId}`);
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForLoadState('networkidle');
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    await attention.getByRole('link', { name: 'Connected Family gathering', exact: true }).focus();
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(new RegExp(`/events/${eventId}$`));
    await expect(page.locator('main h1')).toBeFocused();
  });

  await test.step('RSVP, personal Need responsibility and visible organizer Update', async () => {
    const participation = page.locator('app-event-participation');
    const needs = page.locator('app-event-needs');
    const updates = page.locator('app-event-updates');
    await expect(participation).toContainText('Current RSVP: Pending');
    await participation.getByRole('radio', { name: 'Going', exact: true }).check();
    await participation.getByRole('button', { name: 'Save RSVP' }).click();
    await expect(participation).toContainText('Your RSVP was saved');
    await needs.getByRole('button', { name: 'Claim Need', exact: true }).focus();
    await page.keyboard.press('Enter');
    await expect(needs).toContainText('You claimed this Need');
    await expect(needs.getByRole('button', { name: 'Add Need', exact: true })).toHaveCount(0);
    await expect(updates).toContainText('Use the side entrance.');
    await expect(updates.getByRole('button', { name: 'Post Update', exact: true })).toHaveCount(0);
    await expect(page.getByRole('link', { name: 'Edit event details', exact: true })).toHaveCount(0);
    // One intercepted read error checks that recovery returns to real authorized data.
    await page.route(`**${base}/updates`, route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
    await updates.getByRole('button', { name: 'Refresh Updates', exact: true }).click();
    await expect(updates).toContainText('could not be loaded');
    await updates.getByRole('button', { name: 'Retry loading Updates', exact: true }).click();
    await expect(updates).toContainText('Use the side entrance.');
    await page.waitForLoadState('networkidle');
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    await page.screenshot({ path: resolve(captureDirectory('connected-event-coordination'), 'event-member-390.png'), fullPage: true });
    await page.getByRole('link', { name: 'Back to Activities', exact: true }).click();
    await expect(page).toHaveURL(/\/activities$/);
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await expect(page.locator('.home-attention')).toContainText('No pending RSVPs or relationship suggestions.');
    await page.reload();
    await expect(page.locator('.home-attention').getByRole('link', { name: 'Connected Family gathering', exact: true })).toHaveCount(0);
    await expect(page.locator('.current-family strong')).toHaveText('Development Family');
  });
});
