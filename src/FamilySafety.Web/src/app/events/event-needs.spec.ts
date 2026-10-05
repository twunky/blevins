import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EventNeed, EventNeeds, EventNeedsView } from './event-needs';

const event = { eventId: 'event-a', familyGroupId: 'family-a', organizerProfileId: 'organizer' };
const need: EventNeed = { needId: 'need-a', title: 'Bring chairs', detail: 'Folding chairs', category: 'Supplies', status: 'Open',
  responsibleProfileId: null, responsibleName: null, isResponsible: false, version: 'v1', history: [] };
const view: EventNeedsView = { canManage: false, canParticipate: true, eligibleParticipants: [], needs: [need] };

describe('Event Needs (WI-095-002)', () => {
  let fixture: ComponentFixture<EventNeeds>;
  let http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => Array.from(root().querySelectorAll<HTMLButtonElement>('button')).find(b => b.textContent?.trim() === label)!;
  function create() {
    fixture = TestBed.createComponent(EventNeeds);
    fixture.componentRef.setInput('event', event); fixture.componentRef.setInput('ownProfileId', 'invitee');
    fixture.componentRef.setInput('currentFamilyId', event.familyGroupId); fixture.detectChanges();
  }
  function read(data = view) { http.expectOne('/api/events/event-a/needs').flush(data); fixture.detectChanges(); }
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [EventNeeds], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });

  it('distinguishes loading, failure, Retry and empty without management', () => {
    create(); expect(text()).toContain('Loading Needs');
    http.expectOne('/api/events/event-a/needs').flush({}, { status: 500, statusText: 'Failed' }); fixture.detectChanges();
    expect(text()).not.toContain('No Needs'); button('Retry loading Needs').click(); read({ ...view, needs: [] });
    expect(text()).toContain('No Needs have been added'); expect(root().querySelector('form')).toBeNull();
  });

  it('renders anonymous responsibility state and pending acceptance without personal or editor actions', () => {
    create(); read({ ...view, needs: [{ ...need, status: 'Assigned' }] });
    expect(text()).toContain('Assigned'); expect(text()).toContain('Acceptance pending');
    expect(button('Accept assignment')).toBeUndefined(); expect(button('Edit Need')).toBeUndefined();
    expect(button('Claim Need')).toBeUndefined(); expect(root().querySelector('details')).toBeNull();
  });

  it('uses server management authority and presents organizer-approved names', () => {
    create(); read({ ...view, canManage: true, needs: [{ ...need, status: 'Claimed', responsibleProfileId: 'other', responsibleName: 'Adult B' }] });
    expect(text()).toContain('Adult B'); expect(button('Edit Need')).toBeDefined(); expect(button('Complete Need')).toBeDefined();
    expect(button('Release Need')).toBeUndefined();
  });

  it('claims using the version and refreshes server state with feedback', () => {
    create(); read(); button('Claim Need').click(); fixture.detectChanges();
    expect(button('Claim Need').disabled).toBe(true);
    const request = http.expectOne('/api/events/event-a/needs/need-a/claim'); expect(request.request.body).toEqual({ version: 'v1' });
    request.flush({}); read({ ...view, needs: [{ ...need, status: 'Claimed', isResponsible: true }] });
    expect(text()).toContain('You claimed this Need'); expect(button('Release Need')).toBeDefined(); expect(button('Complete Need')).toBeDefined();
  });

  for (const action of ['Accept assignment', 'Decline assignment']) {
    it(`supports the personal pending action ${action}`, () => {
      create(); read({ ...view, needs: [{ ...need, status: 'Assigned', isResponsible: true }] });
      expect(text()).toContain('You have a pending assignment'); button(action).click();
      const request = http.expectOne(`/api/events/event-a/needs/need-a/${action.startsWith('Accept') ? 'accept' : 'decline'}`);
      request.flush({}); read();
    });
  }

  it('creates, edits, assigns and confirms removal using protected versioned requests', async () => {
    create(); read({ ...view, canManage: true, eligibleParticipants: [{ profileId: 'person', displayName: 'Adult B' }] });
    await fixture.whenStable();
    const input = root().querySelector<HTMLInputElement>('#need-title')!;
    input.value = 'New Need'; input.dispatchEvent(new Event('input')); await fixture.whenStable(); fixture.detectChanges();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const createRequest = http.expectOne('/api/events/event-a/needs');
    expect(createRequest.request.body).toEqual({ title: 'New Need', detail: null, category: null }); createRequest.flush({});
    read({ ...view, canManage: true }); button('Edit Need').click(); fixture.detectChanges(); await fixture.whenStable();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const editRequest = http.expectOne('/api/events/event-a/needs/need-a'); expect(editRequest.request.body.version).toBe('v1'); editRequest.flush({});
    read({ ...view, canManage: true, eligibleParticipants: [{ profileId: 'person', displayName: 'Adult B' }] });
    const select = root().querySelector<HTMLSelectElement>('#assignee-need-a')!;
    select.value = 'person'; select.dispatchEvent(new Event('change')); await fixture.whenStable(); fixture.detectChanges();
    button('Request assignment').click();
    const assign = http.expectOne('/api/events/event-a/needs/need-a/assign'); expect(assign.request.body).toEqual({ version: 'v1', profileId: 'person' });
    assign.flush({}); read({ ...view, canManage: true }); expect(text()).toContain('Acceptance is pending');
    button('Remove Need').click(); fixture.detectChanges(); http.expectNone(request => request.method === 'POST');
    expect(text()).toContain('responsibility history?'); button('Confirm remove').click();
    http.expectOne('/api/events/event-a/needs/need-a/remove').flush(null); read({ ...view, canManage: true, needs: [] });
    expect(text()).toContain('Need removed');
  });

  for (const status of [409, 500, 404]) {
    it(`requires refresh after ${status} and never replays a mutation`, () => {
      create(); read(); button('Claim Need').click();
      http.expectOne('/api/events/event-a/needs/need-a/claim').flush({}, { status, statusText: 'Failed' }); fixture.detectChanges();
      if (status === 404) { expect(text()).not.toContain('Bring chairs'); expect(button('Claim Need')).toBeUndefined(); }
      else expect(button('Claim Need').disabled).toBe(true);
      button('Refresh Needs').click(); read(); http.expectNone(request => request.method === 'POST');
    });
  }

  it('clears sensitive state and cancels delayed reads and writes on Event, profile or Family changes', () => {
    create(); const oldRead = http.expectOne('/api/events/event-a/needs');
    fixture.componentRef.setInput('event', { ...event, eventId: 'event-b' }); fixture.detectChanges(); expect(oldRead.cancelled).toBe(true);
    http.expectOne('/api/events/event-b/needs').flush(view); fixture.detectChanges(); button('Claim Need').click();
    const oldWrite = http.expectOne('/api/events/event-b/needs/need-a/claim');
    fixture.componentRef.setInput('ownProfileId', 'other'); fixture.detectChanges(); expect(oldWrite.cancelled).toBe(true);
    const otherRead = http.expectOne('/api/events/event-b/needs');
    fixture.componentRef.setInput('currentFamilyId', 'family-b'); fixture.detectChanges(); expect(otherRead.cancelled).toBe(true);
    expect(text()).not.toContain('Bring chairs'); expect(root().querySelector('form')).toBeNull();
    http.expectNone('/api/events/event-b/needs');
  });
});
