import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type ConcernState = 'Open' | 'Frozen' | 'Resolved';
export type ConcernFreezeReason = 'Inactivity' | 'CreatorEligibilityLost';
export type ConcernEntryKind = 'Observation' | 'Unknown' | 'Correction' | 'Challenge';
export type ConcernCategory = 'Confirmed' | 'Reported' | 'Assumption' | 'Unknown' | 'Outdated';
export type OriginalConcernCategory = 'Confirmed' | 'Reported' | 'Assumption';

export interface ConcernPerson {
  profileId: string;
  displayName: string;
}

export interface ConcernEntry {
  entryId: string;
  kind: ConcernEntryKind;
  category: ConcernCategory;
  text: string;
  sourceDescription: string | null;
  sourceUnknown: boolean;
  contributorProfileId: string;
  contributorName: string;
  recordedAt: string;
  eventAt: string | null;
  relatedEntryId: string | null;
  explanation: string | null;
  isOutdated: boolean;
}

export interface Concern {
  concernId: string;
  familyGroupId: string;
  familyGroupName: string;
  creatorProfileId: string;
  creatorName: string;
  subjectProfileId: string;
  subjectName: string;
  creationReason: string;
  state: ConcernState;
  createdAt: string;
  lastAuthorizedMutationAt: string;
  frozenAt: string | null;
  freezeReason: ConcernFreezeReason | null;
  resolvedAt: string | null;
  resolutionNote: string | null;
  expiresAt: string | null;
  participants: ConcernPerson[];
  entries: ConcernEntry[];
}

export interface ActiveFamilyMember {
  profileId: string;
  displayName: string;
}

export interface CreateConcernRequest {
  subjectProfileId: string;
  reason: string;
  participantProfileIds: string[];
}

export interface AddConcernEntryRequest {
  kind: ConcernEntryKind;
  category: ConcernCategory;
  text: string;
  sourceDescription: string | null;
  sourceUnknown: boolean;
  eventAt: string | null;
  relatedEntryId: string | null;
  explanation: string | null;
  isOutdated: boolean;
  originalCategory: OriginalConcernCategory | null;
}

export interface ReplaceConcernParticipantsRequest {
  participantProfileIds: string[];
}

export interface ResolveConcernRequest {
  note: string;
}

@Injectable({ providedIn: 'root' })
export class ConcernApi {
  private readonly http = inject(HttpClient);

  list(): Observable<Concern[]> {
    return this.http.get<Concern[]>('/api/concerns');
  }

  get(concernId: string): Observable<Concern> {
    return this.http.get<Concern>(`/api/concerns/${encodeURIComponent(concernId)}`);
  }

  members(familyGroupId: string): Observable<ActiveFamilyMember[]> {
    return this.http.get<ActiveFamilyMember[]>(
      `/api/family-groups/${encodeURIComponent(familyGroupId)}/members`,
    );
  }

  create(familyGroupId: string, request: CreateConcernRequest): Observable<Concern> {
    return this.http.post<Concern>(
      `/api/family-groups/${encodeURIComponent(familyGroupId)}/concerns`, request,
    );
  }

  replaceParticipants(concernId: string, request: ReplaceConcernParticipantsRequest): Observable<Concern> {
    return this.http.put<Concern>(
      `/api/concerns/${encodeURIComponent(concernId)}/participants`, request,
    );
  }

  addEntry(concernId: string, request: AddConcernEntryRequest): Observable<Concern> {
    return this.http.post<Concern>(
      `/api/concerns/${encodeURIComponent(concernId)}/entries`, request,
    );
  }

  resolve(concernId: string, request: ResolveConcernRequest): Observable<Concern> {
    return this.http.post<Concern>(
      `/api/concerns/${encodeURIComponent(concernId)}/resolve`, request,
    );
  }
}
