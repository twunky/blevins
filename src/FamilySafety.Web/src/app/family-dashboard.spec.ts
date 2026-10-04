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

  it('renders /family as the dashboard: title, gear for every member, members, and the coordination grid', async () => {
    const harness = await open('/family');
    const root = harness.routeNativeElement!;
    expect(root.querySelector('h1')?.textContent?.trim()).toBe('Family');
    expect(root.querySelector('.page-title p')?.textContent).toContain('Family A');
    const gear = root.querySelector<HTMLAnchorElement>('a.family-settings-link')!;
    expect(gear.getAttribute('aria-label')).toBe('Family settings');
    expect(gear.getAttribute('href')).toBe('/family/settings');
    const dashboard = visible(root, 'section.family-dashboard');
    expect(dashboard.hidden).toBe(false);
    expect(visible(root, 'section.family-settings').hidden).toBe(true);
    expect(Array.from(dashboard.querySelectorAll('.family-coordination h2')).map(h => h.textContent?.trim()))
      .toEqual(['Waiting for you', 'Upcoming birthdays', 'Recent Family Updates', 'Recent Check-Ins']);
    // Management controls are not on the dashboard.
    expect(dashboard.querySelector('#relationship-member, #invitation-email')).toBeNull();
    expect(root.querySelector('[aria-current="page"]')?.textContent?.trim()).toBe('Family');
  });

  it('keeps placeholders data-free, action-free, and request-free (D1, D7)', async () => {
    const harness = await open('/family');
    const placeholders = Array.from(harness.routeNativeElement!.querySelectorAll<HTMLElement>('.family-coordination .home-future'));
    expect(placeholders.map(section => section.querySelector('.home-future-note')?.textContent?.trim())).toEqual([
      'Birthdays shared with your family will appear here.',
      'Family updates will appear here as activity is shared.',
      'Recent family check-ins will appear here when available.',
    ]);
    for (const section of placeholders) {
      expect(section.querySelectorAll('button, a, input, select, img, li, .avatar')).toHaveLength(0);
      expect(section.textContent).not.toMatch(/\d|Coming later|Phase|roadmap|work item/i);
    }
    http.expectNone(request => !/^\/api\/(profile|family-groups)/.test(request.url));
  });

  it('shows only incoming suggestions in Waiting for you, and Accept and Decline call the existing endpoints (D2)', async () => {
    const harness = await open('/family', [incoming, outgoing, acceptedWithMe]);
    const waiting = harness.routeNativeElement!.querySelector<HTMLElement>('.family-waiting')!;
    const items = waiting.querySelectorAll('.family-request');
    expect(items).toHaveLength(1);
    expect(items[0].textContent).toContain('Sam Lee suggested a relationship');
    expect(waiting.textContent).not.toContain('Kim Lee');
    expect(waiting.querySelector<HTMLAnchorElement>('a.family-card-link')?.getAttribute('href')).toBe('/family/settings');

    buttons(waiting, 'Accept relationship')[0].click();
    http.expectOne(r => r.method === 'POST' && r.url === '/api/family-groups/relationships/incoming/accept').flush({});
    http.expectOne('/api/family-groups/family-a/relationships').flush([relationship('incoming', 'sam', 'me', 'Accepted', 'Parent', 'AdultChild')]);
    harness.detectChanges();
    expect(waiting.querySelectorAll('.family-request')).toHaveLength(0);
    expect(waiting.textContent).toContain('Nothing is waiting for you right now.');
    expect(waiting.textContent).toContain('Relationship confirmed.');
  });

  it('declines from Waiting for you', async () => {
    const harness = await open('/family', [incoming]);
    buttons(harness.routeNativeElement!.querySelector('.family-waiting')!, 'Decline suggestion')[0].click();
    http.expectOne(r => r.method === 'POST' && r.url === '/api/family-groups/relationships/incoming/decline').flush({});
    http.expectOne('/api/family-groups/family-a/relationships').flush([]);
    harness.detectChanges();
    expect(harness.routeNativeElement!.querySelector('.family-waiting')?.textContent).toContain('Nothing is waiting for you right now.');
  });

  it('shows a tile note only for an accepted relationship with the signed-in adult, and no administrator status (D5)', async () => {
    const harness = await open('/family', [incoming, outgoing, acceptedWithMe, acceptedOthers], [family]);
    const tiles = Array.from(harness.routeNativeElement!.querySelectorAll<HTMLElement>('.member-list .member-button'));
    const note = (name: string) => tiles.find(tile => tile.querySelector('.member-name')?.textContent?.trim() === name)!
      .querySelector('.member-note')?.textContent?.trim() ?? null;
    expect(note('Pat Lee')).toBe('Parent'); // Pat's own side of the accepted relationship with me.
    expect(note('Alex')).toBeNull(); // No fallback such as "You".
    expect(note('Sam Lee')).toBeNull(); // Pending only.
    expect(note('Kim Lee')).toBeNull(); // Accepted, but not with me.
    expect(harness.routeNativeElement!.querySelector('.member-list')?.textContent).not.toMatch(/administrator|admin/i);
  });

  it('opens member details with the shared fields and accepted relationships for that member', async () => {
    const harness = await open('/family', [acceptedOthers]);
    const sam = Array.from(harness.routeNativeElement!.querySelectorAll<HTMLButtonElement>('.member-button'))
      .find(button => button.textContent?.includes('Sam Lee'))!;
    sam.click();
    http.expectOne('/api/family-members/sam/profile').flush({
      profileId: 'sam', displayName: 'Sam Lee', contactEmail: 'sam@example.invalid', contactPhone: null, birthdayMonth: null, birthdayDay: null,
    });
    harness.detectChanges();
    const detail = harness.routeNativeElement!.querySelector('[aria-labelledby="selected-member-heading"]')!;
    expect(detail.textContent).toContain('sam@example.invalid');
    expect(detail.textContent).toContain('Sam Lee: Spouse or partner');
    expect(sam.classList).toContain('member-button--selected');
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

  it('offers separate Retry actions for failed member and relationship reads on the Family dashboard', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/family', App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    const fail = { status: 503, statusText: 'Unavailable' };
    http.expectOne('/api/family-groups/family-a/members').flush({}, fail);
    http.expectOne('/api/family-groups/family-a/relationships').flush({}, fail);
    http.expectOne('/api/family-groups/family-a/events').flush([]);
    http.expectOne('/api/family-groups/family-a/plans').flush([]);
    harness.detectChanges();
    const dashboard = harness.routeNativeElement!.querySelector('section.family-dashboard')!;
    expect(dashboard.textContent).toContain('Your family directory could not be loaded.');
    expect(dashboard.textContent).toContain('Family relationships could not be loaded.');

    buttons(dashboard, 'Retry loading Family members')[0].click();
    http.expectNone('/api/family-groups/family-a/relationships');
    http.expectOne('/api/family-groups/family-a/members').flush(members);
    harness.detectChanges();
    expect(dashboard.textContent).toContain('Sam Lee');
    expect(buttons(dashboard, 'Retry loading Family members')).toHaveLength(0);

    buttons(dashboard, 'Retry loading relationships')[0].click();
    http.expectNone('/api/family-groups/family-a/members');
    http.expectOne('/api/family-groups/family-a/relationships').flush([incoming]);
    harness.detectChanges();
    expect(dashboard.textContent).toContain('Sam Lee suggested a relationship');
    expect(buttons(dashboard, 'Retry loading relationships')).toHaveLength(0);
  });

  it('does not offer Retry for an unavailable member profile, which is not a failed list read', async () => {
    const harness = await open('/family');
    const dashboard = harness.routeNativeElement!.querySelector('section.family-dashboard')!;
    Array.from(dashboard.querySelectorAll<HTMLButtonElement>('button.member-button'))
      .find(button => button.textContent?.includes('Sam Lee'))!.click();
    http.expectOne('/api/family-members/sam/profile').flush({}, { status: 404, statusText: 'Not Found' });
    harness.detectChanges();
    expect(dashboard.textContent).toContain('This family member is unavailable.');
    expect(buttons(dashboard, 'Retry loading Family members')).toHaveLength(0);
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
