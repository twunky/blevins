import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CheckIns } from '../check-ins/check-ins';
import { SharingSessions } from '../sharing-sessions/sharing-sessions';
import { CircleRecipientPreset } from './circle-recipient-preset';
const url = '/api/family-groups/family/safety-circles';
const circle = { safetyCircleId: 'circle', familyGroupId: 'family', name: 'Close family', version: 'v1', members: [{ profileId: 'person', displayName: 'Pat' }, { profileId: 'author', displayName: 'Author' }] };
describe('Safety Circle recipient presets (WI-095-008)', () => {
  let fixture: ComponentFixture<any>, http: HttpTestingController;
  const root = () => fixture.nativeElement as HTMLElement;
  const button = (s: string) => [...root().querySelectorAll('button')].find(b => b.textContent?.trim() === s)!;
  beforeEach(async () => { await TestBed.configureTestingModule({ imports: [CheckIns, SharingSessions, CircleRecipientPreset], providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] }).compileComponents(); http = TestBed.inject(HttpTestingController); });
  afterEach(() => { fixture.destroy(); http.verify({ ignoreCancelled: true }); });
  for (const kind of ['check-in', 'session']) it('populates visible adjustable ' + kind + ' recipients and submits only explicit grants', async () => {
    fixture = TestBed.createComponent(kind === 'check-in' ? CheckIns : SharingSessions as any);
    fixture.componentRef.setInput('familyId', 'family'); fixture.componentRef.setInput('ownProfileId', 'author'); fixture.detectChanges();
    const base = kind === 'check-in' ? '/api/check-ins' : '/api/sharing-sessions';
    http.expectOne(base + '/mine').flush([]); http.expectOne(base + '/received').flush([]);
    http.expectOne('/api/family-groups/family/members').flush([{ profileId: 'author', displayName: 'Author' }, { profileId: 'person', displayName: 'Pat' }, { profileId: 'other', displayName: 'Other' }]); fixture.detectChanges();
    button(kind === 'check-in' ? 'Create a Check-In' : 'Start location sharing').click(); fixture.detectChanges(); await fixture.whenStable();
    button('Use a Safety Circle (optional)').click(); http.expectOne(url).flush([circle]); fixture.detectChanges();
    const preset = root().querySelector('app-circle-recipient-preset select') as HTMLSelectElement; preset.value = 'circle'; preset.dispatchEvent(new Event('change'));
    fixture.detectChanges(); expect((root().querySelector('fieldset') as HTMLFieldSetElement).disabled).toBe(true);
    http.expectOne(url + '/circle').flush(circle); fixture.detectChanges(); await fixture.whenStable(); fixture.detectChanges();
    expect((root().querySelector('app-circle-recipient-preset select') as HTMLSelectElement).value).toBe('circle');
    const boxes = [...root().querySelectorAll<HTMLInputElement>('input[type=checkbox]')]; expect(boxes.map(b => b.checked)).toEqual([true, false]);
    boxes[0].click(); boxes[1].click();
    if (kind === 'check-in') { const status = root().querySelector<HTMLSelectElement>('#check-in-status')!; status.value = 'Arrived'; status.dispatchEvent(new Event('change')); }
    await fixture.whenStable(); fixture.detectChanges(); root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const req = http.expectOne(base); expect(req.request.body.recipientProfileIds).toEqual(['other']); expect(req.request.body.safetyCircleId).toBeUndefined();
    req.flush({}, { status: 400, statusText: 'Test end' }); // No geolocation collection for this form test.
    if (kind === 'session') { http.expectOne(base + '/mine').flush([]); http.expectOne(base + '/received').flush([]); }
  });
  it('allows manual selection after preset read failure and cancels data on context change', () => {
    fixture = TestBed.createComponent(CircleRecipientPreset); fixture.componentRef.setInput('familyId', 'family'); fixture.componentRef.setInput('ownProfileId', 'author'); fixture.detectChanges();
    button('Use a Safety Circle (optional)').click(); http.expectOne(url).flush({}, { status: 500, statusText: 'Failed' }); fixture.detectChanges(); expect(root().textContent).toContain('choose recipients manually');
    button('Use a Safety Circle (optional)').click(); const req = http.expectOne(url); fixture.componentRef.setInput('familyId', 'second'); fixture.detectChanges(); expect(req.cancelled).toBe(true); expect(root().querySelector('select')).toBeNull();
  });
});
