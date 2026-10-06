import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from './app';
import { routes } from './app.routes';

const familyA = { familyGroupId: 'family-a', name: 'Family A' };
const familyB = { familyGroupId: 'family-b', name: 'Family B' };
const profile = { profileId: 'me', displayName: 'Alex', contactEmailVisibility: 'OnlyMe',
  contactPhoneVisibility: 'OnlyMe', birthdayVisibility: 'OnlyMe' };
const invitation = { invitationId: 'invitation-a', eventId: 'aaaaaaaa-0000-4000-8000-000000000001',
  name: 'Past gathering', date: '2020-01-01', time: '18:30:00' };
const pendingUrl = (family = familyA) => `/api/family-groups/${family.familyGroupId}/event-invitations/mine`;

describe('Home pending RSVP attention (WI-095-009)', () => {
  let http: HttpTestingController;
  beforeEach(async () => {
    history.replaceState(null, '', '/');
    await TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    // Only unrelated pre-existing Home reads may remain; attention requests must be asserted by each test.
    http.expectNone(request => request.url.endsWith('/event-invitations/mine'));
    http.match(() => true).forEach(request => { if (!request.cancelled) request.flush([]); });
    http.verify({ ignoreCancelled: true });
    history.replaceState(null, '', '/');
  });
  async function open(groups = [familyA]) {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/home', App);
    shell(groups);
    return harness;
  }
  function shell(groups: typeof familyA[]) {
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush(groups);
  }
  function relationships(family = familyA, rows: object[] = []) {
    http.expectOne(`/api/family-groups/${family.familyGroupId}/relationships`).flush(rows);
  }
  function card(harness: RouterTestingHarness) {
    harness.detectChanges();
    return harness.routeNativeElement!.querySelector<HTMLElement>('.home-attention')!;
  }
  function select(harness: RouterTestingHarness, family: typeof familyA) {
    const selector = harness.routeNativeElement!.querySelector<HTMLSelectElement>('#current-family')!;
    selector.value = family.familyGroupId;
    selector.dispatchEvent(new Event('change'));
    harness.detectChanges();
  }

  it('loads then shows a past-dated invitation with RSVP context and the existing detail URL', async () => {
    const harness = await open();
    relationships();
    expect(card(harness).textContent).toContain('Checking pending Event invitations');
    expect(card(harness).textContent).not.toContain('No pending RSVPs or relationship suggestions');
    http.expectOne(pendingUrl()).flush([invitation]);
    const attention = card(harness);
    expect(attention.textContent).toContain('RSVP needed');
    expect(attention.textContent).toContain('2020');
    expect(attention.textContent).toContain('1 item');
    expect(attention.querySelector('a')?.getAttribute('href')).toBe(`/events/${invitation.eventId}`);
    expect(attention.querySelector('input, button')).toBeNull();
  });

  it('keeps RSVP and relationship attention together without showing premature all-clear', async () => {
    const harness = await open();
    http.expectOne(pendingUrl()).flush([invitation]);
    expect(card(harness).textContent).not.toContain('No pending RSVPs or relationship suggestions');
    relationships(familyA, [{ relationshipId: 'r', status: 'Pending', isCurrentUserRecipient: true }]);
    expect(card(harness).textContent).toContain('Relationship suggestion');
    expect(card(harness).textContent).toContain(invitation.name);
    expect(card(harness).textContent).toContain('2 items');
    expect(card(harness).textContent).not.toContain('No pending RSVPs or relationship suggestions');
    const retry = harness.routeNativeElement!.querySelector<HTMLButtonElement>('.attention-row button')!;
    expect(retry.textContent).toContain('Review in Family');
  });

  it('distinguishes errors from empty success and Retry rereads only pending invitations', async () => {
    const harness = await open();
    relationships();
    http.match(request => request.url !== pendingUrl()).forEach(request => request.flush([]));
    http.expectOne(pendingUrl()).flush({}, { status: 503, statusText: 'Unavailable' });
    expect(card(harness).querySelector('[role="alert"]')?.textContent).toContain('could not be loaded');
    expect(card(harness).textContent).not.toContain('No pending RSVPs or relationship suggestions');
    http.expectOne('/api/sharing-sessions/mine').flush([]);
    card(harness).querySelector<HTMLButtonElement>('button')!.click();
    expect(card(harness).textContent).toContain('Checking pending Event invitations');
    http.expectNone(request => request.url !== pendingUrl());
    http.expectOne(pendingUrl()).flush([invitation]);
    expect(card(harness).textContent).toContain(invitation.name);
    expect(card(harness).querySelector('[role="alert"]')).toBeNull();
  });

  it('clears prior Family attention, cancels its delayed read and loads the new selection', async () => {
    const harness = await open([familyA, familyB]);
    http.expectNone(pendingUrl());
    harness.detectChanges();
    select(harness, familyA);
    relationships();
    http.expectOne(pendingUrl()).flush([invitation]);
    expect(card(harness).textContent).toContain(invitation.name);
    // Start a second Family read, then leave before it completes.
    select(harness, familyB);
    const delayed = http.expectOne(pendingUrl(familyB));
    expect(card(harness).textContent).not.toContain(invitation.name);
    select(harness, familyA);
    expect(delayed.cancelled).toBe(true);
    relationships();
    http.expectOne(pendingUrl()).flush([]);
    expect(card(harness).textContent).toContain('No pending RSVPs or relationship suggestions');
  });

  it('rereads live state on return to Home and removes a resolved invitation', async () => {
    const harness = await open();
    relationships();
    http.expectOne(pendingUrl()).flush([invitation]);
    expect(card(harness).textContent).toContain(invitation.name);
    await harness.navigateByUrl('/me', App);
    shell([familyA]);
    http.match(() => true).forEach(request => { if (!request.cancelled) request.flush([]); });
    await harness.navigateByUrl('/home', App);
    shell([familyA]);
    relationships();
    http.expectOne(pendingUrl()).flush([]);
    expect(card(harness).textContent).not.toContain(invitation.name);
    expect(card(harness).textContent).toContain('No pending RSVPs or relationship suggestions');
  });
});
