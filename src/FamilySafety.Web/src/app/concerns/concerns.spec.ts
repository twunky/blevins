import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Concerns } from './concerns';
import { Concern } from './concern-api';
import { ItemFamilyOutcome } from '../item-route';

const groupId = '30000000-0000-0000-0000-000000000001';
const aId = '10000000-0000-0000-0000-000000000001';
const bId = '20000000-0000-0000-0000-000000000002';
const cId = '30000000-0000-0000-0000-000000000003';
const concernId = '40000000-0000-0000-0000-000000000004';

function concern(overrides: Partial<Concern> = {}): Concern {
  return {
    concernId, familyGroupId: groupId, familyGroupName: 'Family',
    creatorProfileId: aId, creatorName: 'Adult A', subjectProfileId: bId, subjectName: 'Adult B',
    creationReason: 'Missed an expected call', state: 'Open',
    createdAt: '2026-10-01T10:00:00Z', lastAuthorizedMutationAt: '2026-10-01T10:20:00Z',
    frozenAt: null, freezeReason: null, resolvedAt: null, resolutionNote: null, expiresAt: null,
    participants: [{ profileId: cId, displayName: 'Adult C' }],
    entries: [],
    ...overrides,
  };
}

/** Opens a Concern the way /concerns/:concernId does: through the routed input (WI-009-005). */
function openRouted(fixture: ComponentFixture<Concerns>, id: string): void {
  fixture.componentRef.setInput('concernId', id);
  fixture.detectChanges();
}

const members = [
  { profileId: aId, displayName: 'Adult A' },
  { profileId: bId, displayName: 'Adult B' },
  { profileId: cId, displayName: 'Adult C' },
];

