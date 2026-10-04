import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Shapes mirror src/FamilySafety.Application/Incidents/IncidentContracts.cs (WI-008-003); enums are strings.

export type IncidentState = 'Open' | 'Frozen' | 'Resolved';
export type IncidentCategory = 'Confirmed' | 'Reported' | 'Assumption' | 'Unknown' | 'Outdated';
export type KnownCategory = 'Confirmed' | 'Reported' | 'Assumption';
export type IncidentEntryKind = 'Observation' | 'Unknown' | 'Summary' | 'ContactAttempt' | 'TaskResult' | 'Correction' | 'Challenge';
export type IncidentContactMethod = 'Call' | 'TextMessage' | 'Email' | 'InAppMessage' | 'InPerson' | 'Other';
export type IncidentContactOutcome = 'Reached' | 'NoAnswer' | 'LeftMessage' | 'SentNoReplyYet' | 'Other' | 'Unknown';
export type IncidentTaskStatus = 'Open' | 'Done' | 'Cancelled';
export type IncidentTimelineItemType = 'IncidentCreated' | 'ParticipantAdded' | 'CoordinatorHandedOff' | 'Entry'
  | 'TaskCreated' | 'TaskCompleted' | 'TaskCancelled' | 'ParticipantRemoved' | 'ParticipantAccessEnded' | 'Frozen' | 'Resolved';

export interface IncidentSummary {
  incidentId: string;
  familyGroupId: string;
  familyGroupName: string;
  subjectProfileId: string;
  subjectName: string;
  state: IncidentState;
  coordinatorProfileId: string;
  coordinatorName: string;
  createdAt: string;
  lastAuthorizedMutationAt: string;
  expiresAt: string | null;
}

export interface IncidentParticipant {
  profileId: string;
  displayName: string;
  isCoordinator: boolean;
  selectedAt: string;
}

export interface IncidentEntry {
  entryId: string;
  kind: IncidentEntryKind;
  category: IncidentCategory;
  isOutdated: boolean;
  text: string | null;
  sourceDescription: string | null;
  sourceUnknown: boolean;
  contributorProfileId: string;
  contributorName: string;
  recordedAt: string;
  eventAt: string | null;
  eventTimeUnknown: boolean;
  relatedEntryId: string | null;
  relatedTaskId: string | null;
  explanation: string | null;
  attemptedByDescription: string | null;
  targetLabel: string | null;
  contactMethod: IncidentContactMethod | null;
  contactOutcome: IncidentContactOutcome | null;
}

export interface IncidentTask {
  taskId: string;
  description: string;
  assigneeProfileId: string;
  assigneeName: string;
  status: IncidentTaskStatus;
  notCompleted: boolean;
  createdByProfileId: string;
  createdByName: string;
  createdAt: string;
  completedAt: string | null;
  resultEntryId: string | null;
  cancelledAt: string | null;
  cancelledByProfileId: string | null;
  cancelledByName: string | null;
  cancelReason: string | null;
}

export interface IncidentResolution {
  summary: string;
  outcomeText: string;
  outcomeCategory: IncidentCategory;
  outcomeSourceDescription: string | null;
  outcomeSourceUnknown: boolean;
  contactRestoredAt: string | null;
  contactRestoredTimeUnknown: boolean;
  assistanceProvided: string | null;
  unresolvedFollowUp: string | null;
  lessons: string | null;
  resolvedByProfileId: string;
  resolvedByName: string;
  resolvedAt: string;
}

export interface IncidentTimelineItem {
  itemId: string;
  type: IncidentTimelineItemType;
  eventAt: string | null;
  recordedAt: string;
  actorProfileId: string | null;
  actorName: string;
  entryKind: IncidentEntryKind | null;
  category: IncidentCategory | null;
  isOutdated: boolean;
  entryId: string | null;
  relatedEntryId: string | null;
  taskId: string | null;
  affectedProfileId: string | null;
  affectedName: string | null;
  /** Returned by the server but never displayed (nondisclosure, WI-008-004). */
  freezeReason: string | null;
}

