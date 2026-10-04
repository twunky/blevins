import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';
import { writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

// WI-009-011 / PRP-030 M10. No disabled rules or excluded elements.
useIsolatedScenario({ tripAudienceAdult: true });
const captures = captureDirectory('phase9-exit-accessibility');
const routes = ['/home', '/family', '/family/settings', '/events', '/events/new', '/events/all',
  '/plans-trips', '/plans', '/trips', '/safety', '/concerns', '/incidents', '/me'];
interface Identity { accountId: string; label: string }
function date(days: number): string {
  const d = new Date(); d.setDate(d.getDate() + days);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}
let seeded = false;

for (const width of [1440, 390]) {
  test(`primary routes: axe and rendered Home freshness at ${width}px`, async ({ page, request }) => {
    test.setTimeout(300_000);
    await recordScenario(test.info());
    const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
    const a = identities.find(i => i.label.startsWith('Adult A'))!;
    const b = identities.find(i => i.label.startsWith('Adult B'))!;
    if (!seeded) {
      const family = (await apiJson<{ familyGroupId: string }[]>(request, '/api/family-groups/mine', a.accountId)).body[0];
      const members = (await apiJson<{ profileId: string; displayName: string }[]>(request,
        `/api/family-groups/${family.familyGroupId}/members`, a.accountId)).body;
      const own = members.find(m => m.displayName === 'Development Adult')!.profileId;
      const other = members.find(m => m.displayName === 'Development Audience Member')!.profileId;
      const create = async (kind: string, data: unknown) => {
        const r = await request.post(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/${kind}`,
          { headers: { [developmentAccountHeader]: a.accountId }, data });
        expect(r.status(), `seed ${kind}`).toBe(201);
      };
      for (const [title, start, end] of [['Ended Trip', -8, -1], ['Current Trip', -2, 0], ['Upcoming Trip', 2, 4]] as const) {
        await create('trips', { title, primaryDestination: 'Review coast', startDate: date(start), endDate: date(end),
          description: null, transportationNotes: null, accommodationNotes: null, expectedArrivalDate: null,
          expectedArrivalTime: null, expectedArrivalTimeZone: null, travelerProfileIds: [own], contacts: [],
          audienceType: 'FamilyGroup', audienceProfileIds: [] });
      }
      await create('events', { name: 'Upcoming Event', description: 'Review', date: date(2), time: '18:30',
        location: 'Review hall', audienceProfileIds: [other], editorProfileIds: [] });
      await create('plans', { activityOrDestination: 'Upcoming Plan', date: date(2), time: '09:00',
        expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null,
        notes: null, audienceType: 'FamilyGroup', audienceProfileIds: [] });
      await create('concerns', { subjectProfileId: other, reason: 'Review concern', participantProfileIds: [other] });
      await create('incidents', { subjectProfileId: other, reason: 'Review incident', reasonCategory: 'Reported',
        reasonSourceDescription: 'Development Adult', reasonSourceUnknown: false, participantProfileIds: [other], originatingConcernId: null });
      seeded = true;
    }
    await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
    const reports: unknown[] = [];
    const findings: unknown[] = [];
    for (const route of routes) {
      await page.goto(route);
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
      await expect(page.locator('.current-family strong')).toHaveText('Development Family');
      // Wait for each route's data reads and lazy feature component before scanning.
      await page.waitForLoadState('networkidle');
      const result = await new AxeBuilder({ page }).analyze();
      reports.push({ route, width, violations: result.violations, incomplete: result.incomplete,
        passes: result.passes.map(p => p.id), testEngine: result.testEngine });
      findings.push(...result.violations.filter(v => v.impact === 'serious' || v.impact === 'critical')
        .map(v => ({ route, id: v.id, impact: v.impact, nodes: v.nodes })));
      expect(await page.evaluate(() => document.documentElement.scrollWidth), `${route} overflow`).toBeLessThanOrEqual(width);
      await page.screenshot({ path: resolve(captures, `${route.slice(1).replaceAll('/', '-')}-${width}.png`), fullPage: true });
      if (route === '/home') {
        const trips = page.locator('[aria-labelledby="home-trips-heading"]');
        await expect(trips.getByRole('link', { name: /Current Trip/ })).toBeVisible();
        await expect(trips.getByRole('link', { name: /Upcoming Trip/ })).toBeVisible();
        await expect(trips.getByText('Ended Trip', { exact: true })).toHaveCount(0);
      }
      if (route === '/trips') await expect(page.getByRole('link', { name: /Ended Trip/ })).toBeVisible();
    }
    // No-Family and ordinary member states are additional scans, not substituted primary routes.
    for (const [prefix, state] of [['Adult B', 'no-family'], ['Adult C', 'member']] as const) {
      await page.getByLabel('Active test identity').selectOption(identities.find(i => i.label.startsWith(prefix))!.accountId);
      await expect(page).toHaveURL(/\/home$/);
      await page.waitForLoadState('networkidle');
      const result = await new AxeBuilder({ page }).analyze();
      reports.push({ route: '/home', state, width, violations: result.violations, incomplete: result.incomplete });
      findings.push(...result.violations.filter(v => v.impact === 'serious' || v.impact === 'critical')
        .map(v => ({ state, id: v.id, nodes: v.nodes })));
      if (prefix === 'Adult B') expect(await page.getByLabel('Active test identity').inputValue()).toBe(b.accountId);
    }
    const json = JSON.stringify(reports, null, 2);
    writeFileSync(resolve(captures, `axe-${width}.json`), json);
    await test.info().attach(`axe-${width}.json`, { body: json, contentType: 'application/json' });
    expect(findings, 'Unfiltered axe serious/critical findings').toEqual([]);
  });
}
