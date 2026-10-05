import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { resolve } from 'node:path';
import {
  apiBaseUrl,
  apiJson,
  captureDirectory,
  expectHealthyApi,
  recordScenario,
  useIsolatedScenario,
} from './fixtures/invitation-scenario';

// WI-009-007: the Family dashboard (/family) and Family Settings (/family/settings).
// Adult A is the Family administrator; Adult C is an active member who is not an administrator. B is not used.
useIsolatedScenario({ tripAudienceAdult: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }

const captures = captureDirectory('family-dashboard');
const heading = (page: Page) => page.locator('main h1');
const mainNavigation = (page: Page) => page.getByRole('navigation', { name: 'Main navigation' });
const gear = (page: Page) => page.getByRole('main').getByRole('link', { name: 'Family settings', exact: true });
const inviteHeading = (page: Page) => page.getByRole('heading', { name: 'Invite an adult family member' });

async function identities(request: APIRequestContext) {
  await expectHealthyApi(request);
  const all = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json() as { identities: DevelopmentIdentity[] }).identities;
  return { adultA: all.find(i => i.label.startsWith('Adult A'))!, adultC: all.find(i => i.label.startsWith('Adult C'))! };
}

async function useIdentity(page: Page, identity: DevelopmentIdentity, path: string): Promise<void> {
  await page.goto('/home');
  const selector = page.getByLabel('Active test identity');
  if (await selector.inputValue() !== identity.accountId) await selector.selectOption({ label: identity.label });
  await expect(selector).toHaveValue(identity.accountId);
  await page.goto(path);
  await expect(selector).toHaveValue(identity.accountId);
}

async function expectFamilyCurrent(page: Page, value: 'page' | 'true'): Promise<void> {
  const current = mainNavigation(page).locator('a[aria-current]');
  await expect(current).toHaveCount(1);
  await expect(current).toHaveText('Family');
  await expect(current).toHaveAttribute('aria-current', value);
}

test('direct entry, gear, Back to Family, administrator Settings, member detail, and data-free placeholders', async ({ page, request }) => {
  await recordScenario(test.info());
  const { adultA, adultC } = await identities(request);
  const family = (await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultA.accountId)).body[0];
  await page.setViewportSize({ width: 1440, height: 1000 });

  // 9: the dashboard reads only existing profile and Family data; the placeholders add no request.
  const apiPaths: string[] = [];
  page.on('request', sent => { const url = new URL(sent.url()); if (url.pathname.startsWith('/api/')) apiPaths.push(url.pathname); });

  // 1: /family direct entry. Requests are recorded from this load only (Home, visited first, reads Trips).
  await useIdentity(page, adultA, '/home');
  apiPaths.length = 0;
  await page.goto('/family');
  await expect(heading(page)).toHaveText('Family');
  await expect(page.locator('.page-title')).toContainText(family.name);
  await expectFamilyCurrent(page, 'page');
  await expect(page.locator('.family-coordination h2')).toHaveText(['Shared Family Dates', 'Waiting for you', 'Upcoming birthdays', 'Recent Family Updates', 'Recent Check-Ins']);
  await expect(page.getByText('Nothing is waiting for you right now.')).toBeVisible();
  const placeholders = page.locator('.family-coordination .home-future');
  await expect(placeholders).toHaveCount(3);
  await expect(placeholders.locator('button, a, input, select, img, li')).toHaveCount(0);
  await page.waitForLoadState('networkidle');
  expect(apiPaths.filter(path => !/^\/api\/(profile|development\/|family-groups(\/|$)|family-members\/)/.test(path)),
    'the Family dashboard makes no request beyond existing profile and Family reads').toEqual([]);
  expect(apiPaths.filter(path => /check-?in|activity|update|birthday|notification/i.test(path))).toEqual([]);
  await expect(page.getByRole('heading', { name: 'Family relationships' })).toBeHidden();
  await expect(inviteHeading(page)).toBeHidden();

  // 7: member selection and detail.
  await page.getByRole('list', { name: 'Active adult family members' }).getByRole('button', { name: /Development Audience Member/ }).click();
  await expect(page.getByRole('heading', { level: 2, name: 'Development Audience Member' })).toBeVisible();
  await expect(page.locator('[aria-labelledby="selected-member-heading"]')).toContainText('No optional profile details are shared.');
  await page.screenshot({ path: resolve(captures, 'family-dashboard-1440.png'), fullPage: true });

  // 3: gear → Settings moves focus to the h1; 10: Family stays current on the child page; 5: administrator sees Invite.
  await gear(page).click();
  await expect(page).toHaveURL(/\/family\/settings$/);
  await expect(heading(page)).toHaveText('Family settings');
  await expect(heading(page)).toBeFocused();
  await expectFamilyCurrent(page, 'true');
  await expect(inviteHeading(page)).toBeVisible();
  await expect(page.getByLabel('Recipient email')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Family details' })).toBeVisible();

  // 4: Back to Family.
  await page.getByRole('link', { name: 'Back to Family' }).click();
  await expect(page).toHaveURL(/\/family$/);
  await expect(heading(page)).toHaveText('Family');
  await expect(heading(page)).toBeFocused();
  await expectFamilyCurrent(page, 'page');

  // 2: /family/settings direct entry (refresh-safe).
  await page.goto('/family/settings');
  await expect(heading(page)).toHaveText('Family settings');
  await expectFamilyCurrent(page, 'true');
  await expect(inviteHeading(page)).toBeVisible();

  // Narrow: the gear stays beside the heading, with no horizontal overflow.
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/family');
  await expect(gear(page)).toBeVisible();
  const [h1Box, gearBox] = [await heading(page).boundingBox(), await gear(page).boundingBox()];
  expect(Math.abs(h1Box!.y - gearBox!.y)).toBeLessThan(24);
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect(adultC).toBeTruthy();
});

