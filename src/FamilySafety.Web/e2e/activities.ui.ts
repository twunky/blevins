import { expect, test, Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { mkdirSync } from 'node:fs';
import { resolve } from 'node:path';

const familyId = '33333333-3333-3333-3333-333333333333';
const ownId = '11111111-1111-1111-1111-111111111111';
const otherId = '22222222-2222-2222-2222-222222222222';
const id = (n: number) => `aaaaaaaa-0000-4000-8000-${String(n).padStart(12, '0')}`;
const captures = resolve(__dirname, 'captures/activities');
const date = (days: number) => { const d = new Date(); d.setDate(d.getDate() + days); return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`; };

async function fixture(page: Page) {
  const family = { familyGroupId: familyId, name: 'Development Family' };
  const profile = { profileId: ownId, displayName: 'Sean Blevins', contactEmail: null, contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe', birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe' };
  const members = [{ profileId: ownId, displayName: 'Sean Blevins' }, { profileId: otherId, displayName: 'Family Member' }];
  const events: any[] = [{ eventId: id(1), familyGroupId: familyId, name: 'Sebastian', description: 'Family gathering', date: date(2), time: '09:00', location: 'Sebastian, FL', organizerProfileId: ownId, organizerDisplayName: 'Sean Blevins', canEdit: true }];
  const plans: any[] = [{ planId: id(2), familyGroupId: familyId, activityOrDestination: 'Visit Roadhouse', date: date(4), time: '13:00', expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, audienceType: 'FamilyGroup', notes: 'Meet outside.', creatorProfileId: ownId, creatorDisplayName: 'Sean Blevins', canCorrectActivityTiming: true, version: id(9) }];
  const trips: any[] = [{ tripId: id(3), familyGroupId: familyId, familyGroupName: family.name, title: 'Rome Move', primaryDestination: 'Rome E. Base', startDate: date(3), endDate: date(14), canEdit: true, isCompleted: false, creatorProfileId: ownId, creatorDisplayName: profile.displayName, audienceType: 'FamilyGroup', travelers: [members[0]], contacts: [], audienceRecipients: [members[1]], version: id(9) }];
  const writes: { path: string; body: any }[] = [];
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.route('**/api/**', async route => {
    const req = route.request(), path = new URL(req.url()).pathname;
    const send = (body: unknown, status = 200) => route.fulfill({ status, json: body });
    if (path === '/api/development/identities') return send({}, 404);
    if (path === '/api/profile') return send(profile);
    if (path === '/api/family-groups/mine') return send([family]);
    if (path === '/api/family-groups/administered') return send([]);
    if (path.endsWith('/members')) return send(members);
    if (path.endsWith('/relationships') || path.endsWith('/event-invitations/mine')) return send([]);
    const listMatch = path.match(/\/family-groups\/[^/]+\/(events|plans|trips)$/);
    if (listMatch) {
      const kind = listMatch[1], list = kind === 'events' ? events : kind === 'plans' ? plans : trips;
      if (req.method() === 'POST') {
        const body = req.postDataJSON(); writes.push({ path, body });
        const created = { ...list[0], ...body, [kind === 'events' ? 'eventId' : kind === 'plans' ? 'planId' : 'tripId']: id(10 + writes.length) };
        list.push(created); return send(created);
      }
      return send(list);
    }
    if (path === '/api/trips') return send(trips);
    if (path.endsWith('/invitations')) return send({ eligibleViewers: [], invitations: [] });
    if (path.endsWith('/invitation/mine')) return send({ invitation: null });
    if (path.endsWith('/needs')) return send({ canManage: true, canParticipate: false, eligibleParticipants: [], needs: [] });
    if (path.endsWith('/updates')) return send({ canManage: true, updates: [] });
    if (path.endsWith('/notes')) return send([]);
    const match = path.match(/\/(events|plans|trips)\/([a-f0-9-]+)(?:\/(activity-timing))?$/);
    if (match) {
      const list = match[1] === 'events' ? events : match[1] === 'plans' ? plans : trips;
      const key = match[1] === 'events' ? 'eventId' : match[1] === 'plans' ? 'planId' : 'tripId';
      const item = list.find(x => x[key] === match[2]);
      if (!item) return send({}, 404);
      if (req.method() === 'PUT') { const body = req.postDataJSON(); writes.push({ path, body }); Object.assign(item, body); }
      return send(item);
    }
    return send([]);
  });
  return { writes, errors, events, plans, trips };
}

test('Activities filters, old links, keyboard navigation and responsive mockup layouts', async ({ page }) => {
  const data = await fixture(page); mkdirSync(captures, { recursive: true });
  await page.setViewportSize({ width: 1672, height: 941 });
  await page.goto('/activities');
  await expect(page.locator('nav[aria-label="Main navigation"] a')).toHaveCount(5);
  await expect(page.locator('.activity-row')).toHaveCount(3);
  await expect(page.locator('.activity-row .activity-copy > strong')).toHaveText(['Sebastian', 'Rome Move', 'Visit Roadhouse']);
  await expect(page.locator('form.plans-form')).toHaveCount(0);
  await page.screenshot({ path: resolve(captures, 'activities-desktop.png'), fullPage: true });
  await page.getByRole('button', { name: /^Plans/ }).click();
  await expect(page.locator('.activity-row')).toHaveCount(1);
  await page.getByRole('searchbox', { name: 'Search activities' }).fill('missing');
  await expect(page.getByText('No activities match this view.', { exact: false })).toBeVisible();
  await page.goto('/events/all');
  await expect(page).toHaveURL(/activities\?type=Event&period=all/);
  await expect(page.getByLabel('Activity dates')).toHaveValue('all');
  await page.goto('/activities');
  await page.getByRole('link', { name: 'Create Activity', exact: true }).focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/activities\/new$/);
  await page.getByRole('link', { name: /^Plan A lightweight/ }).click();
  await expect(page).toHaveURL(/plans\/new$/);
  await page.getByRole('radio', { name: 'This Family Group', exact: true }).check();
  await page.screenshot({ path: resolve(captures, 'plan-create-desktop.png'), fullPage: true });
  for (const path of ['/activities', '/plans/new', '/events/new', '/trips/new']) {
    await page.setViewportSize({ width: 390, height: 844 }); await page.goto(path);
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: resolve(captures, path.replaceAll('/', '-') + '-390.png'), fullPage: true });
    expect(await page.evaluate(() => document.documentElement.scrollWidth), path + ': ' + await page.evaluate(() => [...document.querySelectorAll('*')].filter(e => e.getBoundingClientRect().right > 391).map(e => e.tagName + '.' + e.className).slice(0, 12).join(', '))).toBeLessThanOrEqual(390);
    expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
    await page.screenshot({ path: resolve(captures, path.replaceAll('/', '-') + '-390.png'), fullPage: true });
  }
  expect(data.errors).toEqual([]); expect(data.writes).toEqual([]);
});

test('all three create flows save to item URLs; edit, refresh and cancel retain domain fields', async ({ page }) => {
  const data = await fixture(page);
  await page.goto('/plans/new');
  await page.locator('#plan-activity').fill('Museum Visit');
  await page.locator('#plan-date').fill(date(8)); await page.locator('#plan-time').fill('10:00');
  await page.getByRole('radio', { name: 'Selected people', exact: true }).check();
  await page.getByRole('checkbox', { name: 'Family Member', exact: true }).check();
  await page.locator('#plan-notes').fill('Bring tickets.');
  await page.getByRole('button', { name: 'Create plan', exact: true }).click();
  await expect(page).toHaveURL(/plans\/[a-f0-9-]+$/); const planUrl = page.url();
  expect(data.writes[0].body.audienceProfileIds).toEqual([otherId]);
  await page.getByRole('link', { name: 'Correct activity and timing' }).click();
  await expect(page).toHaveURL(/\/edit$/); await page.reload();
  await expect(page.locator('#plan-correction-activity')).toHaveValue('Museum Visit');
  await page.locator('#plan-correction-activity').fill('Museum Afternoon');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page).toHaveURL(planUrl);
  expect(data.writes[1].body.version).toBe(id(9));
  expect(data.writes[1].body).not.toHaveProperty('notes');
  await expect(page.locator('.plan-notes')).toHaveText('Bring tickets.');

  await page.goto('/events/new');
  await page.locator('#event-name').fill('Family Dinner'); await page.locator('#event-description').fill('Bring a dish.');
  await page.locator('#event-location').fill('Community room'); await page.locator('#event-date').fill(date(5)); await page.locator('#event-time').fill('18:00');
  await page.locator(`#event-access-${otherId}`).selectOption('edit');
  await page.getByRole('button', { name: 'Create event', exact: true }).click();
  await expect(page).toHaveURL(/events\/[a-f0-9-]+$/); const eventUrl = page.url();
  expect(data.writes[2].body.editorProfileIds).toEqual([otherId]);
  await page.getByRole('link', { name: 'Edit event details' }).click(); await page.reload();
  await page.getByRole('textbox', { name: 'Event name', exact: true }).fill('Family Supper');
  await page.getByRole('button', { name: 'Save event details' }).click(); await expect(page).toHaveURL(eventUrl);

  await page.goto('/trips/new');
  await page.locator('#trip-title').fill('Coast trip'); await page.locator('#trip-destination').fill('The coast');
  await page.locator('#trip-start').fill(date(6)); await page.locator('#trip-end').fill(date(10));
  await page.getByRole('group', { name: /^Travelers/ }).getByRole('checkbox', { name: /Sean Blevins/ }).check();
  await page.getByRole('radio', { name: 'This Family', exact: true }).check();
  await page.getByRole('button', { name: 'Create trip', exact: true }).click();
  await expect(page).toHaveURL(/trips\/[a-f0-9-]+$/); const tripUrl = page.url();
  await page.getByRole('link', { name: 'Edit trip details' }).click(); await page.reload();
  await expect(page.locator('#trip-title')).toHaveValue('Coast trip');
  await page.locator('#trip-title').fill('Coast vacation');
  await page.getByRole('button', { name: 'Save changes', exact: true }).click(); await expect(page).toHaveURL(tripUrl);
  await page.getByRole('link', { name: 'Edit trip details' }).click();
  await page.locator('#trip-title').fill('Unsaved'); await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(page).toHaveURL(tripUrl); await expect(page.getByRole('heading', { name: 'Coast vacation', exact: true })).toBeVisible();
  expect(data.errors).toEqual([]);
});
