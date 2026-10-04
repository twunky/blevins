import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ItemFamilyOutcome } from '../item-route';
import { Incident, IncidentEntry, IncidentTask, IncidentTimelineItem } from './incident-api';
import { IncidentDetail } from './incident-detail';

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
function buttons(root: Element, name: string | RegExp): HTMLButtonElement[] {
  return Array.from(root.querySelectorAll('button')).filter(item => matches(norm(item.getAttribute('aria-label') ?? text(item)), name));
}
const button = (root: Element, name: string | RegExp) => buttons(root, name)[0];
function heading(root: Element, name: string | RegExp): HTMLElement | undefined {
  return Array.from(root.querySelectorAll<HTMLElement>('h2, h3, h4')).find(item => matches(text(item), name));
}
function control<T extends HTMLElement = HTMLInputElement>(root: Element, label: string | RegExp): T {
  const found = Array.from(root.querySelectorAll('label')).find(item => matches(text(item), label));
  if (!found) throw new Error(`No label ${label}`);
  return (found.htmlFor ? root.querySelector(`#${found.htmlFor}`) : found.querySelector('input, select, textarea')) as T;
}
function group(root: Element, legend: string | RegExp): HTMLFieldSetElement | undefined {
  return Array.from(root.querySelectorAll('fieldset')).find(item => matches(text(item.querySelector('legend')), legend));
}
function type(element: HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement, value: string): void {
  element.value = value;
  element.dispatchEvent(new Event(element instanceof HTMLSelectElement ? 'change' : 'input'));
}
const timelineList = (root: Element) => root.querySelector('ol[aria-label="Incident timeline, oldest first"]')!;

const id = '50000000-0000-4000-8000-000000000005';
const groupId = '30000000-0000-4000-8000-000000000001';
const aId = 'a0000000-0000-4000-8000-00000000000a';
const bId = 'b0000000-0000-4000-8000-00000000000b';
const cId = 'c0000000-0000-4000-8000-00000000000c';
const concernId = '40000000-0000-4000-8000-000000000004';
const t = (minute: number) => `2026-10-01T14:${String(minute).padStart(2, '0')}:00Z`;

function entry(overrides: Partial<IncidentEntry>): IncidentEntry {
  return {
    entryId: 'e-reason', kind: 'Summary', category: 'Reported', isOutdated: false, text: 'No reply since the expected arrival.',
    sourceDescription: 'Adult A', sourceUnknown: false, contributorProfileId: aId, contributorName: 'Adult A', recordedAt: t(0),
    eventAt: null, eventTimeUnknown: true, relatedEntryId: null, relatedTaskId: null, explanation: null,
    attemptedByDescription: null, targetLabel: null, contactMethod: null, contactOutcome: null, ...overrides,
  };
}
function item(overrides: Partial<IncidentTimelineItem>): IncidentTimelineItem {
  return {
    itemId: 'x', type: 'Entry', eventAt: null, recordedAt: t(0), actorProfileId: aId, actorName: 'Adult A', entryKind: null,
    category: null, isOutdated: false, entryId: null, relatedEntryId: null, taskId: null, affectedProfileId: null, affectedName: null,
    freezeReason: null, ...overrides,
  };
}
const entryItem = (value: IncidentEntry) => item({
  itemId: value.entryId, recordedAt: value.recordedAt, eventAt: value.eventAt, actorProfileId: value.contributorProfileId,
  actorName: value.contributorName, entryKind: value.kind, category: value.category, isOutdated: value.isOutdated,
  entryId: value.entryId, relatedEntryId: value.relatedEntryId, taskId: value.relatedTaskId,
});
function task(overrides: Partial<IncidentTask>): IncidentTask {
  return {
    taskId: 'task-1', description: 'Call the hotel front desk', assigneeProfileId: cId, assigneeName: 'Adult C', status: 'Open',
    notCompleted: false, createdByProfileId: aId, createdByName: 'Adult A', createdAt: t(5), completedAt: null, resultEntryId: null,
    cancelledAt: null, cancelledByProfileId: null, cancelledByName: null, cancelReason: null, ...overrides,
  };
}

const reason = entry({});
const finding = entry({ entryId: 'e-finding', kind: 'Observation', text: 'Adult C saw a message at noon.', contributorProfileId: cId,
  contributorName: 'Adult C', sourceDescription: 'Adult C', recordedAt: t(3), eventAt: '2026-10-01T12:00:00Z', eventTimeUnknown: false });

function incident(overrides: Partial<Incident> = {}): Incident {
  const entries = overrides.entries ?? [reason, finding];
  return {
    incidentId: id, familyGroupId: groupId, familyGroupName: 'Family', subjectProfileId: bId, subjectName: 'Adult B',
    createdByProfileId: aId, createdByName: 'Adult A', coordinatorProfileId: aId, coordinatorName: 'Adult A', state: 'Open',
    createdAt: t(0), lastAuthorizedMutationAt: t(3), inactivityFreezeAt: null, frozenAt: null, freezeReason: null, expiresAt: null,
    resolution: null, originatingConcernId: null,
    participants: [
      { profileId: aId, displayName: 'Adult A', isCoordinator: true, selectedAt: t(0) },
      { profileId: cId, displayName: 'Adult C', isCoordinator: false, selectedAt: t(1) },
    ],
    entries, tasks: [],
    timeline: [
      item({ itemId: id, type: 'IncidentCreated' }),
      item({ itemId: 'g-c', type: 'ParticipantAdded', recordedAt: t(1), affectedProfileId: cId, affectedName: 'Adult C' }),
      ...entries.map(entryItem),
    ],
    ...overrides,
  };
}

