import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EventUpdate, EventUpdates, EventUpdatesView } from './event-updates';

const event = { eventId: 'event-a', familyGroupId: 'family-a', organizerProfileId: 'organizer' };
const update: EventUpdate = { updateId: 'update-a', authorProfileId: 'organizer', authorName: 'Adult A', text: 'Bring a coat',
  createdAtUtc: '2026-10-04T12:00:00Z', updatedAtUtc: null, version: 'v1' };
const view: EventUpdatesView = { canManage: false, updates: [update] };

describe('Event Updates (WI-095-003)', () => {
  let fixture: ComponentFixture<EventUpdates>;
  let http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => Array.from(root().querySelectorAll<HTMLButtonElement>('button')).find(b => b.textContent?.trim() === label)!;
  function create() {
    fixture = TestBed.createComponent(EventUpdates);
    fixture.componentRef.setInput('event', event); fixture.componentRef.setInput('ownProfileId', 'viewer');
    fixture.componentRef.setInput('currentFamilyId', event.familyGroupId); fixture.detectChanges();
  }
  function read(data = view) { http.expectOne('/api/events/event-a/updates').flush(data); fixture.detectChanges(); }
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [EventUpdates], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });

  it('distinguishes loading, failure, Retry and empty for read-only viewers', () => {
    create(); expect(text()).toContain('Loading Updates');
    http.expectOne('/api/events/event-a/updates').flush({}, { status: 500, statusText: 'Failed' }); fixture.detectChanges();
    expect(text()).not.toContain('No Updates'); button('Retry loading Updates').click(); read({ ...view, updates: [] });
    expect(text()).toContain('No Updates have been posted'); expect(root().querySelector('form')).toBeNull();
  });
  it('preserves server chronological order, author/times and escaped plain text without viewer actions', () => {
    create(); read({ ...view, updates: [update, { ...update, updateId: 'b', text: '<b>Later message</b>',
      createdAtUtc: '2026-10-04T13:00:00Z', updatedAtUtc: '2026-10-04T14:00:00Z' }] });
    expect([...root().querySelectorAll('.update-text')].map(p => p.textContent)).toEqual(['Bring a coat', '<b>Later message</b>']);
    expect(text()).toContain('Adult A'); expect(root().querySelectorAll('time')).toHaveLength(3); expect(text()).toContain('Edited');
    expect(root().querySelector('.update-text b')).toBeNull(); expect(button('Edit Update')).toBeUndefined();
    expect(button('Hide Update')).toBeUndefined(); expect(root().querySelector('form')).toBeNull();
  });
  it('posts and edits with server management authority and versioned requests', async () => {
    create(); read({ ...view, canManage: true }); await fixture.whenStable();
    const input = root().querySelector<HTMLTextAreaElement>('#update-text')!;
    input.value = ' New message '; input.dispatchEvent(new Event('input')); await fixture.whenStable(); fixture.detectChanges();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const post = http.expectOne('/api/events/event-a/updates'); expect(post.request.method).toBe('POST');
    expect(post.request.body).toEqual({ text: 'New message' }); post.flush({}); read({ ...view, canManage: true });
    expect(text()).toContain('Update posted'); button('Edit Update').click(); fixture.detectChanges(); await fixture.whenStable();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const edit = http.expectOne('/api/events/event-a/updates/update-a'); expect(edit.request.method).toBe('PUT');
    expect(edit.request.body).toEqual({ text: 'Bring a coat', version: 'v1' }); edit.flush({});
    read({ ...view, canManage: true, updates: [{ ...update, text: 'Edited text', updatedAtUtc: '2026-10-04T14:00:00Z' }] });
    expect(text()).toContain('Update saved'); expect(text()).toContain('Edited text');
  });
  it('requires confirmation to hide and removes the hidden item after a server refresh', () => {
    create(); read({ ...view, canManage: true }); button('Hide Update').click(); fixture.detectChanges();
    http.expectNone(request => request.method === 'POST'); expect(text()).toContain('cannot be restored here');
    button('Cancel').click(); fixture.detectChanges(); expect(button('Confirm hide')).toBeUndefined();
    button('Hide Update').click(); fixture.detectChanges(); button('Confirm hide').click();
    const hide = http.expectOne('/api/events/event-a/updates/update-a/hide'); expect(hide.request.body).toEqual({ version: 'v1' });
    hide.flush(null); read({ ...view, canManage: true, updates: [] }); expect(text()).toContain('Update hidden');
    expect(text()).not.toContain('Bring a coat');
  });
  it('removes a newly hidden item from a normal viewer refresh', () => {
    create(); read(); expect(text()).toContain('Bring a coat'); button('Refresh Updates').click();
    read({ ...view, updates: [] }); expect(text()).not.toContain('Bring a coat'); expect(root().querySelector('form')).toBeNull();
  });
  for (const status of [409, 500, 404]) {
    it(`requires refresh after ${status}, clears denied content and never replays a write`, async () => {
      create(); read({ ...view, canManage: true }); button('Edit Update').click(); fixture.detectChanges(); await fixture.whenStable();
      root().querySelector('form')!.dispatchEvent(new Event('submit'));
      http.expectOne('/api/events/event-a/updates/update-a').flush({}, { status, statusText: 'Failed' }); fixture.detectChanges();
      if (status === 404) { expect(text()).not.toContain('Bring a coat'); expect(root().querySelector('form')).toBeNull(); }
      else { expect(button('Save Update').disabled).toBe(true); expect(text()).toContain(status === 409 ? 'This Update changed' : 'could not be saved'); }
      button('Refresh Updates').click(); read(); http.expectNone(request => request.method === 'PUT');
    });
  }
  it('keeps validation errors editable and preserves draft text', async () => {
    create(); read({ ...view, canManage: true }); button('Edit Update').click(); fixture.detectChanges(); await fixture.whenStable();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    http.expectOne('/api/events/event-a/updates/update-a').flush({}, { status: 400, statusText: 'Invalid' }); fixture.detectChanges();
    expect(text()).toContain('up to 1,000 characters'); expect(button('Save Update').disabled).toBe(false);
    expect(root().querySelector<HTMLTextAreaElement>('#update-text')!.value).toBe('Bring a coat');
  });
  it('cancels delayed reads/writes and clears text when Event, profile or selected Family changes', async () => {
    create(); const oldRead = http.expectOne('/api/events/event-a/updates');
    fixture.componentRef.setInput('event', { ...event, eventId: 'event-b' }); fixture.detectChanges(); expect(oldRead.cancelled).toBe(true);
    http.expectOne('/api/events/event-b/updates').flush({ ...view, canManage: true }); fixture.detectChanges();
    button('Edit Update').click(); fixture.detectChanges(); await fixture.whenStable(); root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const oldWrite = http.expectOne('/api/events/event-b/updates/update-a');
    fixture.componentRef.setInput('ownProfileId', 'other'); fixture.detectChanges(); expect(oldWrite.cancelled).toBe(true);
    const otherRead = http.expectOne('/api/events/event-b/updates');
    fixture.componentRef.setInput('currentFamilyId', 'family-b'); fixture.detectChanges(); expect(otherRead.cancelled).toBe(true);
    expect(text()).not.toContain('Bring a coat'); expect(root().querySelector('form')).toBeNull(); http.expectNone('/api/events/event-b/updates');
  });
});
