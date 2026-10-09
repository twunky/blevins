import { expect, test, type Page } from '@playwright/test';
import { resolve } from 'node:path';
import {
  apiBaseUrl,
  apiJson,
  captureDirectory,
  expectHealthyApi,
  recordScenario,
  useIsolatedScenario,
} from './fixtures/invitation-scenario';

// WI-009-010 (DEC-080): Family Settings Invite feedback at 390 px, the 390 px invitation arrival with "Continue to Home",
// and a first Family choice that keeps the page (O6). The OD-E second Family gives A two Families, and gives B one
// Family before B joins the Development Family by invitation.
useIsolatedScenario({ tripAudienceAdult: false, secondFamily: true });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }

const captures = captureDirectory('first-use');
const identity = (page: Page) => page.getByLabel('Active test identity');
const familySelect = (page: Page) => page.locator('#current-family');

async function expectNoOverflow(page: Page, where: string): Promise<void> {
  const width = page.viewportSize()!.width;
  expect(await page.evaluate(() => document.documentElement.scrollWidth), where).toBeLessThanOrEqual(width);
}

/** The element is fully inside the current viewport: no scrolling back to the top of the page is needed. */
async function expectInViewport(page: Page, selector: string): Promise<void> {
  const box = await page.locator(selector).boundingBox();
  const viewport = page.viewportSize()!;
  expect(box, selector).not.toBeNull();
  expect(box!.y).toBeGreaterThanOrEqual(0);
  expect(box!.y + box!.height).toBeLessThanOrEqual(viewport.height);
}

async function world(page: Page) {
  await recordScenario(test.info());
  await expectHealthyApi(page.request);
  const identities = (await (await page.request.get(`${apiBaseUrl}/api/development/identities`)).json() as { identities: DevelopmentIdentity[] }).identities;
  const a = identities.find(item => item.label.startsWith('Adult A'))!;
  const b = identities.find(item => item.label.startsWith('Adult B'))!;
  const aFamilies = (await apiJson<FamilyGroup[]>(page.request, '/api/family-groups/mine', a.accountId)).body;
  const family1 = aFamilies.find(group => group.name === 'Development Family')!;
  const family2 = aFamilies.find(group => group.name === 'Development Second Family')!;
  return { a, b, family1, family2 };
}

async function useIdentity(page: Page, account: DevelopmentIdentity): Promise<void> {
  if (await identity(page).count() === 0) await page.goto('/home');
  if (await identity(page).inputValue() !== account.accountId) await identity(page).selectOption({ label: account.label });
  await expect(identity(page)).toHaveValue(account.accountId);
}

