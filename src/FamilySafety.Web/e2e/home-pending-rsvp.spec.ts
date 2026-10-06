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
  await expect(attention).toContainText('No pending RSVPs or relationship suggestions.');
  await expect(link).toHaveCount(0);
  await page.reload();
  await expect(attention).toContainText('No pending RSVPs or relationship suggestions.');
  await expect(link).toHaveCount(0);
  // Phase 10: follow the clarified discovery links using keyboard activation at both widths.
  for (const width of [1440, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    for (const source of ['/home', '/family']) {
      await page.goto(source);
      const discovery = page.locator(source === '/home' ? '[aria-labelledby="home-updates-heading"]' : '[aria-labelledby="family-updates-heading"]');
      await expect(discovery).toContainText('A Family activity feed is not available here.');
      const isHome = source === '/home';
      const updates = discovery.getByRole('link', { name: isHome ? 'Open Events' : 'Open Events for Updates' });
      await expect(updates).toHaveAttribute('href', '/events');
      if (isHome) {
        await expect(updates).toHaveClass(/button-link.*button--secondary|button--secondary.*button-link/);
        await expect(updates).toHaveCSS('border-radius', '10px');
        await expect(updates).toHaveCSS('border-color', 'rgb(219, 231, 251)');
        await expect(updates).toHaveCSS('text-decoration-line', 'none');
        await expect(updates.locator('svg')).toHaveAttribute('aria-hidden', 'true');
        await updates.hover();
        await expect(updates).toHaveCSS('background-color', 'rgb(219, 234, 254)');
      }
      await updates.focus();
      if (isHome) {
        await page.keyboard.press('Tab');
        await page.keyboard.press('Shift+Tab');
        await expect(updates).toBeFocused();
        await expect(updates).toHaveCSS('outline-style', 'solid');
        await expect(updates).toHaveCSS('outline-width', '3px');
      }
      await page.keyboard.press('Enter');
      await expect(page).toHaveURL(/\/events$/);
      await expect(page.locator('main h1')).toBeFocused();
      await page.goto(source);
      const safety = page.locator(source === '/home' ? '[aria-labelledby="home-updates-heading"]' : '[aria-labelledby="family-checkins-heading"]');
      const checkIns = safety.getByRole('link', { name: isHome ? 'Open Check-Ins' : 'Open Safety for Check-Ins' });
      if (isHome) {
        await expect(checkIns).toHaveAttribute('href', '/safety#check-ins');
        await expect(checkIns).toHaveClass(/button-link.*button--secondary|button--secondary.*button-link/);
        await expect(checkIns).toHaveCSS('border-radius', '10px');
        await expect(checkIns).toHaveCSS('border-color', 'rgb(219, 231, 251)');
        await expect(checkIns).toHaveCSS('text-decoration-line', 'none');
        await expect(checkIns.locator('svg')).toHaveAttribute('aria-hidden', 'true');
      }
      await checkIns.focus();
      if (isHome) {
        await page.keyboard.press('Tab');
        await page.keyboard.press('Shift+Tab');
        await expect(checkIns).toBeFocused();
        await expect(checkIns).toHaveCSS('outline-style', 'solid');
        await expect(checkIns).toHaveCSS('outline-width', '3px');
      }
      await page.keyboard.press('Enter');
      if (isHome) await expect(page).toHaveURL(/\/safety#check-ins$/);
      else await expect(page).toHaveURL(/\/safety$/);
      await expect(page.getByRole('heading', { name: 'Check-Ins', exact: true })).toBeVisible();
      if (isHome) await expect(page.locator('#check-ins')).toBeInViewport();
      else await expect(page.locator('main h1')).toBeFocused();
    }
    for (const source of ['/home', '/family', '/plans']) {
      await page.goto(source);
      await page.waitForLoadState('networkidle');
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
      await page.screenshot({ path: resolve(captures, `readiness-${source.slice(1)}-${width}.png`), fullPage: true });
    }
  }
  const expectation = 'Expected check-in records a planned time only. It does not schedule a Check-In or send reminders.';
  await expect(page.locator('#plan-checkin-help')).toContainText(expectation);
  await expect(page.locator('#plan-checkin-date')).toHaveAttribute('aria-describedby', 'plan-checkin-help');
  await expect(page.locator('#plan-checkin-time')).toHaveAttribute('aria-describedby', 'plan-checkin-help');
  await page.locator('#plan-activity').fill('Recorded expectation only');
  await page.locator('#plan-date').fill('2030-01-01');
  await page.locator('#plan-time').fill('10:00');
  await page.locator('#plan-checkin-date').fill('2030-01-01');
  await page.locator('#plan-checkin-time').fill('12:00');
  await page.locator('#plan-audience-group').check();
  await page.getByRole('button', { name: 'Create plan', exact: true }).click();
  await page.getByRole('link', { name: 'Recorded expectation only', exact: true }).click();
  for (const width of [1440, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    await page.reload();
    await expect(page.locator('.item-page')).toContainText(expectation);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    await page.screenshot({ path: resolve(captures, `readiness-plan-detail-${width}.png`), fullPage: true });
  }
});
