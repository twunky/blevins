import { expect, test } from '@playwright/test';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

// One session, seven logical steps: no direct route entry after the initial Home arrival.
useIsolatedScenario({ tripAudienceAdult: true });
const captures = captureDirectory('phase9-exit-journey');
function date(days: number): string {
  const d = new Date(); d.setDate(d.getDate() + days);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

test('invited adult completes the continuous Phase 9 cross-feature journey', async ({ page, request }) => {
  test.setTimeout(240_000);
  page.setDefaultTimeout(15_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as { accountId: string; label: string }[];
  const a = identities.find(i => i.label.startsWith('Adult A'))!;
  const b = identities.find(i => i.label.startsWith('Adult B'))!;
  const family = (await apiJson<{ familyGroupId: string }[]>(request, '/api/family-groups/mine', a.accountId)).body[0];
  const aProfile = (await apiJson<{ profileId: string }>(request, '/api/profile', a.accountId)).body.profileId;
  // An existing whole-Family Plan is available to a later accepted member. No acceptance shortcut.
  const seed = await request.post(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/plans`, {
    headers: { [developmentAccountHeader]: a.accountId }, data: {
      activityOrDestination: 'Existing Family walk', date: date(1), time: '09:00', expectedReturnDate: null,
      expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, notes: null,
      audienceType: 'FamilyGroup', audienceProfileIds: [],
    },
  });
  expect(seed.status()).toBe(201);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto('/home');
  const nav = page.getByRole('navigation', { name: 'Main navigation' });
  const area = async (name: string) => {
    await nav.getByRole('link', { name, exact: true }).click();
    await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
  };
  const capture = async (name: string) => {
    for (const width of [1440, 390]) {
      await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      await page.screenshot({ path: resolve(captures, `${name}-${width}.png`), fullPage: true });
    }
    await page.setViewportSize({ width: 1440, height: 1000 });
  };
  await test.step('administrator invites; recipient opens, accepts and skips optional edits', async () => {
    await area('Family');
    await page.getByRole('link', { name: 'Family settings', exact: true }).click();
    await page.getByLabel('Recipient email').fill('development-recipient@example.invalid');
    await page.getByRole('button', { name: 'Send or resend invitation' }).click();
    await expect(page.locator('.invite-feedback')).toHaveText('Invitation sent.');
    await page.getByLabel('Active test identity').selectOption(b.accountId);
    await expect(page.locator('.home-no-family')).toBeVisible();
    await page.getByRole('button', { name: 'Show Development invitation links' }).click();
    const loaded = page.waitForEvent('load');
    await page.getByRole('link', { name: 'Open invitation', exact: true }).click();
    await loaded;
    await page.getByRole('button', { name: 'Accept invitation', exact: true }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Review your profile' })).toBeVisible();
    await page.getByRole('button', { name: 'Continue to Home without changes' }).click();
    await expect(page).toHaveURL(/\/home$/);
    await expect(page.locator('.current-family strong')).toHaveText('Development Family');
    await capture('invited-home');
  });
  await test.step('Home opens an existing item; Family member and Settings remain reachable', async () => {
    await page.getByRole('link', { name: /Existing Family walk/ }).click();
    await expect(page).toHaveURL(/\/plans\/[\da-f-]+$/);
    await page.reload();
    await expect(page.getByRole('heading', { name: 'Existing Family walk', exact: true })).toBeVisible();
    await page.getByRole('link', { name: 'Back to Plans', exact: true }).click();
    await area('Family');
    await page.getByRole('button', { name: 'Development Adult', exact: true }).click();
    await expect(page.locator('.member-detail')).toBeVisible();
    await page.getByRole('link', { name: 'Family settings', exact: true }).click();
    await expect(page.getByLabel('Recipient email')).toHaveCount(0);
    await page.getByRole('link', { name: 'Back to Family', exact: true }).click();
  });
  await test.step('create an Event and refresh its item route', async () => {
    await area('Events');
    await page.getByRole('link', { name: 'Create event', exact: true }).click();
    await page.locator('#event-name').fill('Invited adult Event');
    await page.locator('#event-location').fill('Family hall');
    await page.locator('#event-description').fill('Coordination created by the invited adult.');
    await page.locator('#event-date').fill(date(2));
    await page.locator('#event-time').fill('18:30');
    await page.locator(`#event-access-${aProfile}`).selectOption('view');
    await page.getByRole('button', { name: 'Create event', exact: true }).click();
    await expect(page).toHaveURL(/\/events\/[\da-f-]+$/);
    await page.reload();
    await expect(page.getByRole('heading', { name: 'Invited adult Event', exact: true })).toBeVisible();
    await page.getByRole('link', { name: 'Back to Events', exact: true }).click();
  });
  await test.step('Plans & Trips reaches both; create and open a Plan and Trip', async () => {
    await area('Plans & Trips');
    await page.getByRole('link', { name: /^Open Plans/ }).click();
    await page.locator('#plan-activity').fill('Invited adult Plan');
    await page.locator('#plan-date').fill(date(3));
    await page.locator('#plan-time').fill('10:00');
    await page.locator('#plan-audience-group').check();
    await page.getByRole('button', { name: 'Create plan', exact: true }).click();
    await page.getByRole('link', { name: /Invited adult Plan/ }).click();
    await page.reload();
    await expect(page.getByRole('heading', { name: 'Invited adult Plan', exact: true })).toBeVisible();
    await area('Plans & Trips');
    await page.getByRole('link', { name: /^Open Trips/ }).click();
    await page.getByRole('button', { name: 'Create trip', exact: true }).click();
    await page.locator('#trip-title').fill('Invited adult Trip');
    await page.locator('#trip-destination').fill('Family coast');
    await page.locator('#trip-start').fill(date(4));
    await page.locator('#trip-end').fill(date(6));
    await page.getByRole('group', { name: /^Travelers/ }).getByRole('checkbox', { name: /Development Recipient/ }).check();
    await page.getByRole('radio', { name: 'This Family', exact: true }).check();
    await page.locator('form.trip-form').getByRole('button', { name: 'Create trip', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Invited adult Trip', exact: true })).toBeVisible();
    await capture('created-trip');
    await page.getByRole('link', { name: 'Back to Trips', exact: true }).click();
  });
  await test.step('create a Concern; start a separate Incident from it', async () => {
    await area('Safety');
    await page.getByRole('link', { name: 'View all Concerns', exact: true }).click();
    await page.getByRole('button', { name: 'Create a Concern', exact: true }).click();
    await page.getByLabel('Who is this Concern about?').selectOption(aProfile);
    await page.getByLabel('Reason or context').fill('Invited adult coordination');
    await page.getByRole('button', { name: 'Create Concern', exact: true }).click();
    await expect(page).toHaveURL(/\/concerns\/[\da-f-]+$/);
    await page.getByRole('button', { name: /^Start an Incident/ }).click();
    await expect(page).toHaveURL(/\/incidents\/new$/);
    await page.getByLabel('Why are you starting this Incident?').fill('Separate invited adult follow-up');
    await page.getByRole('radio', { name: /^Reported/ }).check();
    await page.getByRole('textbox', { name: 'Source description' }).fill('Invited adult');
    await page.getByRole('button', { name: 'Create Incident', exact: true }).click();
    await expect(page).toHaveURL(/\/incidents\/[\da-f-]+$/);
    await expect(page.getByText('Started from a Concern')).toBeVisible();
  });
  await test.step('Safety opens its authorized Incident; refresh and return', async () => {
    await area('Safety');
    await page.getByRole('link', { name: /^Open Incident about Development Adult/ }).click();
    await page.reload();
    await expect(page.getByRole('heading', { name: 'Incident about Development Adult', exact: true })).toBeVisible();
    await page.getByRole('link', { name: 'Back to Incidents', exact: true }).click();
    await page.getByRole('link', { name: 'Back to Safety', exact: true }).click();
    await capture('authorized-safety');
  });
  await test.step('Me / Settings remains reachable; Home shows the new linked items', async () => {
    await area('Me / Settings');
    await expect(page.locator('#display-name')).toHaveValue('Development Recipient');
    await area('Home');
    await expect(page.getByRole('link', { name: /Invited adult Plan/ })).toBeVisible();
    await expect(page.getByRole('link', { name: /Invited adult Trip/ })).toBeVisible();
    await capture('journey-home');
  });
});