describe('Incident detail', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  function open(value: Incident | null, ownProfileId = aId): { fixture: ComponentFixture<IncidentDetail>; root: HTMLElement } {
    const fixture = TestBed.createComponent(IncidentDetail);
    fixture.componentRef.setInput('incidentId', id);
    fixture.componentRef.setInput('ownProfileId', ownProfileId);
    fixture.detectChanges();
    const request = http.expectOne(`/api/incidents/${id}`);
    if (value) request.flush(value); else request.flush({}, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    return { fixture, root: fixture.nativeElement as HTMLElement };
  }
  function refresh(fixture: ComponentFixture<IncidentDetail>) { fixture.detectChanges(); }

  describe('header and lineage', () => {
    it('shows subject, state, reason with provenance, coordinator, started-by, and times as cards without tabs', () => {
      const { root } = open(incident());
      expect(root.querySelector<HTMLAnchorElement>('a[href="/incidents"]')?.textContent?.trim()).toBe('Back to Incidents');
      expect(button(root, 'Back to Incidents')).toBeUndefined();
      expect(heading(root, 'Incident about Adult B')).toBeDefined();
      const header = heading(root, 'Incident about Adult B')!.closest('section')!;
      expect(text(header)).toContain('Open Coordination is open');
      expect(text(heading(root, 'Reason for starting')!.parentElement!))
        .toBe('Reason for starting No reply since the expected arrival. Reported Source: Adult A');
      expect(text(header)).toContain('Coordinator: Adult A (you) · Started by Adult A');
      expect(text(header)).toContain('Created Oct 1, 2026');
      expect(heading(root, 'Current summary')).toBeUndefined();
      expect(root.querySelector('[role="tablist"], [role="tab"]')).toBeNull();
      for (const title of ['Timeline', 'People with access', 'Contribute', 'Tasks', 'Coordination']) expect(heading(root, title)).toBeDefined();
    });

    it('shows the latest summary as current only when it is newer than the reason', () => {
      const update = entry({ entryId: 'e-sum2', text: 'Adult C is checking with the hotel.', recordedAt: t(9), category: 'Assumption' });
      const { root } = open(incident({ entries: [reason, update] }));
      expect(text(heading(root, 'Current summary')!.parentElement!)).toContain('Adult C is checking with the hotel. Assumption');
    });

    it('shows only "Started from a Concern" and a link to the Concern URL when lineage is returned (WI-009-005)', () => {
      const { root } = open(incident({ originatingConcernId: concernId }));
      const header = heading(root, 'Incident about Adult B')!.closest('section')!;
      expect(text(header)).toContain('Started from a Concern Open the Concern');
      const link = Array.from(root.querySelectorAll('a')).find(item => text(item) === 'Open the Concern')!;
      // The Concern is authorized on its own when opened; the link carries its ID only.
      expect(link.getAttribute('href')).toBe(`/concerns/${concernId}`);
      expect(button(root, 'Open the Concern')).toBeUndefined();
    });

    it('shows nothing about lineage when the API returns null', () => {
      const { root } = open(incident({ originatingConcernId: null }));
      expect(text(root)).not.toMatch(/Concern/);
      expect(root.querySelector('a[href^="/concerns"]')).toBeNull();
    });
  });

  describe('timeline', () => {
    it('renders every item type in server order with a text label, actor or System, and both times', () => {
      const outdated = entry({ entryId: 'e-old', kind: 'Observation', category: 'Reported', isOutdated: true, recordedAt: t(4), text: 'Was at the station.' });
      const unknown = entry({ entryId: 'e-unk', kind: 'Unknown', category: 'Unknown', recordedAt: t(5), text: 'Whether the train left.', sourceUnknown: true, sourceDescription: null });
      const contact = entry({ entryId: 'e-call', kind: 'ContactAttempt', category: 'Reported', text: 'Voicemail', recordedAt: t(6), targetLabel: 'Adult B',
        contactMethod: 'Call', contactOutcome: 'NoAnswer', attemptedByDescription: 'Grandma' });
      const result = entry({ entryId: 'e-result', kind: 'TaskResult', text: 'Desk confirmed check-in.', recordedAt: t(8), relatedTaskId: 'task-1', contributorProfileId: cId, contributorName: 'Adult C' });
      const correction = entry({ entryId: 'e-corr', kind: 'Correction', text: 'Saw it at 1 PM.', recordedAt: t(10), relatedEntryId: 'e-finding',
        explanation: 'Checked my phone.', contributorProfileId: cId, contributorName: 'Adult C' });
      const challenge = entry({ entryId: 'e-chal', kind: 'Challenge', text: 'I heard from the desk too.', recordedAt: t(11), relatedEntryId: 'e-finding', explanation: 'Separate call.' });
      const entries = [reason, finding, outdated, unknown, contact, result, correction, challenge];
      const doneTask = task({ status: 'Done', completedAt: t(8), resultEntryId: 'e-result' });
      const cancelled = task({ taskId: 'task-2', description: 'Check the bus', status: 'Cancelled', cancelledAt: t(12), cancelledByProfileId: aId, cancelledByName: 'Adult A', cancelReason: 'Not needed' });
      const value = incident({
        state: 'Frozen', frozenAt: t(20), freezeReason: 'CoordinatorEligibilityLost', entries, tasks: [doneTask, cancelled],
        timeline: [
          item({ itemId: id, type: 'IncidentCreated' }),
          item({ itemId: 'g-c', type: 'ParticipantAdded', recordedAt: t(1), affectedProfileId: cId, affectedName: 'Adult C' }),
          ...entries.slice(0, 5).map(entryItem),
          item({ itemId: 'task-1', type: 'TaskCreated', recordedAt: t(7), taskId: 'task-1', affectedName: 'Adult C' }),
          item({ itemId: 'task-1', type: 'TaskCompleted', recordedAt: t(8), taskId: 'task-1', actorName: 'Adult C', affectedName: 'Adult C' }),
          entryItem(result), entryItem(correction), entryItem(challenge),
          item({ itemId: 'task-2', type: 'TaskCancelled', recordedAt: t(12), taskId: 'task-2', affectedName: 'Adult C' }),
          item({ itemId: 'h1', type: 'CoordinatorHandedOff', recordedAt: t(13), affectedName: 'Adult C' }),
          item({ itemId: 'g-c2', type: 'ParticipantRemoved', recordedAt: t(14), affectedName: 'Adult D' }),
          item({ itemId: 'g-c3', type: 'ParticipantAccessEnded', recordedAt: t(15), actorProfileId: null, actorName: 'System', affectedName: 'Adult E' }),
          item({ itemId: id, type: 'Frozen', recordedAt: t(20), actorProfileId: null, actorName: 'System', freezeReason: 'CoordinatorEligibilityLost' }),
        ],
      });
      const { root } = open(value);
      const items = Array.from(timelineList(root).children).map(node => text(node));
      expect(items).toHaveLength(17);
      expect(items[0]).toContain('Incident started Adult A started this Incident about Adult B');
      expect(items[1]).toContain('Person added Adult A added Adult C');
      expect(items[2]).toMatch(/^Coordinator summary Adult A Reported/);
      expect(items[3]).toMatch(/^Finding Adult C Reported/);
      expect(items[3]).toContain('Event time Oct 1, 2026');
      expect(items[3]).toContain('Recorded Oct 1, 2026');
      expect(items[4]).toContain('Reported · Outdated');
      expect(items[5]).toMatch(/^Unknown Adult A Unknown/);
      expect(items[5]).toContain('Source unknown');
      expect(items[5]).toContain('Event time unknown');
      expect(items[6]).toContain('Contact attempt Grandma tried to reach Adult B · Call · No answer');
      expect(items[6]).toContain('Recorded by Adult A');
      expect(items[7]).toContain('Task added Adult A added a task for Adult C Call the hotel front desk');
      expect(items[8]).toContain('Task completed Adult C completed a task');
      expect(items[9]).toContain('Task result Adult C');
      expect(items[9]).toContain('Result for the task “Call the hotel front desk”');
      expect(items[10]).toContain("Correction to Adult C's entry from Oct 1, 2026");
      expect(items[10]).toContain('Explanation: Checked my phone.');
      expect(items[11]).toMatch(/^Another report Adult A/);
      expect(items[12]).toContain('Task cancelled Adult A cancelled a task Check the bus Reason: Not needed');
      expect(items[13]).toContain('Coordination handed off Adult A handed coordination to Adult C');
      expect(items[14]).toContain('Person removed Adult A removed Adult D');
      expect(items[15]).toContain("Access ended Adult E's access ended System · Recorded");
      expect(items[16]).toContain('Coordination stopped Coordination stopped. This record is now read-only. System');
      expect(text(root)).not.toMatch(/Eligibility|Inactivity|eligib/i);
      expect(timelineList(root).querySelectorAll('time[datetime]').length).toBeGreaterThan(17);
      const link = Array.from(timelineList(root).querySelectorAll('a')).find(anchor => text(anchor).startsWith('Correction to'))!;
      expect(link.getAttribute('href')).toBe('#incident-entry-e-finding');
      expect(root.querySelector('#incident-entry-e-finding')).not.toBeNull();
    });
  });

  describe('contributions', () => {
    it('adds a finding with an explicit Unknown event time and announces it', () => {
      const { fixture, root } = open(incident(), cId);
      button(root, 'Add a finding').click();
      refresh(fixture);
      expect(heading(root, 'Add a finding')).toBeDefined();
      type(control<HTMLTextAreaElement>(root, 'Information'), 'Bus arrived at 6.');
      control(root, /^Reported/).click();
      type(control(root, 'Source description'), 'Bus tracker');
      refresh(fixture);
      button(root, 'Add finding').click();
      const request = http.expectOne(`/api/incidents/${id}/entries`);
      expect(request.request.body).toEqual(expect.objectContaining({
        kind: 'Observation', category: 'Reported', text: 'Bus arrived at 6.', sourceDescription: 'Bus tracker', sourceUnknown: false,
        eventAt: null, eventTimeUnknown: true, relatedEntryId: null, originalCategory: null, targetLabel: null, contactMethod: null,
      }));
      request.flush(incident());
      refresh(fixture);
      expect(root.querySelector('[role="status"]')?.textContent).toContain('Finding added.');
      expect(heading(root, 'Add a finding')).toBeUndefined();
    });

    it('sends Outdated with its earlier classification and offers no numeric confidence', () => {
      const { fixture, root } = open(incident(), cId);
      button(root, 'Add a finding').click();
      refresh(fixture);
      const categories = group(root, 'How is this known?')!;
      expect(Array.from(categories.querySelectorAll('label strong')).map(node => node.textContent)).toEqual(['Confirmed', 'Reported', 'Assumption', 'Outdated']);
      expect(root.querySelector('input[type="range"], input[type="number"]')).toBeNull();
      control(root, /^Outdated/).click();
      refresh(fixture);
      type(control<HTMLSelectElement>(root, 'Earlier classification'), 'Assumption');
      type(control<HTMLTextAreaElement>(root, 'Information'), 'Was at the station earlier.');
      control(root, 'Source is unknown').click();
      refresh(fixture);
      button(root, 'Add finding').click();
      const request = http.expectOne(`/api/incidents/${id}/entries`);
      expect(request.request.body).toEqual(expect.objectContaining({ category: 'Outdated', originalCategory: 'Assumption', sourceUnknown: true, sourceDescription: null }));
      request.flush(incident());
    });

    it('records an Unknown with a fixed category and validates required text', () => {
      const { fixture, root } = open(incident(), cId);
      button(root, 'Record an Unknown').click();
      refresh(fixture);
      expect(group(root, 'How is this known?')).toBeUndefined();
      expect(text(root)).toContain('An Unknown records that no reliable answer is available yet.');
      button(root, 'Record Unknown').click();
      refresh(fixture);
      const unknownText = control<HTMLTextAreaElement>(root, 'What is unknown');
      expect(unknownText.getAttribute('aria-invalid')).toBe('true');
      expect(root.querySelector(`#${unknownText.getAttribute('aria-describedby')}`)?.textContent).toContain('Enter information');
      http.expectNone(`/api/incidents/${id}/entries`);
      type(unknownText, 'Whether the bus left.');
      control(root, 'Source is unknown').click();
      refresh(fixture);
      button(root, 'Record Unknown').click();
      const request = http.expectOne(`/api/incidents/${id}/entries`);
      expect(request.request.body).toEqual(expect.objectContaining({ kind: 'Unknown', category: 'Unknown' }));
      request.flush(incident());
    });

    it('offers summary updates only to the coordinator and redraws the current summary', () => {
      expect(button(open(incident(), cId).root, 'Post a summary update')).toBeUndefined();
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] });
      http = TestBed.inject(HttpTestingController);
      const { fixture, root } = open(incident(), aId);
      button(root, 'Post a summary update').click();
      refresh(fixture);
      type(control<HTMLTextAreaElement>(root, 'Summary'), 'Hotel confirms arrival.');
      control(root, /^Confirmed/).click();
      type(control(root, 'Source description'), 'Hotel desk');
      refresh(fixture);
      button(root, 'Post summary').click();
      const request = http.expectOne(`/api/incidents/${id}/entries`);
      expect(request.request.body).toEqual(expect.objectContaining({ kind: 'Summary', category: 'Confirmed' }));
      const latest = entry({ entryId: 'e-s2', text: 'Hotel confirms arrival.', category: 'Confirmed', recordedAt: t(30) });
      request.flush(incident({ entries: [reason, finding, latest] }));
      refresh(fixture);
      expect(text(heading(root, 'Current summary')!.parentElement!)).toContain('Hotel confirms arrival.');
    });

    it('offers correction only on own entries and another report only on others, never Edit or Delete', () => {
      const { fixture, root } = open(incident(), cId);
      const findingItem = root.querySelector('#incident-entry-e-finding')!;
      const reasonItem = root.querySelector('#incident-entry-e-reason')!;
      expect(button(findingItem, 'Add a correction')).toBeDefined();
      expect(button(findingItem, 'Add another report')).toBeUndefined();
      expect(button(reasonItem, 'Add another report')).toBeDefined();
      expect(button(reasonItem, 'Add a correction')).toBeUndefined();
      expect(buttons(root, /^(Edit|Delete)/)).toHaveLength(0);
      button(findingItem, 'Add a correction').click();
      refresh(fixture);
      expect(text(root)).toContain('This adds a separate entry linked to the original. The original stays as it was.');
      type(control<HTMLTextAreaElement>(root, 'Corrected information'), 'It was at 1 PM.');
      control(root, /^Reported/).click();
      type(control(root, 'Source description'), 'Adult C');
      refresh(fixture);
      button(root, 'Add correction').click();
      refresh(fixture);
      expect(text(root)).toContain('Enter an explanation of 1–1,000 characters.');
      http.expectNone(`/api/incidents/${id}/entries`);
      type(control<HTMLTextAreaElement>(root, 'Explanation'), 'Checked my phone.');
      button(root, 'Add correction').click();
      const request = http.expectOne(`/api/incidents/${id}/entries`);
      expect(request.request.body).toEqual(expect.objectContaining({ kind: 'Correction', relatedEntryId: 'e-finding', explanation: 'Checked my phone.' }));
      request.flush(incident());
    });

    it('records a contact attempt with exactly one time choice, a free-text target, and no contact actions', () => {
      const { fixture, root } = open(incident(), aId);
      button(root, 'Record a contact attempt').click();
      refresh(fixture);
      expect(text(root)).toContain('A name or short label. Do not enter phone numbers or addresses.');
      expect(control(root, 'Person or place contacted').value).toBe('');
      expect(buttons(root, /^(Call|Text|Email|Contact)\b/)).toHaveLength(0);
      expect(root.querySelector('a[href^="tel:"], a[href^="sms:"], a[href^="mailto:"]')).toBeNull();
      expect(Array.from(control<HTMLSelectElement>(root, 'Method').options).map(option => text(option)))
        .toEqual(['Choose a method', 'Call', 'Text message', 'Email', 'In-app message', 'In person', 'Other']);
      expect(Array.from(control<HTMLSelectElement>(root, 'Outcome').options).map(option => text(option)))
        .toEqual(['Choose an outcome', 'Reached', 'No answer', 'Left message', 'Sent — no reply yet', 'Other', 'Unknown']);
      control(root, 'Someone else').click();
      refresh(fixture);
      expect(control(root, /^Reported/).checked).toBe(true);
      type(control(root, 'Who attempted contact'), 'Grandma');
      type(control(root, 'Person or place contacted'), 'Hotel front desk');
      type(control<HTMLSelectElement>(root, 'Method'), 'Call');
      type(control<HTMLSelectElement>(root, 'Outcome'), 'Reached');
      control(root, 'Source is unknown').click();
      refresh(fixture);
      button(root, 'Record contact attempt').click();
      refresh(fixture);
      expect(text(root)).toContain('Enter when the attempt happened, or choose Time unknown.');
      http.expectNone(`/api/incidents/${id}/entries`);
      control(root, 'Time unknown').click();
      refresh(fixture);
      expect(root.querySelector('#composer-event-time')).toBeNull();
      button(root, 'Record contact attempt').click();
      const request = http.expectOne(`/api/incidents/${id}/entries`);
      expect(request.request.body).toEqual(expect.objectContaining({
        kind: 'ContactAttempt', category: 'Reported', text: null, attemptedByDescription: 'Grandma', targetLabel: 'Hotel front desk',
        contactMethod: 'Call', contactOutcome: 'Reached', eventAt: null, eventTimeUnknown: true,
      }));
      request.flush(incident());
      refresh(fixture);
      expect(text(root)).toContain('Open Coordination is open');
    });
  });

  describe('tasks', () => {
    it('shows every status, creator, completion link, cancellation, and the no-edit helper text', () => {
      const tasks = [
        task({}),
        task({ taskId: 'task-2', description: 'Check the bus', status: 'Done', completedAt: t(8), resultEntryId: 'e-finding' }),
        task({ taskId: 'task-3', description: 'Ask the neighbor', status: 'Cancelled', cancelledAt: t(9), cancelledByName: 'Adult A', cancelReason: 'Not needed' }),
      ];
      const { root } = open(incident({ tasks }), aId);
      const card = heading(root, 'Tasks')!.closest('section')!;
      const rows = Array.from(card.querySelectorAll('li')).map(node => text(node));
      expect(rows[0]).toMatch(/^Call the hotel front desk Open Assigned to Adult C Added by Adult A · Oct 1, 2026/);
      expect(rows[1]).toContain('Done');
      expect(rows[1]).toContain('Completed Oct 1, 2026');
      expect(rows[1]).toContain('View result for “Check the bus”');
      expect(rows[2]).toContain('Cancelled by Adult A');
      expect(rows[2]).toContain('Reason: Not needed');
      expect(text(card)).toContain("Tasks can't be edited or reassigned. To change one, cancel it and add a new task.");
      expect(buttons(card, /Edit|Reassign/)).toHaveLength(0);
    });

    it('lets the coordinator assign to any current participant, including themself', () => {
      const { fixture, root } = open(incident(), aId);
      button(root, 'Add a task').click();
      refresh(fixture);
      const assignee = control<HTMLSelectElement>(root, 'Assign to');
      expect(Array.from(assignee.options).map(option => text(option))).toEqual(['Choose a person with access', 'Adult A (you)', 'Adult C']);
      type(control<HTMLTextAreaElement>(root, 'Task description'), 'Call the hotel front desk');
      type(assignee, cId);
      button(root, 'Add task').click();
      const request = http.expectOne(`/api/incidents/${id}/tasks`);
      expect(request.request.body).toEqual({ description: 'Call the hotel front desk', assigneeProfileId: cId });
      request.flush(incident({ tasks: [task({})] }));
      refresh(fixture);
      expect(root.querySelector('[role="status"]')?.textContent).toContain('Task added.');
    });

    it('lets a participant add a task only for themself', () => {
      const { fixture, root } = open(incident(), cId);
      button(root, 'Add a task for yourself').click();
      refresh(fixture);
      expect(() => control(root, 'Assign to')).toThrow();
      expect(text(root)).toContain('Assigned to you');
      type(control<HTMLTextAreaElement>(root, 'Task description'), 'Check the bus');
      button(root, 'Add task').click();
      const request = http.expectOne(`/api/incidents/${id}/tasks`);
      expect(request.request.body).toEqual({ description: 'Check the bus', assigneeProfileId: cId });
      request.flush(incident());
    });

    it('shows Complete only to the assignee and requires a categorized result', () => {
      const value = incident({ tasks: [task({})] });
      expect(button(open(value, aId).root, /^Complete…/)).toBeUndefined();
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] });
      http = TestBed.inject(HttpTestingController);
      const { fixture, root } = open(value, cId);
      button(root, /^Complete…/).click();
      refresh(fixture);
      button(root, 'Complete task').click();
      refresh(fixture);
      expect(text(root)).toContain('Enter a result of 1–2,000 characters.');
      expect(text(root)).toContain('Choose how this information is known.');
      http.expectNone(`/api/incidents/${id}/tasks/task-1/complete`);
      type(control<HTMLTextAreaElement>(root, 'Result'), 'Desk confirmed check-in.');
      control(root, /^Confirmed/).click();
      type(control(root, 'Source description'), 'Front desk');
      refresh(fixture);
      button(root, 'Complete task').click();
      const request = http.expectOne(`/api/incidents/${id}/tasks/task-1/complete`);
      expect(request.request.body).toEqual({ resultText: 'Desk confirmed check-in.', category: 'Confirmed', sourceDescription: 'Front desk', sourceUnknown: false, eventAt: null });
      request.flush(value);
    });

    it('limits Cancel to the coordinator, or a participant on their own self-assigned task, and requires a reason', () => {
      const tasks = [task({}), task({ taskId: 'task-self', description: 'My own check', createdByProfileId: cId, createdByName: 'Adult C' })];
      const participant = open(incident({ tasks }), cId).root;
      expect(buttons(participant, /^Cancel task…/).map(item => text(item))).toEqual(['Cancel task… My own check']);
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] });
      http = TestBed.inject(HttpTestingController);
      const { fixture, root } = open(incident({ tasks }), aId);
      expect(buttons(root, /^Cancel task…/)).toHaveLength(2);
      buttons(root, /^Cancel task…/)[0].click();
      refresh(fixture);
      button(root, 'Cancel task').click();
      refresh(fixture);
      expect(text(root)).toContain('Enter a reason of 1–1,000 characters.');
      type(control<HTMLTextAreaElement>(root, 'Reason for cancelling'), 'Covered by another call.');
      button(root, 'Cancel task').click();
      const request = http.expectOne(`/api/incidents/${id}/tasks/task-1/cancel`);
      expect(request.request.body).toEqual({ reason: 'Covered by another call.' });
      request.flush(incident());
    });
  });

  describe('people with access and handoff', () => {
    it('labels roles, notes the subject has no access unless added, and lists no former participants', () => {
      const { root } = open(incident());
      const card = heading(root, 'People with access')!.closest('section')!;
      expect(Array.from(card.querySelectorAll('li')).map(node => text(node))).toEqual(['Adult A (you) Coordinator', 'Adult C Participant']);
      expect(text(card)).toContain('Adult B is the subject of this Incident and does not have access unless added.');
    });

    it('marks an explicitly added subject', () => {
      const value = incident({ participants: [...incident().participants, { profileId: bId, displayName: 'Adult B', isCoordinator: false, selectedAt: t(2) }] });
      const card = heading(open(value).root, 'People with access')!.closest('section')!;
      expect(text(card)).toContain('Adult B Participant · subject, added explicitly');
      expect(text(card)).not.toContain('does not have access unless added');
    });

    it('edits the full set without offering or sending the coordinator', () => {
      const { fixture, root } = open(incident(), aId);
      button(root, 'Manage people with access').click();
      http.expectOne(`/api/family-groups/${groupId}/members`).flush([
        { profileId: aId, displayName: 'Adult A' }, { profileId: bId, displayName: 'Adult B' }, { profileId: cId, displayName: 'Adult C' },
      ]);
      refresh(fixture);
      const editor = group(root, 'People with access')!;
      const boxes = Array.from(editor.querySelectorAll<HTMLInputElement>('input[type="checkbox"]'));
      expect(boxes.map(box => text(box.closest('label')))).toEqual(['Adult B (subject)', 'Adult C']);
      expect(boxes.map(box => box.checked)).toEqual([false, true]);
      expect(text(editor)).toContain('People you remove lose access right away.');
      expect(text(editor)).toContain('old access is never restored');
      expect(text(editor)).toContain("The coordinator can't be removed. To step back, hand off coordination first.");
      expect(buttons(root, /Remove/)).toHaveLength(0);
      control(root, /^Adult B/).click();
      button(root, 'Save people with access').click();
      const request = http.expectOne(`/api/incidents/${id}/participants`);
      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual({ participantProfileIds: [cId, bId] });
      request.flush(incident());
    });

    it('hands off in two explicit steps with Escape to go back, then redraws for the new coordinator', () => {
      const { fixture, root } = open(incident(), aId);
      button(root, 'Hand off coordination…').click();
      refresh(fixture);
      const choose = group(root, 'Hand off coordination to')!;
      expect(Array.from(choose.querySelectorAll('label')).map(node => text(node))).toEqual(['Adult C']);
      expect(text(choose)).toContain('becomes the only coordinator, right away');
      expect(text(choose)).toContain('You will stay on this Incident as a participant. There is no shared coordination.');
      control(root, 'Adult C').click();
      refresh(fixture);
      button(root, 'Continue').click();
      refresh(fixture);
      expect(heading(root, 'Hand off to Adult C?')).toBeDefined();
      heading(root, 'Hand off to Adult C?')!.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
      refresh(fixture);
      expect(heading(root, 'Hand off to Adult C?')).toBeUndefined();
      expect(group(root, 'Hand off coordination to')).toBeDefined();
      button(root, 'Continue').click();
      refresh(fixture);
      button(root, 'Hand off coordination').click();
      const request = http.expectOne(`/api/incidents/${id}/coordinator`);
      expect(request.request.body).toEqual({ participantProfileId: cId });
      request.flush(incident({
        coordinatorProfileId: cId, coordinatorName: 'Adult C',
        participants: [{ profileId: aId, displayName: 'Adult A', isCoordinator: false, selectedAt: t(0) }, { profileId: cId, displayName: 'Adult C', isCoordinator: true, selectedAt: t(1) }],
      }));
      refresh(fixture);
      expect(root.querySelector('[role="status"]')?.textContent).toContain('Adult C now coordinates this Incident.');
      expect(heading(root, 'Coordination')).toBeUndefined();
      expect(button(root, 'Manage people with access')).toBeUndefined();
      expect(button(root, 'Post a summary update')).toBeUndefined();
      expect(button(root, 'Add a task for yourself')).toBeDefined();
    });

    it('explains when nobody is available for a handoff', () => {
      const { fixture, root } = open(incident({ participants: [incident().participants[0]] }), aId);
      expect(text(root)).toContain('Only you have access. Add people when you are ready.');
      button(root, 'Hand off coordination…').click();
      refresh(fixture);
      expect(text(root)).toContain('Add a person under People with access before handing off');
      expect(button(root, 'Continue')).toBeUndefined();
    });
  });

  describe('lifecycle', () => {
    it('Frozen: neutral read-only notice, kept-until date, Not completed tasks, no controls, no reason', () => {
      const value = incident({ state: 'Frozen', frozenAt: t(30), freezeReason: 'Inactivity', expiresAt: '2027-10-01T12:00:00Z', tasks: [task({ notCompleted: true })] });
      const { root } = open(value, aId);
      const header = heading(root, 'Incident about Adult B')!.closest('section')!;
      expect(text(header)).toContain('Frozen Read-only record');
      expect(text(header)).toContain('Coordination in this Incident is no longer editable. Its history stays available to people with access until Oct 1, 2027, then it is removed.');
      expect(text(header)).toContain('To continue coordinating, start a new Incident.');
      expect(text(heading(root, 'Tasks')!.closest('section')!)).toContain('Not completed');
      for (const name of [/Add a/, /Record/, /Post a summary/, /Manage people/, /Hand off/, /Resolve/, /Complete…/, /Cancel task/, /another report/, /correction/])
        expect(buttons(root, name)).toHaveLength(0);
      expect(heading(root, 'Contribute')).toBeUndefined();
      expect(text(root)).not.toMatch(/Inactivity|inactiv|Eligibility/i);
      expect(text(header)).not.toMatch(/safe|danger|verified|resolved/i);
    });

    it('Resolved: every returned resolution field, Unknown time, coordination-ended line, kept-until, no verdict or reopen', () => {
      const value = incident({
        state: 'Resolved', expiresAt: '2027-10-01T12:00:00Z',
        resolution: {
          summary: 'Adult B reached the hotel.', outcomeText: 'Adult B called from the lobby.', outcomeCategory: 'Reported',
          outcomeSourceDescription: 'Adult C', outcomeSourceUnknown: false, contactRestoredAt: null, contactRestoredTimeUnknown: true,
          assistanceProvided: 'Hotel desk checked.', unresolvedFollowUp: 'Charge the phone.', lessons: 'Share the itinerary.',
          resolvedByProfileId: cId, resolvedByName: 'Adult C', resolvedAt: t(40),
        },
      });
      const { root } = open(value, aId);
      const card = heading(root, 'Resolution')!.closest('section')!;
      const cardText = text(card);
      for (const expected of ['Resolution summary Adult B reached the hotel.', 'Outcome Adult B called from the lobby. Reported Source: Adult C',
        'Contact restored or resolution time Unknown', 'Assistance provided Hotel desk checked.', 'Follow-up still open Charge the phone.',
        'Lessons Share the itinerary.', 'Resolved by Adult C · Oct 1, 2026', 'Kept until Oct 1, 2027, then removed.',
        'Resolution means coordination ended. It does not confirm that anyone is safe or that information was verified.'])
        expect(cardText).toContain(expected);
      expect(text(root)).toContain('Resolved Coordination ended');
      expect(text(root)).not.toMatch(/All clear|Safe and|Dangerous|Emergency resolved|Reopen/i);
      expect(buttons(root, /Reopen|Add a|Record|Resolve|Hand off|Manage/)).toHaveLength(0);
    });

    it('resolution requires its fields and exactly one time choice, then sends the explicit final action', () => {
      const { fixture, root } = open(incident(), aId);
      button(root, 'Resolve this Incident…').click();
      refresh(fixture);
      expect(text(root)).toContain("Resolving ends coordination for everyone with access. It can't be reopened. Resolving does not mean anyone is confirmed safe.");
      expect(Array.from(group(root, 'How is the outcome known?')!.querySelectorAll('label strong')).map(node => node.textContent))
        .toEqual(['Confirmed', 'Reported', 'Assumption', 'Unknown']);
      button(root, 'Resolve Incident').click();
      refresh(fixture);
      for (const message of ['Enter a resolution summary', 'Enter an outcome', 'Choose how this information is known.', 'or choose Unknown.'])
        expect(text(root)).toContain(message);
      http.expectNone(`/api/incidents/${id}/resolve`);
      type(control<HTMLTextAreaElement>(root, 'Resolution summary'), 'Coordination is complete.');
      type(control<HTMLTextAreaElement>(root, 'Outcome'), 'Adult B called.');
      control(root, /^Unknown/).click();
      control(root, 'Source is unknown').click();
      control(root, 'Unknown').click();
      type(control<HTMLTextAreaElement>(root, /^Lessons/), 'Share plans.');
      refresh(fixture);
      button(root, 'Resolve Incident').click();
      const request = http.expectOne(`/api/incidents/${id}/resolve`);
      expect(request.request.body).toEqual({
        summary: 'Coordination is complete.', outcomeText: 'Adult B called.', outcomeCategory: 'Unknown', outcomeSourceDescription: null,
        outcomeSourceUnknown: true, contactRestoredAt: null, contactRestoredTimeUnknown: true, assistanceProvided: null,
        unresolvedFollowUp: null, lessons: 'Share plans.',
      });
      request.flush(incident({ state: 'Resolved', resolution: {
        summary: 'Coordination is complete.', outcomeText: 'Adult B called.', outcomeCategory: 'Unknown', outcomeSourceDescription: null,
        outcomeSourceUnknown: true, contactRestoredAt: null, contactRestoredTimeUnknown: true, assistanceProvided: null,
        unresolvedFollowUp: null, lessons: 'Share plans.', resolvedByProfileId: aId, resolvedByName: 'Adult A', resolvedAt: t(50) } }));
      refresh(fixture);
      expect(root.querySelector('[role="status"]')?.textContent).toContain('Incident resolved.');
      expect(heading(root, 'Resolution')).toBeDefined();
    });
  });

  describe('authorize-first Family decision (REQ-UX-014, WI-009-005)', () => {
    function openWithAdopt(outcome: Promise<ItemFamilyOutcome>, incidentId = id) {
      const calls: [string, string][] = [];
      const fixture = TestBed.createComponent(IncidentDetail);
      fixture.componentRef.setInput('incidentId', incidentId);
      fixture.componentRef.setInput('ownProfileId', aId);
      fixture.componentRef.setInput('adoptItemFamily', (familyGroupId: string, kind: string) => { calls.push([familyGroupId, kind]); return outcome; });
      fixture.detectChanges();
      return { fixture, calls, root: fixture.nativeElement as HTMLElement };
    }

    it('decides the Family only after the item read returns, and renders only after that decision', async () => {
      let decide!: (value: ItemFamilyOutcome) => void;
      const { fixture, calls, root } = openWithAdopt(new Promise(resolve => decide = resolve));
      expect(calls).toEqual([]);
      http.expectOne(`/api/incidents/${id}`).flush(incident({ familyGroupName: 'Name from the item response' }));
      fixture.detectChanges();
      expect(calls).toEqual([[groupId, 'incident']]);
      expect(heading(root, 'Incident about Adult B')).toBeUndefined();
      expect(text(root)).not.toContain('Adult B');
      decide('switched');
      await fixture.whenStable();
      fixture.detectChanges();
      expect(heading(root, 'Incident about Adult B')).toBeDefined();
    });

    it('shows the generic unavailable state when the Family is not one of the caller’s Families (OD-B)', async () => {
      const { fixture, root } = openWithAdopt(Promise.resolve('unavailable'));
      let shown = 0;
      fixture.componentInstance.unavailableShown.subscribe(() => shown++);
      http.expectOne(`/api/incidents/${id}`).flush(incident());
      await fixture.whenStable();
      fixture.detectChanges();
      expect(heading(root, 'This Incident is unavailable')).toBeDefined();
      expect(text(root)).not.toContain('Adult B');
      expect(shown).toBe(1);
    });

    it('shows the unavailable state for a non-GUID ID without any request or Family decision', () => {
      const { fixture, calls, root } = openWithAdopt(Promise.resolve('current'), 'not-a-guid');
      http.expectNone(() => true);
      fixture.detectChanges();
      expect(heading(root, 'This Incident is unavailable')).toBeDefined();
      expect(calls).toEqual([]);
    });

    it('offers Retry for a transient read failure and repeats the same read before any Family decision', async () => {
      const { fixture, calls, root } = openWithAdopt(Promise.resolve('current'));
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 503, statusText: 'Unavailable' });
      fixture.detectChanges();
      expect(text(root)).toContain('This Incident could not be opened. Please try again.');
      expect(heading(root, 'This Incident is unavailable')).toBeUndefined();
      expect(calls).toEqual([]);

      button(root, 'Retry loading Incident').click();
      fixture.detectChanges();
      expect(text(root)).toContain('Loading Incident…');
      expect(calls).toEqual([]);
      http.expectOne(`/api/incidents/${id}`).flush(incident());
      await fixture.whenStable();
      fixture.detectChanges();
      expect(calls).toEqual([[groupId, 'incident']]);
      expect(heading(root, 'Incident about Adult B')).toBeDefined();
      expect(button(root, 'Retry loading Incident')).toBeUndefined();
    });

    it('shows the same generic unavailable state, with no Retry, when a Retry returns 404', () => {
      const { fixture, calls, root } = openWithAdopt(Promise.resolve('current'));
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 503, statusText: 'Unavailable' });
      fixture.detectChanges();
      button(root, 'Retry loading Incident').click();
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 404, statusText: 'Not Found' });
      fixture.detectChanges();
      expect(heading(root, 'This Incident is unavailable')).toBeDefined();
      expect(button(root, 'Retry loading Incident')).toBeUndefined();
      expect(calls).toEqual([]);
    });

    it('never asks for a Family decision when the read is 404', () => {
      const { fixture, calls, root } = openWithAdopt(Promise.resolve('switched'));
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 404, statusText: 'Not Found' });
      fixture.detectChanges();
      expect(calls).toEqual([]);
      expect(heading(root, 'This Incident is unavailable')).toBeDefined();
    });
  });

  describe('unavailable and save denial', () => {
    it('shows one generic unavailable state for a detail 404', () => {
      const fixture = TestBed.createComponent(IncidentDetail);
      let shown = 0;
      fixture.componentInstance.unavailableShown.subscribe(() => shown++);
      fixture.componentRef.setInput('incidentId', id);
      fixture.detectChanges();
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 404, statusText: 'Not Found' });
      fixture.detectChanges();
      const root = fixture.nativeElement as HTMLElement;
      expect(heading(root, 'This Incident is unavailable')).toBeDefined();
      expect(text(root)).toContain('Only Incidents currently shared with you can be opened.');
      expect(button(root, 'Return to Incidents')).toBeDefined();
      expect(shown).toBe(1);
    });

    it('refetches after a write 404 and redraws with a neutral message when the Incident still loads', () => {
      const { fixture, root } = open(incident(), cId);
      button(root, 'Add a finding').click();
      refresh(fixture);
      type(control<HTMLTextAreaElement>(root, 'Information'), 'Draft text');
      control(root, /^Reported/).click();
      type(control(root, 'Source description'), 'Me');
      button(root, 'Add finding').click();
      http.expectOne(`/api/incidents/${id}/entries`).flush({}, { status: 404, statusText: 'Not Found' });
      http.expectOne(`/api/incidents/${id}`).flush(incident({ state: 'Frozen' }));
      refresh(fixture);
      expect(text(root)).toContain('That change could not be made. This Incident has been refreshed with its current details.');
      expect(text(root)).toContain('Frozen Read-only record');
      expect(heading(root, 'This Incident is unavailable')).toBeUndefined();
    });

    it('keeps the draft after a denied write when the form is reopened', () => {
      const { fixture, root } = open(incident(), cId);
      button(root, 'Add a finding').click();
      refresh(fixture);
      type(control<HTMLTextAreaElement>(root, 'Information'), 'Draft text');
      control(root, /^Reported/).click();
      type(control(root, 'Source description'), 'Me');
      button(root, 'Add finding').click();
      http.expectOne(`/api/incidents/${id}/entries`).flush({}, { status: 404, statusText: 'Not Found' });
      http.expectOne(`/api/incidents/${id}`).flush(incident());
      refresh(fixture);
      button(root, 'Add a finding').click();
      refresh(fixture);
      expect(control<HTMLTextAreaElement>(root, 'Information').value).toBe('Draft text');
    });

    it('shows the unavailable state when the refetch after a write 404 is also 404', () => {
      const { fixture, root } = open(incident(), aId);
      let shown = 0;
      fixture.componentInstance.unavailableShown.subscribe(() => shown++);
      button(root, 'Hand off coordination…').click();
      refresh(fixture);
      control(root, 'Adult C').click();
      refresh(fixture);
      button(root, 'Continue').click();
      refresh(fixture);
      button(root, 'Hand off coordination').click();
      http.expectOne(`/api/incidents/${id}/coordinator`).flush({}, { status: 404, statusText: 'Not Found' });
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 404, statusText: 'Not Found' });
      refresh(fixture);
      expect(heading(root, 'This Incident is unavailable')).toBeDefined();
      expect(text(root)).not.toContain('could not be made');
      expect(shown).toBe(1);
    });

    it('uses the session message for 401', () => {
      const fixture = TestBed.createComponent(IncidentDetail);
      fixture.componentRef.setInput('incidentId', id);
      fixture.detectChanges();
      http.expectOne(`/api/incidents/${id}`).flush({}, { status: 401, statusText: 'Unauthorized' });
      fixture.detectChanges();
      expect((fixture.nativeElement as HTMLElement).querySelector('[role="alert"]')?.textContent).toContain('Your session may have ended');
    });
  });
});
