import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, apiJson, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: false });
interface Identity { accountId: string; label: string }
interface Contact { trustedContactId: string; displayName: string; version: string }
const component = (page: Page) => page.locator('app-trusted-contacts');
async function actAs(page: Page, actor: Identity) {
  await page.goto('/home'); await page.getByLabel('Active test identity').selectOption(actor.accountId);
  await page.goto('/safety'); await expect(component(page)).toBeVisible();
  await expect(component(page)).not.toContainText('Loading Trusted Contacts');
}

test('private manual contacts work without Family membership; owner edits/removes; administrator gets nothing; Retry and responsive form', async ({ page, request }) => {
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities as Identity[];
  const admin = identities.find(i => i.label.startsWith('Adult A'))!;
  const owner = identities.find(i => i.label.startsWith('Adult B'))!;
  expect((await apiJson<unknown[]>(request, '/api/family-groups/mine', owner.accountId)).body).toEqual([]);
  await actAs(page, owner); await expect(component(page)).toContainText('no Trusted Contacts yet');
  await component(page).getByRole('button', { name: 'Add a Trusted Contact', exact: true }).click();
  const fill = async (name: string, phone: string, email: string, label: string) => {
    await component(page).getByLabel('Display Name', { exact: true }).fill(name);
    await component(page).getByLabel('Phone Number', { exact: true }).fill(phone);
    await component(page).getByLabel('Email Address', { exact: true }).fill(email);
    await component(page).getByLabel('Relationship / Label (optional)', { exact: true }).fill(label);
  };
  await fill('Manual neighbor', '+44 (20) 7946-0958', 'neighbor@example.invalid', 'Neighbor');
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('trusted-contacts'), 'form-390.png'), fullPage: true });
  await component(page).getByRole('button', { name: 'Create Trusted Contact', exact: true }).focus(); await page.keyboard.press('Enter');
  await expect(component(page)).toContainText('Trusted Contact added');
  const entry = (await apiJson<Contact[]>(request, '/api/trusted-contacts', owner.accountId)).body[0];
  await actAs(page, admin); await expect(component(page)).not.toContainText('neighbor@example.invalid');
  const denied = await request.get(`${apiBaseUrl}/api/trusted-contacts/${entry.trustedContactId}?selectedFamily=known&ownerProfileId=known`,
    { headers: { [developmentAccountHeader]: admin.accountId } });
  expect(denied.status()).toBe(404); expect(await denied.text()).toBe(''); expect(denied.headers()['cache-control']).toBe('no-store');
  await actAs(page, owner);
  await component(page).getByRole('button', { name: 'Add a Trusted Contact', exact: true }).click();
  await fill('Another name', '0044 20 7946 0958', '', 'Friend');
  await component(page).getByRole('button', { name: 'Create Trusted Contact', exact: true }).click();
  await expect(component(page)).toContainText('already have a Trusted Contact');
  await component(page).getByRole('button', { name: 'Cancel edit', exact: true }).click();
  await component(page).getByRole('button', { name: 'Edit contact', exact: true }).click();
  await fill('Updated neighbor', '', 'new-neighbor@example.invalid', 'Friend');
  await component(page).getByRole('button', { name: 'Save Trusted Contact', exact: true }).click();
  await expect(component(page)).toContainText('Trusted Contact updated'); await expect(component(page)).not.toContainText('+44');
  await page.setViewportSize({ width: 1440, height: 1000 });
  expect((await new AxeBuilder({ page }).analyze()).violations).toEqual([]);
  await page.screenshot({ path: resolve(captureDirectory('trusted-contacts'), 'list-1440.png'), fullPage: true });
  await page.route('**/api/trusted-contacts', route => route.fulfill({ status: 500, body: '{}' }), { times: 1 });
  await component(page).getByRole('button', { name: 'Refresh Trusted Contacts', exact: true }).click();
  await expect(component(page)).toContainText('could not be loaded');
  await component(page).getByRole('button', { name: 'Retry loading Trusted Contacts', exact: true }).click();
  await expect(component(page)).toContainText('Updated neighbor');
  await component(page).getByRole('button', { name: 'Remove contact', exact: true }).click();
  await component(page).getByRole('button', { name: 'Cancel removal', exact: true }).click();
  expect((await apiJson<Contact[]>(request, '/api/trusted-contacts', owner.accountId)).body).toHaveLength(1);
  await component(page).getByRole('button', { name: 'Remove contact', exact: true }).click();
  await component(page).getByRole('button', { name: 'Confirm removal', exact: true }).click();
  await expect(component(page)).toContainText('permanently removed');
  expect((await apiJson<Contact[]>(request, '/api/trusted-contacts', owner.accountId)).body).toEqual([]);
  expect((await request.get(`${apiBaseUrl}/api/trusted-contacts/${entry.trustedContactId}`, { headers: { [developmentAccountHeader]: owner.accountId } })).status()).toBe(404);
});
