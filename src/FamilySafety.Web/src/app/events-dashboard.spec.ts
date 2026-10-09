import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from './app';
import { routes } from './app.routes';
import { CurrentFamilyContext } from './family-context';

/** WI-009-008: the Events dashboard (/events), Create event (/events/new), All events (/events/all), and the item page. */
const profile = {
  profileId: 'me', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const familyA = { familyGroupId: 'family-a', name: 'Family A' };
const familyB = { familyGroupId: 'family-b', name: 'Family B' };
const members = [{ profileId: 'me', displayName: 'Alex' }, { profileId: 'sam', displayName: 'Sam Lee' }, { profileId: 'kim', displayName: 'Kim Lee' }];

function dateAfter(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}
const guid = (n: number) => `aaaaaaaa-0000-4000-8000-${String(n).padStart(12, '0')}`;
const event = (n: number, name: string, days: number, overrides: object = {}) => ({
  eventId: guid(n), familyGroupId: familyA.familyGroupId, name, description: `${name} details`, date: dateAfter(days), time: '18:00:00',
  location: `${name} place`, organizerProfileId: 'sam', organizerDisplayName: 'Sam Lee', canEdit: false, ...overrides,
});
const trip = (id: string, familyGroupId: string, title: string, days: number) => ({
  tripId: id, familyGroupId, title, primaryDestination: `${title} town`, startDate: dateAfter(days), endDate: dateAfter(days + 3),
});

describe('Events dashboard, Create event, and All events (WI-009-008)', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    history.replaceState(null, '', '/');
    await TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.match(() => true).forEach(request => { if (!request.cancelled) request.flush([]); });
    http.verify({ ignoreCancelled: true });
    history.replaceState(null, '', '/');
    vi.useRealTimers();
  });

  function shellReads(groups: object[] = [familyA]) {
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush(groups);
  }
  function familyReads(familyId: string, events: object[], plans: object[] = []) {
    http.expectOne(`/api/family-groups/${familyId}/members`).flush(members);
    http.expectOne(`/api/family-groups/${familyId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${familyId}/events`).flush(events);
    http.expectOne(`/api/family-groups/${familyId}/plans`).flush(plans);
  }

  async function open(path: string, events: object[] = [], trips: object[] | null = [], pending: object[] | null = []) {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(path, App);
    shellReads();
    familyReads(familyA.familyGroupId, events);
    if (trips) http.expectOne('/api/trips').flush(trips);
    if (path !== '/events/new' && pending) http.expectOne(`/api/family-groups/${familyA.familyGroupId}/event-invitations/mine`).flush(pending);
    harness.detectChanges();
    return harness;
  }

  const root = (harness: RouterTestingHarness) => harness.routeNativeElement!;
  const card = (harness: RouterTestingHarness, name: string) => root(harness).querySelector<HTMLElement>(`.events-dash--${name}`)!;
  const rowNames = (element: Element) => Array.from(element.querySelectorAll('li a.item-link')).map(link => link.textContent?.trim());

  it('uses local start times for upcoming Events and Plans and inclusive end dates for Trips', async () => {
    vi.useFakeTimers({ toFake: ['Date'] });
    vi.setSystemTime(new Date(2026, 9, 8, 12, 0, 30));
    const events = [
      event(1, 'Event later today', 0, { time: '13:00' }),
      event(2, 'Event earlier today', 0, { time: '11:00:00' }),
      event(3, 'Event tomorrow', 1, { time: '00:00' }),
      event(4, 'Event date only', 0, { time: '' }),
      event(5, 'Event starting now', 0, { time: '12:00:30' }),
      event(6, 'Event seconds later', 0, { time: '12:00:31' }),
      event(7, 'Event yesterday date only', -1, { time: '' }),
    ];
    const plan = (n: number, title: string, days: number, time: string, overrides: object = {}) => ({
      planId: guid(n), familyGroupId: familyA.familyGroupId, activityOrDestination: title,
      date: dateAfter(days), time, expectedReturnDate: null, ...overrides,
    });
    const plans = [
      plan(11, 'Plan later today', 0, '13:00:00'),
      plan(12, 'Plan earlier today', 0, '11:00', { expectedReturnDate: dateAfter(2) }),
      plan(13, 'Plan tomorrow', 1, '00:00'),
      plan(14, 'Plan date only', 0, ''),
      plan(15, 'Plan starting now', 0, '12:00:30'),
      plan(16, 'Plan seconds later', 0, '12:00:31'),
      plan(17, 'Plan yesterday date only', -1, ''),
    ];
    const trips = [
      { ...trip('progress', familyA.familyGroupId, 'Trip in progress', -2), isCompleted: false },
      { ...trip('today', familyA.familyGroupId, 'Trip ending today', -3), isCompleted: false },
      { ...trip('yesterday', familyA.familyGroupId, 'Trip ended yesterday', -4), isCompleted: false },
      { ...trip('tomorrow', familyA.familyGroupId, 'Trip tomorrow', 1), isCompleted: false },
      { ...trip('completed', familyA.familyGroupId, 'Completed Trip', 1), isCompleted: true },
    ];
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/activities', App);
    shellReads(); familyReads(familyA.familyGroupId, events, plans);
    http.expectOne('/api/trips').flush(trips);
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/event-invitations/mine`).flush([]);
    harness.detectChanges();
    const titles = () => Array.from(root(harness).querySelectorAll('.activity-copy > strong')).map(x => x.textContent).sort();
    expect(titles()).toEqual([
      'Event later today', 'Event tomorrow', 'Event date only', 'Event seconds later',
      'Plan later today', 'Plan tomorrow', 'Plan date only', 'Plan seconds later',
      'Trip in progress', 'Trip ending today', 'Trip tomorrow',
    ].sort());
    expect(root(harness).querySelector('.activity-filters button')?.textContent).toContain('All 11');
    const period = root(harness).querySelector<HTMLSelectElement>('[aria-label="Activity dates"]')!;
    period.value = 'all'; period.dispatchEvent(new Event('change')); harness.detectChanges();
    expect(titles()).toEqual([...events.map(x => x.name), ...plans.map(x => x.activityOrDestination), ...trips.map(x => x.title)].sort());
    expect(root(harness).querySelector('.activity-filters button')?.textContent).toContain('All 19');
  });

  it('renders /events/new as the existing create form, without an item read, keeping the audience and editor rules', async () => {
    const harness = await open('/events/new', [], null);
    expect(root(harness).querySelector('h1')?.textContent?.trim()).toBe('Create Activity');
    expect(root(harness).querySelector('.page-title p')?.textContent).toContain('does not send an invitation or start location sharing');
    expect(root(harness).querySelector<HTMLAnchorElement>('.activity-form-actions a')?.getAttribute('href')).toBe('/activities');
    expect(http.match('/api/events/new')).toHaveLength(0);
    expect(http.match('/api/trips')).toHaveLength(0);
    expect(root(harness).textContent).not.toContain('unavailable');
    for (const id of ['event-name', 'event-location', 'event-description', 'event-date', 'event-time', 'event-audience-search'])
      expect(root(harness).querySelector(`#${id}`)).not.toBeNull();
    // The signed-in adult is never an audience choice; each other adult has the three existing access choices.
    expect(root(harness).querySelector('#event-access-me')).toBeNull();
    const access = root(harness).querySelector<HTMLSelectElement>('#event-access-sam')!;
    expect(Array.from(access.options).map(option => option.textContent?.trim())).toEqual(['No access', 'Can view', 'Can edit and view']);

    const component = harness.routeDebugElement!.componentInstance as {
      eventForm: { patchValue: (value: object) => void };
      setEventMemberAccess: (profileId: string, access: 'none' | 'view' | 'edit') => void;
      createEvent: () => void;
    };
    component.createEvent();
    harness.detectChanges();
    expect(root(harness).textContent).toContain('Choose at least one active adult family member to share this event with.');
    component.eventForm.patchValue({ name: 'Picnic', description: 'Bring food', date: dateAfter(7), time: '12:00', location: 'Park' });
    component.setEventMemberAccess('sam', 'edit');
    component.setEventMemberAccess('kim', 'view');
    component.createEvent();
    const create = http.expectOne(`/api/family-groups/${familyA.familyGroupId}/events`);
    expect(create.request.method).toBe('POST');
    expect(create.request.body.audienceProfileIds).toEqual(['sam', 'kim']);
    expect(create.request.body.editorProfileIds).toEqual(['sam']);
    create.flush({}, { status: 500, statusText: 'Error' });
    harness.detectChanges();
    expect(root(harness).textContent).toContain('The event could not be created. Please try again.');
    expect(TestBed.inject(Router).url).toBe('/events/new');
  });

  it('opens the created Event at /events/{eventId} with the success message (D10)', async () => {
    const harness = await open('/events/new', [], null);
    const component = harness.routeDebugElement!.componentInstance as {
      eventForm: { patchValue: (value: object) => void };
      setEventMemberAccess: (profileId: string, access: 'none' | 'view' | 'edit') => void;
      createEvent: () => void;
    };
    component.eventForm.patchValue({ name: 'Picnic', description: 'Bring food', date: dateAfter(7), time: '12:00', location: 'Park' });
    component.setEventMemberAccess('sam', 'view');
    component.createEvent();
    const created = event(9, 'Picnic', 7, { organizerProfileId: 'me', organizerDisplayName: 'Alex', canEdit: true });
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/events`).flush(created);
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe(`/events/${guid(9)}`);
    shellReads();
    http.expectOne(`/api/events/${guid(9)}`).flush(created);
    familyReads(familyA.familyGroupId, [created]);
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(root(harness).querySelector('h1')?.textContent?.trim()).toBe('Events');
    expect(root(harness).querySelector('article h2')?.textContent?.trim()).toBe('Picnic');
    expect(root(harness).querySelector('article [role="status"]')?.textContent).toContain('Event created and shared with the selected family members.');
  });

  it('shows no create message on a plain visit to an Event item URL', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(`/events/${guid(9)}`, App);
    shellReads();
    http.expectOne(`/api/events/${guid(9)}`).flush(event(9, 'Picnic', 7));
    familyReads(familyA.familyGroupId, []);
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(root(harness).querySelector('article h2')?.textContent?.trim()).toBe('Picnic');
    expect(root(harness).textContent).not.toContain('Event created');
  });

  it('on /events/new, shows a failed member read with Retry instead of the one-person explanation', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/events/new', App);
    shellReads();
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/members`).flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/events`).flush([]);
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/plans`).flush([]);
    harness.detectChanges();
    const form = root(harness).querySelector<HTMLElement>('form.events-create-form')!;
    expect(form.textContent).toContain('Your family directory could not be loaded.');
    expect(form.textContent).not.toContain('This Family has no other active adult members yet.');
    Array.from(form.querySelectorAll('button')).find(button => button.textContent?.trim() === 'Retry loading Family members')!.click();
    http.expectNone(`/api/family-groups/${familyA.familyGroupId}/events`);
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/members`).flush(members);
    harness.detectChanges();
    expect(root(harness).querySelector('#event-access-sam')).not.toBeNull();
    expect(form.textContent).not.toContain('could not be loaded');
  });

  it('unifies authorized projections, sorts across types and excludes other Families', async () => {
    const harness = await open('/activities', [event(1, 'Dinner', 8), event(2, 'Other family', 1, { familyGroupId: 'elsewhere' })], [trip('t', 'family-a', 'Travel', 2)]);
    expect(root(harness).querySelector('h1')?.textContent).toBe('Activities');
    expect(root(harness).querySelectorAll('nav[aria-label="Main navigation"] a')).toHaveLength(5);
    expect(root(harness).querySelectorAll('form.plans-form, form.events-create-form')).toHaveLength(0);
    expect(Array.from(root(harness).querySelectorAll('.activity-copy > strong')).map(x => x.textContent)).toEqual(['Travel', 'Dinner']);
    expect(root(harness).querySelector('.events-create-link')?.getAttribute('href')).toBe('/activities/new');
  });

  it.each([['/events', '/activities?type=Event'], ['/events/all', '/activities?type=Event&period=all'], ['/plans', '/activities?type=Plan'], ['/plans-trips', '/activities']])('redirects %s to %s', async (path, target) => {
      const harness = await open(path);
      expect(TestBed.inject(Router).url).toBe(target);
      expect(root(harness).querySelector('h1')?.textContent).toBe('Activities');
      await harness.navigateByUrl('/me', App);
      http.match(() => true).forEach(r => { if (!r.cancelled) r.flush(r.request.url === '/api/profile' ? profile : []); });
  });

  it('cancels pending RSVP reads and clears all visible activity data on Family switch', async () => {
    TestBed.inject(CurrentFamilyContext).familyId.set(familyA.familyGroupId);
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/activities', App);
    shellReads([familyA, familyB]);
    familyReads('family-a', [event(1, 'A dinner', 3)]);
    http.expectOne('/api/trips').flush([trip('a', 'family-a', 'A trip', 2)]);
    const pending = http.expectOne('/api/family-groups/family-a/event-invitations/mine');
    harness.detectChanges();
    const select = root(harness).querySelector<HTMLSelectElement>('#current-family')!;
    select.value = 'family-b'; select.dispatchEvent(new Event('change')); harness.detectChanges();
    expect(pending.cancelled).toBe(true);
    expect(root(harness).querySelectorAll('.activity-row')).toHaveLength(0);
    familyReads('family-b', [event(2, 'B dinner', 4, { familyGroupId: 'family-b' })]);
    http.expectOne('/api/trips').flush([]);
    http.expectOne('/api/family-groups/family-b/event-invitations/mine').flush([{ invitationId: 'pending', eventId: guid(2), name: 'B dinner', date: '2020-01-01', time: '12:00' }]);
    harness.detectChanges();
    expect(root(harness).querySelector('.activity-pending')?.textContent).toContain('B dinner');
    expect(root(harness).querySelector('.activity-copy > strong')?.textContent).toBe('B dinner');
    expect(root(harness).querySelector('app-activities')?.textContent).not.toContain('A dinner');
  });

  it('opens edit URLs only after the item read authorizes access', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/events/' + guid(1) + '/edit', App);
    shellReads(); familyReads('family-a', []);
    http.expectOne('/api/events/' + guid(1)).flush({}, { status: 404, statusText: 'Not found' });
    harness.detectChanges();
    expect(root(harness).querySelector('form.events-edit-form')).toBeNull();
    expect(root(harness).querySelector('app-item-unavailable')).not.toBeNull();
  });
});