export interface Incident {
  incidentId: string;
  familyGroupId: string;
  familyGroupName: string;
  subjectProfileId: string;
  subjectName: string;
  createdByProfileId: string;
  createdByName: string;
  coordinatorProfileId: string;
  coordinatorName: string;
  state: IncidentState;
  createdAt: string;
  lastAuthorizedMutationAt: string;
  inactivityFreezeAt: string | null;
  frozenAt: string | null;
  /** Returned by the server but never displayed (nondisclosure, WI-008-004). */
  freezeReason: string | null;
  expiresAt: string | null;
  resolution: IncidentResolution | null;
  originatingConcernId: string | null;
  participants: IncidentParticipant[];
  entries: IncidentEntry[];
  tasks: IncidentTask[];
  timeline: IncidentTimelineItem[];
}

export interface FamilyMember {
  profileId: string;
  displayName: string;
}

export interface CreateIncidentRequest {
  subjectProfileId: string;
  reason: string;
  reasonCategory: IncidentCategory;
  reasonSourceDescription: string | null;
  reasonSourceUnknown: boolean;
  participantProfileIds: string[];
  originatingConcernId: string | null;
}

export interface AddIncidentEntryRequest {
  kind: IncidentEntryKind;
  category: IncidentCategory;
  text: string | null;
  sourceDescription: string | null;
  sourceUnknown: boolean;
  eventAt: string | null;
  eventTimeUnknown: boolean;
  relatedEntryId: string | null;
  explanation: string | null;
  originalCategory: KnownCategory | null;
  attemptedByDescription: string | null;
  targetLabel: string | null;
  contactMethod: IncidentContactMethod | null;
  contactOutcome: IncidentContactOutcome | null;
}

export interface CompleteIncidentTaskRequest {
  resultText: string;
  category: IncidentCategory;
  sourceDescription: string | null;
  sourceUnknown: boolean;
  eventAt: string | null;
}

export interface ResolveIncidentRequest {
  summary: string;
  outcomeText: string;
  outcomeCategory: IncidentCategory;
  outcomeSourceDescription: string | null;
  outcomeSourceUnknown: boolean;
  contactRestoredAt: string | null;
  contactRestoredTimeUnknown: boolean;
  assistanceProvided: string | null;
  unresolvedFollowUp: string | null;
  lessons: string | null;
}

/** Typed client for the ten WI-008-003 Incident routes. The server is the only authorization authority. */
@Injectable({ providedIn: 'root' })
export class IncidentApi {
  private readonly http = inject(HttpClient);

  list(): Observable<IncidentSummary[]> {
    return this.http.get<IncidentSummary[]>('/api/incidents');
  }

  get(incidentId: string): Observable<Incident> {
    return this.http.get<Incident>(`/api/incidents/${encodeURIComponent(incidentId)}`);
  }

  members(familyGroupId: string): Observable<FamilyMember[]> {
    return this.http.get<FamilyMember[]>(`/api/family-groups/${encodeURIComponent(familyGroupId)}/members`);
  }

  create(familyGroupId: string, request: CreateIncidentRequest): Observable<Incident> {
    return this.http.post<Incident>(`/api/family-groups/${encodeURIComponent(familyGroupId)}/incidents`, request);
  }

  replaceParticipants(incidentId: string, participantProfileIds: string[]): Observable<Incident> {
    return this.http.put<Incident>(`${this.base(incidentId)}/participants`, { participantProfileIds });
  }

  transferCoordinator(incidentId: string, participantProfileId: string): Observable<Incident> {
    return this.http.post<Incident>(`${this.base(incidentId)}/coordinator`, { participantProfileId });
  }

  addEntry(incidentId: string, request: AddIncidentEntryRequest): Observable<Incident> {
    return this.http.post<Incident>(`${this.base(incidentId)}/entries`, request);
  }

  createTask(incidentId: string, description: string, assigneeProfileId: string): Observable<Incident> {
    return this.http.post<Incident>(`${this.base(incidentId)}/tasks`, { description, assigneeProfileId });
  }

  completeTask(incidentId: string, taskId: string, request: CompleteIncidentTaskRequest): Observable<Incident> {
    return this.http.post<Incident>(`${this.base(incidentId)}/tasks/${encodeURIComponent(taskId)}/complete`, request);
  }

  cancelTask(incidentId: string, taskId: string, reason: string): Observable<Incident> {
    return this.http.post<Incident>(`${this.base(incidentId)}/tasks/${encodeURIComponent(taskId)}/cancel`, { reason });
  }

  resolve(incidentId: string, request: ResolveIncidentRequest): Observable<Incident> {
    return this.http.post<Incident>(`${this.base(incidentId)}/resolve`, request);
  }

  private base(incidentId: string): string {
    return `/api/incidents/${encodeURIComponent(incidentId)}`;
  }
}
