import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

const profile = {
  profileId: '22222222-2222-2222-2222-222222222222',
  displayName: 'Development Adult',
  contactEmail: null,
  contactEmailVisibility: 'OnlyMe',
  contactPhone: null,
  contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null,
  birthdayDay: null,
  birthdayVisibility: 'OnlyMe',
};

/** Shows one routed page in a router-less fixture (the shell picks its view from the route in the app). */
function showView(fixture: { componentInstance: unknown }, view: string): void {
  (fixture.componentInstance as { view: { set: (value: string) => void } }).view.set(view);
}

describe('App', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    history.replaceState(null, '', '/');
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.match((request) => request.url.endsWith('/events')).forEach((request) => request.flush([]));
    http.match((request) => request.url.endsWith('/plans')).forEach((request) => request.flush([]));
    http.match('/api/trips').forEach((request) => request.flush([]));
    http.verify();
  });

  it('loads the adult profile with private optional fields', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    // No routes or outlet here: check the Me / Settings link's target, then show it through in-app navigate().
    const profileLink = Array.from(element.querySelectorAll('nav a')).find(link => link.textContent?.trim() === 'Me / Settings');
    expect(profileLink?.getAttribute('href')).toBe('/me');
    (fixture.componentInstance as unknown as { navigate(view: string): void }).navigate('profile');
    fixture.detectChanges();
    expect(element.querySelector('h1')?.textContent).toContain('Your profile');
    const displayName = element.querySelector<HTMLInputElement>('#display-name');
    expect(displayName, element.innerHTML).not.toBeNull();
    expect(displayName?.value).toBe('Development Adult');
    expect((element.querySelector('#contact-email-visibility') as HTMLSelectElement).value).toBe(
      'OnlyMe',
    );
    expect(element.textContent).toContain('We do not collect a birth year');
  });

  it('sends an explicit visibility choice when saving', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    fixture.detectChanges();

    const component = fixture.componentInstance as unknown as {
      profileForm: { patchValue: (value: object) => void };
      save: () => void;
    };
    component.profileForm.patchValue({
      contactEmail: 'family@example.com',
      contactEmailVisibility: 'ActiveFamilyGroup',
    });
    component.save();

    const request = http.expectOne('/api/profile');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.contactEmailVisibility).toBe('ActiveFamilyGroup');
    request.flush({
      ...profile,
      contactEmail: 'family@example.com',
      contactEmailVisibility: 'ActiveFamilyGroup',
    });
  });

  it('associates every profile input with a visible label', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const controls = Array.from(
      element.querySelectorAll<HTMLInputElement | HTMLSelectElement>('input, select'),
    );
    const labels = Array.from(element.querySelectorAll<HTMLLabelElement>('label'));

    for (const control of controls) {
      expect(labels.some((label) => label.htmlFor === control.id)).toBe(true);
    }
  });

  it('creates an event with an explicit audience of active adult family members', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([
      {
        familyGroupId: '33333333-3333-3333-3333-333333333333',
        name: 'Development Family',
      },
    ]);

    const memberRequests = http.match('/api/family-groups/33333333-3333-3333-3333-333333333333/members');
    expect(memberRequests).toHaveLength(1);
    memberRequests.forEach((request) =>
      request.flush([
        { profileId: profile.profileId, displayName: profile.displayName },
        {
          profileId: '77777777-7777-7777-7777-777777777777',
          displayName: 'Development Recipient',
        },
        {
          profileId: '99999999-9999-9999-9999-999999999999',
          displayName: 'Other Adult',
        },
      ]),
    );
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/relationships')
      .flush([]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/events').flush([]);
    // WI-009-008: the create form is the /events/new page and the full list is /events/all.
    showView(fixture, 'eventCreate');
    fixture.detectChanges();

    const audienceAccessId = 'event-access-77777777-7777-7777-7777-777777777777';
    const audienceAccess = (fixture.nativeElement as HTMLElement).querySelector<HTMLSelectElement>(
      `#${audienceAccessId}`,
    );
    expect(audienceAccess).not.toBeNull();
    expect(
      Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('label')).some(
        (label) => label.htmlFor === audienceAccessId,
      ),
    ).toBe(true);

    const component = fixture.componentInstance as unknown as {
      eventForm: { patchValue: (value: object) => void };
      eventAudienceSearch: { set: (value: string) => void };
      visibleEventAudienceMembers: () => readonly { profileId: string; displayName: string }[];
      setEventMemberAccess: (profileId: string, access: 'none' | 'view' | 'edit') => void;
      createEvent: () => void;
      beginEventEdit: (event: object) => void;
      eventEditForm: { patchValue: (value: object) => void };
      saveEventEdits: (event: object) => void;
    };
    component.eventForm.patchValue({
      name: 'Family dinner',
      description: 'Bring a favorite dish.',
      date: '2026-10-03',
      time: '18:30',
      location: 'Community room',
    });
    component.eventAudienceSearch.set('recipient');
    expect(component.visibleEventAudienceMembers()).toEqual([
      {
        profileId: '77777777-7777-7777-7777-777777777777',
        displayName: 'Development Recipient',
      },
    ]);
    component.setEventMemberAccess('77777777-7777-7777-7777-777777777777', 'edit');
    component.createEvent();

    const createRequest = http.expectOne(
      '/api/family-groups/33333333-3333-3333-3333-333333333333/events',
    );
    expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body.audienceProfileIds).toEqual([
      '77777777-7777-7777-7777-777777777777',
    ]);
    expect(createRequest.request.body.editorProfileIds).toEqual([
      '77777777-7777-7777-7777-777777777777',
    ]);
    createRequest.flush({
      eventId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      familyGroupId: '33333333-3333-3333-3333-333333333333',
      name: 'Family dinner',
      description: 'Bring a favorite dish.',
      date: '2026-10-03',
      time: '18:30:00',
      location: 'Community room',
      organizerProfileId: profile.profileId,
      organizerDisplayName: profile.displayName,
      canEdit: true,
    });
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/events')
      .flush([
        {
          eventId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          familyGroupId: '33333333-3333-3333-3333-333333333333',
          name: 'Family dinner',
          description: 'Bring a favorite dish.',
          date: '2026-10-03',
          time: '18:30:00',
          location: 'Community room',
          organizerProfileId: profile.profileId,
          organizerDisplayName: profile.displayName,
          canEdit: true,
        },
      ]);
    showView(fixture, 'eventsAll');
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Family dinner');
    component.beginEventEdit({
      eventId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      familyGroupId: '33333333-3333-3333-3333-333333333333',
      name: 'Family dinner',
      description: 'Bring a favorite dish.',
      date: '2026-10-03',
      time: '18:30:00',
      location: 'Community room',
      organizerProfileId: profile.profileId,
      organizerDisplayName: profile.displayName,
      canEdit: true,
    });
    component.eventEditForm.patchValue({ name: 'Updated family dinner' });
    component.saveEventEdits({
      eventId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      familyGroupId: '33333333-3333-3333-3333-333333333333',
      canEdit: true,
    });
    const updateRequest = http.expectOne(
      '/api/family-groups/33333333-3333-3333-3333-333333333333/events/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    );
    expect(updateRequest.request.method).toBe('PUT');
    expect(updateRequest.request.body.name).toBe('Updated family dinner');
    updateRequest.flush({
      eventId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      familyGroupId: '33333333-3333-3333-3333-333333333333',
      name: 'Updated family dinner',
      description: 'Bring a favorite dish.',
      date: '2026-10-03',
      time: '18:30:00',
      location: 'Community room',
      organizerProfileId: profile.profileId,
      organizerDisplayName: profile.displayName,
      canEdit: true,
    });
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Updated family dinner');
  });

  it('creates a plan with selected active adult recipients and no combined audience option', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([
      {
        familyGroupId: '33333333-3333-3333-3333-333333333333',
        name: 'Development Family',
      },
    ]);

    const memberRequests = http.match('/api/family-groups/33333333-3333-3333-3333-333333333333/members');
    expect(memberRequests).toHaveLength(1);
    memberRequests[0].flush([
      { profileId: profile.profileId, displayName: profile.displayName },
      {
        profileId: '77777777-7777-7777-7777-777777777777',
        displayName: 'Development Recipient',
      },
      {
        profileId: '99999999-9999-9999-9999-999999999999',
        displayName: 'Other Adult',
      },
    ]);
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/relationships')
      .flush([]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/events').flush([]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/plans').flush([]);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const audienceChoices = Array.from(
      element.querySelectorAll<HTMLInputElement>('input[name="audienceType"]'),
    );
    expect(audienceChoices).toHaveLength(2);
    expect(element.textContent).not.toContain('This Family Group and selected people');
    const returnDate = element.querySelector('#plan-return-date');
    const audience = element.querySelector('#plan-audience-group');
    const checkInDate = element.querySelector('#plan-checkin-date');
    const planNotes = element.querySelector('#plan-notes');
    expect(returnDate).not.toBeNull();
    expect(audience).not.toBeNull();
    expect(checkInDate).not.toBeNull();
    expect(planNotes).not.toBeNull();
    expect(returnDate!.compareDocumentPosition(audience!) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    expect(audience!.compareDocumentPosition(checkInDate!) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    expect(checkInDate!.compareDocumentPosition(planNotes!) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();

    const component = fixture.componentInstance as unknown as {
      planForm: { patchValue: (value: object) => void };
      planAudienceSearch: { set: (value: string) => void };
      setPlanAudienceMember: (profileId: string, selected: boolean) => void;
      createPlan: () => void;
    };
    component.planForm.patchValue({
      activityOrDestination: 'Visit the lake',
      date: '2026-10-03',
      time: '10:00',
      audienceType: 'SelectedPeople',
      expectedCheckInDate: '2026-10-03',
      expectedCheckInTime: '12:00',
      notes: ' Bring snacks.\nMeet by the north entrance. ',
    });
    component.planAudienceSearch.set('recipient');
    component.setPlanAudienceMember('77777777-7777-7777-7777-777777777777', true);
    component.createPlan();

    const createRequest = http.expectOne(
      '/api/family-groups/33333333-3333-3333-3333-333333333333/plans',
    );
    expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body.audienceType).toBe('SelectedPeople');
    expect(createRequest.request.body.audienceProfileIds).toEqual([
      '77777777-7777-7777-7777-777777777777',
    ]);
    expect(createRequest.request.body.expectedCheckInTime).toBe('12:00');
    expect(createRequest.request.body.notes).toBe(' Bring snacks.\nMeet by the north entrance. ');
    createRequest.flush({
      planId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      familyGroupId: '33333333-3333-3333-3333-333333333333',
      activityOrDestination: 'Visit the lake',
      date: '2026-10-03',
      time: '10:00:00',
      expectedReturnDate: null,
      expectedReturnTime: null,
      expectedCheckInDate: '2026-10-03',
      expectedCheckInTime: '12:00:00',
      audienceType: 'SelectedPeople',
      creatorProfileId: profile.profileId,
      creatorDisplayName: profile.displayName,
      notes: 'Bring snacks.\nMeet by the north entrance.',
    });
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/plans')
      .flush([
        {
          planId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          familyGroupId: '33333333-3333-3333-3333-333333333333',
          activityOrDestination: 'Visit the lake',
          date: '2026-10-03',
          time: '10:00:00',
          expectedReturnDate: null,
          expectedReturnTime: null,
          expectedCheckInDate: '2026-10-03',
          expectedCheckInTime: '12:00:00',
          audienceType: 'SelectedPeople',
          creatorProfileId: profile.profileId,
          creatorDisplayName: profile.displayName,
          notes: 'Bring snacks.\nMeet by the north entrance.',
        },
      ]);
    fixture.detectChanges();
    expect(element.textContent).toContain('Visit the lake');
    expect(element.textContent).toContain('Plans you can see');
    const renderedNotes = element.querySelector('.plan-notes');
    expect(renderedNotes?.textContent).toBe('Bring snacks.\nMeet by the north entrance.');
    expect(renderedNotes?.innerHTML).not.toContain('<br>');
  });

  it('keeps Plan recipient search available and selection valid through one and zero matches', () => {
    const groupId = '33333333-3333-3333-3333-333333333333';
    const blairId = '77777777-7777-7777-7777-777777777777';
    const caseyId = '99999999-9999-9999-9999-999999999999';
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([{ familyGroupId: groupId, name: 'Development Family' }]);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush([
      { profileId: profile.profileId, displayName: profile.displayName },
      { profileId: blairId, displayName: 'Blair' },
      { profileId: caseyId, displayName: 'Casey' },
    ]);
    http.expectOne(`/api/family-groups/${groupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${groupId}/events`).flush([]);
    http.expectOne(`/api/family-groups/${groupId}/plans`).flush([]);
    showView(fixture, 'plans');
    fixture.detectChanges();

    const form = (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form.plans-form')!;
    form.querySelector<HTMLInputElement>('#plan-audience-people')!.click();
    fixture.detectChanges();
    const search = () => form.querySelector<HTMLInputElement>('#plan-audience-search');
    const create = () => form.querySelector<HTMLButtonElement>('button[type="submit"]')!;
    const filter = (value: string) => {
      const input = search()!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();
    };
    expect(search()?.labels?.[0]?.textContent).toContain('Filter family members');
    expect(create().disabled).toBe(true);
    form.querySelector<HTMLInputElement>(`#plan-recipient-${blairId}`)!.click();
    fixture.detectChanges();

    filter('Casey');
    expect(search()).not.toBeNull();
    expect(form.querySelector(`#plan-recipient-${blairId}`)).toBeNull();
    expect(form.querySelector(`#plan-recipient-${caseyId}`)).not.toBeNull();
    expect(create().disabled).toBe(false);

    filter('nobody');
    expect(search()).not.toBeNull();
    expect(form.textContent).toContain('No matching Family members.');
    expect(form.textContent).not.toContain('No other active adult family members are available');
    expect(create().disabled).toBe(false);

    filter('Blair');
    expect(form.querySelector<HTMLInputElement>(`#plan-recipient-${blairId}`)?.checked).toBe(true);
    filter('');
    expect(form.querySelectorAll('ul[aria-label="Active adult plan recipients"] input[type="checkbox"]')).toHaveLength(2);

    const component = fixture.componentInstance as unknown as {
      planForm: { patchValue: (value: object) => void };
    };
    component.planForm.patchValue({
      activityOrDestination: 'Visit the lake', date: '2026-10-10', time: '10:00', audienceType: 'SelectedPeople',
    });
    filter('nobody');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    const request = http.expectOne(`/api/family-groups/${groupId}/plans`);
    expect(request.request.body.audienceProfileIds).toEqual([blairId]);
    request.flush({}, { status: 400, statusText: 'Bad Request' });
  });

  it('keeps the true no-eligible-recipient Plan state distinct from search with no matches', () => {
    const groupId = '33333333-3333-3333-3333-333333333333';
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([{ familyGroupId: groupId, name: 'Development Family' }]);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush([
      { profileId: profile.profileId, displayName: profile.displayName },
    ]);
    http.expectOne(`/api/family-groups/${groupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${groupId}/events`).flush([]);
    http.expectOne(`/api/family-groups/${groupId}/plans`).flush([]);
    showView(fixture, 'plans');
    fixture.detectChanges();

    const form = (fixture.nativeElement as HTMLElement).querySelector<HTMLFormElement>('form.plans-form')!;
    form.querySelector<HTMLInputElement>('#plan-audience-people')!.click();
    fixture.detectChanges();
    expect(form.querySelector('#plan-audience-search')).toBeNull();
    expect(form.textContent).toContain('No other active adult family members are available to select.');
    expect(form.textContent).not.toContain('No matching Family members.');
    expect(form.querySelector<HTMLButtonElement>('button[type="submit"]')?.disabled).toBe(true);

    form.querySelector<HTMLInputElement>('#plan-audience-group')!.click();
    fixture.detectChanges();
    expect(form.querySelector<HTMLButtonElement>('button[type="submit"]')?.disabled).toBe(false);
  });

  it('does not offer event editing to a view-only audience member', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([
      {
        familyGroupId: '33333333-3333-3333-3333-333333333333',
        name: 'Development Family',
      },
    ]);
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/members')
      .flush([
        { profileId: profile.profileId, displayName: profile.displayName },
        { profileId: '77777777-7777-7777-7777-777777777777', displayName: 'Development Recipient' },
      ]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/relationships').flush([]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/events').flush([
      {
        eventId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        familyGroupId: '33333333-3333-3333-3333-333333333333',
        name: 'Family dinner',
        description: 'Bring a favorite dish.',
        date: '2026-10-03',
        time: '18:30:00',
        location: 'Community room',
        organizerProfileId: '88888888-8888-8888-8888-888888888888',
        organizerDisplayName: 'Another Organizer',
        canEdit: false,
      },
    ]);
    showView(fixture, 'eventsAll');
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Family dinner');
    expect(
      Array.from(element.querySelectorAll('button')).some((button) =>
        button.textContent?.includes('Edit event details'),
      ),
    ).toBe(false);
  });

  it('sends an invitation for an administered family group', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http
      .expectOne('/api/family-groups/administered')
      .flush([
        { familyGroupId: '33333333-3333-3333-3333-333333333333', name: 'Development Family' },
      ]);
    http.expectOne('/api/family-groups/mine').flush([
      { familyGroupId: '33333333-3333-3333-3333-333333333333', name: 'Development Family' },
    ]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/members').flush([]);
    http.expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/relationships').flush([]);
    fixture.detectChanges();

    const component = fixture.componentInstance as unknown as {
      invitationForm: { patchValue: (value: object) => void };
      sendInvitation: () => void;
    };
    component.invitationForm.patchValue({ recipientEmail: 'adult@example.com' });
    component.sendInvitation();

    const request = http.expectOne('/api/membership-invitations');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.recipientEmail).toBe('adult@example.com');
    request.flush(null, { status: 202, statusText: 'Accepted' });
  });

  it('keeps a fragment token out of the preview URL and supports acceptance', () => {
    history.replaceState(null, '', '/#invite=opaque-token');
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    const preview = http.expectOne('/api/membership-invitations/preview');
    expect(preview.request.url).not.toContain('opaque-token');
    expect(preview.request.body.token).toBe('opaque-token');
    preview.flush({
      familyGroupName: 'Development Family',
      inviterDisplayName: 'Development Adult',
    });
    fixture.detectChanges();

    const component = fixture.componentInstance as unknown as { acceptInvitation: () => void };
    component.acceptInvitation();
    const accept = http.expectOne('/api/membership-invitations/accept');
    expect(accept.request.body.token).toBe('opaque-token');
    accept.flush({ familyGroupId: 'accepted-family' });
    http.expectOne('/api/family-groups/mine').flush([{ familyGroupId: 'accepted-family', name: 'Family' }]);
    http.expectOne('/api/family-groups/accepted-family/members').flush([]);
    http.expectOne('/api/family-groups/accepted-family/relationships').flush([]);
  });

  it('hides a one-group selector and loads a minimal directory', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http
      .expectOne('/api/family-groups/mine')
      .flush([
        { familyGroupId: '33333333-3333-3333-3333-333333333333', name: 'Development Family' },
      ]);
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/members')
      .flush([{ profileId: profile.profileId, displayName: 'Development Adult' }]);
    http
      .expectOne('/api/family-groups/33333333-3333-3333-3333-333333333333/relationships')
      .flush([]);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('#directory-family-group')).toBeNull();
    expect(element.textContent).toContain('Development Adult');
  });

  it('requires explicit Family selection and clears stale detail when the context changes', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([
      { familyGroupId: 'group-one', name: 'Family One' },
      { familyGroupId: 'group-two', name: 'Family Two' },
    ]);
    http.expectNone('/api/family-groups/group-one/members');
    fixture.detectChanges();
    const selector = (fixture.nativeElement as HTMLElement).querySelector<HTMLSelectElement>('#current-family');
    expect(selector).not.toBeNull();
    expect(selector?.value).toBe('');
    selector!.value = 'group-one';
    selector!.dispatchEvent(new Event('change'));
    http
      .expectOne('/api/family-groups/group-one/members')
      .flush([{ profileId: profile.profileId, displayName: 'Development Adult' }]);
    const previousGroupRelationships = http.expectOne('/api/family-groups/group-one/relationships');
    fixture.detectChanges();

    const component = fixture.componentInstance as unknown as {
      selectDirectoryMember: (member: object) => void;
      selectedMember: () => object | null;
      familyRelationships: () => readonly object[];
    };
    component.selectDirectoryMember({
      profileId: profile.profileId,
      displayName: profile.displayName,
    });
    expect(component.selectedMember()).not.toBeNull();
    selector!.value = 'group-two';
    selector!.dispatchEvent(new Event('change'));
    expect(component.selectedMember()).toBeNull();
    expect(component.familyRelationships()).toEqual([]);
    http.expectOne('/api/family-groups/group-two/members').flush([]);
    http.expectOne('/api/family-groups/group-two/relationships').flush([]);
    previousGroupRelationships.flush([
      {
        relationshipId: 'old-group-relationship',
        proposerProfileId: 'old-proposer',
        proposerDisplayName: 'Old Group Adult',
        proposerLabel: 'Parent',
        recipientProfileId: 'old-recipient',
        recipientDisplayName: 'Old Group Member',
        recipientLabel: 'AdultChild',
        status: 'Accepted',
        isCurrentUserProposer: false,
        isCurrentUserRecipient: false,
      },
    ]);
    fixture.detectChanges();
    expect(component.familyRelationships()).toEqual([]);

    expect((fixture.nativeElement as HTMLElement).querySelector('#current-family')).not.toBeNull();
  });

  it('loads another member profile only after selection', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http
      .expectOne('/api/family-groups/mine')
      .flush([{ familyGroupId: 'group-one', name: 'Family One' }]);
    http
      .expectOne('/api/family-groups/group-one/members')
      .flush([{ profileId: 'other-profile', displayName: 'Other Adult' }]);
    http.expectOne('/api/family-groups/group-one/relationships').flush([]);
    fixture.detectChanges();

    const component = fixture.componentInstance as unknown as {
      selectDirectoryMember: (member: object) => void;
    };
    component.selectDirectoryMember({ profileId: 'other-profile', displayName: 'Other Adult' });
    http.expectOne('/api/family-members/other-profile/profile').flush({
      profileId: 'other-profile',
      displayName: 'Other Adult',
      contactEmail: 'shared@example.com',
      contactPhone: null,
      birthdayMonth: null,
      birthdayDay: null,
    });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('shared@example.com');
  });

  it('renders reciprocal labels and removes self from relationship choices', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http
      .expectOne('/api/family-groups/mine')
      .flush([{ familyGroupId: 'group-one', name: 'Family One' }]);
    http.expectOne('/api/family-groups/group-one/members').flush([
      { profileId: profile.profileId, displayName: profile.displayName },
      { profileId: 'other-profile', displayName: 'Other Adult' },
    ]);
    http.expectOne('/api/family-groups/group-one/relationships').flush([
      {
        relationshipId: 'relationship-one',
        proposerProfileId: 'other-profile',
        proposerDisplayName: 'Other Adult',
        proposerLabel: 'Parent',
        recipientProfileId: profile.profileId,
        recipientDisplayName: profile.displayName,
        recipientLabel: 'AdultChild',
        status: 'Accepted',
        isCurrentUserProposer: false,
        isCurrentUserRecipient: true,
      },
    ]);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const memberOptions = Array.from(
      element.querySelectorAll<HTMLOptionElement>('#relationship-member option'),
    );
    expect(memberOptions.map((option) => option.value)).toEqual(['', 'other-profile']);
    expect(element.textContent).toContain('Other Adult: Parent');
    expect(element.textContent).toContain('Development Adult: Adult child');
    expect(element.textContent).toContain("do not change anyone's access or permissions");
    expect(
      element.querySelector<HTMLLabelElement>('label[for="relationship-label"]')?.textContent,
    ).toContain('Your relationship to them');

    const removeButton = Array.from(element.querySelectorAll('button')).find((button) =>
      button.textContent?.includes('Remove relationship'),
    );
    expect(removeButton).toBeDefined();
    removeButton?.click();
    fixture.detectChanges();
    expect(element.textContent).toContain('Remove this relationship?');
    const confirmButton = Array.from(element.querySelectorAll('button')).find((button) =>
      button.textContent?.includes('Confirm removal'),
    );
    expect(confirmButton).toBeDefined();
    confirmButton?.click();
    const removeRequest = http.expectOne('/api/family-groups/relationships/relationship-one');
    expect(removeRequest.request.method).toBe('DELETE');
    removeRequest.flush(null, { status: 204, statusText: 'No Content' });
    http.expectOne('/api/family-groups/group-one/relationships').flush([]);
    fixture.detectChanges();
    expect(element.textContent).toContain('Relationship removed.');
  });

  it('sends a relationship suggestion and uses participant-only confirmation copy', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http
      .expectOne('/api/family-groups/mine')
      .flush([{ familyGroupId: 'group-one', name: 'Family One' }]);
    http
      .expectOne('/api/family-groups/group-one/members')
      .flush([{ profileId: 'other-profile', displayName: 'Other Adult' }]);
    http.expectOne('/api/family-groups/group-one/relationships').flush([]);
    fixture.detectChanges();

    const component = fixture.componentInstance as unknown as {
      relationshipForm: { patchValue: (value: object) => void };
      proposeRelationship: () => void;
    };
    component.relationshipForm.patchValue({
      recipientProfileId: 'other-profile',
      proposerLabel: 'Parent',
    });
    component.proposeRelationship();

    const request = http.expectOne('/api/family-groups/group-one/relationship-proposals');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      recipientProfileId: 'other-profile',
      proposerLabel: 'Parent',
    });
    request.flush(null, { status: 202, statusText: 'Accepted' });
    http.expectOne('/api/family-groups/group-one/relationships').flush([
      {
        relationshipId: 'pending-one',
        proposerProfileId: profile.profileId,
        proposerDisplayName: profile.displayName,
        proposerLabel: 'Parent',
        recipientProfileId: 'other-profile',
        recipientDisplayName: 'Other Adult',
        recipientLabel: 'AdultChild',
        status: 'Pending',
        isCurrentUserProposer: true,
        isCurrentUserRecipient: false,
      },
    ]);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Only the two of you can see this suggestion',
    );
  });
});
