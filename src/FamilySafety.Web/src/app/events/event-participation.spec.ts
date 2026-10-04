import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EventInvitation, EventParticipation } from './event-participation';

const event = { eventId: 'event-a', familyGroupId: 'family-a', organizerProfileId: 'organizer' };
const invitation: EventInvitation = { invitationId: 'invitation-a', inviteeProfileId: 'invitee', createdAtUtc: '2026-10-04T12:00:00Z', revokedAtUtc: null, rsvpStatus: 'Pending', rsvpUpdatedAtUtc: null, version: 'v1' };
const organizerData = { eligibleViewers: [{ profileId: 'invitee', displayName: 'Adult B' }, { profileId: 'organizer', displayName: 'Organizer' }], invitations: [] as { invitation: EventInvitation; displayName: string }[] };

describe('Event participation (WI-095-001)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<EventParticipation>;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => Array.from(root().querySelectorAll<HTMLButtonElement>('button')).find(b => b.textContent?.trim() === label)!;
  function create(profile = 'invitee') {
    fixture = TestBed.createComponent(EventParticipation);
    fixture.componentRef.setInput('event', event);
    fixture.componentRef.setInput('ownProfileId', profile);
    fixture.componentRef.setInput('currentFamilyId', event.familyGroupId);
    fixture.detectChanges();
  }
  function own(row: EventInvitation | null = invitation) {
    http.expectOne('/api/events/event-a/invitation/mine').flush({ invitation: row });
    fixture.detectChanges();
  }
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [EventParticipation], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });

  it('distinguishes loading and empty own invitation without organizer controls', () => {
    create('editor');
    expect(text()).toContain('Loading invitations');
    own(null);
    expect(text()).toContain('no active invitation');
    expect(root().querySelector('input, button')).toBeNull();
    expect(text()).not.toContain('Adult B');
  });

  it('loads only organizer data, excludes self and presents eligible Invite actions', () => {
    create('organizer');
    http.expectOne('/api/events/event-a/invitations').flush(organizerData);
    fixture.detectChanges();
    expect(root().querySelectorAll('button')).toHaveLength(1);
    expect(root().querySelector('button')?.getAttribute('aria-label')).toBe('Invite Adult B');
    expect(text()).not.toContain('Organizer');
    button('Invite').click();
    const request = http.expectOne('/api/events/event-a/invitations');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ inviteeProfileId: 'invitee' });
    request.flush(invitation);
    http.expectOne('/api/events/event-a/invitations').flush({ ...organizerData, invitations: [{ invitation, displayName: 'Adult B' }] });
    fixture.detectChanges();
    expect(text()).toContain('Invitation issued');
    expect(root().querySelector('[aria-label="Invite Adult B"]')).toBeNull();
    expect(text()).toContain('Active · Pending');
  });

  it('shows named current RSVP and requires confirmation to revoke while retaining history', () => {
    create('organizer');
    const going = { ...invitation, rsvpStatus: 'Going' };
    http.expectOne('/api/events/event-a/invitations').flush({ ...organizerData, invitations: [{ invitation: going, displayName: 'Adult B' }] });
    fixture.detectChanges();
    expect(text()).toContain('Active · Going');
    button('Revoke').click(); fixture.detectChanges();
    http.expectNone(request => request.method === 'POST');
    expect(text()).toContain('final RSVP stays');
    button('Cancel').click(); fixture.detectChanges();
    expect(button('Confirm revoke')).toBeUndefined();
    button('Revoke').click(); fixture.detectChanges();
    button('Confirm revoke').click();
    const request = http.expectOne('/api/events/event-a/invitations/invitation-a/revoke');
    expect(request.request.body).toEqual({ version: 'v1' });
    const revoked = { ...going, revokedAtUtc: '2026-10-04T13:00:00Z', version: 'v2' };
    request.flush(revoked);
    http.expectOne('/api/events/event-a/invitations').flush({ ...organizerData, invitations: [{ invitation: revoked, displayName: 'Adult B' }] });
    fixture.detectChanges();
    expect(text()).toContain('Revoked · Going');
    expect(button('Revoke')).toBeUndefined();
    expect(button('Invite')).toBeDefined();
  });

  it('shows accessible RSVP choices and uses the returned state, version and success feedback', () => {
    create(); own();
    expect(root().querySelector('legend')?.textContent).toBe('Your RSVP');
    expect(Array.from(root().querySelectorAll('label')).map(label => label.textContent?.trim())).toEqual(['Pending', 'Going', 'Maybe', 'Not Going']);
    const radio = root().querySelector<HTMLInputElement>('input[value="Going"]')!;
    radio.click();
    button('Save RSVP').click(); fixture.detectChanges();
    expect(button('Save RSVP').disabled).toBe(true);
    const request = http.expectOne('/api/events/event-a/invitation/mine/rsvp');
    expect(request.request.body).toEqual({ invitationId: 'invitation-a', version: 'v1', rsvpStatus: 'Going' });
    request.flush({ ...invitation, rsvpStatus: 'Going', version: 'v2', rsvpUpdatedAtUtc: '2026-10-04T13:00:00Z' });
    fixture.detectChanges();
    expect(text()).toContain('Current RSVP: Going');
    expect(text()).toContain('Your RSVP was saved');
    expect(text()).not.toContain('not submitted');
  });

  it('allows deliberate Pending and later changes through every state', () => {
    create(); own();
    let version = 'v1';
    for (const state of ['Pending', 'Maybe', 'NotGoing', 'Going']) {
      root().querySelector<HTMLInputElement>(`input[value="${state}"]`)!.click();
      button('Save RSVP').click();
      const request = http.expectOne('/api/events/event-a/invitation/mine/rsvp');
      expect(request.request.body.version).toBe(version);
      expect(request.request.body.rsvpStatus).toBe(state);
      version += '-next';
      request.flush({ ...invitation, rsvpStatus: state, version, rsvpUpdatedAtUtc: '2026-10-04T13:00:00Z' });
      fixture.detectChanges();
    }
  });

  for (const status of [409, 500]) {
    it(`requires authorized refresh after ${status} without replaying the mutation`, () => {
      create(); own(); button('Save RSVP').click();
      http.expectOne('/api/events/event-a/invitation/mine/rsvp').flush({}, { status, statusText: 'Failed' });
      fixture.detectChanges();
      expect(root().querySelector('[role="alert"]')).not.toBeNull();
      expect(button('Save RSVP').disabled).toBe(true);
      button('Refresh invitations').click();
      own({ ...invitation, version: 'new', rsvpStatus: 'Maybe' });
      expect(text()).toContain('Current RSVP: Maybe');
      http.expectNone(request => request.method === 'PUT');
    });
  }

  it('clears invitation data and actions after mutation authorization loss or revocation', () => {
    create(); own(); button('Save RSVP').click();
    http.expectOne('/api/events/event-a/invitation/mine/rsvp').flush(null, { status: 404, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(root().querySelector('input')).toBeNull();
    button('Refresh invitations').click(); own(null);
    expect(text()).toContain('no active invitation');
  });

  it('distinguishes failed read from empty and Retry repeats that protected read', () => {
    create();
    http.expectOne('/api/events/event-a/invitation/mine').flush({}, { status: 500, statusText: 'Failed' });
    fixture.detectChanges();
    expect(text()).not.toContain('no active invitation');
    button('Retry loading invitations').click(); own(null);
    expect(text()).toContain('no active invitation');
  });

  it('shows uniform unavailable and no named data for denied reads', () => {
    create('unrelated');
    http.expectOne('/api/events/event-a/invitation/mine').flush(null, { status: 404, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(text()).toContain('currently unavailable');
    expect(root().querySelector('input')).toBeNull();
    expect(text()).not.toContain('Adult B');
  });

  it('cancels delayed reads on Event and identity changes', () => {
    create(); const stale = http.expectOne('/api/events/event-a/invitation/mine');
    fixture.componentRef.setInput('event', { ...event, eventId: 'event-b' });
    fixture.componentRef.setInput('ownProfileId', 'editor');
    fixture.detectChanges();
    expect(stale.cancelled).toBe(true);
    http.expectOne('/api/events/event-b/invitation/mine').flush({ invitation: null });
    fixture.detectChanges(); expect(text()).toContain('no active invitation');
  });

  it('clears and cancels pending writes on Family change, without adopting the Event Family', () => {
    create(); own(); button('Save RSVP').click();
    const stale = http.expectOne('/api/events/event-a/invitation/mine/rsvp');
    fixture.componentRef.setInput('currentFamilyId', 'family-b'); fixture.detectChanges();
    expect(stale.cancelled).toBe(true);
    expect(text()).not.toContain('Current RSVP');
    expect(root().querySelector('input')).toBeNull();
    http.expectNone('/api/events/event-a/invitation/mine');
  });
});
