import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { displayDateTime } from '../date-format';
import { ItemFamilyOutcome } from '../item-route';
import { Trips } from './trips';

const group = { familyGroupId: '33333333-3333-3333-3333-333333333333', name: 'Family' };
const creator = { profileId: '11111111-1111-1111-1111-111111111111', displayName: 'Alex' };
const traveler = { profileId: '22222222-2222-2222-2222-222222222222', displayName: 'Jordan' };
const recipient = { profileId: '44444444-4444-4444-4444-444444444444', displayName: 'Emma' };
const nextRecipient = { profileId: '77777777-7777-7777-7777-777777777777', displayName: 'Riley' };

function trip(canEdit: boolean) {
  return {
    tripId: '55555555-5555-5555-5555-555555555555',
    familyGroupId: group.familyGroupId, familyGroupName: group.name,
    creatorProfileId: creator.profileId, creatorDisplayName: creator.displayName,
    canEdit, title: 'Family visit', primaryDestination: 'Atlanta',
    startDate: '2026-10-01', endDate: '2026-10-05',
    description: 'Visiting relatives', transportationNotes: null,
    accommodationNotes: null, expectedArrivalDate: '2026-10-01',
    expectedArrivalTime: '17:00:00', expectedArrivalTimeZone: 'America/New_York',
    audienceType: 'SelectedPeople', travelers: [traveler],
    contacts: [{ contactId: '66666666-6666-6666-6666-666666666666', name: 'Guide', relationshipRole: null, phone: '+44 20 7946 0958', email: null }],
    audienceRecipients: canEdit ? [recipient] : [],
  };
}

