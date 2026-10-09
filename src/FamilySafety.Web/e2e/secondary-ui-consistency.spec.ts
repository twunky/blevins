import { expect, test, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { resolve } from 'node:path';
import { apiBaseUrl, captureDirectory, developmentAccountHeader, recordScenario, useIsolatedScenario } from './fixtures/invitation-scenario';

// All populated examples belong to the existing disposable SQL scenario harness.
// Sharing is inspected as a draft only: no session or location collection starts.
useIsolatedScenario({ tripAudienceAdult: true });

test('secondary screens and drafts remain usable at desktop, tablet and phone widths', async ({ page, request }) => {
  test.setTimeout(360_000);
  await recordScenario(test.info());
  const family = (await (await request.get(`${apiBaseUrl}/api/family-groups/mine`)).json())[0];
  const members = await (await request.get(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/members`)).json();
  const owner = await (await request.get(`${apiBaseUrl}/api/profile`)).json();
  const other = members.find((m: { profileId: string }) => m.profileId !== owner.profileId);
  const post = async (path: string, data: unknown) => {
    const response = await request.post(`${apiBaseUrl}${path}`, { data });
    expect(response.ok(), await response.text()).toBeTruthy();
    return response.json();
  };
  const base = `/api/family-groups/${family.familyGroupId}`;
  const event = await post(`${base}/events`, { name: 'Secondary UI gathering', description: 'Synthetic coordination example', date: '2030-06-15', time: '18:30', location: 'Synthetic hall', audienceProfileIds: [other.profileId], editorProfileIds: [] });
  await post(`/api/events/${event.eventId}/needs`, { title: 'Bring supplies', detail: 'Synthetic responsibility', category: 'Supplies' });
  await post(`/api/events/${event.eventId}/updates`, { text: 'Synthetic organizer update.' });
  const plan = await post(`${base}/plans`, { activityOrDestination: 'Secondary UI walk', date: '2030-06-16', time: '09:00', expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, notes: 'Synthetic Plan note.', audienceType: 'SelectedPeople', audienceProfileIds: [other.profileId] });
  const trip = await post(`${base}/trips`, { title: 'Secondary UI trip', primaryDestination: 'Synthetic coast', startDate: '2030-06-17', endDate: '2030-06-19', description: null, transportationNotes: null, accommodationNotes: null, expectedArrivalDate: null, expectedArrivalTime: null, expectedArrivalTimeZone: null, travelerProfileIds: [owner.profileId], contacts: [], audienceType: 'SelectedPeople', audienceProfileIds: [other.profileId] });
  const concern = await post(`${base}/concerns`, { subjectProfileId: other.profileId, reason: 'Synthetic visual inspection only.', participantProfileIds: [other.profileId] });
  const incident = await post(`${base}/incidents`, { subjectProfileId: other.profileId, reason: 'Synthetic visual inspection only.', reasonCategory: 'Reported', reasonSourceDescription: 'Synthetic source', reasonSourceUnknown: false, participantProfileIds: [other.profileId], originatingConcernId: null });
  await post(`${base}/households`, { name: 'Synthetic Household' });
  await post(`${base}/dates`, { title: 'Synthetic Family date', description: 'Synthetic date description', date: '2030-06-20', associatedProfileIds: [other.profileId] });
  const eventPath = `/events/${event.eventId}`;
  const planPath = `/plans/${plan.planId}`;
  const tripPath = `/trips/${trip.tripId}`;
  const concernPath = `/concerns/${concern.concernId}`;
  const incidentPath = `/incidents/${incident.incidentId}`;
  const click = (name: string) => async (page: Page) => { await page.getByRole('button', { name, exact: true }).click(); };
  const surfaces: { name: string; path: string; open?: (page: Page) => Promise<void>; capture?: boolean }[] = [
    { name: 'family-settings', path: '/family/settings', capture: true },
    { name: 'household-create', path: '/family/settings', open: click('Create a Household') },
    { name: 'household-rename', path: '/family/settings', open: click('Rename Household') },
    { name: 'household-delete-confirmation', path: '/family/settings', open: click('Delete Household') },
    { name: 'date-create', path: '/family/settings', open: click('Add a date') },
    { name: 'date-edit', path: '/family/settings', open: click('Edit date') },
    { name: 'date-delete-confirmation', path: '/family/settings', open: click('Delete date') },
    { name: 'member-profile', path: `/family/${family.familyGroupId}/members/${other.profileId}` },
    { name: 'activity-chooser', path: '/activities/new' },
    { name: 'event-create', path: '/events/new' },
    { name: 'event-edit', path: `${eventPath}/edit` },
    { name: 'event-detail', path: eventPath, capture: true },
    { name: 'need-edit', path: eventPath, open: click('Edit Need') },
    { name: 'need-remove-confirmation', path: eventPath, open: click('Remove Need') },
    { name: 'update-edit', path: eventPath, open: click('Edit Update') },
    { name: 'update-hide-confirmation', path: eventPath, open: click('Hide Update') },
    { name: 'plan-create', path: '/plans/new' },
    { name: 'plan-edit', path: `${planPath}/edit` },
    { name: 'plan-note', path: planPath, open: click('Edit note') },
    { name: 'trip-list', path: '/trips' },
    { name: 'trip-create', path: '/trips/new' },
    { name: 'trip-edit', path: `${tripPath}/edit` },
    { name: 'trip-detail', path: tripPath },
    { name: 'trip-completion-confirmation', path: tripPath, open: click('Complete Trip') },
    { name: 'check-in-draft', path: '/safety#check-ins', open: click('Create a Check-In') },
    { name: 'sharing-draft', path: '/safety', open: click('Start location sharing') },
    { name: 'circle-draft', path: '/safety', open: click('Create a Safety Circle') },
    { name: 'contact-draft', path: '/safety', open: click('Add a Trusted Contact') },
    { name: 'concern-list', path: '/concerns' },
    { name: 'concern-create', path: '/concerns', open: click('Create a Concern') },
    { name: 'concern-detail', path: concernPath },
    { name: 'incident-list', path: '/incidents' },
    { name: 'incident-create', path: '/incidents/new', capture: true },
    { name: 'incident-detail', path: incidentPath },
    { name: 'incident-finding', path: incidentPath, open: click('Add a finding') },
    { name: 'incident-contact-attempt', path: incidentPath, open: click('Record a contact attempt') },
    { name: 'incident-task', path: incidentPath, open: click('Add a task') },
    { name: 'incident-handoff', path: incidentPath, open: click('Hand off coordination…') },
    { name: 'incident-resolution', path: incidentPath, open: click('Resolve this Incident…') },
    { name: 'profile', path: '/me' },
    { name: 'unavailable-event', path: '/events/00000000-0000-0000-0000-000000000001' },
    { name: 'unavailable-member', path: `/family/${family.familyGroupId}/members/00000000-0000-0000-0000-000000000001` },
  ];
  for (const width of [1440, 768, 390]) {
    await page.setViewportSize({ width, height: 1000 });
    for (const surface of surfaces) {
      await test.step(`${surface.name} ${width}px`, async () => {
        await page.goto(surface.path);
        await page.waitForLoadState('networkidle');
        if (surface.open) await surface.open(page);
        await expect(page.locator('main h1')).toHaveCount(1);
        expect(await page.evaluate(() => document.documentElement.scrollWidth), surface.name).toBeLessThanOrEqual(width);
        expect((await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa']).analyze()).violations, surface.name).toEqual([]);
        if (surface.capture) await page.screenshot({ path: resolve(captureDirectory('secondary-ui'), `${surface.name}-${width}.png`), fullPage: true });
      });
    }
    const nav = page.getByRole('navigation', { name: 'Main navigation' }).getByRole('link', { name: 'Family', exact: true });
    await nav.focus();
    expect(await nav.evaluate(el => getComputedStyle(el).outlineStyle)).toBe('solid');
    await page.keyboard.press('Enter');
    await expect(page.locator('main h1')).toBeFocused();
  }
});

test('invitation entry, optional review and recovery states retain accessible layouts', async ({ page, request }) => {
  await recordScenario(test.info());
  const identities = (await (await request.get(`${apiBaseUrl}/api/development/identities`)).json()).identities;
  const recipient = identities.find((i: { label: string }) => i.label.startsWith('Adult B'));
  const family = (await (await request.get(`${apiBaseUrl}/api/family-groups/mine`)).json())[0];
  expect((await request.post(`${apiBaseUrl}/api/membership-invitations`, { data: { familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid' } })).status()).toBe(202);
  const outbox = await (await request.get(`${apiBaseUrl}/api/development/invitation-outbox`)).json();
  const invitationUrl = outbox.at(-1).invitationUrl;
  await page.goto('/home');
  await page.getByLabel('Active test identity').selectOption(recipient.accountId);
  await expect(page.locator('.home-no-family')).toBeVisible();
  const inspect = async (name: string) => {
    for (const width of [1440, 768, 390]) {
      await page.setViewportSize({ width, height: 1000 });
      expect(await page.evaluate(() => document.documentElement.scrollWidth), name).toBeLessThanOrEqual(width);
      expect((await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21aa']).analyze()).violations, name).toEqual([]);
    }
  };
  await inspect('no-family');
  // Use a fragment on the current origin, preserving the browser's selected test identity.
  await page.goto('/' + new URL(invitationUrl).hash);
  await expect(page.getByRole('button', { name: 'Accept invitation', exact: true })).toBeVisible();
  await inspect('invitation-preview');
  await page.getByRole('button', { name: 'Accept invitation', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Review your profile', level: 1 })).toBeVisible();
  await inspect('profile-review');
  await page.getByRole('button', { name: 'Continue to Home without changes', exact: true }).click();
  await expect(page).toHaveURL(/\/home$/);
  await page.goto('/#invite=invalid');
  await expect(page.getByRole('alert')).toContainText('This invitation is unavailable.');
  await inspect('unavailable-invitation');
  await page.goto('/safety');
  await page.waitForLoadState('networkidle');
  await page.route('**/api/check-ins/mine', route => route.fulfill({ status: 500 }));
  await page.locator('app-check-ins').getByRole('button', { name: 'Refresh Check-Ins', exact: true }).click();
  await expect(page.locator('app-check-ins').getByRole('button', { name: 'Retry loading Check-Ins', exact: true })).toBeVisible();
  await inspect('read-error');
  await page.unrouteAll();
  // Confirm that the entry exercised only the synthetic recipient account.
  expect((await request.get(`${apiBaseUrl}/api/family-groups/mine`, { headers: { [developmentAccountHeader]: recipient.accountId } })).ok()).toBeTruthy();
});
