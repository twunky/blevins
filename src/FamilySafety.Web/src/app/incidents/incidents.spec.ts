import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { incidentsMatcher } from '../app.routes';
import { Incident, IncidentApi, IncidentSummary } from './incident-api';
import { IncidentCreate } from './incident-create';
import { IncidentList } from './incident-list';
import { Incidents, readOrigin } from './incidents';

// Accessible queries only: roles, labels, names, and text. No CSS-class or layout assertions.
const norm = (value: string | null | undefined) => (value ?? '').replace(/\s+/g, ' ').trim();
/** Accessible text: text nodes joined with spaces, skipping aria-hidden subtrees. */
function text(element: Element | null | undefined): string {
  if (!element) return '';
  const parts: string[] = [];
  const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
  for (let node = walker.nextNode(); node; node = walker.nextNode())
    if (!node.parentElement?.closest('[aria-hidden="true"]')) parts.push(node.textContent ?? '');
  return norm(parts.join(' ')).replace(/ ([.,?:;)”])/g, '$1').replace(/([(“]) /g, '$1');
}
const matches = (value: string, name: string | RegExp) => typeof name === 'string' ? value === name : name.test(value);
function button(root: Element, name: string | RegExp): HTMLButtonElement | undefined {
  return Array.from(root.querySelectorAll('button')).find(item => matches(norm(item.getAttribute('aria-label') ?? text(item)), name));
}
function heading(root: Element, name: string | RegExp): HTMLElement | undefined {
  return Array.from(root.querySelectorAll<HTMLElement>('h1, h2, h3, h4')).find(item => matches(text(item), name));
}
function control<T extends HTMLElement = HTMLInputElement>(root: Element, label: string | RegExp): T {
  const found = Array.from(root.querySelectorAll('label')).find(item => matches(text(item), label));
  if (!found) throw new Error(`No label ${label}`);
  const target = found.htmlFor ? root.querySelector(`#${found.htmlFor}`) : found.querySelector('input, select, textarea');
  return target as T;
}
function group(root: Element, legend: string | RegExp): HTMLFieldSetElement {
  return Array.from(root.querySelectorAll('fieldset')).find(item => matches(text(item.querySelector('legend')), legend))!;
}
function type(element: HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement, value: string): void {
  element.value = value;
  element.dispatchEvent(new Event(element instanceof HTMLSelectElement ? 'change' : 'input'));
}

const groupId = '30000000-0000-4000-8000-000000000001';
const otherGroupId = '30000000-0000-4000-8000-000000000002';
const aId = '10000000-0000-4000-8000-000000000001';
const bId = '20000000-0000-4000-8000-000000000002';
const cId = '30000000-0000-4000-8000-000000000003';
const concernId = '40000000-0000-4000-8000-000000000004';
const incidentId = '50000000-0000-4000-8000-000000000005';
const members = [
  { profileId: aId, displayName: 'Adult A' },
  { profileId: bId, displayName: 'Adult B' },
  { profileId: cId, displayName: 'Adult C' },
];

function summary(overrides: Partial<IncidentSummary> = {}): IncidentSummary {
  return {
    incidentId, familyGroupId: groupId, familyGroupName: 'Family', subjectProfileId: bId, subjectName: 'Adult B',
    state: 'Open', coordinatorProfileId: aId, coordinatorName: 'Adult A', createdAt: '2026-10-01T10:00:00Z',
    lastAuthorizedMutationAt: '2026-10-01T11:00:00Z', expiresAt: null, ...overrides,
  };
}

function created(overrides: Partial<Incident> = {}): Incident {
  return {
    incidentId, familyGroupId: groupId, familyGroupName: 'Family', subjectProfileId: bId, subjectName: 'Adult B',
    createdByProfileId: aId, createdByName: 'Adult A', coordinatorProfileId: aId, coordinatorName: 'Adult A', state: 'Open',
    createdAt: '2026-10-01T10:00:00Z', lastAuthorizedMutationAt: '2026-10-01T10:00:00Z', inactivityFreezeAt: null, frozenAt: null,
    freezeReason: null, expiresAt: null, resolution: null, originatingConcernId: null,
    participants: [{ profileId: aId, displayName: 'Adult A', isCoordinator: true, selectedAt: '2026-10-01T10:00:00Z' }],
    entries: [], tasks: [], timeline: [], ...overrides,
  };
}