describe('Trips', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Trips],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => { http.verify(); });

  it('shows the selected Family Trips without retaining other active Family items', () => {
    const fixture = TestBed.createComponent(Trips);
    const otherGroup = { familyGroupId: 'other-family', name: 'Other Family' };
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.componentRef.setInput('allActiveGroups', [group, otherGroup]);
    fixture.componentRef.setInput('currentFamilyId', group.familyGroupId);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([
      trip(true), { ...trip(true), tripId: 'other-trip', familyGroupId: otherGroup.familyGroupId,
        familyGroupName: otherGroup.name, title: 'Other Family Trip' },
    ]);
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Family visit');
    expect(text).not.toContain('Other Family Trip');
  });

  it('keeps a creator-owned Trip reachable after membership ends without group creation access', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', []);
    fixture.componentRef.setInput('allActiveGroups', []);
    fixture.componentRef.setInput('currentFamilyId', null);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Trips you created outside your active Families');
    expect(element.textContent).toContain('Family visit');
    expect(element.textContent).not.toContain('Create trip');
    expect(element.querySelector('#trip-group')).toBeNull();
  });

  it('creates a Trip with travelers, selected audience, contact, and explicit arrival time zone', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([]);

    const component = fixture.componentInstance as unknown as {
      openCreate: () => void;
      tripForm: { patchValue: (value: object) => void; controls: { contacts: { at: (index: number) => { patchValue: (value: object) => void } } } };
      toggleTraveler: (id: string, selected: boolean) => void;
      toggleRecipient: (id: string, selected: boolean) => void;
      addContact: () => void;
      saveTrip: () => void;
    };
    component.openCreate();
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush([creator, traveler, recipient]);
    component.tripForm.patchValue({
      title: 'Family visit', primaryDestination: 'Atlanta',
      startDate: '2026-10-01', endDate: '2026-10-05',
      audienceType: 'SelectedPeople', expectedArrivalDate: '2026-10-01',
      expectedArrivalTime: '17:00', expectedArrivalTimeZone: 'America/New_York',
    });
    component.toggleTraveler(traveler.profileId, true);
    component.toggleRecipient(recipient.profileId, true);
    component.addContact();
    component.tripForm.controls.contacts.at(0).patchValue({ name: 'Guide', phone: '+44 20 7946 0958' });
    component.saveTrip();

    const request = http.expectOne(`/api/family-groups/${group.familyGroupId}/trips`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body.travelerProfileIds).toEqual([traveler.profileId]);
    expect(request.request.body.audienceProfileIds).toEqual([recipient.profileId]);
    expect(request.request.body.expectedArrivalTimeZone).toBe('America/New_York');
    expect(request.request.body.contacts[0].phone).toBe('+44 20 7946 0958');
    request.flush(trip(true));
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    http.expectOne('/api/trips').flush([trip(true)]);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('America/New_York');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Current recipients:');
  });

  it('shows the selected Family as read-only context when creating a Trip, and creates it in that Family', () => {
    const selected = { familyGroupId: group.familyGroupId, name: 'Rivera Family' };
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', [selected]);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([]);
    const component = fixture.componentInstance as unknown as {
      openCreate: () => void;
      tripForm: { patchValue: (value: object) => void };
      toggleTraveler: (id: string, selected: boolean) => void;
      saveTrip: () => void;
    };
    component.openCreate();
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush([creator, traveler]);
    fixture.detectChanges();

    const form = (fixture.nativeElement as HTMLElement).querySelector('form')!;
    const essentials = form.querySelector<HTMLElement>('section[aria-labelledby="trip-essentials-heading"]')!;
    expect(essentials.textContent).toContain('Family: Rivera Family');
    // No user-editable Family choice: no dropdown in the form, and no essentials control labelled "Family".
    expect(form.querySelector('select')).toBeNull();
    expect(Array.from(essentials.querySelectorAll('label')).some(label => label.textContent?.includes('Family'))).toBe(false);

    component.tripForm.patchValue({
      title: 'Family visit', primaryDestination: 'Atlanta', startDate: '2026-10-01', endDate: '2026-10-05',
      audienceType: 'FamilyGroup',
    });
    component.toggleTraveler(traveler.profileId, true);
    component.saveTrip();
    const request = http.expectOne(`/api/family-groups/${group.familyGroupId}/trips`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body.audienceType).toBe('FamilyGroup');
  });

  it('shows expected arrival with the shared date and time display and keeps the time zone label and stored values', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    const component = fixture.componentInstance as unknown as {
      openRoutedTrip: (id: string) => void;
      beginEdit: () => void;
      tripForm: { getRawValue: () => Record<string, unknown> };
    };
    component.openRoutedTrip(trip(true).tripId);
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    // The same calendar date and wall-clock time, formatted by displayDateTime; no time-zone conversion.
    expect(text).toContain(`Expected arrival: ${displayDateTime('2026-10-01', '17:00:00')} (America/New_York)`);
    expect(text).toContain('Thu, Oct 1, 2026 · 5:00 PM (America/New_York)');
    expect(text).not.toContain('2026-10-01 at 17:00:00');

    // Presentation only: editing still starts from the stored values.
    component.beginEdit();
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush([creator, traveler, recipient]);
    const value = component.tripForm.getRawValue();
    expect(value['expectedArrivalDate']).toBe('2026-10-01');
    expect(value['expectedArrivalTime']).toBe('17:00');
    expect(value['expectedArrivalTimeZone']).toBe('America/New_York');
  });

  it('shows a viewer a generic selected audience label without recipient identities or edit controls', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(false)]);
    const component = fixture.componentInstance as unknown as { openRoutedTrip: (id: string) => void };
    component.openRoutedTrip(trip(false).tripId);
    http.expectOne(`/api/trips/${trip(false).tripId}`).flush(trip(false));
    http.expectOne(`/api/trips/${trip(false).tripId}/notes`).flush([]);
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Shared with selected people');
    expect(text).not.toContain('Emma');
    expect(text).not.toContain('Selected recipients:');
    expect(text).not.toContain('Edit trip details');
  });

  it('renders optional free text as text rather than markup', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([]);
    const component = fixture.componentInstance as unknown as { openRoutedTrip: (id: string) => void };
    component.openRoutedTrip(trip(false).tripId);
    http.expectOne(`/api/trips/${trip(false).tripId}`).flush({
      ...trip(false), description: '<img src=x onerror=alert(1)>',
    });
    http.expectOne(`/api/trips/${trip(false).tripId}/notes`).flush([]);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('<img src=x onerror=alert(1)>');
    expect(element.querySelector('img')).toBeNull();
  });

  it('sends a core edit without audience fields', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    const component = fixture.componentInstance as unknown as {
      openRoutedTrip: (id: string) => void;
      beginEdit: () => void;
      saveTrip: () => void;
      tripForm: { patchValue: (value: object) => void };
    };
    component.openRoutedTrip(trip(true).tripId);
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    component.beginEdit();
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush([creator, traveler, recipient]);
    component.tripForm.patchValue({ title: 'Updated visit' });
    component.saveTrip();
    const request = http.expectOne(`/api/trips/${trip(true).tripId}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.title).toBe('Updated visit');
    expect(request.request.body).not.toHaveProperty('audienceType');
    expect(request.request.body).not.toHaveProperty('audienceProfileIds');
    request.flush({ ...trip(true), title: 'Updated visit' });
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    http.expectOne('/api/trips').flush([{ ...trip(true), title: 'Updated visit' }]);
  });

  it('previews and confirms complete audience replacement, then refreshes a stale preview for reconfirmation', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    const component = fixture.componentInstance as unknown as {
      openRoutedTrip: (id: string) => void;
      beginAudienceChange: () => void;
      toggleAudienceRecipient: (id: string, selected: boolean) => void;
      previewAudience: () => void;
      confirmAudience: () => void;
    };
    component.openRoutedTrip(trip(true).tripId);
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    component.beginAudienceChange();
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush([creator, recipient, nextRecipient]);
    component.toggleAudienceRecipient(recipient.profileId, false);
    component.toggleAudienceRecipient(nextRecipient.profileId, true);
    component.previewAudience();
    const preview = http.expectOne(`/api/trips/${trip(true).tripId}/audience/preview`);
    expect(preview.request.method).toBe('POST');
    expect(preview.request.body).toEqual({ audienceType: 'SelectedPeople', audienceProfileIds: [nextRecipient.profileId] });
    preview.flush({ previewToken: 'first', audienceType: 'SelectedPeople', audienceRecipients: [nextRecipient],
      gainingAccess: [nextRecipient], losingAccess: [recipient] });
    component.confirmAudience();
    const confirm = http.expectOne(`/api/trips/${trip(true).tripId}/audience/confirm`);
    expect(confirm.request.body.previewToken).toBe('first');
    confirm.flush({ message: 'stale' }, { status: 409, statusText: 'Conflict' });
    const refreshed = http.expectOne(`/api/trips/${trip(true).tripId}/audience/preview`);
    refreshed.flush({ previewToken: 'second', audienceType: 'SelectedPeople', audienceRecipients: [nextRecipient],
      gainingAccess: [], losingAccess: [recipient] });
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Review the refreshed changes and confirm again.');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Will lose access: Emma');
    http.expectNone(`/api/trips/${trip(true).tripId}/audience/confirm`);
  });

  it('gives a departed creator saved-recipient reductions without loading the Family directory', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([]);
    const departedTrip = { ...trip(true), audienceRecipients: [recipient] };
    const component = fixture.componentInstance as unknown as {
      openRoutedTrip: (id: string) => void;
      beginAudienceChange: () => void;
      previewAudience: () => void;
    };
    component.openRoutedTrip(departedTrip.tripId);
    http.expectOne(`/api/trips/${departedTrip.tripId}`).flush(departedTrip);
    http.expectOne(`/api/trips/${departedTrip.tripId}/notes`).flush([]);
    component.beginAudienceChange();
    http.expectNone(`/api/family-groups/${group.familyGroupId}/members`);
    component.previewAudience();
    const preview = http.expectOne(`/api/trips/${departedTrip.tripId}/audience/preview`);
    expect(preview.request.body).toEqual({ audienceType: 'SelectedPeople', audienceProfileIds: [recipient.profileId] });
    preview.flush({ previewToken: 'reduce', audienceType: 'SelectedPeople', audienceRecipients: [recipient],
      gainingAccess: [], losingAccess: [] });
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('only keep eligible recipients already saved');
  });

  it('blocks an end date before the start date and zero selected recipients', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([]);
    const component = fixture.componentInstance as unknown as {
      openCreate: () => void;
      saveTrip: () => void;
      tripForm: { patchValue: (value: object) => void };
      toggleTraveler: (id: string, selected: boolean) => void;
    };
    component.openCreate();
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush([traveler]);
    component.tripForm.patchValue({
      title: 'Trip', primaryDestination: 'Atlanta',
      startDate: '2026-10-05', endDate: '2026-10-01', audienceType: 'SelectedPeople',
    });
    component.toggleTraveler(traveler.profileId, true);
    component.saveTrip();
    http.expectNone(`/api/family-groups/${group.familyGroupId}/trips`);
    component.tripForm.patchValue({ endDate: '2026-10-06' });
    component.saveTrip();
    http.expectNone(`/api/family-groups/${group.familyGroupId}/trips`);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Complete the required fields');
  });

  it('shows plain-text Notes with provenance and only server-authorized actions', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    const component = fixture.componentInstance as unknown as { openRoutedTrip: (id: string) => void };
    component.openRoutedTrip(trip(true).tripId);
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([
      { noteId: 'a', authorProfileId: creator.profileId, authorDisplayName: 'Alex',
        content: '<script>alert(1)</script>\nFlight booked', createdAt: '2026-09-27T10:00:00Z',
        updatedAt: '2026-09-27T11:00:00Z', canEdit: true, canDelete: true, canHide: false },
      { noteId: 'b', authorProfileId: recipient.profileId, authorDisplayName: 'Emma',
        content: 'Rental car', createdAt: '2026-09-27T10:00:00Z', updatedAt: null,
        canEdit: false, canDelete: false, canHide: true },
    ]);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('<script>alert(1)</script>');
    expect(element.querySelector('script')).toBeNull();
    expect(element.textContent).toContain('Edited');
    expect(element.querySelectorAll('.trip-note-list button').length).toBe(3);
    expect(element.textContent).not.toContain('Reply');
  });

  it('shows Notes loading before a successful empty read, then shows the empty state', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('tripId', trip(true).tripId);
    fixture.detectChanges();
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    const notesRead = http.expectOne(`/api/trips/${trip(true).tripId}/notes`);
    expect(notesRead.request.method).toBe('GET');
    fixture.detectChanges();

    const notesSection = (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLElement>('section[aria-labelledby="trip-notes-heading"]')!;
    expect(notesSection.querySelector('[role="status"]')?.textContent).toContain('Loading Notes');
    expect(notesSection.textContent).not.toContain('No Notes yet.');

    notesRead.flush([]);
    fixture.detectChanges();
    expect(notesSection.querySelector('[role="status"]')).toBeNull();
    expect(notesSection.textContent).toContain('No Notes yet.');
    expect(notesSection.querySelector('button')?.textContent).toContain('Add note');
  });

  it('shows a Notes read failure and retries the Notes request to display returned Notes', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('tripId', trip(true).tripId);
    fixture.detectChanges();
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    const notesUrl = `/api/trips/${trip(true).tripId}/notes`;
    http.expectOne(notesUrl).flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    const notesSection = (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLElement>('section[aria-labelledby="trip-notes-heading"]')!;
    expect(notesSection.querySelector('[role="alert"]')?.textContent).toContain('We couldn’t load Notes.');
    expect(notesSection.textContent).not.toContain('No Notes yet.');
    const retry = Array.from(notesSection.querySelectorAll('button'))
      .find(button => button.textContent?.trim() === 'Retry loading Notes');
    expect(retry).toBeTruthy();

    retry!.click();
    const retryRead = http.expectOne(notesUrl);
    expect(retryRead.request.method).toBe('GET');
    fixture.detectChanges();
    expect(notesSection.querySelector('[role="status"]')?.textContent).toContain('Loading Notes');
    expect(notesSection.querySelector('[role="alert"]')).toBeNull();
    expect(notesSection.textContent).not.toContain('No Notes yet.');
    retryRead.flush([{ noteId: 'a', authorProfileId: creator.profileId, authorDisplayName: 'Alex',
      content: 'Flight booked', createdAt: '2026-09-27T10:00:00Z', updatedAt: null,
      canEdit: true, canDelete: true, canHide: false }]);
    fixture.detectChanges();
    expect(notesSection.querySelector('[role="status"]')).toBeNull();
    expect(notesSection.textContent).toContain('Flight booked');
    expect(notesSection.textContent).not.toContain('No Notes yet.');
  });

  it('shows a valid empty state after retry and retains loaded Notes if a later read fails', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('tripId', trip(true).tripId);
    fixture.detectChanges();
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    const notesUrl = `/api/trips/${trip(true).tripId}/notes`;
    http.expectOne(notesUrl).flush(null, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    const notesSection = (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLElement>('section[aria-labelledby="trip-notes-heading"]')!;
    Array.from(notesSection.querySelectorAll('button'))
      .find(button => button.textContent?.trim() === 'Retry loading Notes')!.click();
    http.expectOne(notesUrl).flush([]);
    fixture.detectChanges();
    expect(notesSection.textContent).toContain('No Notes yet.');
    expect(notesSection.querySelector('[role="alert"]')).toBeNull();

    const component = fixture.componentInstance as unknown as { loadNotes: (id: string) => void };
    component.loadNotes(trip(true).tripId);
    http.expectOne(notesUrl).flush([{ noteId: 'a', authorProfileId: creator.profileId, authorDisplayName: 'Alex',
      content: 'Flight booked', createdAt: '2026-09-27T10:00:00Z', updatedAt: null,
      canEdit: true, canDelete: true, canHide: false }]);
    fixture.detectChanges();
    component.loadNotes(trip(true).tripId);
    http.expectOne(notesUrl).flush(null, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(notesSection.querySelector('[role="alert"]')?.textContent).toContain('We couldn’t load Notes.');
    expect(notesSection.textContent).toContain('Flight booked');
    expect(notesSection.textContent).not.toContain('No Notes yet.');
  });

  it('sends add and own edit through bounded routes', () => {
    const fixture = TestBed.createComponent(Trips);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    const component = fixture.componentInstance as unknown as {
      openRoutedTrip: (id: string) => void;
      noteDraft: { setValue: (value: string) => void };
      noteEditDraft: { setValue: (value: string) => void };
      addNote: () => void;
      beginNoteEdit: (note: object) => void;
      saveNoteEdit: (note: object) => void;
    };
    const own = { noteId: 'a', authorProfileId: creator.profileId, authorDisplayName: 'Alex',
      content: 'Original', createdAt: '2026-09-27T10:00:00Z', updatedAt: null,
      canEdit: true, canDelete: true, canHide: false };
    const other = { ...own, noteId: 'b', authorProfileId: recipient.profileId,
      canEdit: false, canDelete: false, canHide: true };
    const base = `/api/trips/${trip(true).tripId}/notes`;
    component.openRoutedTrip(trip(true).tripId);
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    http.expectOne(base).flush([own, other]);
    component.noteDraft.setValue(' New Note ');
    component.addNote();
    const add = http.expectOne(base);
    expect(add.request.body).toEqual({ content: 'New Note' });
    add.flush(own);
    http.expectOne(base).flush([own, other]);
    component.beginNoteEdit(own);
    component.noteEditDraft.setValue('Updated');
    component.saveNoteEdit(own);
    const edit = http.expectOne(`${base}/a`);
    expect(edit.request.method).toBe('PUT');
    expect(edit.request.body).toEqual({ content: 'Updated' });
    edit.flush(own);
    http.expectOne(base).flush([own, other]);
  });

  it.each([
    { action: 'Delete note', confirm: 'Confirm delete', noteId: 'a', urlSuffix: '/a', method: 'DELETE', prompt: 'Delete this note?' },
    { action: 'Hide note', confirm: 'Confirm hide', noteId: 'b', urlSuffix: '/b/hide', method: 'POST', prompt: 'Hide this note?' },
  ])('requires confirmation before $action and keeps the Note on Cancel', ({ action, confirm, noteId, urlSuffix, method, prompt }) => {
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true)]);
    const component = fixture.componentInstance as unknown as { openRoutedTrip: (id: string) => void };
    const own = { noteId: 'a', authorProfileId: creator.profileId, authorDisplayName: 'Alex',
      content: 'Flight booked', createdAt: '2026-09-27T10:00:00Z', updatedAt: null,
      canEdit: true, canDelete: true, canHide: false };
    const other = { ...own, noteId: 'b', authorProfileId: recipient.profileId, authorDisplayName: 'Emma',
      content: 'Rental car', canEdit: false, canDelete: false, canHide: true };
    const base = `/api/trips/${trip(true).tripId}/notes`;
    component.openRoutedTrip(trip(true).tripId);
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    http.expectOne(base).flush([own, other]);
    fixture.detectChanges();
    const notes = fixture.nativeElement as HTMLElement;
    const note = () => Array.from(notes.querySelectorAll('li'))
      .find(item => item.textContent?.includes(noteId === 'a' ? 'Flight booked' : 'Rental car'))!;
    const button = (name: string) => Array.from(note().querySelectorAll('button'))
      .find(candidate => candidate.textContent?.trim() === name)!;
    expect(button(action)).toBeTruthy();
    expect(note().textContent).not.toContain(noteId === 'a' ? 'Hide note' : 'Delete note');

    button(action).click();
    fixture.detectChanges();
    expect(note().textContent).toContain(prompt);
    expect(button(confirm)).toBeTruthy();
    http.expectNone(`${base}${urlSuffix}`);
    button('Cancel').click();
    fixture.detectChanges();
    expect(note().textContent).not.toContain(prompt);
    expect(note().textContent).toContain(noteId === 'a' ? 'Flight booked' : 'Rental car');
    http.expectNone(`${base}${urlSuffix}`);

    button(action).click();
    fixture.detectChanges();
    button(confirm).click();
    const removal = http.expectOne(`${base}${urlSuffix}`);
    expect(removal.request.method).toBe(method);
    if (method === 'POST') expect(removal.request.body).toEqual({});
    removal.flush(null);
    http.expectOne(base).flush(noteId === 'a' ? [other] : [own]);
    fixture.detectChanges();
    expect(notes.textContent).not.toContain(noteId === 'a' ? 'Flight booked' : 'Rental car');
  });
});

describe('Trips — item URL /trips/:tripId (REQ-UX-014, WI-009-005)', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Trips],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => { http.verify(); });

  function openRouted(tripId: string, outcome: Promise<ItemFamilyOutcome>) {
    const calls: [string, string][] = [];
    const fixture = TestBed.createComponent(Trips);
    fixture.componentRef.setInput('tripId', tripId);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.componentRef.setInput('adoptItemFamily', (familyGroupId: string, kind: string) => { calls.push([familyGroupId, kind]); return outcome; });
    fixture.detectChanges();
    return { fixture, calls, element: fixture.nativeElement as HTMLElement };
  }

  it('reads only the Trip, decides the Family after the 200, then renders the existing detail', async () => {
    let decide!: (value: ItemFamilyOutcome) => void;
    const { fixture, calls, element } = openRouted(trip(true).tripId, new Promise(resolve => decide = resolve));
    http.expectNone('/api/trips');
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    fixture.detectChanges();
    expect(calls).toEqual([[group.familyGroupId, 'trip']]);
    expect(element.textContent).not.toContain('Family visit');
    expect(element.textContent).toContain('Loading Trip…');
    decide('switched');
    await fixture.whenStable();
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    fixture.detectChanges();
    expect(element.querySelector('h2')?.textContent).toContain('Family visit');
    const back = Array.from(element.querySelectorAll('a')).find(link => link.textContent?.trim() === 'Back to Trips');
    expect(back?.getAttribute('href')).toBe('/trips');
    expect(element.textContent).not.toContain('Create trip');
  });

  it('renders a retained creator Trip with no Family switch (OD-A)', async () => {
    const { fixture, element } = openRouted(trip(true).tripId, Promise.resolve('retained'));
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    await fixture.whenStable();
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    fixture.detectChanges();
    expect(element.querySelector('h2')?.textContent).toContain('Family visit');
    expect(element.textContent).not.toContain('This Trip is unavailable');
  });

  it('shows the generic unavailable card for a 404 and never asks for a Family decision', async () => {
    const { fixture, calls, element } = openRouted(trip(true).tripId, Promise.resolve('switched'));
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    await fixture.whenStable();
    expect(calls).toEqual([]);
    expect(element.querySelector('h2')?.textContent).toBe('This Trip is unavailable');
    expect(element.textContent).toContain('Only Trips currently shared with you can be opened.');
    expect(element.querySelector('a')?.getAttribute('href')).toBe('/trips');
    expect(document.activeElement).toBe(element.querySelector('h2'));
  });

  const retryTrip = (element: HTMLElement) =>
    Array.from(element.querySelectorAll('button')).find(button => button.textContent?.trim() === 'Retry loading Trip');

  it('shows a transient Trip read failure as a read error with Retry, not as the unavailable card or the Trips list', async () => {
    let decide!: (value: ItemFamilyOutcome) => void;
    const { fixture, calls, element } = openRouted(trip(true).tripId, new Promise(resolve => decide = resolve));
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(null, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(element.querySelector('[role="alert"]')?.textContent).toContain('This Trip could not be opened. Please try again.');
    expect(element.textContent).not.toContain('This Trip is unavailable');
    expect(element.querySelector('app-item-unavailable')).toBeNull();
    expect(element.textContent).not.toContain('No Trips are currently available');
    expect(calls).toEqual([]);

    // Retry repeats the same item read; the Family is decided only after its 200.
    retryTrip(element)!.click();
    fixture.detectChanges();
    expect(element.textContent).toContain('Loading Trip…');
    expect(element.textContent).not.toContain('could not be opened');
    http.expectNone('/api/trips');
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    fixture.detectChanges();
    expect(calls).toEqual([[group.familyGroupId, 'trip']]);
    expect(element.textContent).not.toContain('Family visit');
    decide('switched');
    await fixture.whenStable();
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    fixture.detectChanges();
    expect(element.querySelector('h2')?.textContent).toContain('Family visit');
    expect(retryTrip(element)).toBeUndefined();
  });

  it('keeps Retry after a second transient failure, then shows the generic card with no Retry when a Retry returns 404', async () => {
    const { fixture, calls, element } = openRouted(trip(true).tripId, Promise.resolve('switched'));
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(null, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    retryTrip(element)!.click();
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();
    expect(element.textContent).toContain('This Trip could not be opened. Please try again.');
    expect(retryTrip(element)).toBeDefined();

    retryTrip(element)!.click();
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    await fixture.whenStable();
    expect(element.querySelector('h2')?.textContent).toBe('This Trip is unavailable');
    expect(element.textContent).toContain('Only Trips currently shared with you can be opened.');
    expect(element.textContent).not.toContain('could not be opened');
    expect(retryTrip(element)).toBeUndefined();
    expect(calls).toEqual([]);
  });

  it('renders a retained creator Trip after a Retry, still with no Family switch (OD-A)', async () => {
    const { fixture, calls, element } = openRouted(trip(true).tripId, Promise.resolve('retained'));
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(null, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    retryTrip(element)!.click();
    http.expectOne(`/api/trips/${trip(true).tripId}`).flush(trip(true));
    await fixture.whenStable();
    http.expectOne(`/api/trips/${trip(true).tripId}/notes`).flush([]);
    fixture.detectChanges();
    expect(calls).toEqual([[group.familyGroupId, 'trip']]);
    expect(element.querySelector('h2')?.textContent).toContain('Family visit');
    expect(element.textContent).not.toContain('This Trip is unavailable');
  });

  it('shows the same unavailable card for a non-GUID ID without any request', () => {
    const { fixture, calls, element } = openRouted('not-a-trip', Promise.resolve('current'));
    http.expectNone(() => true);
    fixture.detectChanges();
    expect(calls).toEqual([]);
    expect(element.querySelector('h2')?.textContent).toBe('This Trip is unavailable');
  });

  it('links each list row and retained-creator row to its Trip URL', () => {
    const fixture = TestBed.createComponent(Trips);
    const departed = { ...trip(true), tripId: '88888888-8888-8888-8888-888888888888', familyGroupId: 'departed', title: 'Old trip' };
    fixture.componentRef.setInput('activeGroups', [group]);
    fixture.componentRef.setInput('allActiveGroups', [group]);
    fixture.componentRef.setInput('currentFamilyId', group.familyGroupId);
    fixture.componentRef.setInput('ownProfileId', creator.profileId);
    fixture.detectChanges();
    http.expectOne('/api/trips').flush([trip(true), departed]);
    fixture.detectChanges();
    const rows = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('a.trip-list-button'));
    expect(rows.map(row => row.getAttribute('href'))).toEqual([`/trips/${trip(true).tripId}`, `/trips/${departed.tripId}`]);
    expect((fixture.nativeElement as HTMLElement).querySelector('button.trip-list-button')).toBeNull();
  });
});
