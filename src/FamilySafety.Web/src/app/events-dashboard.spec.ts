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
  });

  function shellReads(groups: object[] = [familyA]) {
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush(groups);
  }
  function familyReads(familyId: string, events: object[]) {
    http.expectOne(`/api/family-groups/${familyId}/members`).flush(members);
    http.expectOne(`/api/family-groups/${familyId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${familyId}/events`).flush(events);
    http.expectOne(`/api/family-groups/${familyId}/plans`).flush([]);
  }

  async function open(path: string, events: object[] = [], trips: object[] | null = [], pending: object[] | null = []) {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(path, App);
    shellReads();
    familyReads(familyA.familyGroupId, events);
    if (trips) http.expectOne('/api/trips').flush(trips);
    if (path === '/events' && pending) http.expectOne(`/api/family-groups/${familyA.familyGroupId}/event-invitations/mine`).flush(pending);
    harness.detectChanges();
    return harness;
  }

  const root = (harness: RouterTestingHarness) => harness.routeNativeElement!;
  const card = (harness: RouterTestingHarness, name: string) => root(harness).querySelector<HTMLElement>(`.events-dash--${name}`)!;
  const rowNames = (element: Element) => Array.from(element.querySelectorAll('li a.item-link')).map(link => link.textContent?.trim());

  it('renders /events as the dashboard: title, Family subtitle, one Create event action, and the four panel ③ cards', async () => {
    const harness = await open('/events');
    expect(root(harness).querySelector('h1')?.textContent?.trim()).toBe('Events');
    expect(root(harness).querySelector('.page-title p')?.textContent).toContain('Family A');
    const create = root(harness).querySelectorAll<HTMLAnchorElement>('.page-title a');
    expect(Array.from(create).map(link => [link.textContent?.trim(), link.getAttribute('href')])).toEqual([['Create event', '/events/new']]);
    expect(Array.from(root(harness).querySelectorAll('.events-grid h2')).map(h => h.textContent?.trim()))
      .toEqual(['Upcoming Events', 'Trips', 'Quick Links', 'RSVPs Needed']);
    // The dashboard is a summary: no create form and no full list on it.
    expect(root(harness).querySelector('#event-name')).toBeNull();
    expect(root(harness).querySelector('.events-overview')).toBeNull();
  });

  it('summarizes only current-Family upcoming Events, nearest first, capped at five, with item links and no pill', async () => {
    const events = [
      event(1, 'Past picnic', -2),
      event(2, 'Sixth', 40), event(3, 'Third', 9), event(4, 'First', 0), event(5, 'Fifth', 30),
      event(6, 'Second', 3), event(7, 'Fourth', 12), event(8, 'Other Family dinner', 1, { familyGroupId: 'family-z' }),
    ];
    const harness = await open('/events', events);
    const upcoming = card(harness, 'upcoming');
    expect(rowNames(upcoming)).toEqual(['First', 'Second', 'Third', 'Fourth', 'Fifth']);
    expect(upcoming.querySelector('a.item-link')?.getAttribute('href')).toBe(`/events/${guid(4)}`);
    expect(upcoming.textContent).not.toMatch(/Going|Maybe|Not Going|Organizer/);
    expect(upcoming.querySelector('a.events-view-all')?.getAttribute('href')).toBe('/events/all');
  });

  it('keeps the Upcoming Events card with a compact empty state when nothing is upcoming', async () => {
    const harness = await open('/events', [event(1, 'Past picnic', -5)]);
    expect(card(harness, 'upcoming').textContent).toContain('No upcoming Events are shared with you yet.');
    expect(rowNames(card(harness, 'upcoming'))).toEqual([]);
  });

  it('shows only selected-Family Trips and never a retained creator Trip from another Family', async () => {
    const trips = [
      trip('t-4', familyA.familyGroupId, 'Fourth trip', 50), trip('t-2', familyA.familyGroupId, 'Second trip', 10),
      trip('retained', 'family-former', 'Retained creator trip', 2), trip('t-1', familyA.familyGroupId, 'First trip', 5),
      trip('t-3', familyA.familyGroupId, 'Third trip', 20),
    ];
    const harness = await open('/events', [], trips);
    const tripsCard = card(harness, 'trips');
    expect(rowNames(tripsCard)).toEqual(['First trip', 'Second trip', 'Third trip']);
    expect(tripsCard.textContent).not.toContain('Retained creator trip');
    expect(tripsCard.querySelector('a.item-link')?.getAttribute('href')).toBe('/trips/t-1');
    expect(tripsCard.querySelector('a.events-view-all')?.getAttribute('href')).toBe('/trips');
    expect(tripsCard.textContent).not.toMatch(/Planning|Considering|audience/i);
  });

  it('renders an empty authorized Pending read without deriving invitations from Event audience', async () => {
    const harness = await open('/events', [event(1, 'Upcoming picnic', 4)]);
    const rsvps = card(harness, 'rsvps');
    expect(rsvps.querySelector('h2')?.textContent?.trim()).toBe('RSVPs Needed');
    expect(rsvps.querySelector('p')?.textContent?.trim()).toBe('You have no pending Event invitations in this Family.');
    expect(rsvps.querySelectorAll('button, a, input, select, img, li')).toHaveLength(0);
    expect(rsvps.textContent).not.toMatch(/\d/);
    expect(http.match(request => /rsvp|invitation/i.test(request.url))).toHaveLength(0);
  });

  it('shows invitation loading separately from empty success', async () => {
    const harness = await open('/events', [], [], null);
    expect(card(harness, 'rsvps').textContent).toContain('Loading your pending invitations');
    expect(card(harness, 'rsvps').textContent).not.toContain('no pending');
    http.expectOne('/api/family-groups/family-a/event-invitations/mine').flush([]);
    harness.detectChanges();
    expect(card(harness, 'rsvps').textContent).toContain('no pending Event invitations');
  });

  it('shows past-dated own Pending invitations from the protected read and links to Event detail', async () => {
    const harness = await open('/events', [], [], [{ invitationId: 'invitation', eventId: guid(9), name: 'Past gathering', date: '2020-01-01', time: '18:00' }]);
    expect(rowNames(card(harness, 'rsvps'))).toEqual(['Past gathering']);
    expect(card(harness, 'rsvps').querySelector('a')?.getAttribute('href')).toBe(`/events/${guid(9)}`);
    expect(card(harness, 'rsvps').textContent).toContain('Pending');
  });

  it('shows read failure and Retry repeats only the protected invitation read', async () => {
    const harness = await open('/events', [], [], null);
    http.expectOne('/api/family-groups/family-a/event-invitations/mine').flush({}, { status: 500, statusText: 'Failed' });
    harness.detectChanges();
    expect(card(harness, 'rsvps').querySelector('[role="alert"]')).not.toBeNull();
    expect(card(harness, 'rsvps').textContent).not.toContain('no pending');
    card(harness, 'rsvps').querySelector<HTMLButtonElement>('button')!.click();
    http.expectOne('/api/family-groups/family-a/event-invitations/mine').flush([]);
    harness.detectChanges();
    expect(card(harness, 'rsvps').textContent).toContain('no pending');
  });

  it('cancels pending invitation reads and clears prior data on Family switch', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/events', App);
    shellReads([familyA, familyB]);
    const context = TestBed.inject(CurrentFamilyContext);
    const app = harness.routeDebugElement!.componentInstance as App;
    (app as any).switchCurrentFamily('family-a');
    familyReads('family-a', []);
    http.expectOne('/api/trips').flush([]);
    const old = http.expectOne('/api/family-groups/family-a/event-invitations/mine');
    (app as any).switchCurrentFamily('family-b');
    expect(old.cancelled).toBe(true);
    expect(context.familyId()).toBe('family-b');
    harness.detectChanges();
    expect(card(harness, 'rsvps').querySelectorAll('li')).toHaveLength(0);
    familyReads('family-b', []);
    http.expectOne('/api/trips').flush([]);
    http.expectOne('/api/family-groups/family-b/event-invitations/mine').flush([{ invitationId: 'b', eventId: guid(8), name: 'Family B gathering', date: '2020-01-01', time: '18:00' }]);
    harness.detectChanges();
    expect(rowNames(card(harness, 'rsvps'))).toEqual(['Family B gathering']);
  });

  it('offers exactly the six approved Quick Links as real routes', async () => {
    const harness = await open('/events');
    const links = Array.from(card(harness, 'links').querySelectorAll('a'));
    expect(links.map(link => [link.querySelector('strong')?.textContent?.trim(), link.getAttribute('href')])).toEqual([
      ['Create an event', '/events/new'], ['All events', '/events/all'], ['Trips', '/trips'],
      ['Plans', '/plans'], ['Family members', '/family'], ['Your profile', '/me'],
    ]);
    expect(card(harness, 'links').tagName).toBe('NAV');
  });

  it('renders /events/new as the existing create form, without an item read, keeping the audience and editor rules', async () => {
    const harness = await open('/events/new', [], null);
    expect(root(harness).querySelector('h1')?.textContent?.trim()).toBe('Create event');
    expect(root(harness).querySelector('.page-title p')?.textContent).toContain('does not send an invitation or start location sharing');
    expect(root(harness).querySelector<HTMLAnchorElement>('.events-back-link')?.getAttribute('href')).toBe('/events');
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

  it('renders /events/all as the complete list: past Events, count, details, organizer, and inline edit only where canEdit', async () => {
    const events = [event(1, 'Past picnic', -30, { canEdit: true }), event(2, 'Future dinner', 10)];
    const harness = await open('/events/all', events, null);
    expect(root(harness).querySelector('h1')?.textContent?.trim()).toBe('All events');
    expect(root(harness).querySelector<HTMLAnchorElement>('.events-back-link')?.getAttribute('href')).toBe('/events');
    expect(http.match('/api/events/all')).toHaveLength(0);
    const list = root(harness).querySelector<HTMLElement>('.events-overview')!;
    expect(rowNames(list)).toEqual(['Past picnic', 'Future dinner']);
    expect(list.querySelector('.card-count')?.textContent?.trim()).toBe('2');
    expect(list.textContent).toContain('Past picnic details');
    expect(list.textContent).toContain('Organized by Sam Lee');
    const rows = Array.from(list.querySelectorAll('li.events-card'));
    const edit = (row: Element) => Array.from(row.querySelectorAll('button')).filter(button => button.textContent?.includes('Edit event details'));
    expect(edit(rows[0])).toHaveLength(1);
    expect(edit(rows[1])).toHaveLength(0);
    edit(rows[0])[0].click();
    harness.detectChanges();
    expect(root(harness).querySelector(`#edit-event-name-${guid(1)}`)).not.toBeNull();
    expect(root(harness).querySelector('#event-name')).toBeNull();
  });

  it('shows the existing Event success message on /events/all after a successful inline edit', async () => {
    const editable = event(1, 'Past picnic', -30, { canEdit: true });
    const harness = await open('/events/all', [editable], null);
    const list = root(harness).querySelector<HTMLElement>('.events-overview')!;
    expect(list.querySelector('[role="status"]')).toBeNull();
    Array.from(list.querySelectorAll('button')).find(button => button.textContent?.includes('Edit event details'))!.click();
    harness.detectChanges();
    const component = harness.routeDebugElement!.componentInstance as { eventEditForm: { patchValue: (value: object) => void } };
    component.eventEditForm.patchValue({ name: 'Spring picnic' });
    Array.from(list.querySelectorAll('button')).find(button => button.textContent?.trim() === 'Save event details')!.click();
    const update = http.expectOne(`/api/family-groups/${familyA.familyGroupId}/events/${guid(1)}`);
    expect(update.request.method).toBe('PUT');
    update.flush({ ...editable, name: 'Spring picnic' });
    harness.detectChanges();
    expect(list.querySelector('[role="status"]')?.textContent).toContain('Event details updated.');
    expect(rowNames(list)).toEqual(['Spring picnic']);
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

  it('offers Retry on /events/all when the Event list read fails, and shows the list after it succeeds', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/events/all', App);
    shellReads();
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/members`).flush(members);
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/events`).flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/plans`).flush([]);
    harness.detectChanges();
    const list = root(harness).querySelector<HTMLElement>('.events-overview')!;
    expect(list.textContent).toContain('Events could not be loaded');
    Array.from(list.querySelectorAll('button')).find(button => button.textContent?.trim() === 'Retry loading Events')!.click();
    http.expectOne(`/api/family-groups/${familyA.familyGroupId}/events`).flush([event(1, 'Retried dinner', 5)]);
    harness.detectChanges();
    expect(rowNames(list)).toEqual(['Retried dinner']);
  });

  it('clears and reloads the dashboard Event and Trip summaries when the Family changes', async () => {
    TestBed.inject(CurrentFamilyContext).familyId.set(familyA.familyGroupId);
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/events', App);
    shellReads([familyA, familyB]);
    familyReads(familyA.familyGroupId, [event(1, 'Family A event', 3)]);
    http.expectOne('/api/trips').flush([trip('a-trip', familyA.familyGroupId, 'Family A trip', 4), trip('b-trip', familyB.familyGroupId, 'Family B trip', 6)]);
    harness.detectChanges();
    expect(rowNames(card(harness, 'upcoming'))).toEqual(['Family A event']);
    expect(rowNames(card(harness, 'trips'))).toEqual(['Family A trip']);

    const select = root(harness).querySelector<HTMLSelectElement>('#current-family')!;
    select.value = familyB.familyGroupId;
    select.dispatchEvent(new Event('change'));
    harness.detectChanges();
    expect(rowNames(card(harness, 'upcoming'))).toEqual([]);
    expect(rowNames(card(harness, 'trips'))).toEqual([]);
    familyReads(familyB.familyGroupId, [event(2, 'Family B event', 5, { familyGroupId: familyB.familyGroupId })]);
    http.expectOne('/api/trips').flush([trip('a-trip', familyA.familyGroupId, 'Family A trip', 4), trip('b-trip', familyB.familyGroupId, 'Family B trip', 6)]);
    harness.detectChanges();
    expect(TestBed.inject(Router).url).toBe('/events');
    expect(rowNames(card(harness, 'upcoming'))).toEqual(['Family B event']);
    expect(rowNames(card(harness, 'trips'))).toEqual(['Family B trip']);
    expect(root(harness).querySelector('.page-title p')?.textContent).toContain('Family B');
  });
});
