import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { captureDirectory, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

// Only the harness-owned synthetic database is used. These checks open drafts;
// they never submit changes to a human-testing or ordinary Development database.
useIsolatedScenario({ tripAudienceAdult: true, secondFamily: true });

const widths = [1440, 768, 390];
const surfaces: { name: string; path: string; open?: (page: Page) => Promise<void> }[] = [
  { name: 'home', path: '/home' },
  { name: 'family', path: '/family' },
  { name: 'family-settings', path: '/family/settings' },
  { name: 'household-form', path: '/family/settings', open: async page => {
    await page.getByRole('button', { name: 'Create a Household', exact: true }).click();
  } },
  { name: 'family-date-form', path: '/family', open: async page => {
    await page.getByRole('button', { name: 'Add a date', exact: true }).click();
  } },
  { name: 'profile', path: '/me' },
  { name: 'events', path: '/events' },
  { name: 'events-all', path: '/events/all' },
  { name: 'event-create', path: '/events/new' },
  { name: 'plans-trips', path: '/plans-trips' },
  { name: 'plans', path: '/plans' },
  { name: 'trips', path: '/trips' },
  { name: 'trip-create', path: '/trips', open: async page => {
    await page.getByRole('button', { name: 'Create trip', exact: true }).click();
  } },
  { name: 'safety', path: '/safety' },
  { name: 'check-in-form', path: '/safety', open: async page => {
    await page.getByRole('button', { name: 'Create a Check-In', exact: true }).click();
  } },
  { name: 'sharing-form', path: '/safety', open: async page => {
    await page.getByRole('button', { name: 'Start location sharing', exact: true }).click();
  } },
  { name: 'circle-form', path: '/safety', open: async page => {
    await page.getByRole('button', { name: 'Create a Safety Circle', exact: true }).click();
  } },
  { name: 'contact-form', path: '/safety', open: async page => {
    await page.getByRole('button', { name: 'Add a Trusted Contact', exact: true }).click();
  } },
  { name: 'concerns', path: '/concerns' },
  { name: 'concern-create', path: '/concerns', open: async page => {
    await page.getByRole('button', { name: 'Create a Concern', exact: true }).click();
  } },
  { name: 'incidents', path: '/incidents' },
  { name: 'incident-create', path: '/incidents/new' },
  { name: 'unavailable-event', path: '/events/00000000-0000-0000-0000-000000000001' },
];

for (const width of widths) {
  test(`dark surfaces, drafts and navigation stay accessible at ${width}px`, async ({ page, request }) => {
    test.setTimeout(300_000);
    await recordScenario(test.info());
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    const errors: string[] = [];
    page.on('pageerror', error => errors.push(error.message));
    await page.goto('/home');
    // With two memberships, context must still be explicitly selected.
    const familyResponse = await request.get('http://localhost:5080/api/family-groups/mine');
    const families = await familyResponse.json() as { familyGroupId: string; name: string }[];
    const family = families.find(item => item.name === 'Development Family')!;
    await page.getByLabel('Current Family', { exact: true }).selectOption(family.familyGroupId);

    for (const surface of surfaces) {
      await test.step(surface.name, async () => {
        await page.goto(surface.path);
        // Direct loads intentionally reset the Family context; choose it again.
        await page.getByLabel('Current Family', { exact: true }).selectOption(family.familyGroupId);
        await expect(page.locator('main h1')).toHaveCount(1);
        await page.waitForLoadState('networkidle');
        if (surface.open) await surface.open(page);
        await expect(page.getByRole('navigation', { name: 'Main navigation' }).getByRole('link')).toHaveCount(6);
        expect(await page.evaluate(() => document.documentElement.scrollWidth), surface.name).toBeLessThanOrEqual(width);
        const brightControls = await page.locator('main input:not([type=checkbox]):not([type=radio]), main select, main textarea').evaluateAll(elements => elements.filter(element => {
          if (!(element as HTMLElement).offsetParent) return false;
          const rgb = getComputedStyle(element).backgroundColor.match(/\d+/g)?.map(Number) ?? [];
          return rgb.length >= 3 && Math.min(...rgb.slice(0, 3)) > 180;
        }).map(element => element.id));
        expect(brightControls, `${surface.name}: native controls must use dark surfaces`).toEqual([]);
        expect((await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa']).analyze()).violations, surface.name).toEqual([]);
        await page.screenshot({ path: resolve(captureDirectory('visual-design-rollout'), `${surface.name}-${width}.png`), fullPage: true });
      });
    }
    expect(errors).toEqual([]);
    await page.emulateMedia({ reducedMotion: 'reduce' });
    const navigation = page.getByRole('navigation', { name: 'Main navigation' });
    const home = navigation.getByRole('link', { name: 'Home', exact: true });
    await home.focus();
    expect(await home.evaluate(element => getComputedStyle(element).outlineStyle)).toBe('solid');
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(/\/home$/);
    await expect(page.locator('main h1')).toBeFocused();
    await page.getByRole('button', { name: 'Open Events', exact: true }).click();
    await expect(page).toHaveURL(/\/events$/);
    const action = page.getByRole('link', { name: 'Create event', exact: true });
    expect(await action.evaluate(element => getComputedStyle(element).transitionDuration)).toBe('0s');
  });
}
