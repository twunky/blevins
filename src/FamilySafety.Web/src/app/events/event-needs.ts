import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, takeUntil } from 'rxjs';
import { AuthorizedParticipationEvent } from './event-participation';

export type NeedStatus = 'Open' | 'Claimed' | 'Assigned' | 'Completed';
export type NeedCategory = 'Food' | 'Supplies' | 'Help' | 'Transport' | 'Other';
export interface EventNeed {
  needId: string; title: string; detail: string | null; category: NeedCategory | null;
  status: NeedStatus; responsibleProfileId: string | null; responsibleName: string | null;
  isResponsible: boolean; version: string;
  history: { profileId: string; displayName: string; priorStatus: NeedStatus; status: NeedStatus; reason: string; occurredAtUtc: string }[];
}
export interface EventNeedsView {
  canManage: boolean; canParticipate: boolean;
  eligibleParticipants: { profileId: string; displayName: string }[]; needs: EventNeed[];
}

/** Needs use the already authorized Event boundary; the server independently
 * authorizes every request and filters responsibility identities before delivery. */
@Component({
  selector: 'app-event-needs', imports: [FormsModule, DatePipe],
  templateUrl: './event-needs.html', styleUrl: './event-needs.scss',
})
export class EventNeeds implements OnChanges {
  @Input({ required: true }) event!: AuthorizedParticipationEvent;
  @Input({ required: true }) ownProfileId: string | null = null;
  @Input({ required: true }) currentFamilyId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly contextChanged = new Subject<void>();
  private generation = 0;
  protected readonly status = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly data = signal<EventNeedsView | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly message = signal('');
  protected readonly needsRefresh = signal(false);
  protected readonly editing = signal<EventNeed | null>(null);
  protected readonly removing = signal<string | null>(null);
  protected readonly completing = signal<string | null>(null);
  protected readonly categories: readonly NeedCategory[] = ['Food', 'Supplies', 'Help', 'Transport', 'Other'];
  protected title = '';
  protected detail = '';
  protected category: NeedCategory | '' = '';
  protected assignees: Record<string, string> = {};

  ngOnChanges(): void { this.refresh(); }

  refresh(preserveMessage = false): void {
    ++this.generation;
    this.contextChanged.next();
    this.data.set(null); this.busy.set(false); this.error.set(''); this.needsRefresh.set(false);
    this.cancelEdit(); this.removing.set(null); this.completing.set(null); this.assignees = {};
    if (!preserveMessage) this.message.set('');
    if (!this.ownProfileId || this.currentFamilyId !== this.event.familyGroupId) {
      this.status.set('unavailable'); return;
    }
    this.status.set('loading');
    const generation = this.generation;
    this.http.get<EventNeedsView>(this.url()).pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: data => { if (generation === this.generation) { this.data.set(data); this.status.set('ready'); } },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.status.set(error.status === 404 || error.status === 401 ? 'unavailable' : 'error');
      },
    });
  }

  protected edit(need: EventNeed): void {
    this.editing.set(need); this.title = need.title; this.detail = need.detail ?? ''; this.category = need.category ?? '';
  }
  protected cancelEdit(): void { this.editing.set(null); this.title = ''; this.detail = ''; this.category = ''; }
  protected save(): void {
    if (!this.data()?.canManage || !this.title.trim()) return;
    const need = this.editing();
    const fields = { title: this.title.trim(), detail: this.detail.trim() || null, category: this.category || null };
    this.mutate(need ? this.http.put(`${this.url()}/${need.needId}`, { ...fields, version: need.version })
      : this.http.post(this.url(), fields), need ? 'Need updated.' : 'Need added.');
  }
  protected act(need: EventNeed, action: string): void {
    const profileId = this.assignees[need.needId];
    if (action === 'assign' && !profileId) return;
    this.mutate(this.http.post(`${this.url()}/${need.needId}/${action}`,
      { version: need.version, ...(action === 'assign' ? { profileId } : {}) }), {
        claim: 'You claimed this Need.', release: 'Need released.', assign: 'Assignment requested. Acceptance is pending.',
        accept: 'Assignment accepted.', decline: 'Assignment declined.', complete: 'Need completed.', remove: 'Need removed.',
      }[action] ?? 'Need updated.');
  }
  protected reason(reason: string): string {
    return ({ claim: 'Claimed', assign: 'Assignment requested', accept: 'Assignment accepted', decline: 'Assignment declined',
      release: 'Released', complete: 'Completed', RsvpNotGoing: 'Released after RSVP changed to Not Going',
      InvitationRevoked: 'Released after invitation revocation', EventVisibilityLost: 'Released after Event access ended',
      MembershipLost: 'Released after Family membership ended' } as Record<string, string>)[reason] ?? 'Responsibility changed';
  }
  private mutate(write: Observable<unknown>, success: string): void {
    if (this.busy() || this.needsRefresh() || this.status() !== 'ready') return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    const generation = this.generation;
    write.pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { if (generation === this.generation) { this.message.set(success); this.refresh(true); } },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.busy.set(false); this.removing.set(null); this.completing.set(null);
        if (error.status === 400) {
          this.error.set('Enter a title of up to 200 characters and details of up to 1,000 characters.'); return;
        }
        this.needsRefresh.set(true);
        if (error.status === 404 || error.status === 401) {
          this.data.set(null); this.cancelEdit(); this.assignees = {}; this.status.set('unavailable');
        }
        this.error.set(error.status === 409 ? 'This Need changed. Refresh Needs before trying again.'
          : 'Your change could not be saved. Refresh Needs to check current state and access.');
      },
    });
  }
  private url(): string { return `/api/events/${encodeURIComponent(this.event.eventId)}/needs`; }
}
