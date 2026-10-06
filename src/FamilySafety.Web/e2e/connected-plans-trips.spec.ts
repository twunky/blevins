import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, captureDirectory, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true });
function date(days: number): string {
  const d = new Date(); d.setDate(d.getDate() + days);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

test('Home navigation reaches Plan and Trip creation, supported Notes, and domain returns without implying completion', async ({ page, request }) => {
  test.setTimeout(240_000);
  page.setDefaultTimeout(15_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as { accountId: string; label: string }[];
  const member = identities.find(i => i.label.startsWith('Adult C'))!;
  const admin = identities.find(i => i.label.startsWith('Adult A'))!;
  const nav = page.getByRole('navigation', { name: 'Main navigation' });
  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption(member.accountId);
  await page.setViewportSize({ width: 390, height: 844 });

  await test.step('Home to Plans, create and edit the existing note field', async () => {
    await nav.getByRole('link', { name: 'Plans & Trips', exact: true }).focus();
    await page.keyboard.press('Enter');
    await expect(page.locator('main h1')).toBeFocused();
    await page.getByRole('link', { name: /^Open Plans/ }).click();
    await expect(page).toHaveURL(/\/plans$/);
    await expect(page.locator('#plan-checkin-help')).toContainText('It does not schedule a Check-In or send reminders.');
    await page.locator('#plan-activity').fill('Connected Family walk');
    await page.locator('#plan-date').fill(date(1));
    await page.locator('#plan-time').fill('10:00');
    await page.locator('#plan-checkin-date').fill(date(1));
    await page.locator('#plan-checkin-time').fill('12:00');
    await page.locator('#plan-notes').fill('Meet at the park gate.');
    await page.locator('#plan-audience-group').check();
    await page.getByRole('button', { name: 'Create plan', exact: true }).click();
    await page.getByRole('link', { name: 'Connected Family walk', exact: true }).click();
    await expect(page).toHaveURL(/\/plans\/[\da-f-]+$/);
    await expect(page.locator('.item-page')).toContainText('Meet at the park gate.');
    await expect(page.locator('.item-page')).toContainText('It does not schedule a Check-In or send reminders.');
    await page.getByRole('button', { name: 'Edit note', exact: true }).click();
    await page.locator('#item-plan-note').fill('Meet at the north park gate.');
    await page.getByRole('button', { name: 'Save note', exact: true }).click();
    await expect(page.locator('.item-page')).toContainText('Meet at the north park gate.');
    await expect(page.getByRole('button', { name: /complete plan|edit plan details/i })).toHaveCount(0);
    await page.reload();
    await expect(page.locator('.item-page')).toContainText('Meet at the north park gate.');
    await page.waitForLoadState('networkidle');
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    await page.getByRole('link', { name: 'Back to Plans', exact: true }).click();
    await expect(page).toHaveURL(/\/plans$/);
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await expect(page.getByRole('link', { name: 'Connected Family walk', exact: true })).toBeVisible();
  });

  let tripUrl = '';
  await test.step('grouping to Trips, create/view and contribute a Trip Note', async () => {
    await nav.getByRole('link', { name: 'Plans & Trips', exact: true }).click();
    await page.getByRole('link', { name: /^Open Trips/ }).click();
    await expect(page).toHaveURL(/\/trips$/);
    await page.getByRole('button', { name: 'Create trip', exact: true }).click();
    await page.locator('#trip-title').fill('Connected Family coast trip');
    await page.locator('#trip-destination').fill('Synthetic coast');
    await page.locator('#trip-start').fill(date(2));
    await page.locator('#trip-end').fill(date(4));
    await page.getByRole('group', { name: /^Travelers/ }).getByRole('checkbox', { name: /Development Audience Member/ }).check();
    await page.getByRole('radio', { name: 'This Family', exact: true }).check();
    await expect(page.locator('form.trip-form')).toContainText('Being a traveler does not grant access');
    await page.locator('form.trip-form').getByRole('button', { name: 'Create trip', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Connected Family coast trip', exact: true })).toBeVisible();
    // Creation opens the existing inline detail. The saved list link owns its refresh-safe item URL.
    await page.getByRole('link', { name: 'Back to Trips', exact: true }).click();
    await page.getByRole('link', { name: /Connected Family coast trip/ }).click();
    await expect(page).toHaveURL(/\/trips\/[\da-f-]+$/);
    tripUrl = new URL(page.url()).pathname;
    await page.getByLabel('Add a Note', { exact: true }).fill('Pack a raincoat.');
    await page.getByRole('button', { name: 'Add note', exact: true }).click();
    await expect(page.locator('.trip-note-list')).toContainText('Pack a raincoat.');
    await expect(page.getByRole('button', { name: 'Edit trip details', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: /complete trip/i })).toHaveCount(0);
    await page.reload();
    await expect(page.locator('.trip-note-list')).toContainText('Pack a raincoat.');
    await page.waitForLoadState('networkidle');
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    await page.screenshot({ path: resolve(captureDirectory('connected-plans-trips'), 'trip-notes-390.png'), fullPage: true });
    await page.getByRole('link', { name: 'Back to Trips', exact: true }).click();
    await expect(page).toHaveURL(/\/trips$/);
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await expect(page.getByRole('link', { name: 'Connected Family coast trip', exact: true })).toBeVisible();
  });

  await test.step('Family admin is a Plan viewer and Trip audience contributor, with no creator override', async () => {
    await page.getByLabel('Active test identity').selectOption(admin.accountId);
    await page.getByRole('link', { name: 'Connected Family walk', exact: true }).click();
    await expect(page.locator('.item-page')).toContainText('Meet at the north park gate.');
    await expect(page.getByRole('button', { name: 'Edit note', exact: true })).toHaveCount(0);
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await page.getByRole('link', { name: 'Connected Family coast trip', exact: true }).click();
    await expect(page).toHaveURL(new RegExp(`${tripUrl}$`));
    await expect(page.getByRole('button', { name: 'Edit trip details', exact: true })).toHaveCount(0);
    await page.getByLabel('Add a Note', { exact: true }).fill('I will bring the picnic supplies.');
    await page.getByRole('button', { name: 'Add note', exact: true }).click();
    await expect(page.locator('.trip-note-list')).toContainText('I will bring the picnic supplies.');
    await page.getByRole('link', { name: 'Back to Trips', exact: true }).click();
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await expect(page.locator('.current-family strong')).toHaveText('Development Family');
  });
});
