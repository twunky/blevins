import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from '../app';
import { routes } from '../app.routes';
import { GroupLanding } from './group-landing';

const profile = {
  profileId: 'person', displayName: 'Alex', contactEmail: null,
  contactEmailVisibility: 'OnlyMe', contactPhone: null, contactPhoneVisibility: 'OnlyMe',
  birthdayMonth: null, birthdayDay: null, birthdayVisibility: 'OnlyMe',
};
const family = { familyGroupId: 'family-a', name: 'Family A' };

/** The reads the shell makes on every page; a landing adds nothing to them. */
const shellReads = [
  '/api/profile', '/api/family-groups/administered', '/api/family-groups/mine',
  `/api/family-groups/${family.familyGroupId}/members`, `/api/family-groups/${family.familyGroupId}/relationships`,
  `/api/family-groups/${family.familyGroupId}/events`, `/api/family-groups/${family.familyGroupId}/plans`,
];

/*
 * DEC-079 (WI-009-009) supersedes WI-009-003 D1 and AC-15 for /safety only: /safety is now the read-only Safety
 * dashboard (safety-dashboard.spec.ts), so the Safety landing cases that stood here were removed together with the
 * landing's Safety branch. The Plans & Trips landing keeps every WI-009-003 guarantee below, unchanged.
 */
describe('Plans & Trips landing (WI-009-003 D1)', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    history.replaceState(null, '', '/');
    await TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.match(() => true).forEach(request => { if (!request.cancelled) request.flush([]); });
    http.verify({ ignoreCancelled: true });
    history.replaceState(null, '', '/');
  });

  function render(group: 'plansTrips') {
    const fixture = TestBed.createComponent(GroupLanding);
    fixture.componentRef.setInput('group', group);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it.each([
    ['plansTrips', [['Plans', 'Open Plans', '/plans'], ['Trips', 'Open Trips', '/trips']]],
  ] as const)('renders the %s landing as two headed cards, each with one link, and makes no request', (group, expected) => {
    const element = render(group);
    const cards = Array.from(element.querySelectorAll('section.card'));
    expect(cards.map(card => {
      const link = card.querySelector('a')!;
      return [card.querySelector('h2')?.textContent?.trim(), link.textContent?.trim(), link.getAttribute('href')];
    })).toEqual(expected);
    expect(cards.every(card => card.querySelectorAll('a').length === 1)).toBe(true);
    expect(element.querySelectorAll('button')).toHaveLength(0);
    expect(element.textContent).not.toMatch(/\d/);
    // No state, emptiness, or existence wording beyond the link names themselves.
    expect((element.textContent ?? '').replace(/Open (Plans|Trips)/g, ''))
      .not.toMatch(/\b(open|frozen|resolved|nothing|none|empty|shared with you)\b/i);
    http.expectNone(() => true);
  });

  it.each([
    ['/plans-trips', 'Plans & Trips', 'Choose Plans or Trips.'],
  ])('routes %s to one h1 and the landing, with no reads beyond the shell (AC-15)', async (url, heading, subtitle) => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url, App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([family]);
    harness.detectChanges();
    await harness.fixture.whenStable();
    harness.detectChanges();
    const requested = http.match(() => true).map(request => request.request.url);
    expect(requested.sort()).toEqual(shellReads.slice(3).sort());
    expect(requested.some(path => /^\/api\/(concerns|incidents|trips)/.test(path))).toBe(false);
    const root = harness.routeNativeElement!;
    expect(TestBed.inject(Router).url).toBe(url);
    expect(Array.from(root.querySelectorAll('h1')).map(h1 => h1.textContent?.trim())).toEqual([heading]);
    expect(root.querySelector('.page-title p')?.textContent?.trim()).toBe(subtitle);
    expect(root.querySelectorAll('app-group-landing section.card')).toHaveLength(2);
    expect(root.querySelector('[aria-label="Family context required"]')).toBeNull();
  });

  it('shows the landing without a selected Family, since it shows no Family data', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/plans-trips', App);
    http.expectOne('/api/profile').flush(profile);
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([family, { familyGroupId: 'family-b', name: 'Family B' }]);
    harness.detectChanges();
    http.expectNone(() => true);
    const root = harness.routeNativeElement!;
    expect(root.querySelectorAll('app-group-landing a')).toHaveLength(2);
    expect(root.querySelector('[aria-label="Family context required"]')).toBeNull();
  });
});
