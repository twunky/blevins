import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SafetyCircles, SafetyCircleView } from './safety-circles';
const url = '/api/family-groups/family/safety-circles';
const row: SafetyCircleView = { safetyCircleId: 'one', familyGroupId: 'family', name: 'Close family', version: 'v1', members: [] };
describe('Safety Circle management (WI-095-008)', () => {
  let fixture: ComponentFixture<SafetyCircles>, http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const button = (s: string) => [...root().querySelectorAll('button')].find(b => b.textContent?.trim() === s)!;
  const text = () => root().textContent!;
  function start() { fixture = TestBed.createComponent(SafetyCircles); fixture.componentRef.setInput('familyId', 'family'); fixture.componentRef.setInput('ownProfileId', 'owner'); fixture.detectChanges(); }
  function load(rows: SafetyCircleView[] = []) {
    http.expectOne(url).flush(rows); http.expectOne('/api/family-groups/family/members').flush([{ profileId: 'owner', displayName: 'Owner' }, { profileId: 'member', displayName: 'Member' }]); fixture.detectChanges();
  }
  async function name(value: string) { const input = root().querySelector('input')!; input.value = value; input.dispatchEvent(new Event('input')); await fixture.whenStable(); fixture.detectChanges(); }
  beforeEach(async () => { await TestBed.configureTestingModule({ imports: [SafetyCircles], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents(); http = TestBed.inject(HttpTestingController); });
  afterEach(() => { fixture.destroy(); http.verify({ ignoreCancelled: true }); });
  it('shows loading, error and Retry, then honest empty state', () => {
    start(); expect(text()).toContain('Loading Safety Circles');
    const people = http.expectOne('/api/family-groups/family/members'); http.expectOne(url).flush({}, { status: 500, statusText: 'Failed' }); expect(people.cancelled).toBe(true);
    fixture.detectChanges(); expect(text()).toContain('could not be loaded'); button('Retry loading Safety Circles').click(); load(); expect(text()).toContain('no Safety Circles');
  });
  it('creates, renames, adds/removes members and confirms permanent deletion with versions', async () => {
    start(); load(); button('Create a Safety Circle').click(); fixture.detectChanges(); await fixture.whenStable(); await name(' Close family ');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    let req = http.expectOne(url); expect(req.request.body).toEqual({ name: 'Close family' }); req.flush(row); load([row]); expect(text()).toContain('Safety Circle created');
    button('Rename circle').click(); fixture.detectChanges(); await fixture.whenStable(); await name('Renamed'); root().querySelector('form')!.dispatchEvent(new Event('submit'));
    req = http.expectOne(url + '/one'); expect(req.request.body).toEqual({ name: 'Renamed', version: 'v1' }); const renamed = { ...row, name: 'Renamed', version: 'v2' }; req.flush(renamed); load([renamed]);
    const select = root().querySelector('select')!; select.value = 'member'; select.dispatchEvent(new Event('change'));
    req = http.expectOne(url + '/one/members'); expect(req.request.body).toEqual({ version: 'v2', memberProfileId: 'member' }); const member = { ...renamed, version: 'v3', members: [{ profileId: 'member', displayName: 'Member' }] }; req.flush(member); load([member]);
    button('Remove member').click(); req = http.expectOne(url + '/one/members/member?version=v3'); expect(req.request.method).toBe('DELETE'); req.flush({ ...renamed, version: 'v4' }); load([{ ...renamed, version: 'v4' }]);
    button('Delete circle').click(); fixture.detectChanges(); http.expectNone(url + '/one?version=v4'); expect(text()).toContain('Existing Check-Ins and Sharing Sessions keep their own recipients');
    button('Confirm circle deletion').click(); req = http.expectOne(url + '/one?version=v4'); req.flush(null); load(); expect(text()).toContain('permanently deleted');
  });
  it('preserves duplicate-name feedback, discards state on conflict and cancels Family/identity reads', async () => {
    start(); load([row]); button('Create a Safety Circle').click(); fixture.detectChanges(); await fixture.whenStable(); await name('Close family'); root().querySelector('form')!.dispatchEvent(new Event('submit'));
    http.expectOne(url).flush({ code: 'DuplicateCircleName' }, { status: 409, statusText: 'Conflict' }); fixture.detectChanges(); expect(text()).toContain('already have'); button('Cancel circle edit').click(); fixture.detectChanges();
    button('Rename circle').click(); fixture.detectChanges(); await fixture.whenStable(); await name('New'); root().querySelector('form')!.dispatchEvent(new Event('submit'));
    http.expectOne(url + '/one').flush({ code: 'CircleChanged' }, { status: 409, statusText: 'Conflict' }); fixture.detectChanges(); expect(root().querySelector('article')).toBeNull(); button('Retry loading Safety Circles').click();
    const pending = http.expectOne(url), people = http.expectOne('/api/family-groups/family/members'); fixture.componentRef.setInput('ownProfileId', null); fixture.detectChanges();
    expect(pending.cancelled).toBe(true); expect(people.cancelled).toBe(true); expect(text()).not.toContain('Close family');
  });
});
