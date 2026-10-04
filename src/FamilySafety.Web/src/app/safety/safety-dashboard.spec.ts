import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { App } from '../app';
import { routes } from '../app.routes';
import { Concern, ConcernState } from '../concerns/concern-api';
import { IncidentState, IncidentSummary } from '../incidents/incident-api';
import { SafetyDashboard } from './safety-dashboard';

/*
 * WI-009-009 / DEC-079: the read-only Safety dashboard at /safety. It summarizes only the Open rows the authorized
 * list reads return, in separate Concern and Incident cards, and shows no reason, lineage, workspace detail,
 * hidden count, Check-In data, or safety verdict. Server authorization itself is proven by the API tests and
 * e2e/safety-dashboard.spec.ts; these tests prove the client never adds to what the server returned.
 */

const me = 'me-profile';
const reason = 'SENTINEL-REASON-TEXT';
const entryText = 'SENTINEL-ENTRY-TEXT';
const at = (hoursAgo: number) => new Date(Date.UTC(2026, 9, 3, 12) - hoursAgo * 3600e3).toISOString();

function concern(id: string, subjectName: string, state: ConcernState, options: Partial<Concern> = {}): Concern {
  return {
    concernId: id, familyGroupId: 'family-a', familyGroupName: 'Family A', creatorProfileId: me, creatorName: 'Me',
    subjectProfileId: `subject-${id}`, subjectName, creationReason: reason, state, createdAt: at(48),
    lastAuthorizedMutationAt: at(1), frozenAt: null, freezeReason: null, resolvedAt: null, resolutionNote: null,
    expiresAt: null, participants: [{ profileId: 'p', displayName: 'Participant Person' }],
    entries: [{
      entryId: 'e', kind: 'Observation', category: 'Reported', text: entryText, sourceDescription: 'SENTINEL-SOURCE',
      sourceUnknown: false, contributorProfileId: me, contributorName: 'Me', recordedAt: at(2), eventAt: null,
      relatedEntryId: null, explanation: null, isOutdated: false,
    }],
    ...options,
  };
}

function incident(id: string, subjectName: string, state: IncidentState, options: Partial<IncidentSummary> = {}): IncidentSummary {
  return {
    incidentId: id, familyGroupId: 'family-a', familyGroupName: 'Family A', subjectProfileId: `subject-${id}`, subjectName,
    state, coordinatorProfileId: 'coordinator', coordinatorName: 'Pat Coordinator', createdAt: at(30),
    lastAuthorizedMutationAt: at(2), expiresAt: null, ...options,
  };
}

/** Workspace-only fields a server could add; the summary card must never render them. */
const incidentWorkspaceExtras = {
  originatingConcernId: 'SENTINEL-LINEAGE-ID', entries: [{ kind: 'ContactAttempt', text: 'SENTINEL-CONTACT-ATTEMPT' }],
  tasks: [{ description: 'SENTINEL-TASK' }], timeline: [{ actorName: 'SENTINEL-TIMELINE' }],
};

const forbiddenWording = /all clear|everyone is safe|nothing to worry|is safe\b|in danger|unsafe|monitoring|emergency|overdue|missed/i;

