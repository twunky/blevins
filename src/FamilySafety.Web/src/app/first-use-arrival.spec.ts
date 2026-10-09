import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NavigationEnd, provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { filter, firstValueFrom } from 'rxjs';
import { App } from './app';
import { routes } from './app.routes';
import { CurrentFamilyContext } from './family-context';

const profile = {
  profileId: 'person', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const a = { familyGroupId: 'family-a', name: 'Family A' };
const b = { familyGroupId: 'family-b', name: 'Family B' };
const homeNoFamily = 'You’re not part of a Family yet. Ask a Family administrator to send you an invitation. '
  + 'While you wait, you can review your profile and sharing choices.';
const sharedNoFamily = 'This area needs an active Family. Ask a Family administrator to send you an invitation.';
const innerNoFamily = /Family access begins|do not currently have an active family group/i;

/** WI-009-010 (DEC-080, O1–O11): invitation arrival on Home, no-Family guidance, first Family choice, and Invite feedback. */
describe('First use and invitation arrival (WI-009-010)', () => {
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

  async function visit(path: string) {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(path, App);
    return harness;
  }

  function shellReads(groups: typeof a[], administered: typeof a[] = []) {
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush(administered);
    http.expectOne('/api/family-groups/mine').flush(groups);
  }

  /** Answers the selected Family's reads (and Trips) with empty lists, the directory with the caller only. */
  async function settle(harness: RouterTestingHarness) {
    for (let round = 0; round < 3; round++) {
      harness.detectChanges();
      await harness.fixture.whenStable();
      http.match(() => true).forEach((request: TestRequest) => {
        if (request.cancelled) return;
        request.flush(request.request.url.endsWith('/members') ? [{ profileId: 'person', displayName: 'Alex' }] : []);
      });
    }
    harness.detectChanges();
  }

  const root = (harness: RouterTestingHarness) => harness.routeNativeElement!;
  /**
   * What the page renders: [hidden] elements removed, except .page-content, whose display:grid overrides [hidden]
   * (the WI-009-010 finding; O11 leaves that container rule unchanged), so its children are judged by their own [hidden].
   */
  const rendered = (harness: RouterTestingHarness) => {
    const clone = root(harness).cloneNode(true) as HTMLElement;
    clone.querySelectorAll('[hidden]:not(.page-content)').forEach(element => element.remove());
    return clone;
  };
  const status = (harness: RouterTestingHarness) => root(harness).querySelector('p.family-switch-status')!.textContent!.trim();
  const button = (harness: RouterTestingHarness, label: string) => Array.from(root(harness).querySelectorAll('button'))
    .find(element => element.textContent?.trim() === label && !element.closest('[hidden]'));

  async function click(harness: RouterTestingHarness, label: string, navigates = false) {
    const router = TestBed.inject(Router);
    const navigated = navigates ? firstValueFrom(router.events.pipe(filter(event => event instanceof NavigationEnd))) : null;
    expect(button(harness, label), label).toBeDefined();
    button(harness, label)!.click();
    await navigated;
    harness.detectChanges();
  }

  /** Invitation link → preview → Accept → the server-confirmed membership reload → profile review. */
  async function acceptToProfileReview() {
    history.replaceState(null, '', '/#invite=opaque');
    const harness = await visit('/#invite=opaque');
    shellReads([]);
    // The invitation's own Family name differs on purpose: the announcement must name the membership-list Family.
    http.expectOne('/api/membership-invitations/preview').flush({ familyGroupName: 'Name In Invitation', inviterDisplayName: 'Inviter' });
    harness.detectChanges();
    expect(root(harness).querySelector('nav[aria-label="Main navigation"]')).toBeNull();
    await click(harness, 'Accept invitation');
    http.expectOne('/api/membership-invitations/accept').flush({ familyGroupId: a.familyGroupId });
    http.expectOne('/api/family-groups/mine').flush([a]);
    await settle(harness);
    return harness;
  }

  it.each(['Continue to Home without changes', 'Continue to Home'])(
    'accepts, selects the confirmed Family, keeps the profile review optional, and "%s" lands on /home once announced (O1–O3)', async label => {
      const router = TestBed.inject(Router);
      const context = TestBed.inject(CurrentFamilyContext);
      const harness = await acceptToProfileReview();
      expect(context.familyId()).toBe(a.familyGroupId);
      expect(root(harness).querySelector('h1')?.textContent).toBe('Review your profile');
      expect(root(harness).textContent).toContain('changes are optional');
      // Never Family content while the URL is still the invitation entry.
      expect(router.url.startsWith('/family')).toBe(false);
      expect(root(harness).querySelector<HTMLElement>('section[aria-label="Family dashboard"]')!.hidden).toBe(true);
      expect(status(harness)).toBe('');

      await click(harness, label, true);
      http.expectNone(request => request.method === 'PUT');
      shellReads([a]);
      await settle(harness);
      expect(router.url).toBe('/home');
      expect(context.familyId()).toBe(a.familyGroupId);
      expect(root(harness).querySelector('h1')?.textContent).toMatch(/^Good (morning|afternoon|evening), Alex!$/);
      expect(status(harness)).toBe('You joined Family A.');
      expect(root(harness).querySelector('p.family-switch-status')!.getAttribute('role')).toBe('status');
      expect(root(harness).textContent).not.toContain('Name In Invitation');
      expect(root(harness).textContent!.split('You joined').length).toBe(2);
      expect(document.activeElement?.tagName).toBe('H1');

      // Re-entry: Home again, by an ordinary navigation, never repeats the one-time announcement.
      await harness.navigateByUrl('/family', App);
      shellReads([a]);
      await settle(harness);
      expect(status(harness)).toBe('');
      await harness.navigateByUrl('/home', App);
      shellReads([a]);
      await settle(harness);
      expect(router.url).toBe('/home');
      expect(status(harness)).toBe('');
    });

  it('announces nothing on a fresh load of /home, which carries no navigation state (refresh)', async () => {
    const harness = await visit('/home');
    shellReads([a]);
    await settle(harness);
    expect(status(harness)).toBe('');
  });

  it('names only a Family from the caller\'s own membership list, and only when it is the selected one (O2)', async () => {
    const router = TestBed.inject(Router);
    const harness = await visit('/me');
    shellReads([a]);
    await settle(harness);
    // A navigation state that names a Family the caller is not a member of announces nothing.
    await router.navigateByUrl('/home', { state: { joinedFamilyId: 'not-my-family' } });
    harness.detectChanges();
    shellReads([a]);
    await settle(harness);
    expect(status(harness)).toBe('');
    // With two Families and none selected, the named Family is not the selection: still nothing.
    await router.navigateByUrl('/me');
    harness.detectChanges();
    shellReads([a]);
    await settle(harness);
    TestBed.inject(CurrentFamilyContext).familyId.set(null);
    await router.navigateByUrl('/home', { state: { joinedFamilyId: b.familyGroupId } });
    harness.detectChanges();
    shellReads([a, b]);
    await settle(harness);
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBeNull();
    expect(status(harness)).toBe('');
  });

  it('leaves the accepted invitation token out of everything after acceptance', async () => {
    const harness = await acceptToProfileReview();
    await click(harness, 'Continue to Home without changes', true);
    const after = http.match(() => true);
    expect(after.every(request => !JSON.stringify(request.request.body ?? '').includes('opaque')
      && !request.request.urlWithParams.includes('opaque'))).toBe(true);
    after.forEach(request => request.flush(request.request.url === '/api/profile' ? profile
      : request.request.url.endsWith('/mine') ? [a] : []));
    await settle(harness);
    expect(location.hash).toBe('');
    expect(TestBed.inject(Router).url).toBe('/home');
  });

  it('shows the approved no-Family Home with one action, Review your profile, that opens /me (O4)', async () => {
    const router = TestBed.inject(Router);
    const harness = await visit('/home');
    shellReads([]);
    await settle(harness);
    const card = root(harness).querySelector<HTMLElement>('section.home-no-family')!;
    expect(card.querySelector('p')!.textContent).toBe(homeNoFamily);
    const actions = Array.from(card.querySelectorAll('a, button'));
    expect(actions.map(action => action.textContent?.trim())).toEqual(['Review your profile']);
    expect(actions[0].getAttribute('href')).toBe('/me');
    expect(rendered(harness).querySelector('main')!.textContent).not.toMatch(/Create (a )?Family|Find Family|Join code|Request (to join|access)|Search Families/i);
    expect(Array.from(rendered(harness).querySelectorAll('main button')).map(element => element.textContent?.trim())).toEqual([]);
    expect(root(harness).querySelector('section.context-prompt')).toBeNull();

    const navigated = firstValueFrom(router.events.pipe(filter(event => event instanceof NavigationEnd)));
    (actions[0] as HTMLAnchorElement).click();
    await navigated;
    harness.detectChanges();
    shellReads([]);
    await settle(harness);
    expect(router.url).toBe('/me');
    expect(root(harness).querySelector('h1')?.textContent).toBe('Your profile');
    expect(document.activeElement?.tagName).toBe('H1');
    // Me / Settings itself is unchanged: the user's own profile form, with no Family needed.
    expect(root(harness).querySelector<HTMLElement>('form.profile-form')!.hidden).toBe(false);
    expect(root(harness).querySelector('section.context-prompt')).toBeNull();
  });

  it('keeps the Home profile-load error visible', async () => {
    const harness = await visit('/home');
    http.expectOne('/api/profile').flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([a]);
    await settle(harness);
    const error = Array.from(root(harness).querySelectorAll<HTMLElement>('[role="alert"]'))
      .find(element => element.textContent?.includes('Your profile could not be loaded.'));
    expect(error).toBeDefined();
    // Its only [hidden] ancestor is .page-content, which display:grid keeps visible; O11 keeps that rule.
    expect(error!.closest('[hidden]:not(.page-content)')).toBeNull();
    expect(error!.closest('.page-content')).not.toBeNull();
  });

  it('offers Retry for a failed profile read on Me / Settings that repeats only the profile read', async () => {
    const harness = await visit('/me');
    const unavailable = { status: 503, statusText: 'Unavailable' };
    http.expectOne('/api/profile').flush({}, unavailable);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    await settle(harness);
    const retry = () => Array.from(root(harness).querySelectorAll('button'))
      .find(button => button.textContent?.trim() === 'Retry loading profile');
    const displayName = () => root(harness).querySelector<HTMLInputElement>('#display-name')!.value;
    expect(root(harness).textContent).toContain('Your profile could not be loaded. Please try again.');
    expect(retry()).toBeDefined();

    // A Retry that fails again returns to the same error with Retry still offered.
    retry()!.click();
    harness.detectChanges();
    expect(root(harness).textContent).toContain('Loading your profile…');
    expect(root(harness).textContent).not.toContain('could not be loaded');
    http.expectNone('/api/family-groups/mine');
    http.expectNone('/api/family-groups/administered');
    http.expectOne('/api/profile').flush({}, unavailable);
    harness.detectChanges();
    expect(root(harness).textContent).toContain('Your profile could not be loaded. Please try again.');
    expect(root(harness).textContent).not.toContain('Loading your profile…');
    expect(displayName()).toBe('');

    retry()!.click();
    http.expectOne('/api/profile').flush(profile);
    harness.detectChanges();
    expect(displayName()).toBe('Alex');
    expect(retry()).toBeUndefined();
    expect(root(harness).textContent).not.toContain('could not be loaded');
    expect(root(harness).textContent).not.toContain('Loading your profile…');
  });

  it('does not offer a profile Retry beside a profile save error', async () => {
    const harness = await visit('/me');
    shellReads([]);
    await settle(harness);
    Array.from(root(harness).querySelectorAll('button')).find(button => button.textContent?.trim() === 'Save profile')!.click();
    http.expectOne(request => request.method === 'PUT' && request.url === '/api/profile')
      .flush({}, { status: 503, statusText: 'Unavailable' });
    harness.detectChanges();
    expect(root(harness).textContent).toContain('Your profile could not be saved. Please try again.');
    expect(Array.from(root(harness).querySelectorAll('button')).some(button => button.textContent?.trim() === 'Retry loading profile'))
      .toBe(false);
  });

  it.each(['/family', '/events', '/events/all', '/events/new', '/plans'])(
    'shows one approved Family-required message on %s with no Family, and no inner no-Family copy (O11)', async url => {
      const router = TestBed.inject(Router);
      const harness = await visit(url);
      shellReads([]);
      await settle(harness);
      const prompts = root(harness).querySelectorAll<HTMLElement>('section.context-prompt');
      expect(prompts).toHaveLength(1);
      expect(prompts[0].querySelector('p')!.textContent).toBe(sharedNoFamily);
      const link = prompts[0].querySelector<HTMLAnchorElement>('a')!;
      expect(link.textContent?.trim()).toBe('Review your profile');
      expect(link.getAttribute('href')).toBe('/me');
      expect(rendered(harness).textContent).not.toMatch(innerNoFamily);
      expect(root(harness).querySelector('.events-state--no-family')).toBeNull();
      expect(Array.from(root(harness).querySelectorAll('.plans-state')).some(element => /Family access begins/.test(element.textContent!))).toBe(false);
      expect(rendered(harness).textContent!.split('needs an active Family').length).toBe(2);
      expect(root(harness).textContent).not.toContain('not part of a Family yet');
      // The Family directory card is hidden too, so the page carries the one message only.
      expect(root(harness).querySelector<HTMLElement>('section.family-dashboard')!.hidden).toBe(true);
      expect(root(harness).querySelector('main')!.textContent).not.toMatch(/Create (a )?Family|Join code/i);

      const navigated = firstValueFrom(router.events.pipe(filter(event => event instanceof NavigationEnd)));
      link.click();
      await navigated;
      harness.detectChanges();
      shellReads([]);
      await settle(harness);
      expect(router.url).toBe('/me');
    });

  it('shows only the membership error and Retry, not the no-Family copy, when the Family list fails', async () => {
    const harness = await visit('/events');
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush({}, { status: 503, statusText: 'Unavailable' });
    await settle(harness);
    const prompt = root(harness).querySelector<HTMLElement>('section.context-prompt')!;
    expect(prompt.textContent).toContain('Your Family memberships could not be loaded.');
    expect(rendered(harness).textContent).not.toMatch(innerNoFamily);
    expect(root(harness).textContent).not.toContain(sharedNoFamily);
  });

  it('keeps the multi-Family choice prompt and its page unchanged when no Family is chosen (independent state)', async () => {
    const harness = await visit('/family');
    shellReads([a, b]);
    await settle(harness);
    expect(root(harness).querySelector('section.context-prompt')!.textContent).toContain('Choose your current Family above to use this area.');
    expect(root(harness).querySelector('section.context-prompt')!.textContent).not.toContain(sharedNoFamily);
    expect(root(harness).querySelector<HTMLElement>('section.family-dashboard')!.hidden).toBe(false);
  });

  it('keeps Trips usable with no Family and no prompt (independent state, REQ-UX-011)', async () => {
    const harness = await visit('/trips');
    shellReads([]);
    await settle(harness);
    expect(root(harness).querySelector('section.context-prompt')).toBeNull();
    expect(root(harness).querySelector('app-trips')).not.toBeNull();
  });

  it.each([['/activities', 'app-activities'], ['/plans/new', 'section.plans-page'], ['/family', 'section.family-dashboard']])(
    'renders %s for a selected Family with no prompt and no no-Family copy', async (url, selector) => {
      const harness = await visit(url);
      shellReads([a]);
      await settle(harness);
      expect(root(harness).querySelector('section.context-prompt')).toBeNull();
      expect(root(harness).querySelector<HTMLElement>(selector)!.hidden).toBe(false);
      expect(rendered(harness).textContent).not.toMatch(innerNoFamily);
    });

  it.each(['/activities', '/family'])('keeps %s when the first Family is chosen from no selection, then on a normal switch (O6)', async url => {
    const router = TestBed.inject(Router);
    const context = TestBed.inject(CurrentFamilyContext);
    const harness = await visit(url);
    shellReads([a, b]);
    await settle(harness);
    expect(context.familyId()).toBeNull();
    const selector = root(harness).querySelector<HTMLSelectElement>('#current-family')!;
    selector.value = a.familyGroupId;
    selector.dispatchEvent(new Event('change'));
    harness.detectChanges();
    expect(http.match(`/api/family-groups/${a.familyGroupId}/members`)).toHaveLength(1);
    await settle(harness);
    expect(router.url).toBe(url);
    expect(context.familyId()).toBe(a.familyGroupId);
    expect(root(harness).querySelector('section.context-prompt')).toBeNull();
    expect(root(harness).querySelector('.page-title')?.textContent).toContain('Family A');

    // The ordinary switch between two selected Families is unchanged: same page, the new Family's reads.
    selector.value = b.familyGroupId;
    selector.dispatchEvent(new Event('change'));
    harness.detectChanges();
    expect(http.match(`/api/family-groups/${b.familyGroupId}/members`)).toHaveLength(1);
    await settle(harness);
    expect(router.url).toBe(url);
    expect(context.familyId()).toBe(b.familyGroupId);
    expect(root(harness).querySelector('.page-title')?.textContent).toContain('Family B');
  });

  it.each([['Send or resend invitation', '/api/membership-invitations', 'Invitation sent.'],
    ['Revoke invitation', '/api/membership-invitations/revoke', 'Invitation revoked.']])(
    'shows "%s" feedback inside the Invite card only, and the administrator statement once (O7)', async (label, url, message) => {
      TestBed.inject(CurrentFamilyContext).familyId.set(a.familyGroupId);
      const harness = await visit('/family/settings');
      shellReads([a], [a]);
      await settle(harness);
      const card = root(harness).querySelector<HTMLElement>('form.family-admin-panel')!;
      const email = card.querySelector<HTMLInputElement>('#invitation-email')!;
      email.value = 'recipient@example.invalid';
      email.dispatchEvent(new Event('input'));
      await click(harness, label);
      http.expectOne(url).flush({});
      harness.detectChanges();
      const feedback = card.querySelector<HTMLElement>('.invite-feedback')!;
      expect(feedback.textContent?.trim()).toBe(message);
      expect(feedback.getAttribute('role')).toBe('status');
      expect(root(harness).textContent!.split(message).length).toBe(2);
      expect(root(harness).textContent!.match(/Administrator status does not provide access to private information/g)).toHaveLength(1);

      // An error also stays in the card, once.
      await click(harness, label);
      http.expectOne(url).flush({}, { status: 500, statusText: 'Server Error' });
      harness.detectChanges();
      const error = card.querySelector<HTMLElement>('.invite-feedback[role="alert"]')!;
      expect(error.textContent?.trim()).toBe('The invitation action could not be completed.');
      expect(root(harness).textContent!.split('The invitation action could not be completed.').length).toBe(2);
    });
});