describe('Incident API client', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('uses each WI-008-003 route, method, and body shape with string enums', () => {
    const api = TestBed.inject(IncidentApi);
    const expectCall = (method: string, url: string, body: unknown = null) => {
      const request = http.expectOne(url);
      expect(request.request.method).toBe(method);
      if (body !== null) expect(request.request.body).toEqual(body);
      request.flush({});
    };
    api.list().subscribe(); expectCall('GET', '/api/incidents');
    api.get(incidentId).subscribe(); expectCall('GET', `/api/incidents/${incidentId}`);
    api.members(groupId).subscribe(); expectCall('GET', `/api/family-groups/${groupId}/members`);
    const create = { subjectProfileId: bId, reason: 'r', reasonCategory: 'Reported' as const, reasonSourceDescription: 'A',
      reasonSourceUnknown: false, participantProfileIds: [cId], originatingConcernId: null };
    api.create(groupId, create).subscribe(); expectCall('POST', `/api/family-groups/${groupId}/incidents`, create);
    api.replaceParticipants(incidentId, [cId]).subscribe(); expectCall('PUT', `/api/incidents/${incidentId}/participants`, { participantProfileIds: [cId] });
    api.transferCoordinator(incidentId, cId).subscribe(); expectCall('POST', `/api/incidents/${incidentId}/coordinator`, { participantProfileId: cId });
    api.createTask(incidentId, 'Call hotel', cId).subscribe(); expectCall('POST', `/api/incidents/${incidentId}/tasks`, { description: 'Call hotel', assigneeProfileId: cId });
    api.cancelTask(incidentId, 't1', 'No longer needed').subscribe(); expectCall('POST', `/api/incidents/${incidentId}/tasks/t1/cancel`, { reason: 'No longer needed' });
    const complete = { resultText: 'Done', category: 'Confirmed' as const, sourceDescription: 'Desk', sourceUnknown: false, eventAt: null };
    api.completeTask(incidentId, 't1', complete).subscribe(); expectCall('POST', `/api/incidents/${incidentId}/tasks/t1/complete`, complete);
    api.addEntry(incidentId, { kind: 'ContactAttempt', category: 'Reported' } as never).subscribe();
    expectCall('POST', `/api/incidents/${incidentId}/entries`, { kind: 'ContactAttempt', category: 'Reported' });
    api.resolve(incidentId, { summary: 's' } as never).subscribe(); expectCall('POST', `/api/incidents/${incidentId}/resolve`, { summary: 's' });
  });
});

