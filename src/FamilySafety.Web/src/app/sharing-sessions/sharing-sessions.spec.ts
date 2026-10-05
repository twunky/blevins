import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SharingSessions, SharingSessionView } from './sharing-sessions';

describe('Temporary Sharing Sessions (WI-095-006)', () => {
  let fixture: ComponentFixture<SharingSessions>;
  let http: HttpTestingController;
  let position: PositionCallback;
  let failure: PositionErrorCallback;
  let clearWatch: ReturnType<typeof vi.fn>;
  let originalLocation: PropertyDescriptor | undefined;
  let now: number;
  const root = () => fixture.nativeElement as HTMLElement;
  const text = () => root().textContent!;
  const button = (label: string) => [...root().querySelectorAll('button')].find(b => b.textContent?.trim() === label)!;
  const row = (): SharingSessionView => ({ sharingSessionId: 'one', familyGroupId: 'family', authorProfileId: 'author', authorDisplayName: 'Avery',
    startedAtUtc: new Date(now).toISOString(), endsAtUtc: new Date(now + 3600000).toISOString(), state: 'Active', version: 1,
    recipients: [{ profileId: 'person', displayName: 'Pat', hasAccess: true }], location: null });
  const point = (latitude = 42) => position({ coords: { latitude, longitude: -72, accuracy: 12 }, timestamp: now } as GeolocationPosition);
  function create(summaryOnly = false) {
    fixture = TestBed.createComponent(SharingSessions);
    fixture.componentRef.setInput('ownProfileId', 'author'); fixture.componentRef.setInput('familyId', 'family');
    fixture.componentRef.setInput('summaryOnly', summaryOnly); fixture.detectChanges();
  }
  function lists(mine: SharingSessionView[] = [], received: SharingSessionView[] = [], summary = false) {
    http.expectOne('/api/sharing-sessions/mine').flush(mine);
    if (!summary) http.expectOne('/api/sharing-sessions/received').flush(received);
    fixture.detectChanges();
  }
  function people() {
    http.expectOne('/api/family-groups/family/members').flush([{ profileId: 'author', displayName: 'Avery' }, { profileId: 'person', displayName: 'Pat' }]); fixture.detectChanges();
  }
  beforeEach(async () => {
    now = Date.now(); vi.spyOn(Date, 'now').mockImplementation(() => now);
    clearWatch = vi.fn();
    originalLocation = Object.getOwnPropertyDescriptor(navigator, 'geolocation');
    Object.defineProperty(navigator, 'geolocation', { configurable: true, value: {
      watchPosition: vi.fn((success: PositionCallback, error: PositionErrorCallback) => { position = success; failure = error; return 7; }), clearWatch,
    } });
    await TestBed.configureTestingModule({ imports: [SharingSessions], providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    fixture?.destroy(); http.verify({ ignoreCancelled: true }); vi.restoreAllMocks();
    if (originalLocation) Object.defineProperty(navigator, 'geolocation', originalLocation);
    else Reflect.deleteProperty(navigator, 'geolocation');
  });

  it('requires explicit recipients and end time, starts own sharing and requests permission', async () => {
    create(); lists(); people(); button('Start location sharing').click(); fixture.detectChanges(); await fixture.whenStable();
    const checkbox = root().querySelector<HTMLInputElement>('input[type=checkbox]')!;
    expect(checkbox.checked).toBe(false); expect(button('Start sharing and request location').disabled).toBe(true);
    expect(root().querySelector<HTMLInputElement>('input[type=datetime-local]')!.value).not.toBe('');
    checkbox.click(); await fixture.whenStable(); fixture.detectChanges();
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    const request = http.expectOne('/api/sharing-sessions');
    expect(request.request.body.recipientProfileIds).toEqual(['person']);
    expect(request.request.body.familyGroupId).toBe('family'); expect(request.request.body.authorProfileId).toBeUndefined();
    expect(request.request.body.endsAtUtc).toBeNull(); // Server materializes an explicit end exactly one hour from start.
    request.flush(row()); fixture.detectChanges(); expect(text()).toContain('Requesting browser location permission'); expect(text()).toContain('Location not yet available');
    failure({ code: 1 } as GeolocationPositionError); fixture.detectChanges();
    expect(text()).toContain('Location permission denied'); expect(text()).toContain('Sharing until'); expect(clearWatch).toHaveBeenCalledWith(7);
  });

  it('replaces current point and throttles browser events to one minute', () => {
    create(); lists([row()]); people(); button('Enable location updates').click();
    point();
    const request = http.expectOne('/api/sharing-sessions/one/location');
    expect(request.request.body).toEqual({ latitude: 42, longitude: -72, accuracy: 12, capturedAtUtc: new Date(now).toISOString(), version: 1 });
    const updated = { ...row(), version: 2, location: { latitude: 42, longitude: -72, accuracy: 12, capturedAtUtc: new Date(now).toISOString(), receivedAtUtc: new Date(now).toISOString() } };
    request.flush(updated); fixture.detectChanges(); expect(text()).toContain('Location permission granted'); expect(text()).toContain('Latitude 42.0000');
    point(43); http.expectNone('/api/sharing-sessions/one/location');
    now += 60000; point(43);
    const second = http.expectOne('/api/sharing-sessions/one/location'); expect(second.request.body.version).toBe(2);
    second.flush({ ...updated, version: 3, location: { ...updated.location, latitude: 43, capturedAtUtc: new Date(now).toISOString() } });
    fixture.detectChanges(); expect(text()).toContain('Latitude 43.0000'); expect(text()).not.toContain('Latitude 42.0000');
    button('Stop sharing').click(); const stop = http.expectOne('/api/sharing-sessions/one/stop'); expect(stop.request.body.version).toBe(3);
    stop.flush(null); lists(); expect(text()).toContain('Sharing stopped'); expect(text()).not.toContain('Latitude 43'); expect(clearWatch).toHaveBeenCalledWith(7);
  });

  it('shows unavailable and transient failures without inventing points or ending the session', () => {
    create(); lists([row()]); people(); button('Enable location updates').click();
    failure({ code: 2 } as GeolocationPositionError); fixture.detectChanges(); expect(text()).toContain('Location unavailable');
    failure({ code: 3 } as GeolocationPositionError); fixture.detectChanges(); expect(text()).toContain('temporarily failed');
    http.expectNone('/api/sharing-sessions/one/location'); expect(text()).toContain('Location not yet available');
  });

  it('renders recipient latest location/freshness and removes it at expiration without waiting for poll', () => {
    const shared = { ...row(), recipients: null, location: { latitude: 42, longitude: -72, accuracy: 12, capturedAtUtc: new Date(now - 180000).toISOString(), receivedAtUtc: new Date(now).toISOString() } };
    create(); lists([], [shared]); people();
    expect(text()).toContain('Location has not updated recently'); expect(text()).toContain('Last updated'); expect(text()).not.toContain('Pat');
    now = Date.parse(shared.endsAtUtc);
    (fixture.componentInstance as unknown as { expire(): void }).expire(); fixture.detectChanges();
    expect(text()).not.toContain('Latitude 42'); expect(text()).toContain('No active locations');
  });

  it('clears coordinates and watchers on read failure, identity/Family changes and destruction', () => {
    create(); lists([row()]); people(); button('Enable location updates').click(); button('Refresh sharing').click();
    http.expectOne('/api/sharing-sessions/mine').flush({}, { status: 500, statusText: 'Failed' });
    http.match('/api/sharing-sessions/received'); fixture.detectChanges();
    expect(text()).toContain('could not be loaded'); expect(clearWatch).toHaveBeenCalled();
    fixture.componentRef.setInput('ownProfileId', null); fixture.detectChanges(); expect(text()).toContain('unavailable');
  });

  it('Home indicates sharing and expiration without requesting recipients or collecting location', () => {
    create(true); lists([row()], [], true);
    expect(text()).toContain('Sharing your current / last-reported location until'); expect(text()).toContain('1 explicitly selected recipients');
    expect(navigator.geolocation.watchPosition).not.toHaveBeenCalled(); expect(root().querySelector('a')!.getAttribute('href')).toContain('/safety');
  });
  it('Home leaves the no-Family entry path unchanged when there is no active sharing', () => {
    create(true); lists([], [], true);
    expect(root().querySelector('section')).toBeNull(); expect(root().querySelector('button')).toBeNull();
  });
});
