import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MemberProfile } from './member-profile';
const family = 'aaaaaaaa-0000-4000-8000-000000000001';
const member = 'aaaaaaaa-0000-4000-8000-000000000002';
const shared = { profileId: member, displayName: 'Shared Adult', contactEmail: 'shared@example.invalid', contactPhone: null, birthdayMonth: null, birthdayDay: null };
describe('Authorized member profile', () => {
  let http: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] }); http = TestBed.inject(HttpTestingController); });
  afterEach(() => http.verify({ ignoreCancelled: true }));
  function open() {
    const fixture = TestBed.createComponent(MemberProfile);
    const adopt = vi.fn().mockResolvedValue('current');
    fixture.componentRef.setInput('familyId', family); fixture.componentRef.setInput('profileId', member); fixture.componentRef.setInput('adoptFamily', adopt); fixture.detectChanges();
    return { fixture, adopt };
  }
  it('authorizes directory and projection before adopting context and renders only returned fields', async () => {
    const { fixture, adopt } = open();
    expect(adopt).not.toHaveBeenCalled();
    http.expectOne('/api/family-groups/' + family + '/members').flush([{ profileId: member }]);
    expect(adopt).not.toHaveBeenCalled();
    http.expectOne('/api/family-members/' + member + '/profile').flush(shared);
    await fixture.whenStable(); fixture.detectChanges();
    expect(adopt).toHaveBeenCalledWith(family, 'member', expect.any(Function));
    expect(fixture.nativeElement.textContent).toContain('shared@example.invalid');
    expect(fixture.nativeElement.textContent).not.toContain('Birthday');
    expect(fixture.nativeElement.querySelector('input, form')).toBeNull();
    expect(fixture.nativeElement.querySelector('a').getAttribute('href')).toBe('/family');
  });
  it('does not read a member outside the requested directory or adopt their Family', () => {
    const { fixture, adopt } = open(); http.expectOne('/api/family-groups/' + family + '/members').flush([]); fixture.detectChanges();
    http.expectNone('/api/family-members/' + member + '/profile'); expect(adopt).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('This family member is unavailable.');
  });
  it('uses the same nondisclosing result for denied projection and missing member', () => {
    const { fixture, adopt } = open(); http.expectOne('/api/family-groups/' + family + '/members').flush([{ profileId: member }]);
    http.expectOne('/api/family-members/' + member + '/profile').flush({}, { status: 404, statusText: 'Not Found' }); fixture.detectChanges();
    expect(adopt).not.toHaveBeenCalled(); expect(fixture.nativeElement.textContent).toContain('This family member is unavailable.');
  });
  it('rejects malformed deep links without reading a resource', () => {
    const fixture = TestBed.createComponent(MemberProfile); fixture.componentRef.setInput('familyId', 'invalid'); fixture.componentRef.setInput('profileId', member); fixture.componentRef.setInput('adoptFamily', vi.fn()); fixture.detectChanges();
    http.expectNone(() => true); expect(fixture.nativeElement.textContent).toContain('unavailable');
  });
  it('withholds profile data when membership is no longer active after the read', async () => {
    const { fixture, adopt } = open(); adopt.mockResolvedValue('unavailable');
    http.expectOne('/api/family-groups/' + family + '/members').flush([{ profileId: member }]); http.expectOne('/api/family-members/' + member + '/profile').flush(shared);
    await fixture.whenStable(); fixture.detectChanges(); expect(fixture.nativeElement.textContent).not.toContain(shared.contactEmail);
  });
  it('cancels pending reads on destruction', () => {
    const { fixture, adopt } = open(); const req = http.expectOne('/api/family-groups/' + family + '/members'); fixture.destroy(); expect(req.cancelled).toBe(true); expect(adopt).not.toHaveBeenCalled();
  });
  it('invalidates a pending Family adoption when the member view is destroyed', async () => {
    const { fixture, adopt } = open();
    let resolveAdoption!: (value: string) => void;
    adopt.mockImplementation(() => new Promise(resolve => { resolveAdoption = resolve; }));
    http.expectOne('/api/family-groups/' + family + '/members').flush([{ profileId: member }]);
    http.expectOne('/api/family-members/' + member + '/profile').flush(shared);
    const isCurrent = adopt.mock.calls[0][2] as () => boolean;
    expect(isCurrent()).toBe(true);
    fixture.destroy();
    expect(isCurrent()).toBe(false);
    resolveAdoption('current');
    await Promise.resolve();
  });
});
