import { expect, test, type APIRequestContext, type Locator, type Page } from '@playwright/test';
import { resolve } from 'node:path';
import {
  apiBaseUrl,
  apiJson,
  captureDirectory,
  expectHealthyApi,
  recordScenario,
  useIsolatedScenario,
} from './fixtures/invitation-scenario';

// WI-009-003: six-area main navigation (REQ-UX-013). Adult A (the default identity) is an active Family member,
// which is all these journeys need, so no extra adult is seeded.
useIsolatedScenario({ tripAudienceAdult: false });

interface DevelopmentIdentity { accountId: string; label: string }
interface FamilyGroup { familyGroupId: string; name: string }

const widths = [
  { name: 'desktop', viewport: { width: 1440, height: 1000 } },
  { name: 'narrow', viewport: { width: 390, height: 844 } },
] as const;
const labels = ['Home', 'Family', 'Events', 'Plans & Trips', 'Safety', 'Me / Settings'];
const hrefs = ['/home', '/family', '/events', '/plans-trips', '/safety', '/me'];
// Transient captures (WI-009-002): routine runs never write canonical evidence under ai-context/.
const captures = captureDirectory('navigation');

const mainNavigation = (page: Page) => page.getByRole('navigation', { name: 'Main navigation' });
const heading = (page: Page) => page.locator('main h1');
// WI-009-006: Home greets by local time and the signed-in display name; the Family name is in the subtitle.
const homeGreeting = /^Good (morning|afternoon|evening)(, .+)?!$/;

async function setUp(page: Page, request: APIRequestContext, viewport: { width: number; height: number }): Promise<FamilyGroup> {
  await recordScenario(test.info());
  await expectHealthyApi(request);
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json() as
    { identities: DevelopmentIdentity[] }).identities;
  const adultA = identities.find(identity => identity.label.startsWith('Adult A'))!;
  const family = (await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultA.accountId)).body[0];
  expect(family).toBeTruthy();
  await page.setViewportSize(viewport);
  await page.goto('/home');
  await expect(page.getByLabel('Active test identity')).toHaveValue(adultA.accountId);
  await expect(heading(page)).toHaveText(homeGreeting);
  await expect(page.locator('.home-title')).toContainText(`happening with ${family.name} today`);
  return family;
}

/** AC-08: exactly one destination is selected, visually and with aria-current. */
async function expectCurrent(page: Page, label: string, value: 'page' | 'true'): Promise<void> {
  const nav = mainNavigation(page);
  await expect(nav.locator('a[aria-current]')).toHaveCount(1);
  await expect(nav.locator('a[aria-current]')).toHaveText(label);
  await expect(nav.locator('a[aria-current]')).toHaveAttribute('aria-current', value);
  await expect(nav.locator('a.is-active')).toHaveCount(1);
  await expect(nav.locator('a.is-active')).toHaveText(label);
}

/** AC-01: six links, in order, with real URLs, and nothing else. */
async function expectSixLinks(page: Page): Promise<void> {
  const nav = mainNavigation(page);
  const links = nav.getByRole('link');
  await expect(links).toHaveText(labels);
  expect(await links.evaluateAll(items => items.map(item => item.getAttribute('href')))).toEqual(hrefs);
  await expect(nav.getByRole('button')).toHaveCount(0);
  for (const label of labels) await expect(nav.getByRole('link', { name: label, exact: true })).toBeVisible();
}

async function expectNoOverflow(page: Page, width: number, what: string): Promise<void> {
  expect(await page.evaluate(() => document.documentElement.scrollWidth), `${what} must not overflow`).toBeLessThanOrEqual(width);
}

async function capture(page: Page, name: string, widthName: string): Promise<void> {
  await page.screenshot({ path: resolve(captures, `${name}-${widthName}.png`), fullPage: true });
}

async function boxes(links: Locator) {
  return links.evaluateAll(items => items.map(item => {
    const box = item.getBoundingClientRect();
    return { x: Math.round(box.x), y: Math.round(box.y), width: box.width, height: box.height };
  }));
}

