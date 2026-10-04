import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';
import { DevelopmentIdentity, DevelopmentSession, developmentIdentityInterceptor } from './development-session';
import { CurrentFamilyContext } from './family-context';
import { routes } from './app.routes';
import { viewForPath } from './navigation';

const family = { familyGroupId: '33333333-3333-3333-3333-333333333333', name: 'Our Family' };
const accountA = '11111111-1111-1111-1111-111111111111';
const accountB = '66666666-6666-6666-6666-666666666666';
const selectedIdentityStorageKey = 'family-safety.development-identity';
const profile = {
  profileId: '77777777-7777-7777-7777-777777777777', displayName: 'Adult B',
  contactEmail: null, contactEmailVisibility: 'OnlyMe', contactPhone: null,
  contactPhoneVisibility: 'OnlyMe', birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const members = [{ profileId: profile.profileId, displayName: profile.displayName }];

describe('Invited adult first use', () => {
  let http: HttpTestingController;
  beforeEach(async () => {
    sessionStorage.removeItem(selectedIdentityStorageKey);
    history.replaceState(null, '', '/#invite=opaque');
    await TestBed.configureTestingModule({
      imports: [App, DevelopmentSession],
      providers: [provideHttpClient(withInterceptors([developmentIdentityInterceptor])), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    background();
    http.verify();
    sessionStorage.removeItem(selectedIdentityStorageKey);
    history.replaceState(null, '', '/');
    TestBed.resetTestingModule();
  });
  function background() {
    http.match(r => r.url.endsWith('/events') || r.url.endsWith('/plans') || r.url === '/api/trips')
      .forEach(r => { if (!r.cancelled) r.flush([]); });
  }
  function start() {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    http.expectOne('/api/membership-invitations/preview').flush({ familyGroupName: family.name, inviterDisplayName: 'Adult A' });
    fixture.detectChanges();
    background();
    return fixture;
  }
  function click(fixture: ComponentFixture<unknown>, label: string) {
    const button = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'))
      .find(b => b.textContent?.trim() === label && !b.closest('[hidden]'));
    expect(button, label).toBeDefined();
    button!.click();
    fixture.detectChanges();
  }
  /**
   * Follows a main-navigation or landing link. This spec renders App without routes or an outlet, so it
   * checks the link's real target and then shows that page through App's in-app navigate().
   */
  function follow(fixture: ComponentFixture<App>, label: string, href: string) {
    const link = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('a'))
      .find(a => a.textContent?.trim() === label && !a.closest('[hidden]'));
    expect(link, label).toBeDefined();
    expect(link!.getAttribute('href')).toBe(href);
    (fixture.componentInstance as unknown as { navigate(view: string): void }).navigate(viewForPath(href.slice(1))!);
    fixture.detectChanges();
  }
  function accept(fixture: ComponentFixture<App>) {
    click(fixture, 'Accept invitation');
    http.expectOne('/api/membership-invitations/accept').flush({ familyGroupId: family.familyGroupId });
    fixture.detectChanges();
  }
  function finishHandoff(fixture: ComponentFixture<App>) {
    http.expectOne('/api/family-groups/mine').flush([
      { familyGroupId: 'other-family', name: family.name }, family,
    ]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush(members);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([]);
    fixture.detectChanges();
    background();
  }

  // WI-009-010 (DEC-080) supersedes the WI-075-001 "Continue to Family" destination: the arrival now finishes on Home.
  it('completes focused acceptance and the optional profile, then finishes on Home without reloading the page', () => {
    const fixture = start();
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Family invitation');
    expect(fixture.nativeElement.querySelector('[aria-label="Invitation response"]').textContent).toContain('Adult A invited you');
    expect(fixture.nativeElement.querySelector('form').closest('[hidden]')).not.toBeNull();
    accept(fixture);
    finishHandoff(fixture);
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Review your profile');
    expect(fixture.nativeElement.textContent).toContain('Your membership is already active');
    expect(location.hash).toBe('');
    const form = fixture.nativeElement.querySelector('#contact-email-visibility') as HTMLSelectElement;
    expect(form.value).toBe('OnlyMe');
    click(fixture, 'Continue to Home without changes');
    http.expectNone(r => r.method === 'PUT');
    expect(fixture.nativeElement.querySelector('h1').textContent).toMatch(/^Good (morning|afternoon|evening), Adult B!$/);
    expect(fixture.nativeElement.querySelector('[aria-label="Family welcome"]').textContent).toContain('happening with Our Family today');
    expect(fixture.nativeElement.querySelector('p.family-switch-status').textContent.trim()).toBe('You joined Our Family.');
    follow(fixture, 'Family', '/family');
    const directory = fixture.nativeElement.querySelector('section[aria-label="Family dashboard"]');
    expect(directory.hidden).toBe(false);
    expect(directory.textContent).toContain('Adult B');
    // WI-009-007: membership details and relationships live in Family settings (/family/settings).
    const settings = fixture.nativeElement.querySelector('section[aria-label="Family settings"]');
    expect(settings.textContent).toContain('You are an active member');
    expect(settings.querySelector('#relationships-heading')).not.toBeNull();
    follow(fixture, 'Home', '/home');
    expect(fixture.nativeElement.querySelector('[aria-label="Family welcome"]').textContent).toContain('happening with Our Family today');
    expect(fixture.nativeElement.textContent).not.toContain('No family relationship suggestions need your response');
    follow(fixture, 'Plans & Trips', '/plans-trips');
    follow(fixture, 'Open Plans', '/plans');
    expect(fixture.nativeElement.querySelector('[aria-labelledby="plans-heading"]').hidden).toBe(false);
  });

  it.each(['memberships', 'directory', 'relationships'])('retries failed %s loading without accepting the used token again', stage => {
    const fixture = start();
    accept(fixture);
    const reload = http.expectOne('/api/family-groups/mine');
    if (stage === 'memberships') reload.flush({}, { status: 503, statusText: 'Unavailable' });
    else {
      reload.flush([family]);
      const directory = http.expectOne(`/api/family-groups/${family.familyGroupId}/members`);
      const relationships = http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`);
      if (stage === 'directory') {
        relationships.flush([]);
        directory.flush({}, { status: 503, statusText: 'Unavailable' });
      } else {
        directory.flush(members);
        relationships.flush({}, { status: 503, statusText: 'Unavailable' });
      }
    }
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Joining your family');
    expect(fixture.nativeElement.textContent).toContain('Your invitation was accepted');
    click(fixture, 'Retry loading family');
    http.expectNone('/api/membership-invitations/accept');
    finishHandoff(fixture);
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Review your profile');
  });

  it('declines explicitly and does not create a membership handoff', () => {
    const fixture = start();
    click(fixture, 'Decline invitation');
    http.expectOne('/api/membership-invitations/decline').flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();
    http.expectNone('/api/family-groups/mine');
    expect(fixture.nativeElement.textContent).toContain('Invitation declined');
    click(fixture, 'Go to Home');
    expect(fixture.nativeElement.querySelector('.home-no-family').textContent).toContain('You’re not part of a Family yet.');
    expect(fixture.nativeElement.querySelector('p.family-switch-status').textContent.trim()).toBe('');
  });

  it('ignores earlier Family reads that complete after the accepted Family handoff', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([{ familyGroupId: 'previous-family', name: 'Previous Family' }]);
    const previousDirectory = http.expectOne('/api/family-groups/previous-family/members');
    const previousRelationships = http.expectOne('/api/family-groups/previous-family/relationships');
    http.expectOne('/api/membership-invitations/preview').flush({ familyGroupName: family.name, inviterDisplayName: 'Adult A' });
    fixture.detectChanges();
    background();
    accept(fixture);
    finishHandoff(fixture);
    previousDirectory.flush([{ profileId: 'previous', displayName: 'Stale Family Member' }]);
    previousRelationships.flush([{ relationshipId: 'previous', status: 'Pending', isCurrentUserRecipient: true }]);
    fixture.detectChanges();
    click(fixture, 'Continue to Home without changes');
    follow(fixture, 'Family', '/family');
    expect(fixture.nativeElement.textContent).not.toContain('Stale Family Member');
    expect(fixture.nativeElement.querySelector('[aria-label="Active adult family members"]').textContent).toContain('Adult B');
    follow(fixture, 'Home', '/home');
    expect(fixture.nativeElement.textContent).not.toContain('No family relationship suggestions need your response');
  });

  it('handles malformed invitation encoding as unavailable without breaking the page', () => {
    history.replaceState(null, '', '/#invite=%ZZ');
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    fixture.detectChanges();
    http.expectNone('/api/membership-invitations/preview');
    expect(fixture.nativeElement.querySelector('[aria-label="Invitation response"]').textContent)
      .toContain('This invitation is unavailable');
    click(fixture, 'Go to Home');
    expect(fixture.nativeElement.querySelector('.home-no-family')).not.toBeNull();
  });

  // WI-009-010 O5: every outcome that cannot continue offers Go to Home; the messages are unchanged and disclose no reason.
  it.each([[404, 'This invitation is unavailable.'], [410, 'This invitation is unavailable.'], [403, 'This invitation is unavailable.'],
    [409, 'This family group has reached its capacity.']] as const)('preserves the existing acceptance error %s and offers Go to Home', (status, message) => {
    const fixture = start();
    click(fixture, 'Accept invitation');
    http.expectOne('/api/membership-invitations/accept').flush({}, { status, statusText: 'Unavailable' });
    fixture.detectChanges();
    const response = fixture.nativeElement.querySelector('[aria-label="Invitation response"]') as HTMLElement;
    expect(response.querySelector('[role="alert"]')!.textContent).toBe(message);
    expect(response.textContent).not.toMatch(/expired|revoked|already|used|member of|declined|wrong account/i);
    const buttons = Array.from(response.querySelectorAll('button')).map(button => button.textContent?.trim());
    expect(buttons).toContain('Go to Home');
    http.expectNone('/api/family-groups/mine');
    click(fixture, 'Go to Home');
    expect(fixture.nativeElement.querySelector('[aria-label="Invitation response"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('.home-no-family')).not.toBeNull();
  });

  it('shows the uniform unavailable preview with Go to Home and no Accept or reason', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    http.expectOne('/api/membership-invitations/preview').flush({}, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    const response = fixture.nativeElement.querySelector('[aria-label="Invitation response"]') as HTMLElement;
    expect(response.querySelector('[role="alert"]')!.textContent).toBe('This invitation is unavailable.');
    expect(Array.from(response.querySelectorAll('button')).map(button => button.textContent?.trim())).toEqual(['Go to Home']);
    expect(response.textContent).not.toContain('Our Family');
    expect(fixture.nativeElement.querySelector('nav[aria-label="Main navigation"]')).toBeNull();
  });

  it('saves only the approved profile values and continues after saving', () => {
    const fixture = start();
    accept(fixture);
    finishHandoff(fixture);
    const input = fixture.nativeElement.querySelector('#display-name') as HTMLInputElement;
    input.value = 'New display name'; input.dispatchEvent(new Event('input'));
    fixture.nativeElement.querySelector('form[novalidate]').dispatchEvent(new Event('submit'));
    const save = http.expectOne('/api/profile');
    expect(save.request.body.contactEmail).toBeNull();
    expect(Object.keys(save.request.body).sort()).toEqual([
      'displayName', 'contactEmail', 'contactEmailVisibility', 'contactPhone', 'contactPhoneVisibility',
      'birthdayMonth', 'birthdayDay', 'birthdayVisibility',
    ].sort());
    save.flush({ ...profile, displayName: 'New display name' });
    fixture.detectChanges();
    click(fixture, 'Continue to Home');
    expect(fixture.nativeElement.querySelector('h1').textContent).toMatch(/^Good (morning|afternoon|evening), New display name!$/);
    expect(fixture.nativeElement.querySelector('p.family-switch-status').textContent.trim()).toBe('You joined Our Family.');
  });

  it('replaces the workspace, cancels old reads and clears all prior-account forms and data when switching', async () => {
    history.replaceState(null, '', '/');
    const router = TestBed.inject(Router);
    router.resetConfig(routes);
    const fixture = TestBed.createComponent(DevelopmentSession);
    fixture.detectChanges();
    http.expectOne('/api/development/identities').flush({ defaultAccountId: accountA,
      identities: [{ accountId: accountA, label: 'Adult A' }, { accountId: accountB, label: 'Adult B' }] });
    fixture.detectChanges();
    await router.navigateByUrl('/home');
    fixture.detectChanges();
    const aProfile = http.expectOne('/api/profile');
    expect(aProfile.request.headers.get('X-Development-Account')).toBe(accountA);
    aProfile.flush({ ...profile, displayName: 'Private A', contactEmail: 'private-a@example.invalid' });
    http.expectOne('/api/family-groups/administered').flush([family]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([{ profileId: 'a', displayName: 'Private A' }]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([]);
    fixture.detectChanges();
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBe(family.familyGroupId);
    const oldWorkspace = fixture.debugElement.query(By.directive(App)).componentInstance;
    const oldEvents = http.expectOne(`/api/family-groups/${family.familyGroupId}/events`);
    const oldPlans = http.expectOne(`/api/family-groups/${family.familyGroupId}/plans`);
    const selector = fixture.nativeElement.querySelector('#development-account') as HTMLSelectElement;
    selector.value = accountB; selector.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(oldEvents.cancelled).toBe(true);
    expect(oldPlans.cancelled).toBe(true);
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBeNull();
    expect(fixture.debugElement.query(By.directive(App)).componentInstance).not.toBe(oldWorkspace);
    expect(fixture.nativeElement.textContent).not.toContain('Private A');
    expect(fixture.nativeElement.querySelector('#contact-email')).toBeNull();
    const bProfile = http.expectOne('/api/profile');
    expect(bProfile.request.headers.get('X-Development-Account')).toBe(accountB);
    bProfile.flush({ ...profile, displayName: 'Private B' });
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([]);
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('#display-name') as HTMLInputElement).value).toBe('Private B');
    expect(fixture.nativeElement.textContent).not.toContain('Private A');
    expect(selector.value).toBe(accountB);
    expect(sessionStorage.getItem(selectedIdentityStorageKey)).toBe(accountB);

    selector.value = accountA; selector.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBeNull();
    expect(fixture.nativeElement.querySelector('#contact-email')).toBeNull();
    expect(fixture.nativeElement.querySelector('#display-name')).toBeNull();
    const switchedBackProfile = http.expectOne('/api/profile');
    expect(switchedBackProfile.request.headers.get('X-Development-Account')).toBe(accountA);
    switchedBackProfile.flush({ ...profile, displayName: 'Private A' });
    http.expectOne('/api/family-groups/administered').flush([family]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/members`).flush([
      { profileId: 'a', displayName: 'Private A' },
    ]);
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush([]);
    fixture.detectChanges();
    expect(selector.value).toBe(accountA);
    expect(sessionStorage.getItem(selectedIdentityStorageKey)).toBe(accountA);
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBe(family.familyGroupId);
    expect(fixture.nativeElement.textContent).toContain('Private A');
    expect((fixture.nativeElement.querySelector('#display-name') as HTMLInputElement).value).toBe('Private A');
  });

  it('restores a valid selected Development identity after reload and opens the invitation as that identity', async () => {
    history.replaceState(null, '', '/#invite=opaque');
    sessionStorage.setItem(selectedIdentityStorageKey, accountB);
    const router = TestBed.inject(Router);
    router.resetConfig(routes);
    const fixture = TestBed.createComponent(DevelopmentSession);
    fixture.detectChanges();

    http.expectOne('/api/development/identities').flush({ defaultAccountId: accountA,
      identities: [{ accountId: accountA, label: 'Adult A' }, { accountId: accountB, label: 'Adult B' }] });
    fixture.detectChanges();
    expect(TestBed.inject(DevelopmentIdentity).accountId()).toBe(accountB);
    expect((fixture.nativeElement.querySelector('#development-account') as HTMLSelectElement).value)
      .toBe(accountB);
    router.initialNavigation();
    await fixture.whenStable();
    fixture.detectChanges();
    const bProfile = http.expectOne('/api/profile');
    expect(bProfile.request.headers.get('X-Development-Account')).toBe(accountB);
    bProfile.flush(profile);
    const administered = http.expectOne('/api/family-groups/administered');
    expect(administered.request.headers.get('X-Development-Account')).toBe(accountB);
    administered.flush([]);
    const memberships = http.expectOne('/api/family-groups/mine');
    expect(memberships.request.headers.get('X-Development-Account')).toBe(accountB);
    memberships.flush([]);
    const preview = http.expectOne('/api/membership-invitations/preview');
    expect(preview.request.headers.get('X-Development-Account')).toBe(accountB);
    preview.flush({ familyGroupName: family.name, inviterDisplayName: 'Adult A' });
    fixture.detectChanges();

    expect((fixture.nativeElement.querySelector('#development-account') as HTMLSelectElement).value)
      .toBe(accountB);
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toBe('Family invitation');
    expect(fixture.nativeElement.textContent).toContain('Our Family');
    expect(fixture.nativeElement.textContent).not.toContain('Current Family: Our Family');
    expect(TestBed.inject(CurrentFamilyContext).familyId()).toBeNull();
  });

  it('ignores and clears an unknown persisted Development identity before using the fixture default', () => {
    history.replaceState(null, '', '/');
    sessionStorage.setItem(selectedIdentityStorageKey, 'unknown-account-id');
    const fixture = TestBed.createComponent(DevelopmentSession);
    fixture.detectChanges();
    http.expectOne('/api/development/identities').flush({ defaultAccountId: accountA,
      identities: [{ accountId: accountA, label: 'Adult A' }, { accountId: accountB, label: 'Adult B' }] });
    fixture.detectChanges();

    expect((fixture.nativeElement.querySelector('#development-account') as HTMLSelectElement).value)
      .toBe(accountA);
    expect(sessionStorage.getItem(selectedIdentityStorageKey)).toBeNull();
  });
});
