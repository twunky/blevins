import { test, expect, Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { mkdirSync } from 'node:fs';
import { resolve } from 'node:path';

const id = (n: number) => `aaaaaaaa-0000-4000-8000-${String(n).padStart(12, '0')}`;
const familyId = id(1), ownId = id(2), memberId = id(3);
const captures = resolve(__dirname, 'captures/family-homepage');
async function fixture(page: Page) {
  const members = Array.from({ length: 20 }, (_, i) => ({ profileId: id(i + 2), displayName: i === 0 ? 'Test Owner' : i === 1 ? 'Shared Adult' : i === 2 ? 'Alexandria Catherine Long Family Name' : `Test Member ${i}` }));
  const errors: string[] = []; page.on('pageerror', error => errors.push(error.message));
  let denied = false, secondFamily = false;
  await page.route('**/api/**', route => {
    const path = new URL(route.request().url()).pathname;
    const send = (body: unknown, status = 200) => route.fulfill({ status, json: body });
    if (path === '/api/development/identities') return send({}, 404);
    if (path === '/api/profile') return send({ profileId: ownId, displayName: 'Test Owner', contactEmail: null, contactPhone: null, birthdayMonth: null, birthdayDay: null, contactEmailVisibility: 'OnlyMe', contactPhoneVisibility: 'OnlyMe', birthdayVisibility: 'OnlyMe' });
    if (path === '/api/family-groups/mine') return send([{ familyGroupId: familyId, name: 'Development Family' }, ...(secondFamily ? [{ familyGroupId: id(100), name: 'Second Family' }] : [])]);
    if (path === '/api/family-groups/administered') return send([]);
    if (path.endsWith('/members')) return send(denied ? [] : members);
    if (path.endsWith('/households')) return send({ canManage: false, households: [{ householdId: id(30), name: 'Test Household', version: id(31), members: members.slice(0, 6) }], unassignedMembers: members.slice(6) });
    if (path.endsWith('/dates')) return send({ canCreate: true, dates: [{ dateId: id(40), title: 'Shared important date', description: 'Explicitly shared date description.', date: '2026-11-12', creatorProfileId: ownId, associatedPeople: [members[1]], canEdit: false, canDelete: false, canManageAssociations: false, version: id(41) }] });
    if (path.includes('/family-members/')) return denied ? send({}, 404) : send({ profileId: memberId, displayName: 'Shared Adult', contactEmail: 'shared@example.invalid', contactPhone: null, birthdayMonth: null, birthdayDay: null });
    return send([]);
  });
  return { errors, addSecond: () => { secondFamily = true; }, deny: () => { denied = true; }, allow: () => { denied = false; } };
}

test('responsive layout, search, keyboard, summaries and safe profile navigation', async ({ page }) => {
  const data = await fixture(page); mkdirSync(captures, { recursive: true });
  await page.goto('/family');
  for (const width of [1440, 768, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 }); await page.goto('/family');
    await expect(page.locator('.member-button')).toHaveCount(20);
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Development Family');
    await expect(page.getByRole('link', { name: 'Family settings', exact: true })).toBeVisible();
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    const directory = await page.locator('.family-directory-card').boundingBox(), households = await page.locator('app-households').boundingBox();
    if (width === 1440) { expect(directory!.x).toBeGreaterThan(households!.x); expect(households!.width / directory!.width).toBeGreaterThan(1.8); }
    else expect(directory!.y).toBeLessThan(households!.y);
    expect(await page.locator('.member-button .avatar').first().evaluate(e => e.getBoundingClientRect().width)).toBe(60);
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    await page.screenshot({ path: resolve(captures, `family-${width}.png`), fullPage: true });
  }
  const search = page.getByRole('searchbox', { name: 'Search Family' });
  await search.fill('sHaReD'); await expect(page.locator('.member-button')).toHaveCount(1);
  await search.fill('missing'); await expect(page.getByText('No family members match your search.')).toBeVisible();
  await page.screenshot({ path: resolve(captures, 'family-search-empty-390.png'), fullPage: true });
  await page.getByRole('button', { name: 'Clear Family search' }).click(); await expect(page.locator('.member-button')).toHaveCount(20); await expect(search).toBeFocused();
  const member = page.getByRole('link', { name: 'View profile of Shared Adult', exact: true }); await member.focus(); await page.keyboard.press('Tab'); await page.keyboard.press('Shift+Tab');
  expect(await member.evaluate(e => getComputedStyle(e).outlineStyle)).not.toBe('none'); await member.press('Enter');
  await expect(page).toHaveURL(new RegExp(`/family/${familyId}/members/${memberId}$`));
  await expect(page.getByText('shared@example.invalid')).toBeVisible(); await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
  await expect(page.locator('app-member-profile form, app-member-profile input')).toHaveCount(0);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captures, 'member-profile-390.png'), fullPage: true });
  await page.reload(); await expect(page.getByText('shared@example.invalid')).toBeVisible();
  data.deny(); await page.getByRole('button', { name: 'Refresh shared profile' }).click();
  await expect(page.getByText('This family member is unavailable.')).toBeVisible();
  await expect(page.getByText('shared@example.invalid')).toHaveCount(0);
  data.allow();
  await page.getByRole('link', { name: 'Back to Family', exact: true }).click(); await expect(page).toHaveURL(/family$/);
  await page.getByRole('link', { name: 'View all Households' }).click(); await expect(page).toHaveURL(/family\/settings#household-management$/);
  await expect(page.locator('#household-management')).toContainText('Test Household');
  await page.goto('/family'); await page.getByRole('link', { name: 'View all Shared Family Dates' }).click();
  await expect(page).toHaveURL(/family\/settings#shared-date-management$/);
  await expect(page.locator('#shared-date-management')).toContainText('Explicitly shared date description.');
  await page.goto('/family'); await page.getByRole('link', { name: 'View profile of Test Owner', exact: true }).click(); await expect(page).toHaveURL(/me$/);
  data.addSecond(); await page.goto(`/family/${familyId}/members/${memberId}`); await expect(page.getByText('shared@example.invalid')).toBeVisible();
  await page.getByLabel('Current Family', { exact: true }).selectOption(id(100)); await expect(page).toHaveURL(/family$/); await expect(page.locator('app-member-profile')).toHaveCount(0);
  data.deny(); await page.goto(`/family/${familyId}/members/${memberId}`);
  await expect(page.getByText('This family member is unavailable.')).toBeVisible(); await expect(page.getByText('shared@example.invalid')).toHaveCount(0);
  await expect(page.getByLabel('Current Family', { exact: true })).toHaveValue('');
  expect(data.errors).toEqual([]);
});

test('read-only running application captures', async ({ page }) => {
  mkdirSync(captures, { recursive: true }); await page.setViewportSize({ width: 1440, height: 1000 }); await page.goto('/family');
  await expect(page.locator('.family-directory-card')).toBeVisible(); await page.waitForLoadState('networkidle');
  await page.screenshot({ path: resolve(captures, 'family-live-1440.png'), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 }); await page.screenshot({ path: resolve(captures, 'family-live-390.png'), fullPage: true });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
});
