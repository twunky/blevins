import { expect, test } from '@playwright/test';
import {
  apiBaseUrl,
  apiJson,
  developmentAccountHeader,
  expectHealthyApi,
  recordScenario,
  useIsolatedScenario,
} from './fixtures/invitation-scenario';

// Fresh database and API for this file; Adult C remains a Trip recipient after B is removed.
useIsolatedScenario({ tripAudienceAdult: true });

interface DevelopmentIdentity {
  accountId: string;
  label: string;
}

interface FamilyGroup {
  familyGroupId: string;
  name: string;
}

interface FamilyMember {
  profileId: string;
  displayName: string;
}

interface TripPerson {
  profileId: string;
  displayName: string;
}

interface Trip {
  tripId: string;
  familyGroupId: string;
  creatorProfileId: string;
  title: string;
  audienceRecipients: TripPerson[];
}

interface TripNote {
  noteId: string;
  authorProfileId: string;
  authorDisplayName: string;
  content: string;
}

interface TripAudiencePreview {
  previewToken: string;
  audienceRecipients: TripPerson[];
  losingAccess: TripPerson[];
}

test('Adult A removes B from a Trip audience while C and creator access remain authorized', async ({ page, request }) => {
  await recordScenario(test.info());
  await expectHealthyApi(request);
  console.log('API /health: Healthy');

  const identitiesResponse = await request.get(`${apiBaseUrl}/api/development/identities`);
  expect(identitiesResponse.ok()).toBeTruthy();
  const identityOptions = await identitiesResponse.json() as {
    defaultAccountId: string;
    identities: DevelopmentIdentity[];
  };
  const adultA = identityOptions.identities.find(identity => identity.label.startsWith('Adult A'));
  const adultB = identityOptions.identities.find(identity => identity.label.startsWith('Adult B'));
  const adultC = identityOptions.identities.find(identity => identity.label.startsWith('Adult C'));
  expect(adultA).toBeDefined();
  expect(adultB).toBeDefined();
  expect(adultC).toBeDefined();
  expect(identityOptions.defaultAccountId).toBe(adultA!.accountId);

  const aMemberships = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultA!.accountId);
  expect(aMemberships.status).toBe(200);
  expect(aMemberships.body).toHaveLength(1);
  const family = aMemberships.body[0];
  const aAdministered = await apiJson<FamilyGroup[]>(request, '/api/family-groups/administered', adultA!.accountId);
  expect(aAdministered.body.map(group => group.familyGroupId)).toContain(family.familyGroupId);

  const [bBefore, cBefore, membersBefore] = await Promise.all([
    apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId),
    apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultC!.accountId),
    request.get(`${apiBaseUrl}/api/family-groups/${family.familyGroupId}/members`, {
      headers: { [developmentAccountHeader]: adultA!.accountId },
    }),
  ]);
  expect(bBefore.body.length === 0 || bBefore.body.some(group => group.familyGroupId === family.familyGroupId)).toBeTruthy();
  expect(cBefore.body.map(group => group.familyGroupId)).toContain(family.familyGroupId);
  const initialMembers = await membersBefore.json() as FamilyMember[];
  const aProfile = initialMembers.find(member => member.displayName === 'Development Adult');
  const cProfile = initialMembers.find(member => member.displayName === 'Development Audience Member');
  const bProfileBeforeJoin = initialMembers.find(member => member.displayName === 'Development Recipient');
  expect(aProfile).toBeDefined();
  expect(cProfile).toBeDefined();
  const bAlreadyJoined = bBefore.body.some(group => group.familyGroupId === family.familyGroupId);
  expect(Boolean(bProfileBeforeJoin)).toBe(bAlreadyJoined);
  console.log(`Starting identities: A admin; B ${bAlreadyJoined ? 'active member' : 'nonmember'}; C active Family member.`);

  if (!bAlreadyJoined) {
    const invitationIssue = await request.post(`${apiBaseUrl}/api/membership-invitations`, {
      headers: { [developmentAccountHeader]: adultA!.accountId },
      data: { familyGroupId: family.familyGroupId, recipientEmail: 'development-recipient@example.invalid' },
    });
    expect(invitationIssue.status()).toBe(202);
    const outboxResponse = await request.get(`${apiBaseUrl}/api/development/invitation-outbox`, {
      headers: { [developmentAccountHeader]: adultA!.accountId },
    });
    expect(outboxResponse.ok()).toBeTruthy();
    const outbox = await outboxResponse.json() as { invitationUrl: string }[];
    const invitationUrl = outbox.at(-1)?.invitationUrl;
    expect(invitationUrl).toBeTruthy();
    const invitationToken = uriToken(invitationUrl!);
    const invitationPreview = await request.post(`${apiBaseUrl}/api/membership-invitations/preview`, {
      headers: { [developmentAccountHeader]: adultB!.accountId },
      data: { token: invitationToken },
    });
    expect(invitationPreview.status()).toBe(200);
    const invitationAcceptance = await request.post(`${apiBaseUrl}/api/membership-invitations/accept`, {
      headers: { [developmentAccountHeader]: adultB!.accountId },
      data: { token: invitationToken },
    });
    expect(invitationAcceptance.status()).toBe(200);
    console.log('B joined through the existing invitation API path.');
  }
  const bMembershipsAfterJoin = await apiJson<FamilyGroup[]>(request, '/api/family-groups/mine', adultB!.accountId);
  expect(bMembershipsAfterJoin.body.map(group => group.familyGroupId)).toContain(family.familyGroupId);
  console.log('A, B, and C are active Family members before Trip creation.');

  await page.goto('/');
  const activeIdentity = page.getByLabel('Active test identity');
  await expect(activeIdentity).toHaveValue(adultA!.accountId);
  console.log('Angular ready: http://localhost:4200');
  await page.getByRole('link', { name: 'Plans & Trips', exact: true }).click();
  await page.getByRole('link', { name: 'Open Trips', exact: true }).click();
  await page.getByRole('button', { name: 'Create trip', exact: true }).click();

  const tripTitle = `Audience access ${test.info().workerIndex}-${Date.now()}`;
  const noteContent = `B note ${Date.now()}`;
  await page.getByLabel('Trip title').fill(tripTitle);
  await page.getByLabel('Primary destination').fill('Test destination');
  await page.getByLabel('Start date').fill('2099-01-10');
  await page.getByLabel('End date').fill('2099-01-12');
  const travelers = page.getByRole('group', { name: 'Travelers' });
  const tripAudience = page.getByRole('group', { name: 'Who can see this Trip?' });
  await travelers.getByRole('checkbox', { name: /Development Adult/ }).check();
  await tripAudience.getByRole('radio', { name: 'Selected people' }).check();
  await tripAudience.getByRole('checkbox', { name: 'Development Recipient' }).check();
  await tripAudience.getByRole('checkbox', { name: 'Development Audience Member' }).check();

  const createResponsePromise = page.waitForResponse(response =>
    response.url().includes(`/api/family-groups/${family.familyGroupId}/trips`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Create trip', exact: true }).click();
  const createResponse = await createResponsePromise;
  expect(createResponse.status()).toBe(201);
  const createdTrip = await createResponse.json() as Trip;
  const tripId = createdTrip.tripId;
  expect(createdTrip.creatorProfileId).toBe(aProfile!.profileId);
  const recipientProfileIds = createdTrip.audienceRecipients.map(person => person.profileId);
  const bProfileAfterJoin = (await apiJson<FamilyMember[]>(request,
    `/api/family-groups/${family.familyGroupId}/members`, adultA!.accountId)).body
    .find(member => member.displayName === 'Development Recipient');
  expect(bProfileAfterJoin).toBeDefined();
  expect(recipientProfileIds).toContain(bProfileAfterJoin!.profileId);
  expect(recipientProfileIds).toContain(cProfile!.profileId);
  expect(createdTrip.audienceRecipients.map(person => person.profileId)).not.toContain(aProfile!.profileId);
  console.log(`Trip created: ${tripId}; selected recipients B and C; creator A is not a recipient.`);

  const aTripListBefore = await apiJson<Trip[]>(request, '/api/trips', adultA!.accountId);
  const aTripBefore = await apiJson<Trip>(request, `/api/trips/${tripId}`, adultA!.accountId);
  const bTripListBefore = await apiJson<Trip[]>(request, '/api/trips', adultB!.accountId);
  const bTripBefore = await apiJson<Trip>(request, `/api/trips/${tripId}`, adultB!.accountId);
  const cTripListBefore = await apiJson<Trip[]>(request, '/api/trips', adultC!.accountId);
  const cTripDetailBefore = await apiJson<Trip>(request, `/api/trips/${tripId}`, adultC!.accountId);
  expect(aTripListBefore.body.map(trip => trip.tripId)).toContain(tripId);
  expect(aTripBefore.status).toBe(200);
  expect(bTripListBefore.body.map(trip => trip.tripId)).toContain(tripId);
  expect(bTripBefore.status).toBe(200);
  expect(cTripListBefore.body.map(trip => trip.tripId)).toContain(tripId);
  expect(cTripDetailBefore.status).toBe(200);
  console.log('Pre-removal access: A creator, B recipient, and C recipient can retrieve the Trip.');

  await expect(page.getByRole('heading', { name: tripTitle, exact: true })).toBeVisible();
  const detail = page.getByRole('article', { name: 'Trip details' });
  await expect(detail).toContainText('Test destination');
  await expect(detail.getByRole('heading', { name: 'Trip Notes' })).toBeVisible();

  await activeIdentity.selectOption(adultB!.accountId);
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  await page.getByRole('link', { name: 'Plans & Trips', exact: true }).click();
  await page.getByRole('link', { name: 'Open Trips', exact: true }).click();
  await expect(page.getByRole('link', { name: new RegExp(tripTitle) })).toBeVisible();
  await page.getByRole('link', { name: new RegExp(tripTitle) }).click();
  const bDetail = page.getByRole('article', { name: 'Trip details' });
  await expect(bDetail).toContainText('Test destination');
  await page.getByLabel('Add a Note').fill(noteContent);
  const noteCreatePromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/trips/${tripId}/notes`) && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Add note', exact: true }).click();
  const noteCreateResponse = await noteCreatePromise;
  expect(noteCreateResponse.status()).toBe(201);
  const createdNote = await noteCreateResponse.json() as TripNote;
  const bProfile = bProfileAfterJoin;
  expect(bProfile).toBeDefined();
  expect(createdNote.authorProfileId).toBe(bProfile!.profileId);
  expect(createdNote.authorDisplayName).toBe('Development Recipient');
  expect(createdNote.content).toBe(noteContent);
  await expect(bDetail).toContainText(noteContent);

  const aNotesBeforeRemoval = await request.get(`${apiBaseUrl}/api/trips/${tripId}/notes`, {
    headers: { [developmentAccountHeader]: adultA!.accountId },
  });
  expect(aNotesBeforeRemoval.status()).toBe(200);
  const noteAsA = (await aNotesBeforeRemoval.json() as TripNote[]).find(note => note.noteId === createdNote.noteId);
  expect(noteAsA?.authorProfileId).toBe(bProfile!.profileId);
  expect(noteAsA?.content).toBe(noteContent);
  console.log(`Trip Note created by B and persisted: ${createdNote.noteId}.`);

  await activeIdentity.selectOption(adultA!.accountId);
  await expect(activeIdentity).toHaveValue(adultA!.accountId);
  await page.getByRole('link', { name: 'Plans & Trips', exact: true }).click();
  await page.getByRole('link', { name: 'Open Trips', exact: true }).click();
  await expect(page.getByRole('link', { name: new RegExp(tripTitle) })).toBeVisible();
  await page.getByRole('link', { name: new RegExp(tripTitle) }).click();
  await page.getByRole('button', { name: 'Change audience', exact: true }).click();

  const audienceEditor = page.getByRole('region', { name: 'Change Trip audience' });
  await expect(audienceEditor).toBeVisible();
  await expect(audienceEditor.getByRole('checkbox', { name: 'Development Audience Member' })).toBeChecked();
  await audienceEditor.getByRole('checkbox', { name: 'Development Recipient' }).uncheck();

  const previewResponsePromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/trips/${tripId}/audience/preview`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Preview audience change', exact: true }).click();
  const previewResponse = await previewResponsePromise;
  expect(previewResponse.status()).toBe(200);
  const preview = await previewResponse.json() as TripAudiencePreview;
  expect(preview.losingAccess.map(person => person.profileId)).toContain(bProfile!.profileId);
  expect(preview.audienceRecipients.map(person => person.profileId)).toEqual([cProfile!.profileId]);
  await expect(page.getByText(/Will lose access:.*Development Recipient/)).toBeVisible();
  await expect(page.getByText(/Recipients:.*Development Audience Member/)).toBeVisible();
  console.log('Audience preview: B will lose access; C remains a recipient.');

  const confirmResponsePromise = page.waitForResponse(response =>
    response.url().endsWith(`/api/trips/${tripId}/audience/confirm`)
      && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Confirm audience change', exact: true }).click();
  const confirmResponse = await confirmResponsePromise;
  expect(confirmResponse.status()).toBe(200);
  const updatedTrip = await confirmResponse.json() as Trip;
  expect(updatedTrip.audienceRecipients.map(person => person.profileId)).toEqual([cProfile!.profileId]);

  const [bTripsAfter, aTripsAfter, cTripsAfter, aTripAfter, cTripAfter] = await Promise.all([
    apiJson<Trip[]>(request, '/api/trips', adultB!.accountId),
    apiJson<Trip[]>(request, '/api/trips', adultA!.accountId),
    apiJson<Trip[]>(request, '/api/trips', adultC!.accountId),
    apiJson<Trip>(request, `/api/trips/${tripId}`, adultA!.accountId),
    apiJson<Trip>(request, `/api/trips/${tripId}`, adultC!.accountId),
  ]);
  expect(bTripsAfter.status).toBe(200);
  expect(bTripsAfter.body.map(trip => trip.tripId)).not.toContain(tripId);
  expect(aTripsAfter.body.map(trip => trip.tripId)).toContain(tripId);
  expect(cTripsAfter.body.map(trip => trip.tripId)).toContain(tripId);
  expect(aTripAfter.status).toBe(200);
  expect(cTripAfter.status).toBe(200);

  const bDetailAfter = await request.get(`${apiBaseUrl}/api/trips/${tripId}`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  const bNotesAfter = await request.get(`${apiBaseUrl}/api/trips/${tripId}/notes`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  const bAddNoteAfter = await request.post(`${apiBaseUrl}/api/trips/${tripId}/notes`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
    data: { content: `Unauthorized note ${Date.now()}` },
  });
  expect(bDetailAfter.status()).toBe(404);
  expect(bNotesAfter.status()).toBe(404);
  expect(bAddNoteAfter.status()).toBe(404);
  console.log('Post-removal B authorization: list omits Trip; detail, Notes read, and Note creation return 404.');

  const aNotesAfterRemoval = await request.get(`${apiBaseUrl}/api/trips/${tripId}/notes`, {
    headers: { [developmentAccountHeader]: adultA!.accountId },
  });
  expect(aNotesAfterRemoval.status()).toBe(200);
  const notesAfterRemoval = await aNotesAfterRemoval.json() as TripNote[];
  const retainedNote = notesAfterRemoval.find(note => note.noteId === createdNote.noteId);
  expect(retainedNote).toEqual(expect.objectContaining({
    authorProfileId: bProfile!.profileId,
    authorDisplayName: 'Development Recipient',
    content: noteContent,
  }));
  expect(notesAfterRemoval).toHaveLength(1);
  console.log('A retains creator access and the B-authored note; C retains audience access.');

  await activeIdentity.selectOption(adultB!.accountId);
  await expect(activeIdentity).toHaveValue(adultB!.accountId);
  await page.getByRole('link', { name: 'Plans & Trips', exact: true }).click();
  await page.getByRole('link', { name: 'Open Trips', exact: true }).click();
  await expect(page.getByRole('link', { name: new RegExp(tripTitle) })).toHaveCount(0);
  await expect(page.getByText('No Trips are currently available to you.', { exact: false })).toBeVisible();
  const bDirectAfterRefetch = await request.get(`${apiBaseUrl}/api/trips/${tripId}`, {
    headers: { [developmentAccountHeader]: adultB!.accountId },
  });
  expect(bDirectAfterRefetch.status()).toBe(404);
  console.log('B refreshed Trips view omits the Trip and direct access remains unavailable.');
});

function uriToken(invitationUrl: string): string {
  const token = invitationUrl.split('#invite=')[1];
  expect(token).toBeTruthy();
  return decodeURIComponent(token!);
}
