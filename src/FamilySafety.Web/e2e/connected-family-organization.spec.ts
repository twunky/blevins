import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

test('Family navigation preserves Date and Household boundaries across roles, selection and resource-first links', async ({ page, request }) => {
  test.setTimeout(240_000);
  page.setDefaultTimeout(15_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as { accountId: string; label: string }[];
  const admin = identities.find(i => i.label.startsWith('Adult A'))!;
  const member = identities.find(i => i.label.startsWith('Adult B'))!;
  const other = identities.find(i => i.label.startsWith('Adult C'))!;
  const families = (await apiJson<{ familyGroupId: string; name: string }[]>(request, '/api/family-groups/mine', admin.accountId)).body;
  const first = families.find(f => f.name === 'Development Family')!;
  const second = families.find(f => f.name === 'Development Second Family')!;
  const post = (path: string, data: unknown) => request.post(`${apiBaseUrl}${path}`, { headers: { [developmentAccountHeader]: admin.accountId }, data });
  expect((await post('/api/membership-invitations', { familyGroupId: first.familyGroupId, recipientEmail: 'development-recipient@example.invalid' })).status()).toBe(202);
  const outbox = (await apiJson<{ invitationUrl: string }[]>(request, '/api/development/invitation-outbox', admin.accountId)).body;
  const token = decodeURIComponent(outbox.at(-1)!.invitationUrl.split('#invite=')[1]);
  expect((await request.post(`${apiBaseUrl}/api/membership-invitations/accept`, { headers: { [developmentAccountHeader]: member.accountId }, data: { token } })).status()).toBe(200);
  const bp = (await apiJson<{ profileId: string }>(request, '/api/profile', member.accountId)).body;
  const cp = (await apiJson<{ profileId: string }>(request, '/api/profile', other.accountId)).body;
  const secondBase = `/api/family-groups/${second.familyGroupId}`;
  // Existing second-Family fixture seeds ordinary memberships, with no administrator.
  // A's first-Family admin role must not authorize management in the second Family.
  expect((await post(`${secondBase}/households`, { name: 'Denied cross-Family management' })).status()).toBe(404);
  expect((await post(`${secondBase}/dates`, { title: 'Second Family date', description: 'Synthetic second Family coordination', date: '2030-07-16', associatedProfileIds: [bp.profileId] })).status()).toBe(200);
  const event = async (name: string, audienceProfileIds: string[], familyId = second.familyGroupId) => {
    const response = await post(`/api/family-groups/${familyId}/events`, { name, description: 'Synthetic Family Event', date: '2030-07-16', time: '10:00', location: 'Synthetic hall', audienceProfileIds, editorProfileIds: [] });
    expect(response.status(), await response.text()).toBe(201);
    return (await response.json()).eventId as string;
  };
  const sharedEvent = await event('Authorized second Family Event', [bp.profileId]);
  const privateEvent = await event('Private first Family Event', [cp.profileId], first.familyGroupId);
  const nav = page.getByRole('navigation', { name: 'Main navigation' });
  const selected = page.getByLabel('Current Family', { exact: true });
  const dates = page.locator('app-shared-family-dates');
  const households = page.locator('app-households');
  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption(admin.accountId);
  await selected.selectOption(first.familyGroupId);

  await test.step('admin discovers Dates on Family and Households in Settings', async () => {
    await nav.getByRole('link', { name: 'Family', exact: true }).click();
    await expect(dates).toContainText('No shared dates are available to you.');
    await dates.getByRole('link', { name: 'View all Shared Family Dates' }).click();
    await dates.getByRole('button', { name: 'Add a date', exact: true }).click();
    await dates.getByLabel('Title', { exact: true }).fill('First Family date');
    await dates.getByLabel('Date including year').fill('2030-07-15');
    await dates.getByLabel('Description', { exact: true }).fill('Synthetic first Family coordination');
    await dates.getByRole('checkbox', { name: 'Development Recipient', exact: true }).check();
    await dates.getByRole('button', { name: 'Create date', exact: true }).click();
    await expect(dates).toContainText('First Family date');
    await page.getByRole('link', { name: 'Back to Family', exact: true }).click();
    await page.getByRole('link', { name: 'Family settings', exact: true }).focus();
    await page.keyboard.press('Enter');
    await expect(page.locator('main h1')).toBeFocused();
    await expect(households).toContainText('No Households in this Family yet.');
    await households.getByRole('button', { name: 'Create a Household', exact: true }).click();
    await households.getByLabel('Household name', { exact: true }).fill('First Family Household');
    await households.getByRole('button', { name: 'Create Household', exact: true }).click();
    const row = households.getByRole('article', { name: 'First Family Household', exact: true });
    await row.getByRole('combobox', { name: 'Family member to add to First Family Household', exact: true }).selectOption({ label: 'Development Recipient' });
    await row.getByRole('button', { name: 'Add Member', exact: true }).click();
    await expect(row).toContainText('Development Recipient');
    await expect(households.getByRole('region', { name: 'Unassigned members', exact: true })).toContainText('Development Audience Member');
    await expect(households).toContainText('Households do not change Family access or sharing permissions.');
  });

  await test.step('ordinary member reads organization, edits only an associated date, and switches Families', async () => {
    await page.getByLabel('Active test identity').selectOption(member.accountId);
    await selected.selectOption(first.familyGroupId);
    await nav.getByRole('link', { name: 'Family', exact: true }).click();
    await expect(dates).toContainText('First Family date');
    await expect(dates).not.toContainText('Second Family date');
    await expect(dates.getByRole('button', { name: 'Delete date', exact: true })).toHaveCount(0);
    await dates.getByRole('link', { name: 'View all Shared Family Dates' }).click();
    await dates.getByRole('button', { name: 'Edit date', exact: true }).click();
    await expect(dates).toContainText('Only the creator can change associated members or delete this date.');
    await dates.getByLabel('Description', { exact: true }).fill('Reviewed by associated adult');
    await dates.getByRole('button', { name: 'Save date', exact: true }).click();
    await expect(dates).toContainText('Reviewed by associated adult');
    await page.getByRole('link', { name: 'Back to Family', exact: true }).click();
    await page.getByRole('link', { name: 'Family settings', exact: true }).click();
    await expect(households).toContainText('First Family Household');
    await expect(households.getByRole('button', { name: 'Create a Household', exact: true })).toHaveCount(0);
    await expect(households.locator('select')).toHaveCount(0);
    await expect(page.getByLabel('Recipient email')).toHaveCount(0);
    // Hold one real response to inspect the context transition before new data arrives.
    let releaseRead!: () => void;
    const readGate = new Promise<void>(resolve => { releaseRead = resolve; });
    await page.route(`**${secondBase}/households`, async route => { await readGate; await route.continue(); }, { times: 1 });
    try {
      await selected.selectOption(second.familyGroupId);
      await expect(households).toContainText('Loading Households');
      await expect(households).not.toContainText('First Family Household');
    } finally { releaseRead(); }
    await expect(page).toHaveURL(/\/family\/settings$/);
    await expect(households).toContainText('No Households in this Family yet.');
    await expect(households).not.toContainText('First Family Household');
    await page.waitForLoadState('networkidle');
    await page.setViewportSize({ width: 390, height: 844 });
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    await page.getByRole('link', { name: 'Back to Family', exact: true }).click();
    await expect(dates).toContainText('Second Family date');
    await expect(dates).not.toContainText('First Family date');
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForLoadState('networkidle');
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    await page.screenshot({ path: resolve(captureDirectory('connected-family-organization'), 'second-family-member-390.png'), fullPage: true });
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await expect(selected).toHaveValue(second.familyGroupId);
    await nav.getByRole('link', { name: 'Me / Settings', exact: true }).click();
    await expect(page.locator('#display-name')).toHaveValue('Development Recipient');
    await expect(selected).toHaveValue(second.familyGroupId);
  });

  await test.step('authorized deep link reconciles Family; denied existing and missing links do not', async () => {
    await selected.selectOption(first.familyGroupId);
    // Use the existing item-links spec's same-session navigation convention.
    await page.evaluate(id => { history.pushState({}, '', `/events/${id}`); dispatchEvent(new PopStateEvent('popstate', { state: {} })); }, sharedEvent);
    await expect(page.getByRole('heading', { name: 'Authorized second Family Event', exact: true })).toBeVisible();
    await expect(selected).toHaveValue(second.familyGroupId);
    await page.getByRole('link', { name: 'Back to Activities', exact: true }).click();
    await expect(page).toHaveURL(/\/activities$/);
    await expect(selected).toHaveValue(second.familyGroupId);
    for (const id of [privateEvent, '11111111-1111-1111-1111-111111111111']) {
      await page.evaluate(id => { history.pushState({}, '', `/events/${id}`); dispatchEvent(new PopStateEvent('popstate', { state: {} })); }, id);
      await expect(page.getByRole('heading', { name: 'This Event is unavailable', exact: true })).toBeVisible();
      await expect(selected).toHaveValue(second.familyGroupId);
      await expect(page.locator('main')).not.toContainText('Private first Family Event');
      const response = await request.get(`${apiBaseUrl}/api/events/${id}`, { headers: { [developmentAccountHeader]: member.accountId } });
      expect(response.status()).toBe(404);
      expect(await response.text()).toBe('');
    }
    await nav.getByRole('link', { name: 'Family', exact: true }).click();
    await expect(dates).toContainText('Second Family date');
    await expect(dates).not.toContainText('First Family date');
    await selected.selectOption(first.familyGroupId);
    await expect(dates).toContainText('First Family date');
    await page.getByLabel('Active test identity').selectOption(other.accountId);
    await nav.getByRole('link', { name: 'Family', exact: true }).click();
    await expect(dates).toContainText('No shared dates are available to you.');
    await expect(dates).not.toContainText('First Family date');
    const denied = await request.get(`${apiBaseUrl}${secondBase}/households`, { headers: { [developmentAccountHeader]: other.accountId } });
    expect(denied.status()).toBe(404);
    expect(await denied.text()).toBe('');
  });
});
