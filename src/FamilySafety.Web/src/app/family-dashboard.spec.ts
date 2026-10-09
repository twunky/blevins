import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from './app';
import { routes } from './app.routes';

/** WI-009-007: the Family dashboard (/family) and Family Settings (/family/settings). */
const profile = {
  profileId: 'me', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const family = { familyGroupId: 'family-a', name: 'Family A' };
const members = [
  { profileId: 'me', displayName: 'Alex' },
  { profileId: 'sam', displayName: 'Sam Lee' },
  { profileId: 'kim', displayName: 'Kim Lee' },
  { profileId: 'pat', displayName: 'Pat Lee' },
];
const relationship = (id: string, proposer: string, recipient: string, status: 'Pending' | 'Accepted', proposerLabel = 'Sibling', recipientLabel = 'Sibling') => ({
  relationshipId: id,
  proposerProfileId: proposer, proposerDisplayName: members.find(m => m.profileId === proposer)!.displayName, proposerLabel,
  recipientProfileId: recipient, recipientDisplayName: members.find(m => m.profileId === recipient)!.displayName, recipientLabel,
  status, isCurrentUserProposer: proposer === 'me', isCurrentUserRecipient: recipient === 'me',
});
const incoming = relationship('incoming', 'sam', 'me', 'Pending', 'Parent', 'AdultChild');
const outgoing = relationship('outgoing', 'me', 'kim', 'Pending');
const acceptedWithMe = relationship('accepted-me', 'me', 'pat', 'Accepted', 'AdultChild', 'Parent');
const acceptedOthers = relationship('accepted-others', 'sam', 'kim', 'Accepted', 'SpousePartner', 'SpousePartner');

describe('Family dashboard and Family settings (WI-009-007)', () => {
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

  async function open(path: string, relationships: object[] = [], administered: object[] = []) {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(path, App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush(administered);
    http.expectOne('/api/family-groups/mine').flush([family]);
    http.expectOne('/api/family-groups/family-a/members').flush(members);
    http.expectOne('/api/family-groups/family-a/relationships').flush(relationships);
    http.expectOne('/api/family-groups/family-a/events').flush([]);
    http.expectOne('/api/family-groups/family-a/plans').flush([]);
    harness.detectChanges();
    return harness;
  }

  const visible = (root: HTMLElement, selector: string) => root.querySelector<HTMLElement>(selector)!;
  const buttons = (root: Element, name: string) =>
    Array.from(root.querySelectorAll('button')).filter(button => button.textContent?.trim() === name);

  it('renders the selected Family, compact directory and organization summaries', async () => {
    const harness = await open('/family');
    http.expectOne('/api/family-groups/family-a/households').flush({ canManage: false, households: [], unassignedMembers: members });
    http.expectOne('/api/family-groups/family-a/dates').flush({ canCreate: true, dates: [] });
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    expect(root.querySelector('h1')?.textContent?.trim()).toBe('Family A');
    expect(root.querySelector('a.family-settings-link')?.getAttribute('href')).toBe('/family/settings');
    expect(Array.from(root.querySelectorAll('.family-coordination h2')).map(h => h.textContent?.trim())).toEqual(['Households', 'Shared Family Dates']);
    expect(root.querySelector('.member-detail')).toBeNull();
    expect(root.querySelector('.home-future')).toBeNull();
    expect(root.querySelector('.family-dashboard')?.textContent).toContain('No shared dates are available to you.');
    expect(root.querySelector('.family-dashboard #relationship-member')).toBeNull();
    expect(root.querySelector('.member-button')?.getAttribute('href')).toBe('/me');
    expect(root.querySelectorAll('.member-button')[1].getAttribute('href')).toBe('/family/family-a/members/sam');
    expect(root.querySelector('.summary-view-all')?.getAttribute('href')).toBe('/family/settings#household-management');
  });

  it('filters authorized members immediately without another API read, and clears the query', async () => {
    const harness = await open('/family');
    const root = harness.routeNativeElement!;
    const search = root.querySelector<HTMLInputElement>('#family-search')!;
    search.value = 'sAM'; search.dispatchEvent(new Event('input')); harness.detectChanges();
    expect(root.querySelectorAll('.member-button')).toHaveLength(1);
    expect(root.querySelector('.member-name')?.textContent).toBe('Sam Lee');
    http.expectNone('/api/family-groups/family-a/members');
    search.value = 'missing'; search.dispatchEvent(new Event('input')); harness.detectChanges();
    expect(root.querySelector('[role="status"].directory-empty')?.textContent).toContain('No family members match');
    root.querySelector<HTMLButtonElement>('[aria-label="Clear Family search"]')!.click(); harness.detectChanges();
    expect(root.querySelectorAll('.member-button')).toHaveLength(4);
    expect(document.activeElement).toBe(search);
  });

  it('renders Settings for an administrator with Back to Family, details, Invite, and grouped relationships', async () => {
    const harness = await open('/family/settings', [incoming, outgoing, acceptedWithMe], [family]);
    const root = harness.routeNativeElement!;
    expect(root.querySelector('h1')?.textContent?.trim()).toBe('Family settings');
    expect(root.querySelector<HTMLAnchorElement>('a.family-back-link')?.getAttribute('href')).toBe('/family');
    expect(root.querySelector('[aria-current="true"]')?.textContent?.trim()).toBe('Family');
    const settings = visible(root, 'section.family-settings');
    expect(settings.hidden).toBe(false);
    expect(visible(root, 'section.family-dashboard').hidden).toBe(true);
    expect(settings.querySelector('#family-details-heading')).not.toBeNull();
    expect(settings.querySelector('#invite-heading')?.textContent).toBe('Invite an adult family member');
    expect(settings.querySelector('#invitation-email')).not.toBeNull();
    expect(Array.from(settings.querySelectorAll('.relationship-group-heading')).map(h => h.textContent?.trim()))
      .toEqual(['Suggest a relationship', 'Waiting for you', 'Sent by you', 'Accepted relationships']);
    expect(settings.querySelector('#relationship-member')).not.toBeNull();
  });

  it('omits administrator-only Invite from Settings for a member who is not an administrator', async () => {
    const harness = await open('/family/settings', [], []);
    const settings = harness.routeNativeElement!.querySelector('section.family-settings')!;
    expect(settings.querySelector('#family-details-heading')).not.toBeNull();
    expect(settings.querySelector('#relationships-heading')).not.toBeNull();
    expect(settings.querySelector('#invite-heading, #invitation-email')).toBeNull();
    expect(settings.textContent).not.toContain('Administering');
    expect(buttons(settings, 'Send or resend invitation')).toHaveLength(0);
    expect(buttons(settings, 'Revoke invitation')).toHaveLength(0);
  });

  it('explains in Settings, once members load, why a relationship cannot be suggested when no other adult is active', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/family/settings', App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    http.expectOne('/api/family-groups/family-a/relationships').flush([]);
    http.expectOne('/api/family-groups/family-a/events').flush([]);
    http.expectOne('/api/family-groups/family-a/plans').flush([]);
    harness.detectChanges();
    const settings = harness.routeNativeElement!.querySelector('section.family-settings')!;
    const suggest = buttons(settings, 'Suggest relationship')[0];
    const explanation = 'This Family has no other active adult member to suggest a relationship with yet.';

    // Still loading the member list: the action is disabled, but nothing claims there is no one to choose.
    expect(suggest.disabled).toBe(true);
    expect(settings.textContent).not.toContain(explanation);

    http.expectOne('/api/family-groups/family-a/members').flush([members[0]]);
    harness.detectChanges();
    expect(suggest.disabled).toBe(true);
    expect(settings.textContent).toContain(explanation);
    expect(settings.textContent).toContain('A Family administrator may invite another adult.');
    const describedBy = suggest.getAttribute('aria-describedby');
    expect(describedBy && settings.querySelector(`#${describedBy}`)?.textContent).toContain(explanation);
  });

  it('shows no missing-adult explanation in Settings when another active adult can be chosen', async () => {
    const harness = await open('/family/settings');
    const settings = harness.routeNativeElement!.querySelector('section.family-settings')!;
    const suggest = buttons(settings, 'Suggest relationship')[0];
    expect(suggest.disabled).toBe(false);
    expect(suggest.hasAttribute('aria-describedby')).toBe(false);
    expect(settings.textContent).not.toContain('no other active adult member');
    expect(Array.from(settings.querySelectorAll<HTMLOptionElement>('#relationship-member option')).map(option => option.textContent?.trim()))
      .toEqual(['Choose a family member', 'Sam Lee', 'Kim Lee', 'Pat Lee']);
  });

  it('offers Retry when the authorized directory read fails', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/family', App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    http.expectOne('/api/family-groups/family-a/members').flush({}, { status: 503, statusText: 'Unavailable' });
    harness.detectChanges();
    buttons(harness.routeNativeElement!, 'Retry loading Family members')[0].click();
    http.expectOne('/api/family-groups/family-a/members').flush(members); harness.detectChanges();
    expect(harness.routeNativeElement!.querySelectorAll('.member-button')).toHaveLength(4);
  });

  it('in Settings, shows a failed member read with Retry instead of claiming no other adult exists', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/family/settings', App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    http.expectOne('/api/family-groups/family-a/members').flush({}, { status: 503, statusText: 'Unavailable' });
    http.expectOne('/api/family-groups/family-a/relationships').flush([]);
    http.expectOne('/api/family-groups/family-a/events').flush([]);
    http.expectOne('/api/family-groups/family-a/plans').flush([]);
    harness.detectChanges();
    const settings = harness.routeNativeElement!.querySelector('section.family-settings')!;
    expect(settings.textContent).toContain('Your family directory could not be loaded.');
    expect(settings.textContent).not.toContain('no other active adult member');
    expect(buttons(settings, 'Suggest relationship')[0].disabled).toBe(true);

    buttons(settings, 'Retry loading Family members')[0].click();
    http.expectNone('/api/family-groups/family-a/relationships');
    http.expectOne('/api/family-groups/family-a/members').flush(members);
    harness.detectChanges();
    expect(buttons(settings, 'Suggest relationship')[0].disabled).toBe(false);
    expect(settings.textContent).not.toContain('could not be loaded');
  });

  it('withdraws a sent suggestion and removes an accepted relationship after confirmation in Settings', async () => {
    const harness = await open('/family/settings', [outgoing, acceptedWithMe]);
    const settings = harness.routeNativeElement!.querySelector('section.family-settings')!;
    buttons(settings, 'Withdraw suggestion')[0].click();
    http.expectOne(r => r.method === 'POST' && r.url === '/api/family-groups/relationships/outgoing/withdraw').flush({});
    http.expectOne('/api/family-groups/family-a/relationships').flush([acceptedWithMe]);
    harness.detectChanges();
    buttons(settings, 'Remove relationship')[0].click();
    harness.detectChanges();
    http.expectNone(r => r.method === 'DELETE');
    buttons(settings, 'Confirm removal')[0].click();
    http.expectOne(r => r.method === 'DELETE' && r.url === '/api/family-groups/relationships/accepted-me').flush({});
    http.expectOne('/api/family-groups/family-a/relationships').flush([]);
    harness.detectChanges();
    expect(settings.textContent).toContain('Relationship removed.');
    expect(settings.textContent).toContain('No Family relationships or suggestions are currently visible here.');
  });
});