test('390 px: Invite feedback stays in the Invite card; B arrives on Home after saving the profile review', async ({ page }) => {
  test.setTimeout(120_000);
  const w = await world(page);
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/home');
  await useIdentity(page, w.a);

  // A has two Families and none selected: choosing one on Family settings keeps the page (O6).
  await page.goto('/family/settings');
  await expect(familySelect(page)).toHaveValue('');
  await familySelect(page).selectOption(w.family1.familyGroupId);
  await expect(page).toHaveURL(/\/family\/settings$/);
  await expect(page.getByRole('heading', { name: 'Invite an adult family member' })).toBeVisible();

  // J. Send, revoke, and resend: every message appears in the card, in view, once (O7).
  const card = 'form.family-admin-panel';
  await page.getByLabel('Recipient email').fill('development-recipient@example.invalid');
  for (const [action, message] of [['Send or resend invitation', 'Invitation sent.'], ['Revoke invitation', 'Invitation revoked.'],
    ['Send or resend invitation', 'Invitation sent.']] as const) {
    await page.getByRole('button', { name: action }).click();
    await expect(page.locator(`${card} .invite-feedback`)).toHaveText(message);
    await expectInViewport(page, `${card} .invite-feedback`);
    await expect(page.getByText(message)).toHaveCount(1);
  }
  await expect(page.getByText('Administrator status does not provide access to private information.')).toHaveCount(1);
  await expectNoOverflow(page, 'family settings 390');
  await page.screenshot({ path: resolve(captures, 'family-settings-invite-feedback-390.png') });
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.locator(`${card} .invite-feedback`).scrollIntoViewIfNeeded();
  await expectInViewport(page, `${card} .invite-feedback`);
  await page.screenshot({ path: resolve(captures, 'family-settings-invite-feedback-1440.png') });
  await page.setViewportSize({ width: 390, height: 844 });

  // An error stays in the card too. Client validation only: nothing is sent.
  await page.getByLabel('Recipient email').fill('not-an-email');
  await page.getByRole('button', { name: 'Send or resend invitation' }).click();
  await expect(page.locator(`${card} .invite-feedback[role="alert"]`)).toHaveText('Choose a family group and enter a valid email address.');
  await expectInViewport(page, `${card} .invite-feedback`);

  const outbox = await apiJson<{ invitationUrl: string }[]>(page.request, '/api/development/invitation-outbox', w.a.accountId);
  const encodedToken = outbox.body.at(-1)!.invitationUrl.split('#invite=')[1];

  // B belongs to the Second Family only, so it is selected; then B opens the Development Family invitation.
  await useIdentity(page, w.b);
  await page.goto(`/#invite=${encodedToken}`);
  const response = page.getByRole('region', { name: 'Invitation response' });
  await expect(response.locator('p').first()).toHaveText(`Development Adult invited you to join ${w.family1.name}.`);
  await expect(page.getByRole('navigation', { name: 'Main navigation' })).toHaveCount(0);
  await expect(response.getByRole('button')).toHaveText(['Accept invitation', 'Decline invitation']);
  await expectNoOverflow(page, 'preview 390');
  await page.screenshot({ path: resolve(captures, 'invitation-preview-390.png'), fullPage: true });

  // E at 390: the documented capacity 409, answered in the browser only (no fixture fills a Family).
  await page.route('**/api/membership-invitations/accept', route => route.fulfill({
    status: 409, json: { message: 'The family group has reached its capacity.' } }));
  await response.getByRole('button', { name: 'Accept invitation' }).click();
  await expect(response.getByRole('alert')).toHaveText('This family group has reached its capacity.');
  await expect(response.getByRole('button', { name: 'Go to Home' })).toBeVisible();
  await expectNoOverflow(page, 'capacity 390');
  await page.screenshot({ path: resolve(captures, 'invitation-capacity-390-browser-controlled.png'), fullPage: true });
  await page.unroute('**/api/membership-invitations/accept');
  expect((await apiJson<FamilyGroup[]>(page.request, '/api/family-groups/mine', w.b.accountId)).body.map(group => group.name))
    .toEqual([w.family2.name]);

  // Accept for real → profile review at 390 → save a change → Continue to Home.
  await response.getByRole('button', { name: 'Accept invitation' }).click();
  await expect(page.getByRole('heading', { level: 1, name: 'Review your profile' })).toBeVisible();
  await expect(page.getByText('changes are optional')).toBeVisible();
  await expect(page.getByRole('navigation', { name: 'Main navigation' })).toBeVisible();
  await expectNoOverflow(page, 'profile review 390');
  await page.screenshot({ path: resolve(captures, 'profile-review-390.png'), fullPage: true });
  await page.locator('#display-name').fill('Development Recipient B');
  await page.getByRole('button', { name: 'Save profile' }).click();
  await expect(page.getByRole('status').filter({ hasText: /saved/i })).toBeVisible();
  await page.getByRole('button', { name: 'Continue to Home', exact: true }).click();

  await expect(page).toHaveURL(/\/home$/);
  await expect(page.locator('p.family-switch-status')).toHaveText(`You joined ${w.family1.name}.`);
  await expect(page.getByText('You joined')).toHaveCount(1);
  await expect(familySelect(page)).toHaveValue(w.family1.familyGroupId);
  await expect(page.locator('.home-title')).toContainText(`happening with ${w.family1.name} today`);
  await expect(page.getByRole('heading', { level: 1, name: /^Good (morning|afternoon|evening), Development Recipient B!$/ })).toBeFocused();
  // Noticeable but not dominant: one line of status text, smaller than the page heading.
  const sizes = await page.evaluate(() => ({
    status: parseFloat(getComputedStyle(document.querySelector('p.family-switch-status')!).fontSize),
    heading: parseFloat(getComputedStyle(document.querySelector('main h1')!).fontSize),
  }));
  expect(sizes.status).toBeLessThan(sizes.heading);
  await expectNoOverflow(page, 'home arrival 390');
  await page.screenshot({ path: resolve(captures, 'home-arrival-390.png'), fullPage: true });
  // Acceptance granted membership only.
  expect((await apiJson<FamilyGroup[]>(page.request, '/api/family-groups/administered', w.b.accountId)).body).toEqual([]);

  // Refresh: no announcement. With two Families the in-memory selection starts empty again (REQ-UX-011).
  await page.reload();
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  await expect(page.locator('p.family-switch-status')).toHaveText('');
  await expect(page.getByText('You joined')).toHaveCount(0);
});

test('a first Family choice preserves the Events redirect and /family, and the ordinary switch is unchanged (O6)', async ({ page }) => {
  const w = await world(page);
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto('/home');
  await useIdentity(page, w.a);
  for (const path of ['/events', '/family']) {
    const destination = path === '/events' ? /\/activities\?type=Event$/ : /\/family$/;
    await page.goto(path);
    await expect(page).toHaveURL(destination);
    await expect(familySelect(page)).toHaveValue('');
    await expect(page.locator('section.context-prompt')).toContainText('Choose your current Family above to use this area.');
    await familySelect(page).selectOption(w.family1.familyGroupId);
    await expect(page).toHaveURL(destination);
    await expect(page.locator('section.context-prompt')).toHaveCount(0);
    await expect(page.locator('header.page-title')).toContainText(w.family1.name);
    if (path === '/family') await expect(page.getByRole('heading', { name: 'Family Members', exact: true })).toBeVisible();
    else {
      await expect(page.getByRole('heading', { level: 1, name: 'Activities', exact: true })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Upcoming Activities', exact: true })).toBeVisible();
      await expect(page.getByRole('group', { name: 'Activity type' }).getByRole('button', { name: /^Events / })).toHaveAttribute('aria-pressed', 'true');
    }

    await familySelect(page).selectOption(w.family2.familyGroupId);
    await expect(page).toHaveURL(destination);
    await expect(page.locator('header.page-title')).toContainText(w.family2.name);
    await expect(page.locator('header.page-title')).not.toContainText(`${w.family1.name}.`);
  }
});
