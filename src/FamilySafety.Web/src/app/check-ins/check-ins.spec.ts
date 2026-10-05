import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CheckIns, CheckInView } from './check-ins';

const entry: CheckInView = { checkInId: 'one', familyGroupId: 'family', authorProfileId: 'author', authorDisplayName: 'Avery',
  createdAtUtc: '2026-10-04T12:00:00Z', status: 'Arrived', note: '<b>Plain note</b>', recipientProfileIds: ['person'] };
describe('Standalone Check-Ins (WI-095-005)', () => {
  let fixture: ComponentFixture<CheckIns>;
  let http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => [...root().querySelectorAll('button')].find(b => b.textContent?.trim() === label)!;
  function create(family: string | null = 'family') {
    fixture = TestBed.createComponent(CheckIns);
    fixture.componentRef.setInput('ownProfileId', 'author'); fixture.componentRef.setInput('familyId', family); fixture.detectChanges();
  }
  function lists(mine: CheckInView[] = [], received: CheckInView[] = []) {
    http.expectOne('/api/check-ins/mine').flush(mine); http.expectOne('/api/check-ins/received').flush(received); fixture.detectChanges();
  }
  function people() {
    http.expectOne('/api/family-groups/family/members').flush([{ profileId: 'author', displayName: 'Avery' }, { profileId: 'person', displayName: 'Pat' }]); fixture.detectChanges();
  }
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [CheckIns], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });
  it('shows loading, explicit empty histories, error and Retry', () => {
    create(null); expect(text()).toContain('Loading Check-Ins');
    const oldReceived = http.expectOne('/api/check-ins/received');
    http.expectOne('/api/check-ins/mine').flush({}, { status: 500, statusText: 'Failure' });
    expect(oldReceived.cancelled).toBe(true);
    fixture.detectChanges(); expect(text()).toContain('could not be loaded'); expect(text()).not.toContain('You have no Check-Ins');
    button('Retry loading Check-Ins').click(); lists();
    expect(text()).toContain('You have no Check-Ins yet'); expect(text()).toContain('No Check-Ins are shared with you');
    expect(button('Create a Check-In')).toBeUndefined();
  });
  it('creates with only explicitly selected recipients and a canonical status/note', async () => {
    create(); lists(); people(); button('Create a Check-In').click(); fixture.detectChanges(); await fixture.whenStable();
    const select = root().querySelector('select')!;
    expect([...select.options].slice(1).map(o => o.textContent)).toEqual(["I'm okay", 'Arrived', 'Leaving now', 'Delayed', 'Plans changed']);
    const checkbox = root().querySelector<HTMLInputElement>('input[type=checkbox]')!;
    expect(checkbox.checked).toBe(false); expect(button('Create Check-In').disabled).toBe(true);
    select.value = 'PlansChanged'; select.dispatchEvent(new Event('change'));
    const note = root().querySelector('textarea')!; note.value = ' Changed plans '; note.dispatchEvent(new Event('input'));
    checkbox.click(); await fixture.whenStable(); fixture.detectChanges();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const request = http.expectOne('/api/check-ins');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ familyGroupId: 'family', status: 'PlansChanged', note: 'Changed plans', recipientProfileIds: ['person'] });
    request.flush(entry); lists([entry]); people();
    expect(text()).toContain('Check-In created'); expect(root().querySelector('form')).toBeNull();
  });
  it('separates sent/shared history, renders notes as text and has no mutation controls', () => {
    create(null); lists([entry], [{ ...entry, checkInId: 'two', authorDisplayName: 'Pat', recipientProfileIds: null }]);
    const sections = [...root().querySelectorAll('section[aria-labelledby]')];
    const sent = sections.find(s => s.querySelector('h3')?.textContent === 'Sent by me')!;
    const shared = sections.find(s => s.querySelector('h3')?.textContent === 'Shared with me')!;
    expect(sent.textContent).toContain('1 explicitly selected recipients'); expect(shared.textContent).toContain('Pat');
    expect(shared.textContent).not.toContain('selected recipients'); expect(shared.querySelector('b')).toBeNull();
    expect(shared.textContent).toContain('<b>Plain note</b>');
    expect(button('Edit')).toBeUndefined(); expect(button('Delete')).toBeUndefined();
  });
  it('does not infer creation eligibility from selected Family and handles recipient Retry', () => {
    create(); lists(); http.expectOne('/api/family-groups/family/members').flush({}, { status: 500, statusText: 'Failed' }); fixture.detectChanges();
    expect(button('Create a Check-In')).toBeUndefined(); button('Retry loading recipients').click();
    http.expectOne('/api/family-groups/family/members').flush([{ profileId: 'person', displayName: 'Pat' }]); fixture.detectChanges();
    expect(button('Create a Check-In')).toBeUndefined(); expect(text()).toContain('cannot create');
  });
  it('clears drafts/content and cancels old reads when identity or Family context changes', () => {
    create(); const mine = http.expectOne('/api/check-ins/mine'); const received = http.expectOne('/api/check-ins/received');
    const members = http.expectOne('/api/family-groups/family/members');
    fixture.componentRef.setInput('ownProfileId', null); fixture.detectChanges();
    expect(mine.cancelled).toBe(true); expect(received.cancelled).toBe(true); expect(members.cancelled).toBe(true);
    expect(text()).not.toContain('Plain note'); expect(text()).toContain('unavailable');
    fixture.componentRef.setInput('ownProfileId', 'author'); fixture.componentRef.setInput('familyId', null); fixture.detectChanges(); lists([entry]);
    expect(text()).toContain('Plain note'); expect(button('Create a Check-In')).toBeUndefined();
  });
});
