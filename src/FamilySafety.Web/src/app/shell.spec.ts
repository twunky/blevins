import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NavigationEnd, provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from './app';
import { routes } from './app.routes';
import { CurrentFamilyContext } from './family-context';
import { filter, firstValueFrom } from 'rxjs';

const profile = {
  profileId: 'person', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const a = { familyGroupId: 'family-a', name: 'Family A' };
const b = { familyGroupId: 'family-b', name: 'Family B' };

describe('Returning-user routed shell', () => {
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

  function initial(groups: typeof a[] = [], administered: typeof a[] = []) {
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush(administered);
    http.expectOne('/api/family-groups/mine').flush(groups);
  }

  function familyReads(groupId: string, memberName = 'Alex') {
    http.expectOne(`/api/family-groups/${groupId}/members`).flush([
      { profileId: 'person', displayName: memberName },
    ]);
    http.expectOne(`/api/family-groups/${groupId}/relationships`).flush([]);
    http.expectOne(`/api/family-groups/${groupId}/events`).flush([]);
    http.expectOne(`/api/family-groups/${groupId}/plans`).flush([]);
  }

  it('lands a returning member on Home with the sole Family, then routes to every existing area and back', async () => {
    const harness = await visit('/home');
    initial([a]);
    familyReads(a.familyGroupId);
    http.expectOne('/api/trips').flush([]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('happening with Family A today');
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBe(a.familyGroupId);
    expect(harness.routeNativeElement?.querySelector('[aria-current="page"]')?.textContent?.trim()).toBe('Home');
    const destinations = [
      ['/family', 'Family', 'Family', 'page'], ['/events', 'Events', 'Events', 'page'],
      ['/plans-trips', 'Plans & Trips', 'Plans & Trips', 'page'], ['/plans', 'Plans', 'Plans & Trips', 'true'],
      ['/trips', 'Trips', 'Plans & Trips', 'true'], ['/safety', 'Safety', 'Safety', 'page'],
      ['/me', 'Your profile', 'Me / Settings', 'page'],
    ] as const;
    for (const [url, heading, navLabel, current] of destinations) {
      await harness.navigateByUrl(url, App);
      initial([a]);
      familyReads(a.familyGroupId);
      harness.detectChanges();
      // The Events dashboard (WI-009-008 D3) reads Trips for its summary card, as Home does.
      if (url === '/trips' || url === '/events') http.expectOne('/api/trips').flush([]);
      expect(TestBed.inject(Router).url).toBe(url);
      expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain(heading);
      expect(harness.routeNativeElement?.querySelector(`nav [aria-current="${current}"]`)?.textContent?.trim())
        .toBe(navLabel);
      await harness.navigateByUrl('/home', App);
      initial([a]);
      familyReads(a.familyGroupId);
      http.expectOne('/api/trips').flush([]);
      harness.detectChanges();
      expect(harness.routeNativeElement?.textContent).toContain('happening with Family A today');
    }
  }, 15_000); // Fourteen full shell navigations; the default 5 s is too tight on a loaded machine.

  it('redirects the ordinary root entry to Home', async () => {
    const harness = await visit('/');
    initial([]);
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(TestBed.inject(Router).url).toBe('/home');
    expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain('Home');
  });

  it('distinguishes zero memberships from a failed membership load and retries', async () => {
    const harness = await visit('/home');
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush({}, { status: 503, statusText: 'Unavailable' });
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('could not be loaded');
    expect(harness.routeNativeElement?.textContent).not.toContain('not part of a Family yet');
    const retry = [...harness.routeNativeElement!.querySelectorAll('button')]
      .find(button => button.textContent?.includes('Retry loading Families')) as HTMLButtonElement;
    retry.click();
    http.expectOne('/api/family-groups/mine').flush([]);
    harness.detectChanges();
    // WI-009-010 O4: the approved no-Family Home wording.
    expect(harness.routeNativeElement?.textContent).toContain('not part of a Family yet');
    expect(harness.routeNativeElement?.textContent).not.toContain('Create Family');
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBeNull();
  });

  it('requires an explicit choice for multiple Families and clears the previous Family before loading another', async () => {
    const harness = await visit('/home');
    initial([a, b], [a]);
    harness.detectChanges();
    const selector = harness.routeNativeElement!.querySelector<HTMLSelectElement>('#current-family')!;
    expect(selector.value).toBe('');
    http.expectNone(`/api/family-groups/${a.familyGroupId}/members`);
    selector.value = a.familyGroupId;
    selector.dispatchEvent(new Event('change'));
    http.expectOne(`/api/family-groups/${a.familyGroupId}/members`).flush([
      { profileId: 'a-member', displayName: 'A Member' },
    ]);
    http.expectOne(`/api/family-groups/${a.familyGroupId}/relationships`).flush([]);
    const oldEvents = http.expectOne(`/api/family-groups/${a.familyGroupId}/events`);
    http.expectOne(`/api/family-groups/${a.familyGroupId}/plans`).flush([]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('happening with Family A today');
    const state = harness.routeDebugElement!.componentInstance as {
      directoryMembers: () => readonly { displayName: string }[];
      selectDirectoryMember: (member: { profileId: string; displayName: string }) => void;
      selectedMember: () => object | null;
      familyEvents: () => readonly object[];
      eventForm: { patchValue: (value: object) => void; controls: { name: { value: string | null } } };
      planForm: { patchValue: (value: object) => void; controls: { notes: { value: string | null } } };
      ownerProfile: () => { displayName: string } | null;
    };
    expect(state.directoryMembers()[0].displayName).toBe('A Member');
    state.selectDirectoryMember({ profileId: 'a-member', displayName: 'A Member' });
    const oldMemberProfile = http.expectOne('/api/family-members/a-member/profile');
    state.eventForm.patchValue({ name: 'Private A draft' });
    state.planForm.patchValue({ notes: 'Private A notes' });
    selector.value = b.familyGroupId;
    selector.dispatchEvent(new Event('change'));
    harness.detectChanges();
    expect(state.directoryMembers()).toEqual([]);
    expect(state.selectedMember()).toBeNull();
    expect(oldMemberProfile.cancelled).toBe(true);
    expect(state.eventForm.controls.name.value).toBe('');
    expect(state.planForm.controls.notes.value).toBe('');
    expect(state.ownerProfile()?.displayName).toBe('Alex');
    expect(harness.routeNativeElement?.textContent).not.toContain('Administering Family A');
    familyReads(b.familyGroupId, 'B Member');
    oldEvents.flush([{ eventId: 'old-event', name: 'Private A event' }]);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('happening with Family B today');
    expect(state.familyEvents()).toEqual([]);
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBe(b.familyGroupId);
  });

  it('shows existing invitation administration only for the selected administered Family', async () => {
    TestBed.inject(CurrentFamilyContext).familyId.set(a.familyGroupId);
    const harness = await visit('/family');
    initial([a, b], [a]);
    familyReads(a.familyGroupId);
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('Administering Family A');
    expect(harness.routeNativeElement?.querySelector('#invitation-email')).not.toBeNull();
    const selector = harness.routeNativeElement!.querySelector<HTMLSelectElement>('#current-family')!;
    selector.value = b.familyGroupId;
    selector.dispatchEvent(new Event('change'));
    familyReads(b.familyGroupId);
    harness.detectChanges();
    expect(harness.routeNativeElement?.querySelector('#invitation-email')).toBeNull();
  });

  it('keeps a valid invitation fragment ahead of the root Home redirect', async () => {
    history.replaceState(null, '', '/#invite=opaque-token');
    const harness = await visit('/#invite=opaque-token');
    initial([]);
    http.expectOne('/api/membership-invitations/preview').flush({
      familyGroupName: a.name, inviterDisplayName: 'Inviter',
    });
    harness.detectChanges();
    expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain('Family invitation');
    expect(harness.routeNativeElement?.textContent).toContain('Inviter invited you');
    expect(TestBed.inject(Router).url).toContain('invite=opaque-token');
  });

  /** Answers every outstanding request so a page can render; item reads answer "unavailable". */
  function flushRemaining() {
    http.match(() => true).forEach(request => {
      if (request.cancelled) return;
      if (/^\/api\/(incidents|concerns)\/[^/]+/.test(request.request.url)) request.flush({}, { status: 404, statusText: 'Not Found' });
      else request.flush([]);
    });
  }

  function navLinks(harness: RouterTestingHarness) {
    return Array.from(harness.routeNativeElement!.querySelectorAll<HTMLAnchorElement>('nav[aria-label="Main navigation"] a'));
  }

  it('renders exactly six main-navigation links in order, with real URLs and no buttons (AC-01)', async () => {
    const harness = await visit('/home');
    initial([a]);
    familyReads(a.familyGroupId);
    flushRemaining();
    harness.detectChanges();
    const links = navLinks(harness);
    expect(links.map(link => link.textContent?.trim()))
      .toEqual(['Home', 'Family', 'Events', 'Plans & Trips', 'Safety', 'Me / Settings']);
    expect(links.map(link => link.getAttribute('href')))
      .toEqual(['/home', '/family', '/events', '/plans-trips', '/safety', '/me']);
    expect(harness.routeNativeElement!.querySelectorAll('nav button')).toHaveLength(0);
    expect(links.every(link => link.querySelector('svg')?.getAttribute('aria-hidden') === 'true')).toBe(true);
  });

  it.each([
    ['/home', 'Home', 'page'], ['/family', 'Family', 'page'], ['/events', 'Events', 'page'],
    ['/events/new', 'Events', 'true'], ['/events/all', 'Events', 'true'],
    ['/plans-trips', 'Plans & Trips', 'page'], ['/plans', 'Plans & Trips', 'true'], ['/trips', 'Plans & Trips', 'true'],
    ['/safety', 'Safety', 'page'], ['/concerns', 'Safety', 'true'], ['/incidents', 'Safety', 'true'],
    ['/incidents/new', 'Safety', 'true'], ['/incidents/some-incident', 'Safety', 'true'], ['/me', 'Me / Settings', 'page'],
  ] as const)('selects exactly one area on %s: %s with aria-current="%s" (AC-08)', async (url, label, current) => {
    const harness = await visit(url);
    initial([a]);
    familyReads(a.familyGroupId);
    await harness.fixture.whenStable();
    flushRemaining();
    harness.detectChanges();
    await harness.fixture.whenStable();
    flushRemaining();
    harness.detectChanges();
    expect(TestBed.inject(Router).url).toBe(url);
    const selected = navLinks(harness).filter(link => link.hasAttribute('aria-current'));
    expect(selected.map(link => [link.textContent?.trim(), link.getAttribute('aria-current')])).toEqual([[label, current]]);
    expect(navLinks(harness).filter(link => link.classList.contains('is-active')).map(link => link.textContent?.trim()))
      .toEqual([label]);
  });

  it('moves focus to the page h1 only after following a navigation link, not on load or other navigation (D4)', async () => {
    const router = TestBed.inject(Router);
    const harness = await visit('/home');
    initial([a]);
    familyReads(a.familyGroupId);
    flushRemaining();
    harness.detectChanges();
    await harness.fixture.whenStable();
    expect(document.activeElement?.tagName).not.toBe('H1');

    const navigated = firstValueFrom(router.events.pipe(filter(event => event instanceof NavigationEnd)));
    navLinks(harness).find(link => link.textContent?.trim() === 'Family')!.click();
    await navigated;
    harness.detectChanges();
    initial([a]);
    familyReads(a.familyGroupId);
    harness.detectChanges();
    await harness.fixture.whenStable();
    expect(router.url).toBe('/family');
    expect(document.activeElement?.tagName).toBe('H1');
    expect(document.activeElement?.textContent?.trim()).toBe('Family');
    expect(document.activeElement?.getAttribute('tabindex')).toBe('-1');

    // A navigation without the link's state (for example an in-page button) does not move focus.
    await harness.navigateByUrl('/events', App);
    initial([a]);
    familyReads(a.familyGroupId);
    harness.detectChanges();
    await harness.fixture.whenStable();
    expect(document.activeElement?.tagName).not.toBe('H1');

    // Data loads and other internal state changes never move focus.
    (document.activeElement as HTMLElement | null)?.blur();
    harness.detectChanges();
    expect(document.activeElement?.tagName).not.toBe('H1');
  });

  // DEC-079 (WI-009-009): /safety is now the Safety dashboard, so the Safety half follows its View all link, not a landing link.
  it('follows the Plans & Trips landing link and the Safety dashboard View all link, and focuses their h1 (AC-05, AC-06, D4)', async () => {
    const router = TestBed.inject(Router);
    const harness = await visit('/plans-trips');
    initial([a]);
    familyReads(a.familyGroupId);
    harness.detectChanges();
    const follow = async (text: string) => {
      const navigated = firstValueFrom(router.events.pipe(filter(event => event instanceof NavigationEnd)));
      Array.from(harness.routeNativeElement!.querySelectorAll<HTMLAnchorElement>('main a'))
        .find(link => link.textContent?.trim() === text || link.getAttribute('aria-label') === text)!.click();
      await navigated;
      harness.detectChanges();
      initial([a]);
      familyReads(a.familyGroupId);
      harness.detectChanges();
      await harness.fixture.whenStable();
      flushRemaining();
      harness.detectChanges();
      await harness.fixture.whenStable();
    };
    await follow('Open Plans');
    expect(router.url).toBe('/plans');
    expect(document.activeElement?.textContent).toContain('Plans');
    expect(document.activeElement?.tagName).toBe('H1');

    await harness.navigateByUrl('/safety', App);
    initial([a]);
    familyReads(a.familyGroupId);
    harness.detectChanges();
    await harness.fixture.whenStable();
    flushRemaining();
    harness.detectChanges();
    await follow('View all Safety Incidents');
    expect(router.url).toBe('/incidents');
    expect(document.activeElement?.tagName).toBe('H1');
    expect(document.activeElement?.textContent).toContain('Safety Incidents');
  });

  it.each([['/concerns', 'Concerns'], ['/incidents', 'Safety Incidents']])(
    'offers Back to Safety on %s, which returns to /safety and focuses its h1 (WI-009-009 O9)', async (url, heading) => {
      const router = TestBed.inject(Router);
      const harness = await visit(url);
      initial([a]);
      familyReads(a.familyGroupId);
      await harness.fixture.whenStable();
      flushRemaining();
      harness.detectChanges();
      await harness.fixture.whenStable();
      const root = harness.routeNativeElement!;
      expect(root.querySelector('h1')?.textContent).toContain(heading);
      const back = root.querySelector<HTMLAnchorElement>('a.safety-back-link')!;
      expect(back.textContent?.trim()).toBe('Back to Safety');
      expect(back.getAttribute('href')).toBe('/safety');
      const navigated = firstValueFrom(router.events.pipe(filter(event => event instanceof NavigationEnd)));
      back.click();
      await navigated;
      harness.detectChanges();
      initial([a]);
      familyReads(a.familyGroupId);
      harness.detectChanges();
      await harness.fixture.whenStable();
      flushRemaining();
      harness.detectChanges();
      await harness.fixture.whenStable();
      expect(router.url).toBe('/safety');
      expect(document.activeElement?.tagName).toBe('H1');
      expect(document.activeElement?.textContent?.trim()).toBe('Safety');
    });

  it.each(['/incidents/new', '/incidents/some-incident', '/concerns/some-concern'])(
    'keeps item and create pages on their own Back links: no Back to Safety on %s', async url => {
      const harness = await visit(url);
      initial([a]);
      familyReads(a.familyGroupId);
      await harness.fixture.whenStable();
      flushRemaining();
      harness.detectChanges();
      expect(harness.routeNativeElement!.querySelector('a.safety-back-link')).toBeNull();
    });
});