describe('Incident list', () => {
  // Rows are links to each Incident's own URL (WI-009-005), so the list needs a router.
  beforeEach(() => TestBed.configureTestingModule({ providers: [provideRouter([])] }));

  function render(inputs: Partial<IncidentList> = {}) {
    const fixture = TestBed.createComponent(IncidentList);
    for (const [key, value] of Object.entries({ ownProfileId: aId, ...inputs })) fixture.componentRef.setInput(key, value);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('groups open and ended coordination in server order with coordinator, times, and kept-until', () => {
    const root = render({ rows: [
      summary({ incidentId: 'i1', subjectName: 'Newest Open' }),
      summary({ incidentId: 'i2', subjectName: 'Resolved One', state: 'Resolved', expiresAt: '2027-10-01T12:00:00Z', coordinatorProfileId: cId, coordinatorName: 'Adult C' }),
      summary({ incidentId: 'i3', subjectName: 'Older Open', coordinatorProfileId: cId, coordinatorName: 'Adult C' }),
      summary({ incidentId: 'i4', subjectName: 'Frozen One', state: 'Frozen', expiresAt: '2027-09-01T00:00:00Z' }),
    ] });
    const open = heading(root, 'Open coordination')!.closest('section')!;
    const ended = heading(root, 'Ended coordination (read-only)')!.closest('section')!;
    expect(Array.from(open.querySelectorAll('li a')).map(item => item.getAttribute('aria-label')))
      .toEqual(['Open Incident about Newest Open, Open', 'Open Incident about Older Open, Open']);
    expect(Array.from(ended.querySelectorAll('li a')).map(item => item.getAttribute('aria-label')))
      .toEqual(['Open Incident about Resolved One, Resolved', 'Open Incident about Frozen One, Frozen']);
    expect(Array.from(root.querySelectorAll('li a')).map(item => item.getAttribute('href')))
      .toEqual(['/incidents/i1', '/incidents/i3', '/incidents/i2', '/incidents/i4']);
    expect(root.querySelectorAll('li button')).toHaveLength(0);
    expect(text(open)).toContain('You coordinate this Incident');
    expect(text(open)).toContain('Coordinated by Adult C');
    expect(text(open)).toContain('Created Oct 1, 2026');
    expect(text(open)).toContain('Last activity');
    expect(text(ended)).toContain('Read-only · kept until Oct 1, 2027');
    expect(text(root)).not.toMatch(/Concern|reason/i);
    expect(root.querySelectorAll('time[datetime]').length).toBeGreaterThan(0);
  });

  it('omits an empty group and shows the Family name only for multi-Family accounts', () => {
    let root = render({ rows: [summary()] });
    expect(heading(root, 'Ended coordination (read-only)')).toBeUndefined();
    expect(text(root)).not.toContain('Family');
    root = render({ rows: [summary({ familyGroupName: 'Lake House Family' })], showFamilyName: true });
    expect(text(root)).toContain('Lake House Family');
  });

  it('announces loading, shows a neutral empty state, and offers retry on error', () => {
    expect(render({ loading: true }).querySelector('[role="status"]')?.textContent).toContain('Loading Incidents…');
    const empty = render({ rows: [] });
    expect(heading(empty, 'No Incidents are currently shared with you.')).toBeDefined();
    expect(text(empty)).toContain('An Incident is shared only with its coordinator and the people they add.');
    expect(text(empty)).not.toMatch(/subject|administrator/i);
    const fixture = TestBed.createComponent(IncidentList);
    fixture.componentRef.setInput('error', 'Incidents could not be loaded. Please try again.');
    let refreshed = 0;
    fixture.componentInstance.refresh.subscribe(() => refreshed++);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.querySelector('[role="alert"]')?.textContent).toContain('could not be loaded');
    button(root, 'Retry loading Incidents')!.click();
    expect(refreshed).toBe(1);
  });

  it('offers Start an Incident only when a current Family is selected', () => {
    expect(button(render({ canCreate: false }), /Start an Incident/)).toBeUndefined();
    expect(button(render({ canCreate: true }), /Start an Incident/)).toBeDefined();
  });
});