describe('SafetyDashboard (WI-009-009)', () => {
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

  function render(concerns: unknown[] | 'error', incidents: unknown[] | 'error', showFamilyName = false) {
    const fixture = TestBed.createComponent(SafetyDashboard);
    fixture.componentRef.setInput('ownProfileId', me);
    fixture.componentRef.setInput('showFamilyName', showFamilyName);
    fixture.detectChanges();
    const respond = (url: string, body: unknown[] | 'error') => {
      const request = http.expectOne(url);
      if (body === 'error') request.flush({}, { status: 500, statusText: 'Server Error' });
      else request.flush(body);
    };
    respond('/api/concerns', concerns);
    respond('/api/incidents', incidents);
    http.expectNone(() => true);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    const card = (heading: string) => Array.from(root.querySelectorAll('section.card'))
      .find(section => section.querySelector('h2')?.textContent?.trim() === heading) as HTMLElement;
    return { fixture, root, card };
  }

  const rowTitles = (card: HTMLElement) => Array.from(card.querySelectorAll('.safety-rows li a.item-link')).map(a => a.textContent?.trim());

  it('summarizes only Open Concerns and only Open Incidents, in separate cards', () => {
    const { card, root } = render(
      [concern('c1', 'Avery', 'Open'), concern('c2', 'Blake', 'Frozen'), concern('c3', 'Casey', 'Resolved')],
      [incident('i1', 'Drew', 'Open'), incident('i2', 'Ellis', 'Frozen'), incident('i3', 'Finley', 'Resolved')],
    );
    const concerns = card('Open Concerns');
    const incidents = card('Open Safety Incidents');
    expect(concerns).not.toBe(incidents);
    expect(rowTitles(concerns)).toEqual(['Concern about Avery']);
    expect(rowTitles(incidents)).toEqual(['Incident about Drew']);
    expect(concerns.textContent).not.toContain('Drew');
    expect(incidents.textContent).not.toContain('Avery');
    for (const ended of ['Blake', 'Casey', 'Ellis', 'Finley']) expect(root.textContent).not.toContain(ended);
    expect(root.textContent).not.toMatch(/\b(Frozen|Resolved)\b/);
    // No combined list or timeline: each card owns its own list.
    expect(root.querySelectorAll('ul.safety-rows')).toHaveLength(2);
    expect(root.querySelector('ol, time + time')).toBeNull();
  });

  it('never renders the Concern reason, entries, sources, or participants (O10)', () => {
    const { root } = render([concern('c1', 'Avery', 'Open')], []);
    for (const hidden of [reason, entryText, 'SENTINEL-SOURCE', 'Participant Person']) expect(root.textContent).not.toContain(hidden);
  });

  it('never renders Incident lineage, contact attempts, tasks, or timeline content', () => {
    const { root } = render([], [{ ...incident('i1', 'Drew', 'Open'), ...incidentWorkspaceExtras }]);
    expect(root.textContent).toContain('Incident about Drew');
    for (const hidden of ['SENTINEL-LINEAGE-ID', 'SENTINEL-CONTACT-ATTEMPT', 'SENTINEL-TASK', 'SENTINEL-TIMELINE']) {
      expect(root.textContent).not.toContain(hidden);
    }
    expect(root.textContent).toContain('Contact attempts, tasks, and timelines remain inside each Incident.');
  });

  it('shows the existing role and coordinator lines and last activity', () => {
    const { card } = render(
      [concern('c1', 'Avery', 'Open'), concern('c2', 'Blake', 'Open', { creatorProfileId: 'someone-else' })],
      [incident('i1', 'Drew', 'Open', { coordinatorProfileId: me }), incident('i2', 'Ellis', 'Open')],
    );
    expect(card('Open Concerns').textContent).toContain('Created by you');
    expect(card('Open Concerns').textContent).toContain('You are a selected participant');
    expect(card('Open Safety Incidents').textContent).toContain('You coordinate this Incident');
    expect(card('Open Safety Incidents').textContent).toContain('Coordinated by Pat Coordinator');
    expect(card('Open Concerns').querySelector('time')?.getAttribute('datetime')).toBe(at(1));
  });

  it('labels rows with their Family only when the caller has more than one Family', () => {
    const rows = [concern('c1', 'Avery', 'Open'), concern('c2', 'Blake', 'Open', { familyGroupId: 'family-b', familyGroupName: 'Family B' })];
    const multi = render(rows, [incident('i1', 'Drew', 'Open', { familyGroupName: 'Family B' })], true);
    expect(multi.card('Open Concerns').textContent).toContain('Family A');
    expect(multi.card('Open Concerns').textContent).toContain('Family B');
    expect(multi.card('Open Safety Incidents').textContent).toContain('Family B');
    multi.fixture.destroy();

    const single = render([concern('c1', 'Avery', 'Open')], [incident('i1', 'Drew', 'Open')], false);
    expect(single.root.textContent).not.toContain('Family A');
  });

  it('links rows to their item URLs and View all to the full lists', () => {
    const { card } = render([concern('c1', 'Avery', 'Open')], [incident('i1', 'Drew', 'Open')]);
    expect(card('Open Concerns').querySelector('.safety-rows a')?.getAttribute('href')).toBe('/concerns/c1');
    expect(card('Open Safety Incidents').querySelector('.safety-rows a')?.getAttribute('href')).toBe('/incidents/i1');
    const viewAll = (heading: string) => card(heading).querySelector('a.safety-view-all');
    expect([viewAll('Open Concerns')?.getAttribute('href'), viewAll('Open Concerns')?.getAttribute('aria-label')]).toEqual(['/concerns', 'View all Concerns']);
    expect([viewAll('Open Safety Incidents')?.getAttribute('href'), viewAll('Open Safety Incidents')?.getAttribute('aria-label')])
      .toEqual(['/incidents', 'View all Safety Incidents']);
  });

  it('uses neutral empty states and no safety-verdict, monitoring, or emergency wording', () => {
    const { card, root } = render([concern('c2', 'Blake', 'Resolved')], [incident('i2', 'Ellis', 'Frozen')]);
    expect(card('Open Concerns').querySelector('.safety-empty')?.textContent?.trim()).toBe('No open Concerns are shared with you.');
    expect(card('Open Safety Incidents').querySelector('.safety-empty')?.textContent?.trim()).toBe('No open Safety Incidents are shared with you.');
    expect(root.textContent).not.toMatch(forbiddenWording);
    expect(root.querySelectorAll('.safety-state')).toHaveLength(0);
  });

  it('uses only the Open state pill, never Monitoring or another invented state', () => {
    const { root } = render([concern('c1', 'Avery', 'Open')], [incident('i1', 'Drew', 'Open')]);
    expect(Array.from(root.querySelectorAll('.safety-state')).map(pill => pill.textContent?.trim())).toEqual(['Open', 'Open']);
    expect(root.textContent).not.toMatch(forbiddenWording);
  });

  it('offers no create, resolve, contact, settings, or emergency control (O6)', () => {
    const { root } = render([concern('c1', 'Avery', 'Open')], [incident('i1', 'Drew', 'Open')]);
    expect(root.querySelectorAll('button, form, input, select, textarea')).toHaveLength(0);
    const linkTargets = Array.from(root.querySelectorAll('a')).map(a => a.getAttribute('href'));
    expect(linkTargets.sort()).toEqual(['/concerns', '/concerns/c1', '/incidents', '/incidents/i1']);
    expect(root.textContent).not.toMatch(/create a|start an incident|resolve|\bcall\b|send message|settings|emergency/i);
  });

  it('shows the exact Check-Ins placeholder with no data, control, or request (O7)', () => {
    const { card } = render([], []);
    const placeholder = card('Recent Check-Ins');
    expect(placeholder.querySelector('.safety-future-note')?.textContent?.trim()).toBe('Check-ins shared with you will appear here when available.');
    expect(placeholder.querySelectorAll('a, button, input, ul, li, img, time')).toHaveLength(0);
    expect(placeholder.textContent).not.toMatch(/\d/);
    // render() already proved the only requests were the two list reads.
  });

  it('includes the guidance and the canonical safety boundary (O8)', () => {
    const { card } = render([], []);
    const guidance = card('About Concerns and Incidents');
    expect(guidance.textContent).toContain('Keep uncertainty and shared information clear while coordinating with selected people.');
    expect(guidance.textContent).toContain('Coordinate a deliberate family response with the people you choose.');
    expect(guidance.textContent).toContain('Neither one proves danger or safety.');
  });

  it('keeps Concern and Incident failures independent, each with its own retry', () => {
    const { card } = render('error', [incident('i1', 'Drew', 'Open')]);
    expect(card('Open Concerns').querySelector('[role="alert"]')?.textContent).toContain('Concerns could not be loaded');
    expect(card('Open Concerns').querySelector('button')?.textContent?.trim()).toBe('Retry loading Concerns');
    expect(rowTitles(card('Open Safety Incidents'))).toEqual(['Incident about Drew']);
    expect(card('Open Safety Incidents').querySelector('[role="alert"]')).toBeNull();

    card('Open Concerns').querySelector('button')!.click();
    http.expectOne('/api/concerns').flush([concern('c1', 'Avery', 'Open')]);
  });

  it('shows at most three rows per card and says more exist without revealing how many (O13)', () => {
    const concerns = ['A1', 'B2', 'C3', 'D4', 'E5', 'F6', 'G7'].map((name, index) =>
      concern(`c${index}`, `Person ${name}`, 'Open', { lastAuthorizedMutationAt: at(index) }));
    const incidents = ['H8', 'I9', 'J10', 'K11'].map((name, index) =>
      incident(`i${index}`, `Person ${name}`, 'Open', { lastAuthorizedMutationAt: at(index) }));
    const { card } = render(concerns, incidents);
    expect(rowTitles(card('Open Concerns'))).toEqual(['Concern about Person A1', 'Concern about Person B2', 'Concern about Person C3']);
    expect(card('Open Concerns').querySelector('.safety-more')?.textContent?.trim()).toBe('Additional open Concerns are available.');
    expect(card('Open Safety Incidents').querySelector('.safety-more')?.textContent?.trim()).toBe('Additional open Safety Incidents are available.');
    for (const heading of ['Open Concerns', 'Open Safety Incidents']) {
      const text = Array.from(card(heading).querySelectorAll('.card-header, .safety-more')).map(el => el.textContent).join(' ');
      expect(text).not.toMatch(/\d/);
    }
  });

  it('renders at /safety without a selected Family, reading only the two list endpoints beyond the shell (O5)', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/safety', App);
    http.expectOne('/api/profile').flush({ profileId: me, displayName: 'Alex' });
    http.expectOne('/api/family-groups/administered').flush([]);
    http.expectOne('/api/family-groups/mine').flush([
      { familyGroupId: 'family-a', name: 'Family A' }, { familyGroupId: 'family-b', name: 'Family B' },
    ]);
    harness.detectChanges();
    await harness.fixture.whenStable();
    harness.detectChanges();
    const requested = http.match(() => true);
    expect(requested.map(request => request.request.url).sort()).toEqual(['/api/concerns', '/api/incidents']);
    requested.find(request => request.request.url === '/api/concerns')!.flush([
      concern('c1', 'Avery', 'Open'), concern('c2', 'Blake', 'Open', { familyGroupId: 'family-b', familyGroupName: 'Family B' }),
    ]);
    requested.find(request => request.request.url === '/api/incidents')!.flush([]);
    harness.detectChanges();
    const root = harness.routeNativeElement!;
    expect(root.querySelector('h1')?.textContent?.trim()).toBe('Safety');
    expect(root.querySelector('.page-title p')?.textContent?.trim()).toBe('Concerns and Safety Incidents shared with you.');
    expect(root.querySelector('[aria-label="Family context required"]')).toBeNull();
    expect(root.querySelector('#current-family')).not.toBeNull();
    const dashboard = root.querySelector('app-safety-dashboard')!;
    expect(dashboard.textContent).toContain('Concern about Blake');
    expect(dashboard.textContent).toContain('Family B');
  });
});
