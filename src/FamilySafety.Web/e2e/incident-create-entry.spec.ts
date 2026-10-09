import { expect, test } from '@playwright/test';
import { apiBaseUrl, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

useIsolatedScenario({ tripAudienceAdult: true });

test('direct entry, refresh and list entry load equivalent authorized Incident member choices', async ({ page, request }) => {
  page.setDefaultTimeout(15_000);
  await recordScenario(test.info());
  const family = (await (await request.get(`${apiBaseUrl}/api/family-groups/mine`)).json())[0];
  const memberPath = `/api/family-groups/${family.familyGroupId}/members`;
  const members: { profileId: string; displayName: string }[] = await (await request.get(`${apiBaseUrl}${memberPath}`)).json();
  const owner = await (await request.get(`${apiBaseUrl}/api/profile`)).json();
  const others = members.filter(m => m.profileId !== owner.profileId);
  expect(others.length).toBeGreaterThan(0);
  const form = page.locator('app-incident-create');
  const subject = form.getByLabel('Who is this Incident about?', { exact: true });
  const choices = async () => ({
    subjects: await subject.locator('option').evaluateAll(options => options.map(o => (o as HTMLOptionElement).value)),
    people: await form.getByRole('group', { name: 'People with access', exact: true }).getByRole('checkbox').evaluateAll(boxes => boxes.map(box => box.closest('label')!.textContent!.trim())),
  });
  const ready = async () => {
    for (const member of others) await expect(subject.locator(`option[value="${member.profileId}"]`)).toHaveText(member.displayName);
    await expect(form.getByRole('checkbox', { name: others[0].displayName, exact: true })).not.toBeChecked();
    await expect(form.getByRole('button', { name: 'Create Incident', exact: true })).toBeEnabled();
  };
  // Force the shell context to arrive after the creation component mounts.
  let releaseFamily!: () => void;
  const familyGate = new Promise<void>(resolve => releaseFamily = resolve);
  await page.route('**/api/family-groups/mine', async route => { await familyGate; await route.continue(); });
  try {
    await page.goto('/incidents/new');
    await expect(subject).toBeVisible();
    for (const member of others) await expect(subject.locator(`option[value="${member.profileId}"]`)).toHaveCount(0);
  } finally { releaseFamily(); }
  await ready();
  await page.unroute('**/api/family-groups/mine');
  const directChoices = await choices();
  expect(directChoices.subjects).toEqual(['', owner.profileId, ...others.map(m => m.profileId)]);

  // A delayed protected read must expose loading and block creation until it resolves.
  let releaseMembers!: () => void;
  const memberGate = new Promise<void>(resolve => releaseMembers = resolve);
  await page.route(`**${memberPath}`, async route => { await memberGate; await route.continue(); });
  try {
    await page.reload();
    await expect(form.getByRole('status')).toContainText('Loading active adult family members');
    await expect(form.getByRole('button', { name: 'Create Incident', exact: true })).toBeDisabled();
  } finally { releaseMembers(); }
  await ready();
  expect(await choices()).toEqual(directChoices);
  await page.unroute(`**${memberPath}`);

  await page.getByRole('button', { name: /Back to Incidents/ }).click();
  await page.getByRole('button', { name: 'Start an Incident', exact: true }).click();
  await ready();
  expect(await choices()).toEqual(directChoices);

  await page.route(`**${memberPath}`, route => route.fulfill({ status: 500 }));
  await page.reload();
  await expect(form.getByRole('alert')).toContainText('Family members could not be loaded');
  await expect(form.getByRole('button', { name: 'Create Incident', exact: true })).toBeDisabled();
  for (const member of others) await expect(subject.locator(`option[value="${member.profileId}"]`)).toHaveCount(0);
  await page.unroute(`**${memberPath}`);
  await form.getByRole('button', { name: 'Retry loading family members', exact: true }).click();
  await ready();
  expect(await choices()).toEqual(directChoices);

  // Adult B has no membership. Knowing the Family ID must not grant member access.
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities;
  const outsider = identities.find((i: { label: string }) => i.label.startsWith('Adult B'));
  const denied = await request.get(`${apiBaseUrl}${memberPath}`, { headers: { [developmentAccountHeader]: outsider.accountId } });
  expect(denied.status()).toBe(404);
  expect(await denied.text()).toBe('');
  await page.getByLabel('Active test identity').selectOption(outsider.accountId);
  await page.goto('/incidents/new');
  await expect(form).toContainText('Choose your current Family above');
  for (const member of members) await expect(subject.locator(`option[value="${member.profileId}"]`)).toHaveCount(0);
  await expect(form.getByRole('button', { name: 'Create Incident', exact: true })).toBeDisabled();
  const incidents = await request.get(`${apiBaseUrl}/api/incidents`);
  expect(incidents.ok()).toBeTruthy();
  expect(await incidents.json()).toEqual([]);
});
