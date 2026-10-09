import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { mkdirSync } from 'node:fs';
import { resolve } from 'node:path';

const own = '11111111-1111-4111-8111-111111111111';
const familyId = '33333333-3333-4333-8333-333333333333';
const captures = resolve(__dirname, '../../../artifacts/reviews/safety-alignment-2026-10-08');

async function fixture(page: Page) {
  const writes: string[] = [];
  const errors: string[] = [];
  let state: 'empty' | 'populated' | 'error' | 'loading' = 'empty';
  page.on('pageerror', e => errors.push(e.message));
  await page.route('**/api/**', async route => {
    const req = route.request(), path = new URL(req.url()).pathname;
    if (req.method() !== 'GET') { writes.push(path); return route.fulfill({ status: 405, json: {} }); }
    const send = (json: unknown, status = 200) => route.fulfill({ status, json });
    if (path === '/api/development/identities') return send({}, 404);
    if (path === '/api/profile') return send({ profileId: own, displayName: 'Review User' });
    if (path === '/api/family-groups/mine') return send([{ familyGroupId: familyId, name: 'Review Family' }]);
    if (path.endsWith('/members')) return send([{ profileId: own, displayName: 'Review User' }]);
    if (path === '/api/concerns' || path === '/api/incidents') {
      if (state === 'loading') await new Promise(resolve => setTimeout(resolve, 1500));
      if (state === 'error') return send({}, 503);
      if (state === 'populated') return send([{
        concernId: own, incidentId: own, familyGroupId: familyId, familyGroupName: 'Review Family',
        subjectName: 'A family member with a deliberately long display name for wrapping',
        subjectProfileId: own, creatorProfileId: own, coordinatorProfileId: own, coordinatorName: 'Review User',
        state: 'Open', createdAt: '2026-10-08T12:00:00Z', lastAuthorizedMutationAt: '2026-10-08T12:00:00Z',
      }]);
    }
    if (path === '/api/check-ins/mine' && state === 'populated') return send([{
      checkInId: own, familyGroupId: familyId, authorProfileId: own, authorDisplayName: 'Review User',
      createdAtUtc: '2026-10-08T12:00:00Z', status: 'Delayed', note: 'A synthetic status report for layout review.', recipientProfileIds: [],
    }]);
    if (path === '/api/sharing-sessions/received' && state === 'populated') return send([{
      sharingSessionId: familyId, familyGroupId: familyId, authorProfileId: familyId,
      authorDisplayName: 'Synthetic recipient-visible sharing', state: 'Active', version: 1,
      startedAtUtc: new Date().toISOString(), endsAtUtc: new Date(Date.now() + 3600000).toISOString(),
      recipients: null, location: null,
    }]);
    // Never simulate an own active sharing session: it could request geolocation.
    return send([]);
  });
  return { writes, errors, setState: (next: typeof state) => { state = next; } };
}

test('Safety layout, representative states, accessibility and fragment navigation', async ({ page }) => {
  test.setTimeout(180_000);
  const data = await fixture(page);
  mkdirSync(captures, { recursive: true });
  for (const width of [1440, 768, 390]) {
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    // Compare the implemented reference pages at the same viewport. Safety keeps
    // Home's shared gutters; Family/Activities retain their scoped wider layout.
    const references = new Map<string, { background: string; padding: string; heading: string }>();
    for (const path of ['/home', '/family', '/activities', '/me', '/safety']) {
      await page.goto(path);
      await expect(page.locator('main h1')).toBeVisible();
      references.set(path, await page.locator('main').evaluate(e => ({
        background: getComputedStyle(e).backgroundImage, padding: getComputedStyle(e).padding,
        heading: getComputedStyle(e.querySelector('h1')!).fontSize,
      })));
    }
    for (const path of ['/home', '/family', '/activities', '/me']) {
      expect(references.get('/safety')!.background).toBe(references.get(path)!.background);
    }
    expect(references.get('/safety')!.padding).toBe(references.get('/home')!.padding);
    expect(references.get('/safety')!.heading).toBe(references.get('/home')!.heading);
    for (const state of ['empty', 'populated', 'error', 'loading'] as const) {
      data.setState(state);
      await page.goto('/safety');
      await expect(page.locator('#safety-concerns-heading')).toBeVisible();
      if (state === 'loading') await expect(page.getByText('Loading Concerns…', { exact: true })).toBeVisible();
      else await page.waitForLoadState('networkidle');
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      expect((await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa']).analyze()).violations).toEqual([]);
      const columns = await page.locator('.safety-grid').evaluate(e => getComputedStyle(e).gridTemplateColumns.split(' ').length);
      expect(columns).toBe(width === 1440 ? 2 : 1);
      const clipped = await page.locator('main button, main a').evaluateAll(elements => elements.filter(e => {
        const r = e.getBoundingClientRect(); return r.width && (r.right > innerWidth || e.scrollWidth > e.clientWidth + 1);
      }).map(e => e.textContent));
      expect(clipped).toEqual([]);
      expect(await page.locator('main button, main a').evaluateAll(elements => elements.filter(e => {
        const r = e.getBoundingClientRect(); return r.width && r.height < 44;
      }).map(e => e.textContent))).toEqual([]);
      if (width === 1440 && state === 'populated') await page.screenshot({ path: resolve(captures, 'safety-desktop.png'), fullPage: true });
      if (width !== 1440 && state === 'populated') await page.screenshot({ path: test.info().outputPath(`safety-${width}.png`), fullPage: true });
    }
    data.setState('empty');
    await page.goto('/home');
    const checkIns = page.getByRole('link', { name: 'Open Check-Ins', exact: true });
    await checkIns.focus();
    expect(await checkIns.evaluate(e => getComputedStyle(e).outlineStyle)).toBe('solid');
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(/\/safety#check-ins$/);
    await expect(page.locator('#check-ins')).toBeInViewport();
    await expect(page.locator('#check-ins-heading')).toBeFocused();
    await page.reload();
    await expect(page.locator('#check-ins')).toBeInViewport();
    await page.goto('/safety');
    await page.getByRole('link', { name: 'Open Check-Ins', exact: true }).click();
    await expect(page.locator('#check-ins')).toBeInViewport();
    await page.goto('/safety');
    await page.getByRole('link', { name: 'View all Concerns', exact: true }).click();
    await expect(page).toHaveURL(/\/concerns$/);
    await page.getByRole('link', { name: 'Back to Safety', exact: true }).click();
    await page.getByRole('link', { name: 'View all Safety Incidents', exact: true }).click();
    await expect(page).toHaveURL(/\/incidents$/);
  }
  expect(data.writes).toEqual([]);
  expect(data.errors).toEqual([]);
});
