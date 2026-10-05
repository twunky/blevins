import { CircleRecipientPreset } from '../safety-circles/circle-recipient-preset';
import { DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { forkJoin, Subject, takeUntil } from 'rxjs';

export const checkInStatuses = [
  { value: 'ImOkay', label: "I'm okay" }, { value: 'Arrived', label: 'Arrived' },
  { value: 'LeavingNow', label: 'Leaving now' }, { value: 'Delayed', label: 'Delayed' },
  { value: 'PlansChanged', label: 'Plans changed' },
] as const;
export interface CheckInView {
  checkInId: string; familyGroupId: string; authorProfileId: string; authorDisplayName: string;
  createdAtUtc: string; status: string; note: string | null; recipientProfileIds: string[] | null;
}
interface Person { profileId: string; displayName: string }

/** Standalone workflow. Family selection helps creation only; history uses explicit server grants. */
@Component({
  selector: 'app-check-ins', imports: [CircleRecipientPreset, FormsModule, DatePipe],
  templateUrl: './check-ins.html', styleUrl: './check-ins.scss',
})
export class CheckIns implements OnChanges {
  @Input() familyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  private generation = 0;
  protected readonly state = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly mine = signal<CheckInView[]>([]);
  protected readonly received = signal<CheckInView[]>([]);
  protected readonly people = signal<Person[]>([]);
  protected readonly peopleState = signal<'loading' | 'ready' | 'error' | 'unavailable'>('unavailable');
  protected readonly formOpen = signal(false);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly statuses = checkInStatuses;
  protected status = '';
  protected note = '';
  protected presetLoading = false;
  protected recipientIds: string[] = [];

  ngOnChanges(): void { this.refresh(); }
  refresh(preserveMessage = false): void {
    ++this.generation; this.changed.next(); this.cancel(); this.busy.set(false);
    this.mine.set([]); this.received.set([]); this.people.set([]); this.error.set('');
    this.peopleState.set('unavailable');
    if (!preserveMessage) this.message.set('');
    if (!this.ownProfileId) { this.state.set('unavailable'); return; }
    this.state.set('loading');
    const generation = this.generation;
    forkJoin({ mine: this.http.get<CheckInView[]>('/api/check-ins/mine'), received: this.http.get<CheckInView[]>('/api/check-ins/received') })
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: data => {
          if (generation !== this.generation) return;
          this.mine.set(data.mine); this.received.set(data.received); this.state.set('ready');
        },
        error: (error: HttpErrorResponse) => {
          if (generation === this.generation) this.state.set(error.status === 401 ? 'unavailable' : 'error');
        },
      });
    if (this.familyId) this.loadPeople();
  }
  protected loadPeople(): void {
    if (!this.familyId) return;
    this.peopleState.set('loading');
    const generation = this.generation;
    this.http.get<Person[]>(`/api/family-groups/${encodeURIComponent(this.familyId)}/members`)
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: people => {
          if (generation !== this.generation) return;
          // A selected Family alone cannot even establish creation eligibility.
          if (!people.some(p => p.profileId === this.ownProfileId)) { this.peopleState.set('unavailable'); return; }
          this.people.set(people.filter(p => p.profileId !== this.ownProfileId)); this.peopleState.set('ready');
        },
        error: (error: HttpErrorResponse) => {
          if (generation !== this.generation) return;
          this.people.set([]); this.peopleState.set(error.status === 401 || error.status === 404 ? 'unavailable' : 'error');
        },
      });
  }
  protected create(): void { if (this.peopleState() === 'ready') { this.cancel(); this.formOpen.set(true); this.error.set(''); } }
  protected cancel(): void { this.presetLoading = false; this.formOpen.set(false); this.status = ''; this.note = ''; this.recipientIds = []; }
  protected choose(profileId: string, selected: boolean): void {
    this.recipientIds = selected ? [...this.recipientIds, profileId] : this.recipientIds.filter(id => id !== profileId);
  }
  protected label(status: string): string { return this.statuses.find(s => s.value === status)?.label ?? ''; }
  protected valid(): boolean { return this.statuses.some(s => s.value === this.status) && this.note.trim().length <= 1000 && this.peopleState() === 'ready'; }
  protected save(): void {
    if (this.presetLoading || !this.valid() || !this.familyId || this.busy() || this.state() !== 'ready') return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    const generation = this.generation;
    this.http.post<CheckInView>('/api/check-ins', {
      familyGroupId: this.familyId, status: this.status, note: this.note.trim() || null, recipientProfileIds: this.recipientIds,
    }).pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { if (generation === this.generation) { this.message.set('Check-In created.'); this.refresh(true); } },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.busy.set(false);
        if (error.status === 400) { this.error.set('Choose a status and a note of up to 1,000 characters.'); return; }
        this.cancel(); this.people.set([]); this.peopleState.set('unavailable');
        this.error.set('Your Check-In could not be saved. Refresh Check-Ins to check current state and access before trying again.');
      },
    });
  }
}
