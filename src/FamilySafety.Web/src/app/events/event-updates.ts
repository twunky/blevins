import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, takeUntil } from 'rxjs';
import { AuthorizedParticipationEvent } from './event-participation';

export interface EventUpdate {
  updateId: string; authorProfileId: string; authorName: string; text: string;
  createdAtUtc: string; updatedAtUtc: string | null; version: string;
}
export interface EventUpdatesView { canManage: boolean; updates: EventUpdate[] }

/** App supplies an authorized Event; server projections independently enforce
 * current access and exclude all hidden content and prior text. */
@Component({
  selector: 'app-event-updates', imports: [FormsModule, DatePipe],
  templateUrl: './event-updates.html', styleUrl: './event-updates.scss',
})
export class EventUpdates implements OnChanges {
  @Input({ required: true }) event!: AuthorizedParticipationEvent;
  @Input({ required: true }) ownProfileId: string | null = null;
  @Input({ required: true }) currentFamilyId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly contextChanged = new Subject<void>();
  private generation = 0;
  protected readonly status = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly data = signal<EventUpdatesView | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly message = signal('');
  protected readonly needsRefresh = signal(false);
  protected readonly editing = signal<EventUpdate | null>(null);
  protected readonly hiding = signal<string | null>(null);
  protected text = '';

  ngOnChanges(): void { this.refresh(); }

  refresh(preserveMessage = false): void {
    ++this.generation;
    this.contextChanged.next();
    this.data.set(null); this.busy.set(false); this.error.set(''); this.needsRefresh.set(false);
    this.cancelEdit(); this.hiding.set(null);
    if (!preserveMessage) this.message.set('');
    if (!this.ownProfileId || this.currentFamilyId !== this.event.familyGroupId) {
      this.status.set('unavailable'); return;
    }
    this.status.set('loading');
    const generation = this.generation;
    this.http.get<EventUpdatesView>(this.url()).pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: data => { if (generation === this.generation) { this.data.set(data); this.status.set('ready'); } },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.status.set(error.status === 404 || error.status === 401 ? 'unavailable' : 'error');
      },
    });
  }
  protected edit(update: EventUpdate): void {
    this.hiding.set(null); this.editing.set(update); this.text = update.text;
  }
  protected cancelEdit(): void { this.editing.set(null); this.text = ''; }
  protected save(): void {
    if (!this.data()?.canManage || !this.text.trim() || this.text.trim().length > 1000) return;
    const update = this.editing();
    const fields = { text: this.text.trim() };
    this.mutate(update ? this.http.put(`${this.url()}/${update.updateId}`, { ...fields, version: update.version })
      : this.http.post(this.url(), fields), update ? 'Update saved.' : 'Update posted.');
  }
  protected hide(update: EventUpdate): void {
    if (!this.data()?.canManage) return;
    this.mutate(this.http.post(`${this.url()}/${update.updateId}/hide`, { version: update.version }), 'Update hidden.');
  }
  private mutate(write: Observable<unknown>, success: string): void {
    if (this.busy() || this.needsRefresh() || this.status() !== 'ready') return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    const generation = this.generation;
    write.pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { if (generation === this.generation) { this.message.set(success); this.refresh(true); } },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.busy.set(false); this.hiding.set(null);
        if (error.status === 400) { this.error.set('Enter Update text of up to 1,000 characters.'); return; }
        this.needsRefresh.set(true);
        if (error.status === 404 || error.status === 401) {
          this.data.set(null); this.cancelEdit(); this.status.set('unavailable');
        }
        this.error.set(error.status === 409 ? 'This Update changed. Refresh Updates before trying again.'
          : 'Your change could not be saved. Refresh Updates to check current state and access.');
      },
    });
  }
  private url(): string { return `/api/events/${encodeURIComponent(this.event.eventId)}/updates`; }
}