describe('Incident creation', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  function start(origin: IncidentCreate['origin'] = null, familyGroupId = groupId) {
    const fixture = TestBed.createComponent(IncidentCreate);
    fixture.componentRef.setInput('familyGroupId', familyGroupId);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.componentRef.setInput('origin', origin);
    fixture.detectChanges();
    http.expectOne(`/api/family-groups/${familyGroupId}/members`).flush(members);
    fixture.detectChanges();
    return { fixture, root: fixture.nativeElement as HTMLElement };
  }

  function fillValid(root: HTMLElement, fixture: ReturnType<typeof start>['fixture']) {
    type(control<HTMLSelectElement>(root, 'Who is this Incident about?'), bId);
    type(control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?'), 'No reply since the expected arrival.');
    control(root, /^Reported/).click();
    type(control(root, 'Source description'), 'Adult A');
    fixture.detectChanges();
  }

  it('loads authorized choices when the Family arrives after creation initialization', () => {
    const fixture = TestBed.createComponent(IncidentCreate);
    fixture.componentRef.setInput('familyGroupId', null);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    http.expectNone(request => request.url.endsWith('/members'));
    type(control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?'), 'Draft reason');
    fixture.componentRef.setInput('familyGroupId', groupId);
    fixture.detectChanges();
    const read = http.expectOne(`/api/family-groups/${groupId}/members`);
    expect(text(root)).toContain('Loading active adult family members');
    expect(button(root, 'Create Incident')!.disabled).toBe(true);
    expect(group(root, 'People with access').querySelectorAll('input')).toHaveLength(0);
    read.flush(members);
    fixture.detectChanges();
    expect(Array.from(control<HTMLSelectElement>(root, 'Who is this Incident about?').options).map(option => text(option)))
      .toEqual(['Choose a person', 'Adult A (you)', 'Adult B', 'Adult C']);
    expect(control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?').value).toBe('Draft reason');
    expect(button(root, 'Create Incident')!.disabled).toBe(false);
    http.expectNone(request => request.method === 'POST');
  });

  for (const status of [401, 404, 500]) {
    it(`keeps member choices unavailable on ${status} and retries the authorized read`, () => {
      const fixture = TestBed.createComponent(IncidentCreate);
      fixture.componentRef.setInput('familyGroupId', null);
      fixture.componentRef.setInput('ownProfileId', aId);
      fixture.detectChanges();
      fixture.componentRef.setInput('familyGroupId', groupId);
      fixture.detectChanges();
      http.expectOne(`/api/family-groups/${groupId}/members`).flush({}, { status, statusText: 'Read failed' });
      fixture.detectChanges();
      const root = fixture.nativeElement as HTMLElement;
      expect(root.querySelector('[role="alert"]')).not.toBeNull();
      expect(button(root, 'Create Incident')!.disabled).toBe(true);
      expect(group(root, 'People with access').querySelectorAll('input')).toHaveLength(0);
      expect(text(root)).not.toContain('Adult B');
      button(root, 'Retry loading family members')!.click();
      fixture.detectChanges();
      http.expectOne(`/api/family-groups/${groupId}/members`).flush([members[0], members[1]]);
      fixture.detectChanges();
      expect(text(root)).not.toContain('Adult C');
      expect(control<HTMLSelectElement>(root, 'Who is this Incident about?').options.length).toBe(3);
      http.expectNone(request => request.method === 'POST');
    });
  }

  it('cancels old Family reads, clears old choices and selections, and stops reading on destruction', () => {
    const { root, fixture } = start();
    type(control<HTMLSelectElement>(root, 'Who is this Incident about?'), bId);
    control(root, /^Adult B/).click();
    fixture.componentRef.setInput('familyGroupId', otherGroupId);
    fixture.detectChanges();
    const pending = http.expectOne(`/api/family-groups/${otherGroupId}/members`);
    expect(control<HTMLSelectElement>(root, 'Who is this Incident about?').value).toBe('');
    expect(group(root, 'People with access').querySelectorAll('input')).toHaveLength(0);
    fixture.componentRef.setInput('familyGroupId', null);
    fixture.detectChanges();
    expect(pending.cancelled).toBe(true);
    expect(text(root)).not.toContain('Adult B');
    fixture.componentRef.setInput('familyGroupId', groupId);
    fixture.detectChanges();
    const current = http.expectOne(`/api/family-groups/${groupId}/members`);
    current.flush(members);
    fixture.detectChanges();
    expect(Array.from(group(root, 'People with access').querySelectorAll<HTMLInputElement>('input')).every(box => !box.checked)).toBe(true);
    button(root, 'Create Incident')!.click();
    fixture.detectChanges();
    http.expectNone(request => request.method === 'POST');
    fixture.componentRef.setInput('familyGroupId', otherGroupId);
    fixture.detectChanges();
    const destroyedRead = http.expectOne(`/api/family-groups/${otherGroupId}/members`);
    fixture.destroy();
    expect(destroyedRead.cancelled).toBe(true);
  });

  it('offers self as subject, excludes the creator from people, and preselects nobody', () => {
    const { root } = start();
    const subjectOptions = Array.from(control<HTMLSelectElement>(root, 'Who is this Incident about?').options).map(option => text(option));
    expect(subjectOptions).toEqual(['Choose a person', 'Adult A (you)', 'Adult B', 'Adult C']);
    const people = group(root, 'People with access');
    const boxes = Array.from(people.querySelectorAll<HTMLInputElement>('input[type="checkbox"]'));
    expect(boxes.map(box => text(box.closest('label')))).toEqual(['Adult B', 'Adult C']);
    expect(boxes.every(box => !box.checked)).toBe(true);
  });

  it('states before submit that choosing a subject does not share the Incident and marks the subject checkbox', () => {
    const { root, fixture } = start();
    const people = group(root, 'People with access');
    expect(text(people)).toContain('Only you and the people you add can see it.');
    expect(text(people)).toContain('Choosing someone as the subject does not share the Incident with them.');
    expect(text(people)).toContain('Family membership or administrator status alone does not give anyone access.');
    type(control<HTMLSelectElement>(root, 'Who is this Incident about?'), bId);
    fixture.detectChanges();
    expect(text(people)).toContain('Adult B (subject, not added)');
    control(root, /^Adult B/).click();
    fixture.detectChanges();
    expect(text(people)).toContain('Adult B (subject, added)');
  });

  it('offers no Outdated reason category and disables unknown source for Confirmed', () => {
    const { root, fixture } = start();
    const categories = group(root, 'How is the reason known?');
    expect(Array.from(categories.querySelectorAll('label')).map(label => label.querySelector('strong')?.textContent))
      .toEqual(['Confirmed', 'Reported', 'Assumption', 'Unknown']);
    control(root, /^Confirmed/).click();
    fixture.detectChanges();
    expect(control(root, 'Source is unknown').disabled).toBe(true);
    control(root, /^Assumption/).click();
    fixture.detectChanges();
    expect(control(root, 'Source is unknown').disabled).toBe(false);
  });

  it('validates required fields accessibly without sending a request', () => {
    const { root, fixture } = start();
    button(root, 'Create Incident')!.click();
    fixture.detectChanges();
    expect(root.querySelector('[role="alert"]')?.textContent).toContain('Review the highlighted details');
    const subject = control<HTMLSelectElement>(root, 'Who is this Incident about?');
    expect(subject.getAttribute('aria-invalid')).toBe('true');
    expect(root.querySelector(`#${subject.getAttribute('aria-describedby')}`)?.textContent).toContain('Choose the person this Incident is about.');
    const reason = control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?');
    expect(reason.getAttribute('aria-describedby')).toContain('create-reason-error');
    expect(text(root)).toContain('Choose how this information is known.');
    expect(text(root)).toContain('Enter a source or mark the source as unknown.');
    http.expectNone(request => request.method === 'POST');
  });

  it('sends exactly the selected people, disables submit in flight, and emits the created Incident', () => {
    const { root, fixture } = start();
    let result: Incident | null = null;
    fixture.componentInstance.created.subscribe(value => result = value);
    fillValid(root, fixture);
    control(root, /^Adult C/).click();
    fixture.detectChanges();
    button(root, 'Create Incident')!.click();
    fixture.detectChanges();
    expect(button(root, 'Creating Incident…')!.disabled).toBe(true);
    (fixture.componentInstance as unknown as { submit(): void }).submit();
    const requests = http.match(`/api/family-groups/${groupId}/incidents`);
    expect(requests).toHaveLength(1);
    expect(requests[0].request.body).toEqual({
      subjectProfileId: bId, reason: 'No reply since the expected arrival.', reasonCategory: 'Reported',
      reasonSourceDescription: 'Adult A', reasonSourceUnknown: false, participantProfileIds: [cId], originatingConcernId: null,
    });
    requests[0].flush(created());
    expect(result).not.toBeNull();
  });

  it('allows zero people and maps server validation keys to UI-owned field messages', () => {
    const { root, fixture } = start();
    fillValid(root, fixture);
    button(root, 'Create Incident')!.click();
    const request = http.expectOne(`/api/family-groups/${groupId}/incidents`);
    expect(request.request.body.participantProfileIds).toEqual([]);
    request.flush({ errors: { Reason: ['raw server reason text'], ParticipantProfileIds: ['raw server people text'] } }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(text(root)).toContain('Enter a reason of 1–1,000 characters.');
    expect(text(root)).toContain('Review the people with access and try again.');
    expect(text(root)).not.toContain('raw server');
    expect(control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?').value).toBe('No reply since the expected arrival.');
  });

  it('shows the generic creation message on 404 without a reason', () => {
    const { root, fixture } = start();
    fillValid(root, fixture);
    button(root, 'Create Incident')!.click();
    http.expectOne(`/api/family-groups/${groupId}/incidents`).flush({}, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    expect(root.querySelector('[role="alert"]')?.textContent)
      .toContain('The Incident could not be created with the selected information. Refresh your Family members and try again.');
  });

  it('never retries an ambiguous failure and checks the authorized list for a match', () => {
    const { root, fixture } = start();
    fillValid(root, fixture);
    button(root, 'Create Incident')!.click();
    http.expectOne(`/api/family-groups/${groupId}/incidents`).error(new ProgressEvent('network'));
    http.expectOne('/api/incidents').flush([summary({ createdAt: new Date().toISOString() })]);
    fixture.detectChanges();
    http.expectNone(`/api/family-groups/${groupId}/incidents`);
    expect(text(root)).toContain('A matching Incident appears in your list. Review it before creating another.');
    expect(button(root, 'Open the matching Incident')).toBeDefined();
    expect(button(root, 'Create Incident')!.disabled).toBe(true);
  });

  it('from a Concern: shows the notice, prefills only a changeable subject, and sends the lineage ID', () => {
    const origin = { originatingConcernId: concernId, familyGroupId: otherGroupId, subjectProfileId: bId };
    const { root, fixture } = start(origin, otherGroupId);
    const notice = heading(root, 'Starting a separate Incident')!.closest('section')!;
    expect(text(notice)).toContain('It does not change the Concern.');
    expect(text(notice)).toContain('Nothing from the Concern is copied: no entries, history, sources, or people.');
    expect(text(notice)).toContain("The Concern's participants are not added");
    expect(text(notice)).toContain('Choosing someone as the subject does not share the Incident with them.');
    const subject = control<HTMLSelectElement>(root, 'Who is this Incident about?');
    expect(subject.value).toBe(bId);
    expect(control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?').value).toBe('');
    expect(Array.from(group(root, 'People with access').querySelectorAll<HTMLInputElement>('input')).some(box => box.checked)).toBe(false);
    type(subject, cId);
    type(control<HTMLTextAreaElement>(root, 'Why are you starting this Incident?'), 'Coordinating follow-up.');
    control(root, /^Unknown/).click();
    control(root, 'Source is unknown').click();
    fixture.detectChanges();
    button(root, 'Create Incident')!.click();
    const request = http.expectOne(`/api/family-groups/${otherGroupId}/incidents`);
    expect(request.request.body).toEqual({
      subjectProfileId: cId, reason: 'Coordinating follow-up.', reasonCategory: 'Unknown', reasonSourceDescription: null,
      reasonSourceUnknown: true, participantProfileIds: [], originatingConcernId: concernId,
    });
    request.flush(created());
  });

  it('from a Concern: leaves the subject unselected when the Concern subject is not an active member', () => {
    const fixture = TestBed.createComponent(IncidentCreate);
    fixture.componentRef.setInput('familyGroupId', groupId);
    fixture.componentRef.setInput('ownProfileId', aId);
    fixture.componentRef.setInput('origin', { originatingConcernId: concernId, familyGroupId: groupId, subjectProfileId: '99999999-0000-4000-8000-000000000009' });
    fixture.detectChanges();
    http.expectOne(`/api/family-groups/${groupId}/members`).flush(members);
    fixture.detectChanges();
    expect(control<HTMLSelectElement>(fixture.nativeElement, 'Who is this Incident about?').value).toBe('');
  });
});

describe('Incident routes and navigation state', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    history.replaceState(null, '', '/');
    TestBed.configureTestingModule({
      providers: [provideRouter([{ matcher: incidentsMatcher, component: Incidents }]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    http.verify();
    history.replaceState(null, '', '/');
  });

  it('accepts only well-formed ID-only navigation state', () => {
    expect(readOrigin({ originatingConcernId: concernId, familyGroupId: groupId, subjectProfileId: bId }))
      .toEqual({ originatingConcernId: concernId, familyGroupId: groupId, subjectProfileId: bId });
    expect(readOrigin({ originatingConcernId: concernId, familyGroupId: groupId })).toBeNull();
    expect(readOrigin({ originatingConcernId: 'reason text', familyGroupId: groupId, subjectProfileId: bId })).toBeNull();
    expect(readOrigin(null)).toBeNull();
  });

  it('loads the list on /incidents and reports an ended session', async () => {
    const harness = await RouterTestingHarness.create('/incidents');
    http.expectOne('/api/incidents').flush({}, { status: 401, statusText: 'Unauthorized' });
    harness.detectChanges();
    expect(harness.routeNativeElement?.querySelector('[role="alert"]')?.textContent).toContain('Your session may have ended');
  });

  it('starts from a Concern through navigation state only, and falls back to direct creation without it', async () => {
    const harness = await RouterTestingHarness.create('/incidents');
    http.expectOne('/api/incidents').flush([]);
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/incidents/new', { state: { originatingConcernId: concernId, familyGroupId: otherGroupId, subjectProfileId: bId } });
    harness.detectChanges();
    expect(router.url).toBe('/incidents/new');
    expect(router.url).not.toContain(concernId);
    http.expectOne(`/api/family-groups/${otherGroupId}/members`).flush(members);
    harness.detectChanges();
    expect(heading(harness.routeNativeElement!, 'Starting a separate Incident')).toBeDefined();

    await router.navigateByUrl('/incidents');
    http.expectOne('/api/incidents').flush([]);
    await router.navigateByUrl('/incidents/new');
    harness.detectChanges();
    // No origin and no current Family input in this isolated harness: no members request and no notice.
    expect(heading(harness.routeNativeElement!, 'Starting a separate Incident')).toBeUndefined();
    expect(heading(harness.routeNativeElement!, 'Start an Incident')).toBeDefined();
  });
});