describe('Concerns', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Concerns],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([{ path: '**', children: [] }])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function start(ownProfileId = aId, initial: Concern[] = []) {
    const fixture = TestBed.createComponent(Concerns);
    fixture.componentRef.setInput('currentFamilyId', groupId);
    fixture.componentRef.setInput('ownProfileId', ownProfileId);
    fixture.detectChanges();
    http.expectOne('/api/concerns').flush(initial);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    fixture.detectChanges();
    return fixture;
  }

  it('renders the authorized list in server order with supported timestamps and creator context', () => {
    const older = concern({ concernId: 'older', subjectName: 'Older subject', createdAt: '2026-09-01T00:00:00Z' });
    const newest = concern({ concernId: 'newest', subjectName: 'Newest subject', createdAt: '2026-10-02T00:00:00Z', state: 'Frozen' });
    const fixture = start(aId, [newest, older]);
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text.indexOf('Newest subject')).toBeLessThan(text.indexOf('Older subject'));
    expect(text).toContain('Last activity');
    expect(text).toContain('Created by you');
    expect(text).toContain('Frozen');
  });

  it('shows a neutral empty list and loads only returned Concern records', () => {
    const fixture = start(aId, []);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('No Concerns are currently available to you');
    expect(http.match(request => request.url.includes('/api/concerns/')).length).toBe(0);
  });

  it('requires subject and reason before creating', () => {
    const fixture = start();
    const component = fixture.componentInstance as any;
    component.showCreate.set(true);
    component.createConcern();
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Choose a subject and enter a reason');
    http.expectNone(request => request.method === 'POST' && request.url.includes('/concerns'));
  });

  it('supports self-Concern and keeps the subject unchecked in the selected audience', () => {
    const fixture = start();
    const component = fixture.componentInstance as any;
    component.showCreate.set(true);
    fixture.detectChanges();
    const subjectOptions = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('#concern-subject option'))
      .map(option => option.textContent?.trim());
    expect(subjectOptions).toContain('Adult A (you)');
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.toggleCreateParticipant(cId, true);
    expect(component.isCreateParticipantSelected(bId)).toBe(false);
    expect(component.isCreateParticipantSelected(cId)).toBe(true);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('The subject is not included unless you select them');
  });

  it('prevents repeated creation submissions while the first request is pending', () => {
    const fixture = start();
    const component = fixture.componentInstance as any;
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.createConcern();
    component.createConcern();
    const requests = http.match(request => request.method === 'POST' && request.url === `/api/family-groups/${groupId}/concerns`);
    expect(requests).toHaveLength(1);
    requests[0].flush(concern());
  });

  it('preserves successful creation and retries only the failed initial entry', () => {
    const fixture = start();
    const component = fixture.componentInstance as any;
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.toggleCreateParticipant(cId, true);
    component.setInitialKind('Observation');
    component.initialForm.patchValue({ text: 'A call was expected', category: 'Reported', sourceDescription: 'Adult A', eventAt: '' });
    component.createConcern();
    const creation = http.expectOne(`/api/family-groups/${groupId}/concerns`);
    expect(creation.request.body.participantProfileIds).toEqual([cId]);
    creation.flush(concern());
    const firstEntry = http.expectOne(`/api/concerns/${concernId}/entries`);
    expect(firstEntry.request.body.kind).toBe('Observation');
    firstEntry.flush({ title: 'Temporary failure' }, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(component.selected()?.concernId).toBe(concernId);
    expect(component.initialEntryFailure()).toBe(true);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Concern was created, but the initial entry was not saved');
    component.retryInitialEntry();
    const retry = http.expectOne(`/api/concerns/${concernId}/entries`);
    expect(retry.request.body.text).toBe('A call was expected');
    retry.flush(concern({ entries: [{
      entryId: 'entry-1', kind: 'Observation', category: 'Reported', text: 'A call was expected',
      sourceDescription: 'Adult A', sourceUnknown: false, contributorProfileId: aId, contributorName: 'Adult A',
      recordedAt: '2026-10-01T10:30:00Z', eventAt: null, relatedEntryId: null, explanation: null, isOutdated: false,
    }] }));
    expect(component.initialEntryFailure()).toBe(false);
    expect(http.match(request => request.method === 'POST' && request.url.endsWith('/concerns')).length).toBe(0);
  });

  it('maps Outdated requests to the earlier category while rendering the original category plus freshness', () => {
    const fixture = start();
    const component = fixture.componentInstance as any;
    openRouted(fixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush(concern());
    component.openComposer('Observation');
    component.entryForm.patchValue({ text: 'The earlier report is stale', category: 'Outdated', originalCategory: 'Reported', sourceDescription: 'Adult A' });
    component.submitEntry();
    const request = http.expectOne(`/api/concerns/${concernId}/entries`);
    expect(request.request.body.category).toBe('Outdated');
    expect(request.request.body.originalCategory).toBe('Reported');
    expect(request.request.body.isOutdated).toBe(true);
    request.flush(concern({ entries: [{
      entryId: 'stale', kind: 'Observation', category: 'Reported', text: 'The earlier report is stale',
      sourceDescription: 'Adult A', sourceUnknown: false, contributorProfileId: aId, contributorName: 'Adult A',
      recordedAt: '2026-10-01T10:30:00Z', eventAt: null, relatedEntryId: null, explanation: null, isOutdated: true,
    }] }));
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Reported · Outdated');
  });

  it('shows correction as append-only linked history and uses profile IDs for authorship hints', () => {
    const ownEntry: Concern['entries'][number] = {
      entryId: 'own-entry', kind: 'Observation', category: 'Reported', text: 'I saw the car leave',
      sourceDescription: 'Adult A', sourceUnknown: false, contributorProfileId: aId, contributorName: 'Adult A',
      recordedAt: '2026-10-01T10:05:00Z', eventAt: null, relatedEntryId: null, explanation: null, isOutdated: false,
    };
    const correction: Concern['entries'][number] = {
      ...ownEntry, entryId: 'correction', kind: 'Correction', text: 'The car was still parked',
      relatedEntryId: ownEntry.entryId, explanation: 'I checked again', recordedAt: '2026-10-01T10:10:00Z',
    };
    const fixture = start(aId, [concern({ entries: [ownEntry, correction] })]);
    const component = fixture.componentInstance as any;
    openRouted(fixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush(concern({ entries: [ownEntry, correction] }));
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Add a correction');
    expect(element.textContent).toContain('I saw the car leave');
    expect(element.textContent).toContain('Correction to original entry');
    expect(element.querySelector('a[href="#concern-entry-own-entry"]')?.textContent).toContain('View original entry');
    const other = { ...ownEntry, entryId: 'other', contributorProfileId: bId };
    expect(component.canCorrect(other)).toBe(false);
    expect(component.canChallenge(other)).toBe(true);
  });

  it('shows another-report action as a separate linked contribution', () => {
    const otherEntry: Concern['entries'][number] = {
      entryId: 'other-entry', kind: 'Observation', category: 'Reported', text: 'I called at noon',
      sourceDescription: 'Adult C', sourceUnknown: false, contributorProfileId: cId, contributorName: 'Adult C',
      recordedAt: '2026-10-01T10:05:00Z', eventAt: null, relatedEntryId: null, explanation: null, isOutdated: false,
    };
    const fixture = start(aId, [concern({ entries: [otherEntry] })]);
    const component = fixture.componentInstance as any;
    openRouted(fixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush(concern({ entries: [otherEntry] }));
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Add another report');
    component.openComposer('Challenge', otherEntry);
    component.entryForm.patchValue({ text: 'I heard back later', category: 'Reported', sourceDescription: 'Adult A', explanation: 'Additional information' });
    component.submitEntry();
    const request = http.expectOne(`/api/concerns/${concernId}/entries`);
    expect(request.request.body.kind).toBe('Challenge');
    expect(request.request.body.relatedEntryId).toBe('other-entry');
    expect(request.request.body.explanation).toBe('Additional information');
    request.flush(concern());
  });

  it('keeps Frozen and Resolved details read-only', () => {
    const frozenFixture = start(aId, [concern({ state: 'Frozen', frozenAt: '2026-10-01T12:00:00Z' })]);
    const frozen = frozenFixture.componentInstance as any;
    openRouted(frozenFixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush(concern({ state: 'Frozen', frozenAt: '2026-10-01T12:00:00Z' }));
    frozenFixture.detectChanges();
    let text = (frozenFixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Start a new Concern');
    expect(text).not.toContain('Add an observation');
    expect(text).not.toContain('Manage participants');
    expect(text).not.toContain('Resolve Concern');

    frozenFixture.destroy();
    const resolvedConcern = concern({ state: 'Resolved', resolvedAt: '2026-10-01T12:00:00Z', resolutionNote: 'Coordination ended' });
    const resolvedFixture = start(aId, [resolvedConcern]);
    const resolved = resolvedFixture.componentInstance as any;
    openRouted(resolvedFixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush(resolvedConcern);
    resolvedFixture.detectChanges();
    text = (resolvedFixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Resolution');
    expect(text).toContain('Coordination ended');
    expect(text).not.toContain('Resolve Concern');
    resolvedFixture.destroy();
  });

  it('clears stale detail and displays nondisclosing unavailable copy after 404', () => {
    const fixture = start(aId, [concern()]);
    const component = fixture.componentInstance as any;
    openRouted(fixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush({}, { status: 404, statusText: 'Not Found' });
    http.expectOne('/api/concerns').flush([]);
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(component.selected()).toBeNull();
    expect(text).toContain('This Concern is unavailable');
    expect(text).not.toContain('You were removed');
    expect(text).not.toContain('This Concern expired');
    expect(fixture.nativeElement.querySelector('button.concerns-secondary')?.textContent).toContain('Return to Concerns');
  });
});

describe('Concerns — Separate Incident entry point (WI-008-004)', () => {
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Concerns],
      providers: [provideRouter([{ path: '**', children: [] }]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => http.verify());

  function openDetail(ownProfileId: string, value: Concern) {
    const fixture = TestBed.createComponent(Concerns);
    fixture.componentRef.setInput('currentFamilyId', groupId);
    fixture.componentRef.setInput('ownProfileId', ownProfileId);
    fixture.detectChanges();
    http.expectOne('/api/concerns').flush([value]);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    openRouted(fixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush(value);
    fixture.detectChanges();
    return fixture;
  }

  const card = (root: HTMLElement) => Array.from(root.querySelectorAll('h3'))
    .find(heading => heading.textContent?.trim() === 'Separate Incident')?.closest('section') ?? null;

  it('shows the Separate Incident card to the creator in Open, Frozen, and Resolved, and never to a participant', () => {
    for (const state of ['Open', 'Frozen', 'Resolved'] as const) {
      const fixture = openDetail(aId, concern({ state }));
      const section = card(fixture.nativeElement);
      expect(section?.textContent).toContain('Nothing here is copied or shared.');
      expect(Array.from(section!.querySelectorAll('button')).map(button => button.textContent?.trim())).toEqual(['Start an Incident…']);
      fixture.destroy();
    }
    const participant = openDetail(cId, concern());
    expect(card(participant.nativeElement)).toBeNull();
  });

  it('navigates to /incidents/new with ID-only navigation state and changes nothing on the Concern', async () => {
    const fixture = openDetail(aId, concern({ creationReason: 'Private reason text' }));
    const navigate = vi.spyOn(router, 'navigateByUrl');
    Array.from(card(fixture.nativeElement)!.querySelectorAll('button'))[0].click();
    await fixture.whenStable();
    expect(navigate).toHaveBeenCalledWith('/incidents/new', {
      state: { originatingConcernId: concernId, familyGroupId: groupId, subjectProfileId: bId },
    });
    expect(router.url).toBe('/incidents/new');
    http.expectNone(request => request.method !== 'GET');
  });

  it('never opens a Concern from navigation state; only its URL opens it (WI-009-005, AC-07)', async () => {
    await router.navigateByUrl('/concerns', { state: { openConcernId: concernId } });
    const fixture = TestBed.createComponent(Concerns);
    fixture.componentRef.setInput('currentFamilyId', groupId);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.detectChanges();
    http.expectOne('/api/concerns').flush([]);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    http.expectNone(`/api/concerns/${concernId}`);
    openRouted(fixture, concernId);
    http.expectOne(`/api/concerns/${concernId}`).flush({}, { status: 404, statusText: 'Not Found' });
    http.expectOne('/api/concerns').flush([]);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('This Concern is unavailable');
  });
});

describe('Concerns — item URL /concerns/:concernId and create navigation (REQ-UX-014, WI-009-005)', () => {
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Concerns],
      providers: [provideRouter([{ path: '**', children: [] }]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => http.verify());

  function startRouted(id: string, outcome: Promise<ItemFamilyOutcome>) {
    const calls: [string, string][] = [];
    const fixture = TestBed.createComponent(Concerns);
    fixture.componentRef.setInput('currentFamilyId', groupId);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.componentRef.setInput('concernId', id);
    fixture.componentRef.setInput('adoptItemFamily', (familyGroupId: string, kind: string) => { calls.push([familyGroupId, kind]); return outcome; });
    fixture.detectChanges();
    http.expectOne('/api/concerns').flush([]);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    return { fixture, calls, element: fixture.nativeElement as HTMLElement };
  }

  function startList(rows: Concern[] = []) {
    const fixture = TestBed.createComponent(Concerns);
    fixture.componentRef.setInput('currentFamilyId', groupId);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.detectChanges();
    http.expectOne('/api/concerns').flush(rows);
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    fixture.detectChanges();
    return fixture;
  }

  it('reads the Concern once, decides the Family after the 200, then renders it', async () => {
    let decide!: (value: ItemFamilyOutcome) => void;
    const { fixture, calls, element } = startRouted(concernId, new Promise(resolve => decide = resolve));
    http.expectOne(`/api/concerns/${concernId}`).flush(concern({ familyGroupName: 'Name from the item response' }));
    fixture.detectChanges();
    expect(calls).toEqual([[groupId, 'concern']]);
    expect(element.textContent).toContain('Loading Concern…');
    expect(element.textContent).not.toContain('Concern about Adult B');
    decide('switched');
    await fixture.whenStable();
    fixture.detectChanges();
    expect(element.querySelector('h2')?.textContent).toBe('Concern about Adult B');
    expect(element.textContent).not.toContain('Name from the item response');
    const back = Array.from(element.querySelectorAll('a')).find(link => link.textContent?.includes('Back to Concerns'));
    expect(back?.getAttribute('href')).toBe('/concerns');
    http.expectNone(`/api/concerns/${concernId}`);
  });

  it('shows the generic unavailable card when the Family is not one of the caller’s Families (OD-B)', async () => {
    const { fixture, element } = startRouted(concernId, Promise.resolve('unavailable'));
    http.expectOne(`/api/concerns/${concernId}`).flush(concern());
    await fixture.whenStable();
    http.expectOne('/api/concerns').flush([]);
    fixture.detectChanges();
    expect(element.querySelector('h2')?.textContent).toBe('This Concern is unavailable');
    expect(element.textContent).not.toContain('Adult B');
  });

  it('shows the unavailable card for a non-GUID ID without reading it', () => {
    const fixture = TestBed.createComponent(Concerns);
    fixture.componentRef.setInput('currentFamilyId', groupId);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.componentRef.setInput('concernId', 'not-a-concern');
    fixture.detectChanges();
    http.expectNone(request => request.url.startsWith('/api/concerns/'));
    // The initial list read, then the existing list refresh that accompanies the unavailable card.
    http.match('/api/concerns').forEach(request => request.flush([]));
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('h2')?.textContent).toBe('This Concern is unavailable');
  });

  it('links each list row to its Concern URL and keeps its accessible name', () => {
    const fixture = startList([concern()]);
    const row = (fixture.nativeElement as HTMLElement).querySelector<HTMLAnchorElement>('a.concern-card')!;
    expect(row.getAttribute('href')).toBe(`/concerns/${concernId}`);
    expect(row.getAttribute('aria-label')).toBe('Open Concern about Adult B, Open');
    expect((fixture.nativeElement as HTMLElement).querySelector('button.concern-card')).toBeNull();
  });

  it('navigates to the created Concern after a fully successful create, without reading it again (OD-G)', async () => {
    const fixture = startList();
    const component = fixture.componentInstance as any;
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.createConcern();
    http.expectOne(`/api/family-groups/${groupId}/concerns`).flush(concern());
    await fixture.whenStable();
    expect(router.url).toBe(`/concerns/${concernId}`);
    openRouted(fixture, concernId);
    http.expectNone(`/api/concerns/${concernId}`);
    expect(component.selected()?.concernId).toBe(concernId);
  });

  it('stays on the recovery panel after an initial-entry failure and navigates only on Continue or a successful retry (OD-G)', async () => {
    const fixture = startList();
    const component = fixture.componentInstance as any;
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.setInitialKind('Observation');
    component.initialForm.patchValue({ text: 'A call was expected', category: 'Reported', sourceDescription: 'Adult A', eventAt: '' });
    component.createConcern();
    http.expectOne(`/api/family-groups/${groupId}/concerns`).flush(concern());
    http.expectOne(`/api/concerns/${concernId}/entries`).flush({}, { status: 503, statusText: 'Unavailable' });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(router.url).toBe('/');
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Concern was created, but the initial entry was not saved');
    Array.from(element.querySelectorAll('button')).find(button => button.textContent?.trim() === 'Continue to Concern')!.click();
    await fixture.whenStable();
    expect(router.url).toBe(`/concerns/${concernId}`);
    expect(component.initialEntryFailure()).toBe(false);
  });

  it('navigates after a successful retry of the initial entry (OD-G)', async () => {
    const fixture = startList();
    const component = fixture.componentInstance as any;
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.setInitialKind('Unknown');
    component.initialForm.patchValue({ text: 'Not known yet', sourceDescription: 'Adult A' });
    component.createConcern();
    http.expectOne(`/api/family-groups/${groupId}/concerns`).flush(concern());
    http.expectOne(`/api/concerns/${concernId}/entries`).flush({}, { status: 503, statusText: 'Unavailable' });
    await fixture.whenStable();
    expect(router.url).toBe('/');
    component.retryInitialEntry();
    http.expectOne(`/api/concerns/${concernId}/entries`).flush(concern());
    await fixture.whenStable();
    expect(router.url).toBe(`/concerns/${concernId}`);
    expect(component.initialEntryMessage()).toBe('The initial entry was added.');
  });

  it('keeps the ambiguous-create check and navigates only when it resolves to a match (OD-G)', async () => {
    const fixture = startList();
    const component = fixture.componentInstance as any;
    component.createForm.patchValue({ subjectProfileId: bId, reason: 'Need to coordinate' });
    component.createConcern();
    http.expectOne(`/api/family-groups/${groupId}/concerns`).flush({}, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();
    expect(component.createAmbiguous()).toBe(true);
    expect(router.url).toBe('/');
    component.checkAmbiguousCreate();
    http.expectOne('/api/concerns').flush([concern({ creationReason: 'Need to coordinate', createdAt: new Date().toISOString() })]);
    await fixture.whenStable();
    expect(router.url).toBe(`/concerns/${concernId}`);
    expect(component.actionError()).toContain('A matching Concern was found');
  });

  it('returns to the list when the URL changes back to /concerns', () => {
    const { fixture, element } = startRouted(concernId, Promise.resolve('current'));
    http.expectOne(`/api/concerns/${concernId}`).flush({}, { status: 404, statusText: 'Not Found' });
    http.expectOne('/api/concerns').flush([]);
    fixture.detectChanges();
    expect(element.textContent).toContain('This Concern is unavailable');
    fixture.componentRef.setInput('concernId', null);
    fixture.detectChanges();
    expect(element.textContent).toContain('Concerns available to you');
  });
});
