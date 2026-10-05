import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SharedFamilyDate, SharedFamilyDates, SharedFamilyDatesView } from './shared-family-dates';

const entry: SharedFamilyDate = { dateId: 'date-a', creatorProfileId: 'creator', title: 'Reunion', description: 'A specific date',
  date: '2028-02-29', associatedPeople: [{ profileId: 'participant', displayName: 'Pat' }],
  canEdit: true, canManageAssociations: false, canDelete: false, version: 'v1' };
const view: SharedFamilyDatesView = { canCreate: true, dates: [entry] };
const path = '/api/family-groups/family-a/dates';
describe('Shared Family Dates (WI-095-004)', () => {
  let fixture: ComponentFixture<SharedFamilyDates>;
  let http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => [...root().querySelectorAll<HTMLButtonElement>('button')].find(b => b.textContent?.trim() === label)!;
  function create() {
    fixture = TestBed.createComponent(SharedFamilyDates);
    fixture.componentRef.setInput('familyId', 'family-a'); fixture.componentRef.setInput('ownProfileId', 'creator'); fixture.detectChanges();
  }
  function read(data = view, family = 'family-a') {
    http.expectOne(`/api/family-groups/${family}/dates`).flush(data);
    fixture.detectChanges();
  }
  function people(family = 'family-a') {
    http.expectOne(`/api/family-groups/${family}/members`).flush([{ profileId: 'creator', displayName: 'Chris' }, ...entry.associatedPeople]);
    fixture.detectChanges();
  }
  async function fill(id: string, value: string) {
    const input = root().querySelector<HTMLInputElement>(`#family-date-${id}`)!;
    input.value = value; input.dispatchEvent(new Event('input')); await fixture.whenStable(); fixture.detectChanges();
  }
  const creatorView = () => ({ ...view, dates: [{ ...entry, canManageAssociations: true, canDelete: true }] });
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [SharedFamilyDates], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });
  it('shows loading, Retry, and empty rather than pretending a failed read is empty', () => {
    create(); expect(text()).toContain('Loading shared dates');
    http.expectOne(path).flush({}, { status: 500, statusText: 'Failed' });
    fixture.detectChanges();
    expect(text()).toContain('could not be loaded'); expect(text()).not.toContain('No shared dates');
    button('Retry loading dates').click(); read({ canCreate: true, dates: [] });
    expect(text()).toContain('No shared dates are available to you');
  });
  it('displays the exact calendar date with year and server-derived participant actions', () => {
    create(); read(); expect(text()).toContain('Reunion'); expect(text()).toContain('Pat'); expect(text()).toContain('Feb 29, 2028');
    expect(root().querySelector('time')?.getAttribute('datetime')).toBe('2028-02-29');
    expect(button('Edit date')).toBeDefined(); expect(button('Delete date')).toBeUndefined();
    button('Edit date').click(); fixture.detectChanges(); expect(root().querySelector('fieldset')).toBeNull();
    expect(text()).toContain('Only the creator');
  });
  it('creates with explicit selected associations and date-only fields', async () => {
    create(); read({ canCreate: true, dates: [] }); button('Add a date').click(); fixture.detectChanges(); people(); await fixture.whenStable();
    await fill('title', ' New date '); await fill('description', ' Family detail '); await fill('value', '2028-02-29');
    const checkbox = root().querySelector<HTMLInputElement>('input[type="checkbox"]')!; checkbox.click(); fixture.detectChanges();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const request = http.expectOne(path); expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ title: 'New date', description: 'Family detail', date: '2028-02-29', associatedProfileIds: ['participant'] });
    request.flush({}); read(creatorView()); expect(text()).toContain('Date created');
  });
  it('edits participant content with Version while omitting association changes', async () => {
    create(); read(); button('Edit date').click(); fixture.detectChanges(); await fixture.whenStable();
    await fill('title', 'Changed'); await fill('value', '2028-03-01');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const request = http.expectOne(`${path}/date-a`); expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ title: 'Changed', description: entry.description, date: '2028-03-01', version: 'v1' });
    request.flush({}); read({ ...view, dates: [{ ...entry, title: 'Changed', date: '2028-03-01' }] });
    expect(text()).toContain('Date saved'); expect(text()).toContain('Mar 1, 2028');
  });
  it('allows creator association edits, requiring explicit selection', async () => {
    create(); read(creatorView()); button('Edit date').click(); fixture.detectChanges(); people(); await fixture.whenStable();
    root().querySelector<HTMLInputElement>('input[type="checkbox"]')!.click(); fixture.detectChanges();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const request = http.expectOne(`${path}/date-a`); expect(request.request.body.associatedProfileIds).toEqual([]);
    request.flush({}); read(creatorView());
  });
  it('requires inline delete confirmation, supports cancel, and removes deleted content', () => {
    create(); read(creatorView()); button('Delete date').click(); fixture.detectChanges();
    expect(text()).toContain('disappear immediately for everyone'); expect(text()).toContain('cannot restore');
    http.expectNone(r => r.method === 'DELETE'); button('Cancel deletion').click(); fixture.detectChanges();
    expect(button('Confirm delete')).toBeUndefined(); button('Delete date').click(); fixture.detectChanges(); button('Confirm delete').click();
    const request = http.expectOne(`${path}/date-a?version=v1`); expect(request.request.method).toBe('DELETE');
    request.flush(null); read({ canCreate: true, dates: [] }); expect(text()).toContain('Date deleted'); expect(text()).not.toContain('Reunion');
  });
  for (const status of [409, 404, 500]) {
    it(`handles ${status} without replaying edits and clears content on denied access`, async () => {
      create(); read(); button('Edit date').click(); fixture.detectChanges(); await fixture.whenStable();
      root().querySelector('form')!.dispatchEvent(new Event('submit'));
      http.expectOne(`${path}/date-a`).flush({}, { status, statusText: 'Failed' }); fixture.detectChanges();
      if (status === 404) { expect(text()).not.toContain('Reunion'); expect(root().querySelector('form')).toBeNull(); }
      else expect(button('Save date').disabled).toBe(true);
      button('Refresh dates').click(); read(); http.expectNone(r => r.method === 'PUT');
    });
  }
  it('cancels pending reads/writes and clears content/drafts on Family and identity changes', async () => {
    create(); const oldRead = http.expectOne(path);
    fixture.componentRef.setInput('familyId', 'family-b'); fixture.detectChanges(); expect(oldRead.cancelled).toBe(true);
    read(creatorView(), 'family-b'); button('Edit date').click(); fixture.detectChanges(); people('family-b'); await fixture.whenStable();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const oldWrite = http.expectOne('/api/family-groups/family-b/dates/date-a');
    fixture.componentRef.setInput('ownProfileId', null); fixture.detectChanges(); expect(oldWrite.cancelled).toBe(true);
    expect(text()).not.toContain('Reunion'); expect(root().querySelector('form')).toBeNull();
  });
  it('does not infer creation or editing authority from selected Family or own identity', () => {
    create(); read({ canCreate: false, dates: [{ ...entry, canEdit: false }] });
    expect(button('Add a date')).toBeUndefined(); expect(button('Edit date')).toBeUndefined(); expect(button('Delete date')).toBeUndefined();
  });
  it('loads member choices only for creator forms and requires Retry before saving after a member-list failure', async () => {
    create(); read({ canCreate: true, dates: [] }); http.expectNone('/api/family-groups/family-a/members');
    button('Add a date').click(); fixture.detectChanges();
    http.expectOne('/api/family-groups/family-a/members').flush({}, { status: 500, statusText: 'Failed' }); fixture.detectChanges();
    await fixture.whenStable();
    await fill('title', 'Date'); await fill('description', 'Description'); await fill('value', '2028-02-29');
    expect(button('Create date').disabled).toBe(true); button('Retry loading member choices').click(); people();
    expect(button('Create date').disabled).toBe(false);
  });
});