/** Follows a link and checks the new page: URL, single h1 with focus (D4), and selected area. */
async function follow(page: Page, link: Locator, expected: { url: string; h1: string | RegExp; area: string; current: 'page' | 'true' }) {
  await link.click();
  await expect(page).toHaveURL(new RegExp(`${expected.url}$`));
  await expect(page.locator('h1')).toHaveCount(1);
  await expect(heading(page)).toHaveText(expected.h1);
  await expect(heading(page)).toBeFocused();
  await expectCurrent(page, expected.area, expected.current);
}

for (const { name: widthName, viewport } of widths) {
  test.describe(`at ${viewport.width} px`, () => {
    test('reaches every area, and every part of each grouping, through main navigation and landings alone', async ({ page, request }) => {
      test.setTimeout(180_000);
      const family = await setUp(page, request, viewport);
      const nav = mainNavigation(page);
      const link = (label: string) => nav.getByRole('link', { name: label, exact: true });
      const landingLink = (label: string) => page.getByRole('main').getByRole('link', { name: label, exact: true });

      // Initial load: no focus movement (D4).
      await expect(heading(page)).not.toBeFocused();
      await expectSixLinks(page);
      await expectCurrent(page, 'Home', 'page');

      // Layout (AC-09, AC-10): desktop sidebar is one column; narrow is three rows of two, read row by row.
      const layout = await boxes(nav.getByRole('link'));
      if (viewport.width === 390) {
        const rows = [...new Set(layout.map(box => box.y))];
        expect(rows).toHaveLength(3);
        expect(layout.map(box => rows.indexOf(box.y))).toEqual([0, 0, 1, 1, 2, 2]);
        for (let index = 0; index < 6; index += 2) expect(layout[index].x).toBeLessThan(layout[index + 1].x);
        for (const box of layout) {
          expect(box.width).toBeGreaterThanOrEqual(44);
          expect(box.height).toBeGreaterThanOrEqual(44);
        }
      } else {
        expect(new Set(layout.map(box => box.x)).size).toBe(1);
        expect(layout.map(box => box.y)).toEqual([...layout.map(box => box.y)].sort((a, b) => a - b));
        for (const box of layout) expect(box.height).toBeGreaterThanOrEqual(44);
      }
      await expectNoOverflow(page, viewport.width, 'Home');
      await capture(page, 'home', widthName);

      // WI-009-003 AC-15, superseded for /safety by DEC-079 (WI-009-009): /safety is now the read-only Safety dashboard,
      // so it may read the two authorized Safety list endpoints. It still makes no Trip, Check-In, or other domain request.
      const safetyDataRequests: string[] = [];
      const onRequest = (sent: { url(): string }) => {
        if (/\/api\/(concerns|incidents|trips|checkins?)/i.test(sent.url())) safetyDataRequests.push(new URL(sent.url()).pathname);
      };

      const steps: { via: () => Locator; url: string; h1: string | RegExp; area: string; current: 'page' | 'true'; capture: string }[] = [
        { via: () => link('Family'), url: '/family', h1: 'Family', area: 'Family', current: 'page', capture: 'family' },
        // WI-009-007: the gear opens Family settings, a Family child page; Back to Family returns.
        { via: () => page.getByRole('main').getByRole('link', { name: 'Family settings', exact: true }), url: '/family/settings', h1: 'Family settings', area: 'Family', current: 'true', capture: 'family-settings' },
        { via: () => landingLink('Back to Family'), url: '/family', h1: 'Family', area: 'Family', current: 'page', capture: '' },
        { via: () => link('Events'), url: '/events', h1: 'Events', area: 'Events', current: 'page', capture: 'events' },
        { via: () => link('Plans & Trips'), url: '/plans-trips', h1: 'Plans & Trips', area: 'Plans & Trips', current: 'page', capture: 'plans-trips-landing' },
        { via: () => landingLink('Open Plans'), url: '/plans', h1: 'Plans', area: 'Plans & Trips', current: 'true', capture: 'plans' },
        { via: () => link('Plans & Trips'), url: '/plans-trips', h1: 'Plans & Trips', area: 'Plans & Trips', current: 'page', capture: '' },
        { via: () => landingLink('Open Trips'), url: '/trips', h1: 'Trips', area: 'Plans & Trips', current: 'true', capture: 'trips' },
        // WI-009-009: the Safety dashboard's View all links reach Concerns and Incidents; Back to Safety returns.
        { via: () => link('Safety'), url: '/safety', h1: 'Safety', area: 'Safety', current: 'page', capture: 'safety-dashboard' },
        { via: () => landingLink('View all Concerns'), url: '/concerns', h1: 'Concerns', area: 'Safety', current: 'true', capture: 'concerns' },
        { via: () => landingLink('Back to Safety'), url: '/safety', h1: 'Safety', area: 'Safety', current: 'page', capture: '' },
        { via: () => landingLink('View all Safety Incidents'), url: '/incidents', h1: 'Safety Incidents', area: 'Safety', current: 'true', capture: 'incidents' },
        { via: () => link('Me / Settings'), url: '/me', h1: 'Your profile', area: 'Me / Settings', current: 'page', capture: 'me' },
        { via: () => link('Home'), url: '/home', h1: homeGreeting, area: 'Home', current: 'page', capture: '' },
      ];
      for (const step of steps) {
        const watchSafety = step.url === '/safety';
        if (watchSafety) { safetyDataRequests.length = 0; page.on('request', onRequest); }
        await follow(page, step.via(), step);
        await expectSixLinks(page);
        if (step.url === '/plans-trips') {
          const cards = page.locator('app-group-landing section.card');
          await expect(cards).toHaveCount(2);
          await expect(cards.getByRole('heading', { level: 2 })).toHaveText(['Plans', 'Trips']);
          await expect(page.locator('app-group-landing')).not.toContainText(/\d/);
        }
        if (step.url === '/safety') {
          await expect(page.locator('app-safety-dashboard section.card').getByRole('heading', { level: 2 }))
            .toHaveText(['Open Concerns', 'Open Safety Incidents', 'Recent Check-Ins', 'About Concerns and Incidents']);
        }
        if (step.url === '/concerns' || step.url === '/incidents') await page.waitForLoadState('networkidle');
        if (viewport.width === 390) await expectNoOverflow(page, viewport.width, step.url);
        if (step.capture) await capture(page, step.capture, widthName);
        if (watchSafety) {
          await page.waitForLoadState('networkidle');
          page.off('request', onRequest);
          expect([...new Set(safetyDataRequests)].sort(), 'the Safety dashboard reads only the two authorized Safety lists')
            .toEqual(['/api/concerns', '/api/incidents']);
        }
      }
    });

    test('works by keyboard: logical order, visible focus, Enter follows links, and focus moves only after user navigation', async ({ page, request }) => {
      test.setTimeout(120_000);
      await setUp(page, request, viewport);
      const nav = mainNavigation(page);
      const focusedText = () => page.evaluate(() => document.activeElement?.textContent?.trim() ?? '');
      const focusedOutline = () => page.evaluate(() => {
        const style = getComputedStyle(document.activeElement!);
        return `${style.outlineStyle} ${style.outlineWidth}`;
      });

      // Tab from the start of the page reaches the six links after the top-bar controls, in order.
      await page.locator('body').focus();
      for (let presses = 0; presses < 25 && await focusedText() !== 'Home'; presses++) await page.keyboard.press('Tab');
      expect(await page.evaluate(() => !!document.activeElement?.closest('nav[aria-label="Main navigation"]'))).toBe(true);
      const order = [await focusedText()];
      expect(await focusedOutline()).toBe('solid 3px');
      for (let index = 1; index < labels.length; index++) {
        await page.keyboard.press('Tab');
        order.push(await focusedText());
        expect(await focusedOutline(), `${labels[index]} focus is visible`).toBe('solid 3px');
      }
      expect(order).toEqual(labels);

      // Enter on a nav link follows it; focus lands on the page h1 with a visible focus indication.
      await nav.getByRole('link', { name: 'Plans & Trips', exact: true }).focus();
      await page.keyboard.press('Enter');
      await expect(page).toHaveURL(/\/plans-trips$/);
      await expect(heading(page)).toBeFocused();
      expect(await focusedOutline()).toBe('solid 3px');
      await expectCurrent(page, 'Plans & Trips', 'page');

      // From the h1, Tab reaches the landing links in order; Enter follows one.
      await page.keyboard.press('Tab');
      expect(await focusedText()).toBe('Open Plans');
      expect(await focusedOutline()).toBe('solid 3px');
      await page.keyboard.press('Tab');
      expect(await focusedText()).toBe('Open Trips');
      await page.keyboard.press('Shift+Tab');
      await page.keyboard.press('Enter');
      await expect(page).toHaveURL(/\/plans$/);
      await expect(heading(page)).toHaveText('Plans');
      await expect(heading(page)).toBeFocused();
      await expectCurrent(page, 'Plans & Trips', 'true');

      // Refresh restores history state, but it is the initial navigation: no focus movement.
      await page.reload();
      await expect(heading(page)).toHaveText('Plans');
      await expectCurrent(page, 'Plans & Trips', 'true');
      await expect(heading(page)).not.toBeFocused();

      // Back is a popstate navigation: natural, no focus movement.
      await page.goBack();
      await expect(page).toHaveURL(/\/plans-trips$/);
      await expect(heading(page)).toHaveText('Plans & Trips');
      await expect(heading(page)).not.toBeFocused();
      await page.goForward();
      await expect(page).toHaveURL(/\/plans$/);
      await expect(heading(page)).not.toBeFocused();
    });

    test('loads each landing and child route directly and after refresh, with the correct page and area', async ({ page, request }) => {
      test.setTimeout(120_000);
      await setUp(page, request, viewport);
      const routes = [
        { url: '/plans-trips', h1: 'Plans & Trips', area: 'Plans & Trips', current: 'page' },
        { url: '/safety', h1: 'Safety', area: 'Safety', current: 'page' },
        { url: '/plans', h1: 'Plans', area: 'Plans & Trips', current: 'true' },
        { url: '/trips', h1: 'Trips', area: 'Plans & Trips', current: 'true' },
        { url: '/concerns', h1: 'Concerns', area: 'Safety', current: 'true' },
        { url: '/incidents', h1: 'Safety Incidents', area: 'Safety', current: 'true' },
        { url: '/incidents/new', h1: 'Safety Incidents', area: 'Safety', current: 'true' },
      ] as const;
      for (const route of routes) {
        await page.goto(route.url);
        for (const pass of ['direct', 'refresh']) {
          if (pass === 'refresh') await page.reload();
          await expect(page, `${route.url} ${pass}`).toHaveURL(new RegExp(`${route.url}$`));
          await expect(heading(page)).toHaveText(route.h1);
          await expectCurrent(page, route.area, route.current);
          await expect(heading(page)).not.toBeFocused();
          await expectSixLinks(page);
          if (viewport.width === 390) await expectNoOverflow(page, viewport.width, `${route.url} ${pass}`);
        }
      }
      await page.goto('/not-a-real-area');
      await expect(page).toHaveURL(/\/home$/);
      await expectCurrent(page, 'Home', 'page');
    });
  });
}

test('keeps all six destinations visible, in order, without overflow at intermediate widths (D2)', async ({ page, request }) => {
  await setUp(page, request, { width: 1024, height: 900 });
  for (const width of [1024, 768, 600]) {
    await page.setViewportSize({ width, height: 900 });
    await expectSixLinks(page);
    await expectNoOverflow(page, width, `Home at ${width} px`);
    const layout = await boxes(mainNavigation(page).getByRole('link'));
    // Row by row, left to right: the visual order is the logical order.
    const visualOrder = layout.map((box, index) => ({ ...box, index }))
      .sort((left, right) => left.y - right.y || left.x - right.x).map(box => box.index);
    expect(visualOrder).toEqual([0, 1, 2, 3, 4, 5]);
    for (const box of layout) expect(box.height).toBeGreaterThanOrEqual(44);
    await capture(page, 'home', `${width}`);
  }
});
