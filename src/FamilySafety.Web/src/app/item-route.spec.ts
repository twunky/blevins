import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, TestRequest, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from './app';
import { routes } from './app.routes';
import { CurrentFamilyContext } from './family-context';
import { isItemId } from './item-route';

// WI-009-005: item URLs and the authorize-first Family decision (REQ-UX-014, REQ-PRIV-027, DEC-075 OD-A to OD-H).
const ownProfile = {
  profileId: 'person-a', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const one = { familyGroupId: 'family-one', name: 'Family One' };
const two = { familyGroupId: 'family-two', name: 'Family Two' };
const departed = 'family-departed';
const itemId = '40000000-0000-4000-8000-000000000004';

const eventView = (familyGroupId: string, canEdit = false) => ({
  eventId: itemId, familyGroupId, name: 'Lake picnic', description: 'Bring a blanket', date: '2026-11-01', time: '12:00:00',
  location: 'North shore', organizerProfileId: 'person-b', organizerDisplayName: 'Blair', canEdit,
});
const planView = (familyGroupId: string) => ({
  planId: itemId, familyGroupId, activityOrDestination: 'Hike to the falls', date: '2026-11-02', time: '08:00:00',
  expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null,
  audienceType: 'SelectedPeople', creatorProfileId: 'person-b', creatorDisplayName: 'Blair', notes: 'Trail map in the car',
});
const tripView = (familyGroupId: string) => ({
  tripId: itemId, familyGroupId, familyGroupName: 'Name from the item response', creatorProfileId: ownProfile.profileId,
  creatorDisplayName: 'Alex', canEdit: true, title: 'Coast drive', primaryDestination: 'Seaside', startDate: '2026-11-03',
  endDate: '2026-11-05', description: null, transportationNotes: null, accommodationNotes: null, expectedArrivalDate: null,
  expectedArrivalTime: null, expectedArrivalTimeZone: null, audienceType: 'SelectedPeople', travelers: [], contacts: [],
  audienceRecipients: [],
});
const concernView = (familyGroupId: string) => ({
  concernId: itemId, familyGroupId, familyGroupName: 'Name from the item response', creatorProfileId: 'person-b',
  creatorName: 'Blair', subjectProfileId: 'person-c', subjectName: 'Casey', creationReason: 'Missed a call', state: 'Open',
  createdAt: '2026-10-01T10:00:00Z', lastAuthorizedMutationAt: '2026-10-01T10:00:00Z', frozenAt: null, freezeReason: null,
  resolvedAt: null, resolutionNote: null, expiresAt: null, participants: [{ profileId: ownProfile.profileId, displayName: 'Alex' }],
  entries: [],
});

describe('Item URLs and the authorize-first Family decision (WI-009-005)', () => {
  let http: HttpTestingController;
  /** The missing-item card per type, compared with the invalid-ID card in the next test (AC-25). */
  const unavailableMarkup: Record<string, string> = {};
  let context: CurrentFamilyContext;

  beforeEach(async () => {
    history.replaceState(null, '', '/');
    await TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    context = TestBed.inject(CurrentFamilyContext);
  });

  afterEach(() => {
    http.match(() => true).forEach(request => { if (!request.cancelled) request.flush([]); });
    http.verify({ ignoreCancelled: true });
    history.replaceState(null, '', '/');
  });

  async function visit(path: string) {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(path, App);
    return harness;
  }

  /** The shell's own reads. The Family list is returned separately so a test can hold or fail it. */
  function shellReads(): TestRequest {
    http.expectOne('/api/profile').flush(ownProfile);
    http.expectOne('/api/family-groups/administered').flush([]);
    return http.expectOne('/api/family-groups/mine');
  }

  function familyReads(group: { familyGroupId: string }) {
    // The shell reads each part once; a Concerns host also reads the members for its create form.
    for (const part of ['members', 'relationships', 'events', 'plans']) {
      const requests = http.match(`/api/family-groups/${group.familyGroupId}/${part}`);
      expect(requests.length, part).toBeGreaterThan(0);
      requests.forEach(request => request.flush([]));
    }
  }

  const familyScoped = (group: { familyGroupId: string }) =>
    http.match(request => request.url.startsWith(`/api/family-groups/${group.familyGroupId}/`));

  async function settle(harness: RouterTestingHarness) {
    await harness.fixture.whenStable();
    harness.detectChanges();
  }

  const root = (harness: RouterTestingHarness) => harness.routeNativeElement!;
  /** The first item heading not inside a hidden area (the shell keeps other areas in the DOM, hidden). */
  const itemHeading = (harness: RouterTestingHarness) =>
    Array.from(root(harness).querySelectorAll('main h2')).find(heading => !heading.closest('[hidden]'))?.textContent?.trim();
  const status = (harness: RouterTestingHarness) => root(harness).querySelector('p.family-switch-status')!;
  const topFamily = (harness: RouterTestingHarness) => root(harness).querySelector('.current-family strong')?.textContent?.trim() ?? null;
  const selector = (harness: RouterTestingHarness) => root(harness).querySelector<HTMLSelectElement>('#current-family')!;
  const retryButton = (harness: RouterTestingHarness, name: string) =>
    Array.from(root(harness).querySelectorAll('button')).find(button => button.textContent?.trim() === name);

  it('accepts only GUID item IDs', () => {
    expect(isItemId(itemId)).toBe(true);
    expect(isItemId(itemId.toUpperCase())).toBe(true);
    expect(isItemId('not-a-guid')).toBe(false);
    expect(isItemId(`${itemId}x`)).toBe(false);
    expect(isItemId(null)).toBe(false);
  });

  describe.each([
    { kind: 'Event', path: '/events/', read: '/api/events/', view: eventView, title: 'Lake picnic' },
    { kind: 'Plan', path: '/plans/', read: '/api/plans/', view: planView, title: 'Hike to the falls' },
  ] as const)('$kind item page', ({ kind, path, read, view, title }) => {
    it(`switches from Family One to Family Two only after the ${kind} read returns 200, then announces it once (AC-17, AC-18, AC-33)`, async () => {
      context.familyId.set(one.familyGroupId);
      const harness = await visit(`${path}${itemId}`);
      shellReads().flush([one, two]);
      familyReads(one);
      const item = http.expectOne(`${read}${itemId}`);
      await settle(harness);
      // Before the 200: Family One everywhere, nothing for Family Two, no item, no announcement.
      expect(topFamily(harness)).toBe('Family One');
      expect(selector(harness).value).toBe(one.familyGroupId);
      expect(familyScoped(two)).toHaveLength(0);
      expect(root(harness).textContent).not.toContain(title);
      expect(status(harness).textContent).toBe('');
      expect(root(harness).textContent).toContain(`Loading ${kind}…`);

      item.flush(view(two.familyGroupId));
      await settle(harness);
      familyReads(two);
      harness.detectChanges();
      expect(context.familyId()).toBe(two.familyGroupId);
      expect(topFamily(harness)).toBe('Family Two');
      expect(selector(harness).value).toBe(two.familyGroupId);
      expect(status(harness).getAttribute('role')).toBe('status');
      expect(status(harness).textContent).toBe('Switched to Family Two.');
      expect(itemHeading(harness)).toBe(title);
      const shell = harness.routeDebugElement!.componentInstance;
      expect(shell.eventForm.controls.familyGroupId.value).toBe(two.familyGroupId);
      expect(Array.from(root(harness).querySelectorAll('h1')).length).toBe(1);
      // Exactly one announcement and no focus move.
      expect(root(harness).querySelectorAll('[role="status"]:not(:empty)').length).toBeGreaterThanOrEqual(1);
      expect(Array.from(root(harness).querySelectorAll('[role="status"]')).filter(element => element.textContent?.includes('Switched to'))).toHaveLength(1);
      expect(document.activeElement?.closest('main')).toBeNull();
    });

    it(`renders a same-Family ${kind} with no switch, no reload, and no announcement (AC-21)`, async () => {
      context.familyId.set(one.familyGroupId);
      const harness = await visit(`${path}${itemId}`);
      shellReads().flush([one, two]);
      familyReads(one);
      http.expectOne(`${read}${itemId}`).flush(view(one.familyGroupId));
      await settle(harness);
      expect(familyScoped(one)).toHaveLength(0);
      expect(familyScoped(two)).toHaveLength(0);
      expect(context.familyId()).toBe(one.familyGroupId);
      expect(status(harness).textContent).toBe('');
      expect(itemHeading(harness)).toBe(title);
    });

    it(`keeps the Family unchanged and shows one generic card for a missing, unauthorized, or invalid ${kind} (AC-23, AC-24, AC-25)`, async () => {
      context.familyId.set(one.familyGroupId);
      const harness = await visit(`${path}${itemId}`);
      shellReads().flush([one, two]);
      familyReads(one);
      // Missing and unauthorized are the same 404 from the server (AC-31), so the client cannot tell them apart.
      http.expectOne(`${read}${itemId}`).flush(null, { status: 404, statusText: 'Not Found' });
      await settle(harness);
      const missingCard = root(harness).querySelector('app-item-unavailable')!;
      expect(missingCard.querySelector('h2')?.textContent).toBe(`This ${kind} is unavailable`);
      expect(missingCard.textContent).toContain(`Only ${kind}s currently shared with you can be opened.`);
      expect(missingCard.querySelector('a')?.getAttribute('href')).toBe(path.slice(0, -1));
      expect(document.activeElement).toBe(missingCard.querySelector('h2'));
      expect(context.familyId()).toBe(one.familyGroupId);
      expect(topFamily(harness)).toBe('Family One');
      expect(status(harness).textContent).toBe('');
      expect(familyScoped(one)).toHaveLength(0);
      expect(familyScoped(two)).toHaveLength(0);
      expect(root(harness).textContent).not.toContain('Choose your current Family');
      unavailableMarkup[kind] = missingCard.innerHTML;
    });

    it(`offers Retry for a transient ${kind} read failure and repeats the same authorize-first read before any switch`, async () => {
      context.familyId.set(one.familyGroupId);
      const harness = await visit(`${path}${itemId}`);
      shellReads().flush([one, two]);
      familyReads(one);
      http.expectOne(`${read}${itemId}`).flush(null, { status: 503, statusText: 'Unavailable' });
      await settle(harness);
      expect(root(harness).textContent).toContain(`This ${kind} could not be opened. Please try again.`);
      expect(root(harness).querySelector('app-item-unavailable')).toBeNull();
      expect(context.familyId()).toBe(one.familyGroupId);

      retryButton(harness, `Retry loading ${kind}`)!.click();
      harness.detectChanges();
      expect(root(harness).textContent).toContain(`Loading ${kind}…`);
      expect(root(harness).textContent).not.toContain('could not be opened');
      // The Family stays as it was until the repeated read returns 200; nothing for Family Two is read first.
      expect(context.familyId()).toBe(one.familyGroupId);
      expect(familyScoped(two)).toHaveLength(0);
      http.expectOne(`${read}${itemId}`).flush(view(two.familyGroupId));
      await settle(harness);
      familyReads(two);
      harness.detectChanges();
      expect(context.familyId()).toBe(two.familyGroupId);
      expect(status(harness).textContent).toBe('Switched to Family Two.');
      expect(itemHeading(harness)).toBe(title);
      expect(retryButton(harness, `Retry loading ${kind}`)).toBeUndefined();
    });

    it(`shows the same generic card, with no Retry, when a ${kind} Retry returns 404`, async () => {
      context.familyId.set(one.familyGroupId);
      const harness = await visit(`${path}${itemId}`);
      shellReads().flush([one, two]);
      familyReads(one);
      http.expectOne(`${read}${itemId}`).flush(null, { status: 503, statusText: 'Unavailable' });
      await settle(harness);
      retryButton(harness, `Retry loading ${kind}`)!.click();
      http.expectOne(`${read}${itemId}`).flush(null, { status: 404, statusText: 'Not Found' });
      await settle(harness);
      const card = root(harness).querySelector('app-item-unavailable')!;
      expect(card.innerHTML).toBe(unavailableMarkup[kind]);
      expect(retryButton(harness, `Retry loading ${kind}`)).toBeUndefined();
      expect(context.familyId()).toBe(one.familyGroupId);
    });

    it(`shows the identical card for a non-GUID ${kind} ID without any request (AC-23, AC-25)`, async () => {
      context.familyId.set(one.familyGroupId);
      const invalid = await visit(`${path}not-a-guid`);
      shellReads().flush([one, two]);
      familyReads(one);
      http.expectNone(request => request.url.startsWith(read));
      await settle(invalid);
      const card = root(invalid).querySelector('app-item-unavailable')!;
      expect(card.innerHTML).toBe(unavailableMarkup[kind]);
      expect(document.activeElement).toBe(card.querySelector('h2'));
      expect(context.familyId()).toBe(one.familyGroupId);
      expect(status(invalid).textContent).toBe('');
    });

    it(`refreshes the Family list exactly once and shows the generic card when the ${kind}'s Family is still absent (OD-B, AC-26)`, async () => {
      context.familyId.set(one.familyGroupId);
      const harness = await visit(`${path}${itemId}`);
      shellReads().flush([one, two]);
      familyReads(one);
      http.expectOne(`${read}${itemId}`).flush(view(departed));
      await settle(harness);
      http.expectOne('/api/family-groups/mine').flush([one, two]);
      await settle(harness);
      http.expectNone('/api/family-groups/mine');
      expect(familyScoped(one)).toHaveLength(0);
      expect(root(harness).querySelector('app-item-unavailable h2')?.textContent).toBe(`This ${kind} is unavailable`);
      expect(root(harness).textContent).not.toContain(title);
      expect(context.familyId()).toBe(one.familyGroupId);
      expect(status(harness).textContent).toBe('');
    });
  });

  it('switches normally when the one Family-list refresh now contains the item Family (OD-B, AC-26)', async () => {
    const harness = await visit(`/events/${itemId}`);
    shellReads().flush([one]);
    familyReads(one);
    http.expectOne(`/api/events/${itemId}`).flush(eventView(two.familyGroupId));
    await settle(harness);
    http.expectOne('/api/family-groups/mine').flush([one, two]);
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(context.familyId()).toBe(two.familyGroupId);
    expect(status(harness).textContent).toBe('Switched to Family Two.');
    http.expectNone('/api/family-groups/mine');
  });

  it('waits for a Family list still loading when the item returns (AC-26)', async () => {
    const harness = await visit(`/events/${itemId}`);
    const mine = shellReads();
    http.expectOne(`/api/events/${itemId}`).flush(eventView(two.familyGroupId));
    await settle(harness);
    expect(root(harness).textContent).not.toContain('Lake picnic');
    http.expectNone('/api/family-groups/mine');
    mine.flush([one, two]);
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(context.familyId()).toBe(two.familyGroupId);
    expect(itemHeading(harness)).toBe('Lake picnic');
  });

  it('shows the membership error and Retry with no render and no switch when the Family list fails, then continues after Retry (OD-B, AC-26)', async () => {
    const harness = await visit(`/events/${itemId}`);
    shellReads().flush(null, { status: 500, statusText: 'Server Error' });
    http.expectOne(`/api/events/${itemId}`).flush(eventView(two.familyGroupId));
    await settle(harness);
    const prompt = root(harness).querySelector('section.context-prompt')!;
    expect(prompt.textContent).toContain('Your Family memberships could not be loaded.');
    expect(root(harness).querySelector<HTMLElement>('.page-content')!.hidden).toBe(true);
    expect(root(harness).textContent).not.toContain('Lake picnic');
    expect(context.familyId()).toBeNull();
    expect(status(harness).textContent).toBe('');
    Array.from(prompt.querySelectorAll('button')).find(button => button.textContent?.includes('Retry'))!.click();
    http.expectOne('/api/family-groups/mine').flush([one, two]);
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(context.familyId()).toBe(two.familyGroupId);
    expect(itemHeading(harness)).toBe('Lake picnic');
  });

  it('switches with no prior selection and never shows the Family prompt on an item page (AC-10 shape, AC-17)', async () => {
    const harness = await visit(`/plans/${itemId}`);
    shellReads().flush([one, two]);
    await settle(harness);
    expect(root(harness).textContent).not.toContain('Choose your current Family');
    expect(root(harness).textContent).toContain('Loading Plan…');
    http.expectOne(`/api/plans/${itemId}`).flush(planView(two.familyGroupId));
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(status(harness).textContent).toBe('Switched to Family Two.');
    expect(itemHeading(harness)).toBe('Hike to the falls');
    // An audience member can read the note but cannot change it.
    const article = root(harness).querySelector('article.item-detail')!;
    expect(article.textContent).toContain('Trail map in the car');
    expect(article.querySelectorAll('button, a')).toHaveLength(0);
    expect(root(harness).querySelector('a.item-back')?.getAttribute('href')).toBe('/plans');
  });

  it('lets the creator edit, cancel, save, and clear the Plan note', async () => {
    const harness = await visit(`/plans/${itemId}`);
    shellReads().flush([one]);
    familyReads(one);
    const original = { ...planView(one.familyGroupId), creatorProfileId: ownProfile.profileId };
    http.expectOne(`/api/plans/${itemId}`).flush(original);
    await settle(harness);
    const article = root(harness).querySelector('article[aria-label="Plan details"]')!;
    const button = (name: string) => Array.from(article.querySelectorAll<HTMLButtonElement>('button'))
      .find(candidate => candidate.textContent?.trim() === name)!;

    button('Edit note').click();
    harness.detectChanges();
    const note = article.querySelector<HTMLTextAreaElement>('textarea#item-plan-note')!;
    expect(article.querySelector('label[for="item-plan-note"]')?.textContent).toContain('Plan note');
    expect(note.value).toBe('Trail map in the car');
    note.value = 'Unsaved draft';
    note.dispatchEvent(new Event('input'));
    button('Cancel').click();
    harness.detectChanges();
    http.expectNone(`/api/plans/${itemId}/notes`);
    expect(article.textContent).toContain('Trail map in the car');
    expect(article.textContent).not.toContain('Unsaved draft');

    button('Edit note').click();
    harness.detectChanges();
    const edit = article.querySelector<HTMLTextAreaElement>('textarea#item-plan-note')!;
    edit.value = 'Updated route note';
    edit.dispatchEvent(new Event('input'));
    button('Save note').click();
    const save = http.expectOne(`/api/plans/${itemId}/notes`);
    expect(save.request.method).toBe('PUT');
    expect(save.request.body).toEqual({ notes: 'Updated route note' });
    save.flush({ ...original, notes: 'Updated route note' });
    await settle(harness);
    expect(article.textContent).toContain('Updated route note');
    expect(article.textContent).toContain('Plan note saved.');

    button('Edit note').click();
    harness.detectChanges();
    const draft = article.querySelector<HTMLTextAreaElement>('textarea#item-plan-note')!;
    draft.value = 'Keep this draft';
    draft.dispatchEvent(new Event('input'));
    button('Clear note').click();
    harness.detectChanges();
    http.expectNone(`/api/plans/${itemId}/notes`);
    button('Cancel').click();
    harness.detectChanges();
    expect(article.querySelector<HTMLTextAreaElement>('textarea#item-plan-note')?.value).toBe('Keep this draft');
    expect(article.textContent).toContain('Updated route note');
    button('Cancel').click();
    harness.detectChanges();

    button('Clear note').click();
    harness.detectChanges();
    expect(article.textContent).toContain('Clear this note?');
    http.expectNone(`/api/plans/${itemId}/notes`);
    button('Cancel').click();
    harness.detectChanges();
    expect(article.textContent).toContain('Updated route note');
    expect(article.textContent).not.toContain('Clear this note?');
    http.expectNone(`/api/plans/${itemId}/notes`);

    button('Clear note').click();
    harness.detectChanges();
    button('Confirm clear').click();
    const clear = http.expectOne(`/api/plans/${itemId}/notes`);
    expect(clear.request.method).toBe('PUT');
    expect(clear.request.body).toEqual({ notes: null });
    clear.flush({ ...original, notes: null });
    await settle(harness);
    expect(article.textContent).toContain('Plan note cleared.');
    expect(article.textContent).not.toContain('Updated route note');
    expect(button('Add note')).toBeTruthy();
  });

  it('shows an understandable note length error and can add a note to a Plan without one', async () => {
    const harness = await visit(`/plans/${itemId}`);
    shellReads().flush([one]);
    familyReads(one);
    const original = { ...planView(one.familyGroupId), creatorProfileId: ownProfile.profileId, notes: null };
    http.expectOne(`/api/plans/${itemId}`).flush(original);
    await settle(harness);
    const article = root(harness).querySelector('article[aria-label="Plan details"]')!;
    const button = (name: string) => Array.from(article.querySelectorAll<HTMLButtonElement>('button'))
      .find(candidate => candidate.textContent?.trim() === name)!;
    button('Add note').click();
    harness.detectChanges();
    const note = article.querySelector<HTMLTextAreaElement>('textarea#item-plan-note')!;
    note.value = 'x'.repeat(1001);
    note.dispatchEvent(new Event('input'));
    button('Save note').click();
    harness.detectChanges();
    expect(article.querySelector('[role="alert"]')?.textContent).toContain('1,000 characters');
    http.expectNone(`/api/plans/${itemId}/notes`);
    note.value = 'Pack lunch';
    note.dispatchEvent(new Event('input'));
    button('Save note').click();
    const add = http.expectOne(`/api/plans/${itemId}/notes`);
    expect(add.request.body).toEqual({ notes: 'Pack lunch' });
    add.flush({ ...original, notes: 'Pack lunch' });
    await settle(harness);
    expect(article.textContent).toContain('Pack lunch');
  });

  it('offers Edit event details only when canEdit and shows the updated Event after the existing PUT (AC-01)', async () => {
    const harness = await visit(`/events/${itemId}`);
    shellReads().flush([one]);
    familyReads(one);
    http.expectOne(`/api/events/${itemId}`).flush(eventView(one.familyGroupId, true));
    await settle(harness);
    const edit = Array.from(root(harness).querySelectorAll<HTMLButtonElement>('article button')).find(button => button.textContent?.includes('Edit event details'))!;
    edit.click();
    harness.detectChanges();
    const name = root(harness).querySelector<HTMLInputElement>(`#item-edit-event-name-${itemId}`)!;
    name.value = 'Lake picnic and swim';
    name.dispatchEvent(new Event('input'));
    root(harness).querySelector<HTMLFormElement>('article form')!.dispatchEvent(new Event('submit'));
    const put = http.expectOne(`/api/family-groups/${one.familyGroupId}/events/${itemId}`);
    expect(put.request.method).toBe('PUT');
    put.flush({ ...eventView(one.familyGroupId, true), name: 'Lake picnic and swim' });
    await settle(harness);
    expect(root(harness).querySelector('article h2')?.textContent?.trim()).toBe('Lake picnic and swim');
    expect(root(harness).textContent).toContain('Event details updated.');
  });

  it('shows no Edit event details on the Event page for a view-only audience member (AC-01)', async () => {
    const viewer = await visit(`/events/${itemId}`);
    shellReads().flush([one]);
    familyReads(one);
    http.expectOne(`/api/events/${itemId}`).flush(eventView(one.familyGroupId, false));
    await settle(viewer);
    expect(root(viewer).querySelector('article')?.textContent).not.toContain('Edit event details');
  });

  it('renders an authorized Trip whose Family is not selectable without switching (OD-A, AC-22)', async () => {
    context.familyId.set(one.familyGroupId);
    const harness = await visit(`/trips/${itemId}`);
    shellReads().flush([one, two]);
    familyReads(one);
    await settle(harness);
    http.expectOne(`/api/trips/${itemId}`).flush(tripView(departed));
    await settle(harness);
    http.expectOne('/api/family-groups/mine').flush([one, two]);
    await settle(harness);
    http.expectOne(`/api/trips/${itemId}/notes`).flush([]);
    harness.detectChanges();
    http.expectNone('/api/family-groups/mine');
    expect(itemHeading(harness)).toBe('Coast drive');
    expect(context.familyId()).toBe(one.familyGroupId);
    expect(status(harness).textContent).toBe('');
    expect(Array.from(selector(harness).options).map(option => option.value)).toEqual(['', one.familyGroupId, two.familyGroupId]);
    expect(root(harness).textContent).not.toContain('Name from the item response');
  });

  it('switches for an authorized Trip of the other active Family with one Trip read (AC-21)', async () => {
    context.familyId.set(one.familyGroupId);
    const harness = await visit(`/trips/${itemId}`);
    shellReads().flush([one, two]);
    familyReads(one);
    await settle(harness);
    http.expectOne(`/api/trips/${itemId}`).flush(tripView(two.familyGroupId));
    await settle(harness);
    familyReads(two);
    http.expectOne(`/api/trips/${itemId}/notes`).flush([]);
    harness.detectChanges();
    http.expectNone(`/api/trips/${itemId}`);
    expect(status(harness).textContent).toBe('Switched to Family Two.');
    expect(itemHeading(harness)).toBe('Coast drive');
  });

  it('shows the Concern unavailable card, not OD-A, for a Concern whose Family is still absent (OD-B, AC-22)', async () => {
    const harness = await visit(`/concerns/${itemId}`);
    shellReads().flush([one]);
    familyReads(one);
    await settle(harness);
    http.expectOne('/api/concerns').flush([]);
    http.expectOne(`/api/concerns/${itemId}`).flush(concernView(departed));
    await settle(harness);
    http.expectOne('/api/family-groups/mine').flush([one]);
    await settle(harness);
    http.match('/api/concerns').forEach(request => request.flush([]));
    harness.detectChanges();
    expect(itemHeading(harness)).toBe('This Concern is unavailable');
    expect(root(harness).textContent).not.toContain('Casey');
    expect(context.familyId()).toBe(one.familyGroupId);
  });

  it('offers Retry for a transient Concern read failure and repeats the same protected read before any Family decision', async () => {
    context.familyId.set(one.familyGroupId);
    const harness = await visit(`/concerns/${itemId}`);
    shellReads().flush([one, two]);
    familyReads(one);
    await settle(harness);
    http.expectOne('/api/concerns').flush([]);
    http.expectOne(`/api/concerns/${itemId}`).flush(null, { status: 503, statusText: 'Unavailable' });
    await settle(harness);
    expect(root(harness).textContent).toContain('This Concern could not be opened. Please try again.');
    expect(root(harness).textContent).not.toContain('This Concern is unavailable');
    expect(context.familyId()).toBe(one.familyGroupId);

    retryButton(harness, 'Retry loading Concern')!.click();
    harness.detectChanges();
    expect(root(harness).textContent).not.toContain('could not be opened');
    expect(context.familyId()).toBe(one.familyGroupId);
    http.expectNone('/api/concerns');
    http.expectOne(`/api/concerns/${itemId}`).flush(concernView(two.familyGroupId));
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(context.familyId()).toBe(two.familyGroupId);
    expect(itemHeading(harness)).toBe('Concern about Casey');
    expect(retryButton(harness, 'Retry loading Concern')).toBeUndefined();
  });

  it('shows the same Concern unavailable card, with no Retry, when a Concern Retry returns 404', async () => {
    context.familyId.set(one.familyGroupId);
    const harness = await visit(`/concerns/${itemId}`);
    shellReads().flush([one, two]);
    familyReads(one);
    await settle(harness);
    http.expectOne('/api/concerns').flush([]);
    http.expectOne(`/api/concerns/${itemId}`).flush(null, { status: 503, statusText: 'Unavailable' });
    await settle(harness);
    retryButton(harness, 'Retry loading Concern')!.click();
    http.expectOne(`/api/concerns/${itemId}`).flush(null, { status: 404, statusText: 'Not Found' });
    await settle(harness);
    http.match('/api/concerns').forEach(request => request.flush([]));
    harness.detectChanges();
    expect(itemHeading(harness)).toBe('This Concern is unavailable');
    expect(root(harness).textContent).toContain('Only Concerns currently shared with you can be opened.');
    expect(retryButton(harness, 'Retry loading Concern')).toBeUndefined();
    expect(context.familyId()).toBe(one.familyGroupId);
  });

  it('announces a Concern switch with the name from the caller’s own Family list (OD-F, AC-19, AC-33)', async () => {
    context.familyId.set(one.familyGroupId);
    const harness = await visit(`/concerns/${itemId}`);
    shellReads().flush([one, two]);
    familyReads(one);
    await settle(harness);
    http.expectOne('/api/concerns').flush([]);
    http.expectOne(`/api/concerns/${itemId}`).flush(concernView(two.familyGroupId));
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(status(harness).textContent).toBe('Switched to Family Two.');
    expect(root(harness).textContent).not.toContain('Name from the item response');
    expect(itemHeading(harness)).toBe('Concern about Casey');
  });

  it.each([
    ['/events/' + itemId, 'Events', 'Events'],
    ['/plans/' + itemId, 'Plans & Trips', 'Plans'],
    ['/trips/' + itemId, 'Plans & Trips', 'Trips'],
    ['/concerns/' + itemId, 'Safety', 'Concerns'],
    ['/incidents/' + itemId, 'Safety', 'Safety Incidents'],
  ])('marks %s as part of %s with one h1 "%s" (AC-29)', async (url, area, h1) => {
    const harness = await visit(url);
    shellReads().flush([one]);
    await settle(harness);
    expect(Array.from(root(harness).querySelectorAll('nav [aria-current]'))
      .map(link => [link.textContent?.trim(), link.getAttribute('aria-current')])).toEqual([[area, 'true']]);
    expect(Array.from(root(harness).querySelectorAll('h1')).map(heading => heading.textContent?.trim())).toEqual([h1]);
  });

  it('renders an Incident URL after authorization with no preselected Family instead of the Family prompt (AC-10)', async () => {
    const harness = await visit(`/incidents/${itemId}`);
    shellReads().flush([one, two]);
    await settle(harness);
    expect(root(harness).textContent).not.toContain('Choose your current Family');
    const read = http.expectOne(`/api/incidents/${itemId}`);
    expect(root(harness).querySelector<HTMLElement>('.page-content')!.hidden).toBe(false);
    read.flush({
      incidentId: itemId, familyGroupId: two.familyGroupId, familyGroupName: 'Name from the item response', subjectProfileId: 'person-c',
      subjectName: 'Casey', createdByProfileId: 'person-b', createdByName: 'Blair', coordinatorProfileId: 'person-b',
      coordinatorName: 'Blair', state: 'Open', createdAt: '2026-10-01T10:00:00Z', lastAuthorizedMutationAt: '2026-10-01T10:00:00Z',
      inactivityFreezeAt: null, frozenAt: null, freezeReason: null, expiresAt: null, resolution: null, originatingConcernId: null,
      participants: [], entries: [], tasks: [], timeline: [],
    });
    await settle(harness);
    familyReads(two);
    harness.detectChanges();
    expect(context.familyId()).toBe(two.familyGroupId);
    expect(status(harness).textContent).toBe('Switched to Family Two.');
    expect(itemHeading(harness)).toBe('Incident about Casey');
  });
});
