import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({});
const captures = captureDirectory('home-pending-rsvp');

test('Home Pending attention opens Event RSVP and disappears on return and reload', async ({ page, request }) => {
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as { accountId: string; label: string }[];
  const organizer = identities.find(row => row.label.startsWith('Adult A'))!;
  const invitee = identities.find(row => row.label.startsWith('Adult B'))!;
  const post = (path: string, data: unknown) => request.post(`${apiBaseUrl}${path}`, {
    headers: { [developmentAccountHeader]: organizer.accountId }, data,
  });
  const family = (await apiJson<{ familyGroupId: string }[]>(request, '/api/family-groups/mine', organizer.accountId)).body[0];
  expect((await post('/api/membership-invitations', {
    familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid',
  })).status()).toBe(202);
  const outbox = (await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', organizer.accountId)).body;
  const token = decodeURIComponent(outbox.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await request.post(`${apiBaseUrl}/api/membership-invitations/accept`, {
    headers: { [developmentAccountHeader]: invitee.accountId }, data: { token },
  })).status()).toBe(200);
  const profile = (await apiJson<{ profileId: string }>(request, '/api/profile', invitee.accountId)).body;
  const created = await post(`/api/family-groups/${family.familyGroupId}/events`, {
    name: 'Home RSVP gathering', description: 'Current Pending invitation, including past dates.',
    date: '2020-01-01', time: '18:30', location: 'Family hall', audienceProfileIds: [profile.profileId], editorProfileIds: [],
  });
  expect(created.status()).toBe(201);
  const eventId = (await created.json()).eventId;
  expect((await post(`/api/events/${eventId}/invitations`, { inviteeProfileId: profile.profileId })).status()).toBe(200);

  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption({ label: invitee.label });
  await page.goto('/home');
  const attention = page.locator('.home-attention');
  const link = attention.getByRole('link', { name: 'Home RSVP gathering' });
  await expect(link).toHaveAttribute('href', `/events/${eventId}`);
  await expect(attention).toContainText('RSVP needed');
  await expect(attention).toContainText('2020');
  await expect(attention.getByRole('button', { name: 'Save RSVP' })).toHaveCount(0);
  // Two routine captures only; verify the existing composition and narrow layout.
  for (const width of [1440, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    await page.screenshot({ path: resolve(captures, `pending-${width}.png`), fullPage: true });
  }
  // Keyboard activation follows the same refresh-safe Event link and moves focus to the item heading.
  await link.focus();
  await page.keyboard.press('Enter');
  const participation = page.locator('app-event-participation');
  await expect(participation).toContainText('Current RSVP: Pending');
  await participation.getByRole('radio', { name: 'Going', exact: true }).check();
  await participation.getByRole('button', { name: 'Save RSVP' }).click();
  await expect(participation).toContainText('Your RSVP was saved');
  await page.getByRole('link', { name: 'Home', exact: true }).click();
  await expect(attention).toContainText('Nothing needs your attention right now.');
  await expect(link).toHaveCount(0);
  await page.reload();
  await expect(attention).toContainText('Nothing needs your attention right now.');
  await expect(link).toHaveCount(0);
});
