import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from './app';
import { routes } from './app.routes';

const ownProfile = {
  profileId: 'person-a', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const family = { familyGroupId: 'family-a', name: 'Family A' };

function dateAfter(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}

function planPreview(planId: string, start: number, expectedReturn: number | null, familyGroupId = family.familyGroupId) {
  return { planId, familyGroupId, activityOrDestination: planId, date: dateAfter(start), time: '00:00',
    expectedReturnDate: expectedReturn === null ? null : dateAfter(expectedReturn),
    expectedReturnTime: expectedReturn === null ? null : '00:00', expectedCheckInDate: null,
    expectedCheckInTime: null, audienceType: 'FamilyGroup', creatorProfileId: ownProfile.profileId,
    creatorDisplayName: 'Alex', notes: null };
}

describe('Home and feature empty states', () => {
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

  async function visit(path = '/home') {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(path, App);
    return harness;
  }

  function initial(groups: typeof family[] = [], admins: typeof family[] = []) {
    http.expectOne('/api/profile').flush(ownProfile);
    http.expectOne('/api/family-groups/administered').flush(admins);
    http.expectOne('/api/family-groups/mine').flush(groups);
  }

  function familyReads(group = family, members = [{ profileId: ownProfile.profileId, displayName: 'Alex' }], relationships: object[] = []) {
    http.expectOne(`/api/family-groups/${group.familyGroupId}/members`).flush(members);
    http.expectOne(`/api/family-groups/${group.familyGroupId}/relationships`).flush(relationships);
    http.expectOne(`/api/family-groups/${group.familyGroupId}/events`).flush([]);
    http.expectOne(`/api/family-groups/${group.familyGroupId}/plans`).flush([]);
  }

  // WI-009-010 O4 (DEC-080) supersedes the earlier invitation/operator wording.
  it('shows invitation guidance with Me access and no self-service Family actions', async () => {
    const harness = await visit('/home');
    initial([]);
    harness.detectChanges();
    const text = harness.routeNativeElement?.textContent ?? '';
    expect(text).toContain('You’re not part of a Family yet. Ask a Family administrator to send you an invitation.');
    expect(harness.routeNativeElement?.querySelector('.home-no-family a')?.getAttribute('href')).toBe('/me');
    expect(text).not.toContain('Create Family');
    expect(text).not.toContain('Find Family');
    expect(harness.routeNativeElement?.querySelector('button')?.textContent).not.toContain('join');
    expect(harness.routeNativeElement?.querySelector('nav a')?.textContent).toContain('Home');
    expect(Array.from(harness.routeNativeElement!.querySelectorAll('nav a'))
      .some(link => link.textContent?.includes('Me / Settings'))).toBe(true);
    http.expectNone('/api/trips');
  });

  it('keeps every dashboard card on a low-data Home with compact empty states that link to existing areas', async () => {
    const harness = await visit('/home');
    initial([family]);
    familyReads();
    http.expectOne('/api/trips').flush([]);
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    const card = (heading: string) => root.querySelector(`section[aria-labelledby="${heading}"]`) as HTMLElement;
    // Greeting from local time and the already-loaded own profile name; the Family stays in the subtitle.
    expect(root.querySelector('h1')?.textContent?.trim()).toMatch(/^Good (morning|afternoon|evening), Alex!$/);
    expect(root.querySelector('.home-title')?.textContent).toContain("Here's what's happening with Family A today.");
    expect(root.querySelector('.home-title')?.textContent).toContain('View Family');
    expect(card('home-attention-heading').textContent).toContain('Nothing needs your attention right now.');
    for (const [heading, title, area] of [['home-events-heading', 'Upcoming Events', 'Events'],
      ['home-trips-heading', 'Trips', 'Trips'], ['home-plans-heading', 'Plans you can see', 'Plans']]) {
      expect(card(heading).querySelector('h2')?.textContent?.trim()).toBe(title);
      expect(Array.from(card(heading).querySelectorAll('button')).map(button => button.textContent?.trim())).toEqual([`Open ${area}`]);
      expect(card(heading).querySelector('li')).toBeNull();
    }
    expect(card('home-concerns-heading').textContent).toContain('Safety coordination');
    // The old combined low-data strip is gone: each card states its own empty state.
    expect(root.textContent).not.toContain('Nothing is currently visible to you');
    expect(root.textContent).not.toContain('needs attention');
  });

  it('reserves Family Updates, Tasks, and Active Sharing as honest placeholder cards with no data, actions, or requests', async () => {
    const harness = await visit('/home');
    initial([family]);
    familyReads();
    http.expectOne('/api/trips').flush([]);
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    const placeholders = Array.from(root.querySelectorAll<HTMLElement>('.home-grid .home-future'));
    expect(placeholders.map(section => section.querySelector('h2')?.textContent?.trim()))
      .toEqual(['Recent Family Updates', 'My Tasks', 'Active Sharing']);
    for (const section of placeholders) {
      expect(section.querySelector('.home-future-note')?.textContent).toMatch(/will appear here (as|when)/);
      expect(section.querySelectorAll('button, a, input, img, li, .avatar')).toHaveLength(0);
      expect(section.textContent).not.toMatch(/\d|Coming later|Phase|roadmap/i);
    }
    http.expectNone(request => !/^\/api\/(profile|family-groups|trips$)/.test(request.url));
  });

  it('orders upcoming Events, accessible Plans, and current-Family Trips and caps each preview at three', async () => {
    const harness = await visit('/home');
    initial([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([
      { profileId: ownProfile.profileId, displayName: 'Alex' },
      { profileId: 'adult-b', displayName: 'Sam' },
    ]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([{
      relationshipId: 'suggestion-1', proposerProfileId: 'adult-b', proposerDisplayName: 'Sam',
      proposerLabel: 'Parent', recipientProfileId: ownProfile.profileId, recipientDisplayName: 'Alex',
      recipientLabel: 'AdultChild', status: 'Pending', isCurrentUserProposer: false,
      isCurrentUserRecipient: true,
    }]);
    const today = dateAfter(0);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/events`).flush([
      { eventId: 'e-old', familyGroupId: family.familyGroupId, name: 'Past Event', date: dateAfter(-1), time: '10:00', location: '', description: '', organizerProfileId: 'person-a', organizerDisplayName: 'Alex', canEdit: true },
      { eventId: 'e-3', familyGroupId: family.familyGroupId, name: 'Third Event', date: dateAfter(3), time: '10:00', location: '', description: '', organizerProfileId: 'person-a', organizerDisplayName: 'Alex', canEdit: true },
      { eventId: 'e-2', familyGroupId: family.familyGroupId, name: 'Second Event', date: dateAfter(1), time: '12:00', location: '', description: '', organizerProfileId: 'person-a', organizerDisplayName: 'Alex', canEdit: true },
      { eventId: 'e-1b', familyGroupId: family.familyGroupId, name: 'Later Today', date: today, time: '18:00', location: '', description: '', organizerProfileId: 'person-a', organizerDisplayName: 'Alex', canEdit: true },
      { eventId: 'e-1a', familyGroupId: family.familyGroupId, name: 'Soon Today', date: today, time: '09:00', location: '', description: '', organizerProfileId: 'person-a', organizerDisplayName: 'Alex', canEdit: true },
    ]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`).flush([
      { planId: 'p-3', familyGroupId: family.familyGroupId, activityOrDestination: 'Plan Three', date: dateAfter(3), time: '09:00', expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, audienceType: 'FamilyGroup', creatorProfileId: 'person-a', creatorDisplayName: 'Alex', notes: null },
      { planId: 'p-1', familyGroupId: family.familyGroupId, activityOrDestination: 'Plan One', date: dateAfter(1), time: '11:00', expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, audienceType: 'FamilyGroup', creatorProfileId: 'person-a', creatorDisplayName: 'Alex', notes: null },
      { planId: 'p-4', familyGroupId: family.familyGroupId, activityOrDestination: 'Plan Four', date: dateAfter(4), time: '09:00', expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, audienceType: 'FamilyGroup', creatorProfileId: 'person-a', creatorDisplayName: 'Alex', notes: null },
      { planId: 'p-2', familyGroupId: family.familyGroupId, activityOrDestination: 'Plan Two', date: dateAfter(2), time: '09:00', expectedReturnDate: null, expectedReturnTime: null, expectedCheckInDate: null, expectedCheckInTime: null, audienceType: 'FamilyGroup', creatorProfileId: 'person-a', creatorDisplayName: 'Alex', notes: null },
    ]);
    http.expectOne('/api/trips').flush([
      { tripId: 't-out', familyGroupId: 'former-family', title: 'Creator Trip Outside Family', primaryDestination: 'Elsewhere', startDate: dateAfter(0), endDate: dateAfter(1), creatorProfileId: ownProfile.profileId },
      { tripId: 't-3', familyGroupId: family.familyGroupId, title: 'Trip Three', primaryDestination: 'Place 3', startDate: dateAfter(3), endDate: dateAfter(4) },
      { tripId: 't-2', familyGroupId: family.familyGroupId, title: 'Trip Two', primaryDestination: 'Place 2', startDate: dateAfter(2), endDate: dateAfter(3) },
      { tripId: 't-4', familyGroupId: family.familyGroupId, title: 'Trip Four', primaryDestination: 'Place 4', startDate: dateAfter(4), endDate: dateAfter(5) },
      { tripId: 't-1', familyGroupId: family.familyGroupId, title: 'Trip One', primaryDestination: 'Place 1', startDate: dateAfter(1), endDate: dateAfter(2) },
    ]);
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    const names = (selector: string) => Array.from(root.querySelectorAll(`${selector} li strong`)).map(node => node.textContent?.trim());
    expect(names('[aria-labelledby="home-events-heading"]')).toEqual(['Soon Today', 'Later Today', 'Second Event']);
    expect(names('[aria-labelledby="home-plans-heading"]')).toEqual(['Plan One', 'Plan Two', 'Plan Three']);
    expect(names('[aria-labelledby="home-trips-heading"]')).toEqual(['Trip One', 'Trip Two', 'Trip Three']);
    expect(root.querySelectorAll('.home-preview-list li')).toHaveLength(9);
    // WI-009-005 (AC-16): each existing row opens its own item URL and is named by the item; nothing else on Home changes.
    const links = (selector: string) => Array.from(root.querySelectorAll<HTMLAnchorElement>(`${selector} li a`))
      .map(link => [link.textContent?.trim(), link.getAttribute('href')]);
    expect(links('[aria-labelledby="home-events-heading"]'))
      .toEqual([['Soon Today', '/events/e-1a'], ['Later Today', '/events/e-1b'], ['Second Event', '/events/e-2']]);
    expect(links('[aria-labelledby="home-plans-heading"]'))
      .toEqual([['Plan One', '/plans/p-1'], ['Plan Two', '/plans/p-2'], ['Plan Three', '/plans/p-3']]);
    expect(links('[aria-labelledby="home-trips-heading"]'))
      .toEqual([['Trip One', '/trips/t-1'], ['Trip Two', '/trips/t-2'], ['Trip Three', '/trips/t-3']]);
    expect(Array.from(root.querySelectorAll('.home-dashboard button')).map(button => button.textContent?.trim()))
      .toEqual(['View Family', 'Review in Family', 'View all', 'View all', 'View all', 'Open Concerns', 'Open Incidents']);
    expect(root.querySelectorAll('.home-dashboard a')).toHaveLength(9);
    expect(root.textContent).toContain('Family relationship suggestion to respond to');
    expect(root.textContent).not.toContain('Creator Trip Outside Family');
    expect(root.textContent).not.toContain('nothing else');
  });

  it('keeps creator Trips outside the selected Family available on the Trips page', async () => {
    const harness = await visit('/trips');
    initial([family]);
    familyReads();
    harness.detectChanges();
    http.expectOne('/api/trips').flush([
      { tripId: 't-out', familyGroupId: 'former-family', title: 'Retained Creator Trip', primaryDestination: 'Elsewhere', startDate: dateAfter(1), endDate: dateAfter(2), creatorProfileId: ownProfile.profileId },
    ]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('Retained Creator Trip');
    expect(harness.routeNativeElement?.textContent).toContain('retain access to these Trips as their creator');
  });

  it('keeps Home load errors separate from the successful low-data state', async () => {
    const harness = await visit('/home');
    initial([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([{ profileId: 'person-a', displayName: 'Alex' }]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/events`).flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`).flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne('/api/trips').flush({}, { status: 503, statusText: 'Unavailable' });
    harness.detectChanges();
    const text = harness.routeNativeElement?.textContent ?? '';
    expect(text).toContain('Events could not be loaded');
    expect(text).toContain('Plans could not be loaded');
    expect(text).toContain('Trips could not be loaded');
    expect(text).not.toContain('Nothing is currently visible to you in Events, Plans, or Trips');
  });

  it('shows privacy-safe Event empty copy and explains when its existing audience requirement is unmet', async () => {
    // WI-009-008: the full list (and its empty state) is on /events/all; the create form is on /events/new.
    const listHarness = await visit('/events/all');
    initial([family]);
    familyReads(family, [{ profileId: ownProfile.profileId, displayName: 'Alex' }]);
    await listHarness.fixture.whenStable();
    listHarness.detectChanges();
    expect(listHarness.routeNativeElement?.textContent).toContain('No Events are currently shared with you in this Family');
    expect(listHarness.routeNativeElement?.textContent).toContain('family gatherings and activities');
    expect(listHarness.routeNativeElement?.textContent).not.toContain('Create or join a family group');

    const harness = listHarness;
    await harness.navigateByUrl('/events/new', App);
    initial([family]);
    familyReads(family, [{ profileId: ownProfile.profileId, displayName: 'Alex' }]);
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('choose at least one other active adult');
    expect(harness.routeNativeElement?.textContent).not.toContain('Create or join a family group');
    const create = Array.from(harness.routeNativeElement!.querySelectorAll('button'))
      .find(button => button.textContent?.toLowerCase().includes('create event')) as HTMLButtonElement;
    expect(create.disabled).toBe(true);
  });

  it('shows Plan empty copy and preserves its existing creation form', async () => {
    const harness = await visit('/plans');
    initial([family]);
    familyReads();
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('No Plans are currently visible to you in this Family');
    expect(harness.routeNativeElement?.textContent).toContain('lightweight way to share a future activity or destination');
    expect(harness.routeNativeElement?.querySelector('#plan-notes')).not.toBeNull();
    expect(harness.routeNativeElement?.querySelector('#plan-audience-group')).not.toBeNull();
  });

  it('shows Trip empty copy and leaves creation available for the selected Family', async () => {
    const harness = await visit('/trips');
    initial([family]);
    familyReads();
    harness.detectChanges();
    http.expectOne('/api/trips').flush([]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('No Trips are currently available to you');
    expect(harness.routeNativeElement?.textContent).toContain('Trip Notes for authorized viewers');
    expect(Array.from(harness.routeNativeElement!.querySelectorAll('button'))
      .some(button => button.textContent?.trim() === 'Create Trip')).toBe(true);
  });

  it('does not present the Event empty state when its authorized list fails to load', async () => {
    const eventsHarness = await visit('/events');
    initial([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([
      { profileId: ownProfile.profileId, displayName: 'Alex' },
      { profileId: 'adult-b', displayName: 'Sam' },
    ]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/events`).flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`).flush([]);
    eventsHarness.detectChanges();
    expect(eventsHarness.routeNativeElement?.textContent).toContain('Events could not be loaded');
    expect(eventsHarness.routeNativeElement?.textContent).not.toContain('No Events are currently shared with you');

  });

  it('does not present the Plan empty state when its authorized list fails to load', async () => {
    const harness = await visit('/plans');
    initial([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([
      { profileId: ownProfile.profileId, displayName: 'Alex' },
      { profileId: 'adult-b', displayName: 'Sam' },
    ]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/events`).flush([]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`).flush({}, { status: 503, statusText: 'Unavailable' });
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('Plans could not be loaded');
    expect(harness.routeNativeElement?.textContent).not.toContain('No Plans are currently visible to you');
  });

  it('shows a retryable Trip load error without presenting the successful empty state', async () => {
    const harness = await visit('/trips');
    initial([family]);
    familyReads();
    harness.detectChanges();
    http.expectOne('/api/trips').flush({}, { status: 503, statusText: 'Unavailable' });
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    expect(root.textContent).toContain('Trips could not be loaded');
    expect(root.textContent).not.toContain('No Trips are currently available to you');
    expect(Array.from(root.querySelectorAll('button')).some(button => button.textContent?.trim() === 'Retry loading Trips')).toBe(true);
  });

  it('distinguishes directory filter no-match from an unfiltered empty directory and clears the filter', async () => {
    const harness = await visit('/family');
    initial([family]);
    familyReads(family, [{ profileId: ownProfile.profileId, displayName: 'Alex' }]);
    await harness.fixture.whenStable();
    harness.detectChanges();
    const search = harness.routeNativeElement!.querySelector<HTMLInputElement>('#directory-search')!;
    search.value = 'nobody';
    search.dispatchEvent(new Event('input'));
    const filter = Array.from(harness.routeNativeElement!.querySelectorAll('button'))
      .find(button => button.textContent?.trim() === 'Filter members')!;
    filter.click();
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members?search=nobody`).flush([]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('No active adult family members match this filter');
    const clear = Array.from(harness.routeNativeElement!.querySelectorAll('button'))
      .find(button => button.textContent?.trim() === 'Clear filter')!;
    clear.click();
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([{ profileId: ownProfile.profileId, displayName: 'Alex' }]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('currently shows one active adult member');
    expect(harness.routeNativeElement?.textContent).toContain('Family administrator may invite another adult');
  });
  for (const [name, start, expectedReturn, included] of [
    ['future-start', 2, null, true],
    ['starts-today', 0, null, true],
    ['overnight-return-tomorrow', -1, 1, true],
    ['overnight-return-today', -1, 0, true],
    ['returned-yesterday', -2, -1, false],
    ['past-start-no-return', -1, null, false],
  ] as const) {
    it(`${included ? 'includes' : 'excludes'} ${name} on Home using calendar dates`, async () => {
      const harness = await visit('/home');
      initial([family]);
      http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`)
        .flush([planPreview(name, start, expectedReturn)]);
      harness.detectChanges();
      const card = harness.routeNativeElement!.querySelector('[aria-labelledby="home-plans-heading"]')!;
      const links = Array.from(card.querySelectorAll<HTMLAnchorElement>('li a'))
        .map(link => [link.textContent?.trim(), link.getAttribute('href')]);
      expect(links).toEqual(included ? [[name, `/plans/${name}`]] : []);
    });
  }

  it('filters past and other-Family Plans before the Home cap while preserving start-date/time sorting and item links', async () => {
    const harness = await visit('/home');
    initial([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`).flush([
      planPreview('past-1', -10, -4), planPreview('past-2', -8, null), planPreview('past-3', -6, -2),
      planPreview('outside', -5, 2, 'other-family'), planPreview('return-tomorrow', -1, 1),
      { ...planPreview('return-today', -1, 0), time: '01:00' },
      planPreview('future', 2, null), planPreview('today', 0, null),
    ]);
    harness.detectChanges();
    const card = harness.routeNativeElement!.querySelector('[aria-labelledby="home-plans-heading"]')!;
    expect(Array.from(card.querySelectorAll<HTMLAnchorElement>('li a'))
      .map(link => [link.textContent?.trim(), link.getAttribute('href')]))
      .toEqual([['return-tomorrow', '/plans/return-tomorrow'], ['return-today', '/plans/return-today'], ['today', '/plans/today']]);
    expect(card.textContent).not.toMatch(/past-|outside|future/);
  });

  it('shows the real Home Plan empty state with list access when only past Plans remain', async () => {
    const harness = await visit('/home');
    initial([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`).flush([
      planPreview('returned', -2, -1), planPreview('no-return', -1, null),
    ]);
    harness.detectChanges();
    const card = harness.routeNativeElement!.querySelector('[aria-labelledby="home-plans-heading"]')!;
    expect(card.querySelector('li')).toBeNull();
    expect(card.textContent).toContain('Open Plans');
    expect(card.textContent).toContain('No Plans are visible to you yet.');
    expect(card.textContent).not.toMatch(/returned|no-return/);
    (Array.from(card.querySelectorAll('button')).find(button => button.textContent?.trim() === 'Open Plans')!).click();
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/plans');
  });

  it('keeps ongoing and upcoming Trips without letting ended or other-Family Trips consume the three Home slots', async () => {
    const harness = await visit('/home');
    initial([family]);
    familyReads();
    const trip = (tripId: string, start: number, end: number, familyGroupId = family.familyGroupId) => ({
      tripId, familyGroupId, title: tripId, primaryDestination: 'Coast', startDate: dateAfter(start), endDate: dateAfter(end),
    });
    http.expectOne('/api/trips').flush([
      trip('ended-1', -10, -4), trip('ended-2', -8, -1), trip('ended-3', -6, -2),
      trip('outside', -5, 2, 'other-family'), trip('ongoing', -3, 0),
      trip('next', 1, 2), trip('later', 3, 4), trip('fourth', 5, 6),
    ]);
    harness.detectChanges();
    const card = harness.routeNativeElement!.querySelector('[aria-labelledby="home-trips-heading"]')!;
    expect(Array.from(card.querySelectorAll('li a')).map(link => [link.textContent?.trim(), link.getAttribute('href')]))
      .toEqual([['ongoing', '/trips/ongoing'], ['next', '/trips/next'], ['later', '/trips/later']]);
    expect(card.textContent).not.toMatch(/ended|outside|fourth/);
  });

  it('shows the real Trip empty state when only ended Trips remain', async () => {
    const harness = await visit('/home');
    initial([family]);
    familyReads();
    http.expectOne('/api/trips').flush([{
      tripId: 'ended', familyGroupId: family.familyGroupId, title: 'Ended Trip', primaryDestination: 'Coast',
      startDate: dateAfter(-5), endDate: dateAfter(-1),
    }]);
    harness.detectChanges();
    const card = harness.routeNativeElement!.querySelector('[aria-labelledby="home-trips-heading"]')!;
    expect(card.querySelectorAll('li')).toHaveLength(0);
    expect(card.textContent).toContain('Open Trips');
    expect(card.textContent).not.toContain('Ended Trip');
  });

  it('offers static Concern and Incident entry points on the Safety coordination card without loading Incident data', async () => {
    const harness = await visit('/home');
    initial([family]);
    familyReads();
    http.expectOne('/api/trips').flush([]);
    harness.detectChanges();
    const card = harness.routeNativeElement!.querySelector('[aria-labelledby="home-concerns-heading"]') as HTMLElement;
    expect(card.querySelector('h2')?.textContent?.trim()).toBe('Safety coordination');
    const buttons = Array.from(card.querySelectorAll('button')).map(button => button.textContent?.trim());
    expect(buttons).toEqual(['Open Concerns', 'Open Incidents']);
    expect(card.textContent).not.toMatch(/\d/);
    http.expectNone(request => request.url.startsWith('/api/incidents'));
  });

  it('serves the Incident routes in the existing shell under the Safety area without a shell reload', async () => {
    const harness = await visit('/incidents');
    initial([family]);
    familyReads();
    await harness.fixture.whenStable();
    harness.detectChanges();
    http.expectOne('/api/incidents').flush([]);
    await harness.fixture.whenStable();
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    expect(Array.from(root.querySelectorAll('h1')).map(heading => heading.textContent?.trim())).toEqual(['Safety Incidents']);
    const navLabels = Array.from(root.querySelectorAll('nav a')).map(link => link.textContent?.trim());
    expect(navLabels).toEqual(['Home', 'Family', 'Events', 'Plans & Trips', 'Safety', 'Me / Settings']);
    expect(root.querySelector('nav button')).toBeNull();
    expect(Array.from(root.querySelectorAll('nav [aria-current]'))
      .map(link => [link.textContent?.trim(), link.getAttribute('aria-current')])).toEqual([['Safety', 'true']]);
    expect(root.textContent).toContain('No Incidents are currently shared with you.');

    const shell = harness.routeDebugElement!.componentInstance;
    await harness.navigateByUrl('/incidents/new');
    await harness.fixture.whenStable();
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([{ profileId: ownProfile.profileId, displayName: 'Alex' }]);
    harness.detectChanges();
    expect(harness.routeDebugElement!.componentInstance).toBe(shell);
    http.expectNone('/api/profile');
    expect(harness.routeNativeElement?.textContent).toContain('Start an Incident');
    expect(harness.routeNativeElement?.textContent).not.toContain('Starting a separate Incident');
  });
});