test('a member who is not an administrator uses Settings without Invite; Waiting for you accept and remove work across adults', async ({ page, request }) => {
  await recordScenario(test.info());
  const { adultA, adultC } = await identities(request);
  expect((await apiJson<FamilyGroup[]>(request, '/api/family-groups/administered', adultC.accountId)).body).toEqual([]);
  await page.setViewportSize({ width: 1440, height: 1000 });

  // 6: C sees the gear and Settings, but never the administrator-only Invite.
  await useIdentity(page, adultC, '/family');
  await expect(gear(page)).toBeVisible();
  await gear(page).click();
  await expect(heading(page)).toHaveText('Family settings');
  await expect(page.getByText('You are an active member.')).toBeVisible();
  await expect(inviteHeading(page)).toHaveCount(0);
  await expect(page.getByLabel('Recipient email')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Send or resend invitation' })).toHaveCount(0);
  await expect(page.getByText(/Administering/)).toHaveCount(0);
  await page.screenshot({ path: resolve(captures, 'family-settings-member-1440.png'), fullPage: true });

  // C suggests a relationship to A from Settings; it shows under "Sent by you".
  await page.getByLabel('Adult family member', { exact: true }).selectOption({ label: 'Development Adult' });
  await page.getByLabel('Your relationship to them').selectOption({ label: 'Sibling' });
  await page.getByRole('button', { name: 'Suggest relationship' }).click();
  const sent = page.getByRole('list', { name: 'Sent by you' });
  await expect(sent).toContainText('Waiting for Development Adult to confirm Sibling');
  await expect(sent.getByRole('button', { name: 'Withdraw suggestion' })).toBeVisible();

  // 8: A sees it in Waiting for you on the dashboard and accepts it there.
  await useIdentity(page, adultA, '/family');
  const waiting = page.getByRole('region', { name: 'Waiting for you' });
  await expect(waiting).toContainText('Development Audience Member suggested a relationship');
  await waiting.getByRole('button', { name: 'Accept relationship' }).click();
  await expect(waiting.getByRole('status')).toContainText('Relationship confirmed.');
  await expect(waiting).toContainText('Nothing is waiting for you right now.');
  // D5: the tile note appears only for the member in an accepted relationship with A.
  const members = page.getByRole('list', { name: 'Active adult family members' });
  await expect(members.getByRole('button', { name: /Development Audience Member/ }).locator('.member-note')).toHaveText('Sibling');
  await expect(members.getByRole('button', { name: /Development Adult/ }).locator('.member-note')).toHaveCount(0);

  // Remove from Settings, after confirmation.
  await gear(page).click();
  const accepted = page.getByRole('list', { name: 'Accepted relationships' });
  await accepted.getByRole('button', { name: 'Remove relationship' }).click();
  await accepted.getByRole('button', { name: 'Confirm removal' }).click();
  await expect(page.locator('section.family-settings').getByRole('status').filter({ hasText: 'Relationship removed.' })).toBeVisible();
  await expect(page.getByText('No Family relationships or suggestions are currently visible here.')).toBeVisible();
});
