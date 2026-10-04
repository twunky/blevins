import { expect, test } from '@playwright/test';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: false });
const captures = captureDirectory('phase9-home-filtering');
function date(days: number): string {
  const d = new Date(); d.setDate(d.getDate() + days);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

test('Home Plan date filtering preserves empty/list access, overnight relevance and capped links at desktop/mobile', async ({ page, request }) => {
  test.setTimeout(120_000);
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as { accountId: string; label: string }[];
  const a = identities.find(i => i.label.startsWith('Adult A'))!;
  const family = (await apiJson<{ familyGroupId: string }[]>(request, '/api/family-groups/mine', a.accountId)).body[0];
  const ids = new Map<string, string>();
  const create = async (name: string, start: number, expectedReturn: number | null, time = '09:00') => {
    const response = await request.post(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/plans`, {
      headers: { [developmentAccountHeader]: a.accountId },
      data: { activityOrDestination: name, date: date(start), time,
        expectedReturnDate: expectedReturn === null ? null : date(expectedReturn),
        expectedReturnTime: expectedReturn === null ? null : '00:00',
        expectedCheckInDate: null, expectedCheckInTime: null, notes: null,
        audienceType: 'FamilyGroup', audienceProfileIds: [] },
    });
    expect(response.status(), `create ${name}`).toBe(201);
    ids.set(name, (await response.json()).planId);
  };
  const home = page.locator('[aria-labelledby="home-plans-heading"]');
  const render = async (state: string, names: string[]) => {
    for (const width of [1440, 390]) {
      await page.setViewportSize({ width, height: width === 390 ? 844 : 1000 });
      await page.goto('/home');
      await expect(page.locator('.current-family strong')).toHaveText('Development Family');
      await expect(home.locator('li a')).toHaveText(names);
      for (const name of names) await expect(home.getByRole('link', { name, exact: true })).toHaveAttribute('href', `/plans/${ids.get(name)}`);
      await expect(home.getByText('Returned yesterday', { exact: true })).toHaveCount(0);
      await expect(home.getByText('Past without return', { exact: true })).toHaveCount(0);
      expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
      await page.screenshot({ path: resolve(captures, `${state}-${width}.png`), fullPage: true });
    }
  };
  await create('Returned yesterday', -2, -1);
  await create('Past without return', -1, null);
  await render('past-only-empty', []);
  await expect(home.getByText('No Plans are visible to you yet.', { exact: true })).toBeVisible();
  await home.getByRole('button', { name: 'Open Plans', exact: true }).click();
  await expect(page).toHaveURL(/\/plans$/);
  await expect(page.getByRole('link', { name: 'Returned yesterday', exact: true })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Past without return', exact: true })).toBeVisible();
  await create('Future Plan', 2, null);
  await create('Starts today', 0, null);
  await render('today-future', ['Starts today', 'Future Plan']);
  await create('Returns tomorrow', -1, 1, '09:00');
  await create('Returns today', -1, 0, '10:00');
  await render('overnight-capped', ['Returns tomorrow', 'Returns today', 'Starts today']);
  await expect(home.getByRole('link', { name: 'Future Plan', exact: true })).toHaveCount(0);
  await home.getByRole('link', { name: 'Returns tomorrow', exact: true }).click();
  await expect(page).toHaveURL(new RegExp(`/plans/${ids.get('Returns tomorrow')}$`));
  await page.reload();
  await expect(page.getByRole('heading', { name: 'Returns tomorrow', exact: true })).toBeVisible();
  await page.getByRole('link', { name: 'Back to Plans', exact: true }).click();
  await expect(page).toHaveURL(/\/plans$/);
});
