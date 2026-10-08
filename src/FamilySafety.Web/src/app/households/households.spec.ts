import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Household, Households, HouseholdStructure } from './households';

const member = { profileId: 'member', displayName: 'Pat' };
const a: Household = { householdId: 'a', name: 'Home A', version: 'va', members: [] };
const b: Household = { householdId: 'b', name: 'Home B', version: 'vb', members: [] };
const view: HouseholdStructure = { canManage: true, households: [a, b], unassignedMembers: [member] };
const path = '/api/family-groups/family-a/households';

describe('Household organization (WI-095-010)', () => {
  let fixture: ComponentFixture<Households>;
  let http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => [...root().querySelectorAll<HTMLButtonElement>('button')].find(b => b.textContent?.trim() === label)!;
  function create() {
    fixture = TestBed.createComponent(Households);
    fixture.componentRef.setInput('familyId', 'family-a'); fixture.componentRef.setInput('ownProfileId', 'admin'); fixture.detectChanges();
  }
  function read(data: HouseholdStructure = view, family = 'family-a') {
    http.expectOne(`/api/family-groups/${family}/households`).flush(data); fixture.detectChanges();
  }
  async function name(value: string) {
    await fixture.whenStable();
    const input = root().querySelector<HTMLInputElement>('#household-name')!;
    input.value = value; input.dispatchEvent(new Event('input')); await fixture.whenStable(); fixture.detectChanges();
  }
  function assign(household: string, profile = 'member') {
    const select = root().querySelector<HTMLSelectElement>('#household-member-' + household)!;
    select.value = profile; select.dispatchEvent(new Event('change')); fixture.detectChanges();
    const row = select.closest('article')!;
    [...row.querySelectorAll<HTMLButtonElement>('button')].find(b => /^(Add|Move) Member$/.test(b.textContent!.trim()))!.click();
    fixture.detectChanges();
  }
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Households], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { fixture?.destroy(); http.verify({ ignoreCancelled: true }); });

  it('distinguishes loading/error/Retry/empty and shows unassigned members', () => {
    create(); expect(text()).toContain('Loading Households');
    http.expectOne(path).flush({}, { status: 500, statusText: 'Failed' }); fixture.detectChanges();
    expect(text()).toContain('could not be loaded'); expect(text()).not.toContain('No Households');
    button('Retry loading Households').click(); read({ ...view, households: [] });
    expect(text()).toContain('No Households in this Family yet'); expect(text()).toContain('Pat');
  });

  it('uses server-derived read-only membership visibility without admin controls', () => {
    create(); read({ canManage: false, households: [{ ...a, members: [member] }], unassignedMembers: [] });
    expect(text()).toContain('Home A'); expect(text()).toContain('Pat');
    expect(text()).toContain('Households do not change Family access or sharing permissions');
    expect(button('Create a Household')).toBeUndefined(); expect(button('Rename Household')).toBeUndefined();
    expect(root().querySelector('select')).toBeNull(); expect(root().querySelector('input')).toBeNull();
  });

  it('creates and renames with plain-text names and current optimistic versions', async () => {
    create(); read(); button('Create a Household').click(); fixture.detectChanges(); await name(' New Home ');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const createRequest = http.expectOne(path); expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body).toEqual({ name: 'New Home' }); createRequest.flush({}); read();
    expect(text()).toContain('Household created');
    button('Rename Household').click(); fixture.detectChanges(); await name('Renamed');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const rename = http.expectOne(path + '/a'); expect(rename.request.method).toBe('PUT');
    expect(rename.request.body).toEqual({ name: 'Renamed', version: 'va' }); rename.flush({});
    read({ ...view, households: [{ ...a, name: 'Renamed', version: 'va2' }, b] }); expect(text()).toContain('Household renamed');
  });

  it('assigns unassigned members and moves with both source and target versions', () => {
    create(); read(); assign('a');
    const assignRequest = http.expectOne(path + '/a/members');
    expect(assignRequest.request.body).toEqual({ version: 'va', profileId: 'member', currentHouseholdId: null, sourceVersion: null });
    assignRequest.flush(null); read({ ...view, households: [{ ...a, version: 'va2', members: [member] }, b], unassignedMembers: [] });
    assign('b'); const move = http.expectOne(path + '/b/members');
    expect(move.request.body).toEqual({ version: 'vb', profileId: 'member', currentHouseholdId: 'a', sourceVersion: 'va2' });
    move.flush(null); read({ ...view, households: [a, { ...b, version: 'vb2', members: [member] }], unassignedMembers: [] });
    expect(text()).toContain('Pat moved from Home A to Home B');
  });

  it('unassigns and confirms permanent deletion while keeping member organization', () => {
    create(); read({ ...view, households: [{ ...a, members: [member] }, b], unassignedMembers: [] });
    button('Remove from Household').click(); const remove = http.expectOne(path + '/a/members/member?version=va');
    expect(remove.request.method).toBe('DELETE'); remove.flush(null); read(); expect(text()).toContain('Pat removed from Home A');
    expect(text()).toContain('They remain in this Family and are now unassigned');
    button('Delete Household').click(); fixture.detectChanges();
    expect(text()).toContain('remain in this Family'); http.expectNone(r => r.method === 'DELETE');
    button('Cancel Household deletion').click(); fixture.detectChanges(); expect(button('Confirm Household deletion')).toBeUndefined();
    button('Delete Household').click(); fixture.detectChanges(); button('Confirm Household deletion').click();
    const deletion = http.expectOne(path + '/a?version=va'); expect(deletion.request.method).toBe('DELETE');
    deletion.flush(null); read({ ...view, households: [b] }); expect(text()).toContain('Household permanently deleted');
  });

  it('keeps a duplicate-name form editable and prevents invalid names', async () => {
    create(); read(); button('Create a Household').click(); fixture.detectChanges(); await name('');
    expect(button('Create Household').disabled).toBe(true); await name('Home A');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    http.expectOne(path).flush({ code: 'DuplicateHouseholdName' }, { status: 409, statusText: 'Conflict' }); fixture.detectChanges();
    expect(text()).toContain('already has a Household'); expect(root().querySelector('form')).not.toBeNull();
  });

  it('requires an explicit Add Member action, disables it during saving, and clears selection after the authoritative reload', async () => {
    create(); read(); await fixture.whenStable(); fixture.detectChanges();
    expect(button('Add Member').disabled).toBe(true);
    const select = root().querySelector<HTMLSelectElement>('#household-member-a')!;
    select.value = 'member'; select.dispatchEvent(new Event('change')); fixture.detectChanges();
    http.expectNone(r => r.method === 'POST'); expect(button('Add Member').disabled).toBe(false);
    button('Add Member').click(); fixture.detectChanges(); expect(button('Add Member').disabled).toBe(true);
    http.expectOne(path + '/a/members').flush(null);
    read({ ...view, households: [{ ...a, version: 'va2', members: [member] }, b], unassignedMembers: [] });
    await fixture.whenStable(); fixture.detectChanges();
    expect(text()).toContain('Pat added to Home A');
    expect(text()).toContain('All eligible Family members already belong to Home A');
    expect(root().querySelector<HTMLSelectElement>('#household-member-b')!.value).toBe('');
    expect(button('Add Member').disabled).toBe(true);
    expect(root().querySelector('#household-member-b')!.textContent).toContain('Pat (from Home A)');
  });

  it('discards selected members on Family changes without sending an assignment', async () => {
    create(); read(); await fixture.whenStable(); fixture.detectChanges();
    const select = root().querySelector<HTMLSelectElement>('#household-member-a')!;
    select.value = 'member'; select.dispatchEvent(new Event('change')); fixture.detectChanges();
    fixture.componentRef.setInput('familyId', 'family-b'); fixture.detectChanges(); read(view, 'family-b');
    await fixture.whenStable(); fixture.detectChanges();
    expect(root().querySelector<HTMLSelectElement>('#household-member-a')!.value).toBe('');
    expect(button('Add Member').disabled).toBe(true); http.expectNone(r => r.method === 'POST');
  });

  for (const status of [409, 404, 500]) {
    it(`clears stale organization on ${status} and refreshes without replaying mutations`, () => {
      create(); read(); assign('a');
      http.expectOne(path + '/a/members').flush({}, { status, statusText: 'Failed' }); fixture.detectChanges();
      expect(text()).not.toContain('Home A'); expect(root().querySelector('select')).toBeNull();
      button(status === 404 ? 'Refresh Households' : 'Retry loading Households').click(); read();
      http.expectNone(r => r.method === 'POST');
    });
  }

  it('cancels pending reads/writes and clears drafts, confirmations and feedback on Family/identity changes', async () => {
    create(); const oldRead = http.expectOne(path);
    fixture.componentRef.setInput('familyId', 'family-b'); fixture.detectChanges(); expect(oldRead.cancelled).toBe(true);
    read(view, 'family-b'); button('Create a Household').click(); fixture.detectChanges(); await name('Draft');
    root().querySelector('form')!.dispatchEvent(new Event('submit')); const oldWrite = http.expectOne('/api/family-groups/family-b/households');
    fixture.componentRef.setInput('ownProfileId', 'different-profile'); fixture.detectChanges(); expect(oldWrite.cancelled).toBe(true);
    expect(root().querySelector('form')).toBeNull(); expect(text()).not.toContain('Home A');
    read({ ...view, canManage: false }, 'family-b'); expect(button('Create a Household')).toBeUndefined();
    fixture.componentRef.setInput('ownProfileId', null); fixture.detectChanges(); expect(text()).not.toContain('Home A');
  });
});
