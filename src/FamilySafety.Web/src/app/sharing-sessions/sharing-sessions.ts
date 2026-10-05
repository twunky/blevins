import { CircleRecipientPreset } from '../safety-circles/circle-recipient-preset';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, forkJoin, of, takeUntil } from 'rxjs';

export interface SharingSessionView {
  sharingSessionId: string; familyGroupId: string; authorProfileId: string; authorDisplayName: string;
  startedAtUtc: string; endsAtUtc: string; state: string; version: number;
  recipients: { profileId: string; displayName: string; hasAccess: boolean }[] | null;
  location: { latitude: number; longitude: number; accuracy: number; capturedAtUtc: string; receivedAtUtc: string } | null;
}
interface Person { profileId: string; displayName: string }

/** Family is creation context. Every read uses server grants, never a client Family filter. */
@Component({
  selector: 'app-sharing-sessions', imports: [CircleRecipientPreset, FormsModule, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './sharing-sessions.html', styleUrl: './sharing-sessions.scss',
})
export class SharingSessions implements OnChanges {
  @Input() familyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  @Input() summaryOnly = false;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  private readonly readsChanged = new Subject<void>();
  private generation = 0;
  private watchId: number | null = null;
  private lastAttempt = 0;
  private updating = false;
  private reading = false;
  protected readonly watching = signal<string | null>(null);
  protected readonly locationMessage = signal('');
  protected readonly state = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly mine = signal<SharingSessionView[]>([]);
  protected readonly received = signal<SharingSessionView[]>([]);
  protected readonly people = signal<Person[]>([]);
  protected readonly eligible = signal(false);
  protected readonly peopleError = signal('');
  protected readonly formOpen = signal(false);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly now = signal(Date.now());
  protected presetLoading = false;
  protected recipientIds: string[] = [];
  protected endTime = '';
  private defaultEndTime = '';
  protected readonly dateFormat = 'MMM d, y, h:mm:ss a';

  constructor() {
    const tick = setInterval(() => this.expire(), 1000);
    const poll = setInterval(() => this.load(), 15000);
    const hidden = () => {
      if (document.hidden) { this.stopWatch(); this.locationMessage.set('Location updates paused while this page is inactive.'); }
      else { this.expire(); this.load(); }
    };
    document.addEventListener('visibilitychange', hidden);
    this.destroyRef.onDestroy(() => {
      clearInterval(tick); clearInterval(poll); document.removeEventListener('visibilitychange', hidden);
      this.changed.next(); this.stopWatch();
    });
  }
  ngOnChanges(): void {
    ++this.generation; this.changed.next(); this.stopWatch(); this.reading = false; this.updating = false;
    this.mine.set([]); this.received.set([]); this.people.set([]); this.eligible.set(false);
    this.peopleError.set(''); this.error.set(''); this.message.set(''); this.locationMessage.set('');
    this.formOpen.set(false); this.busy.set(false); this.recipientIds = []; this.endTime = '';
    if (!this.ownProfileId) { this.state.set('unavailable'); return; }
    this.state.set('loading'); this.load();
    if (this.familyId && !this.summaryOnly) this.loadPeople();
  }
  protected loadPeople(): void {
    if (!this.familyId) return;
    this.eligible.set(false); this.people.set([]); this.peopleError.set('');
    this.http.get<Person[]>(`/api/family-groups/${encodeURIComponent(this.familyId)}/members`)
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: people => { this.eligible.set(people.some(p => p.profileId === this.ownProfileId)); this.people.set(people.filter(p => p.profileId !== this.ownProfileId)); },
        error: () => { this.peopleError.set('Recipient choices are unavailable. Refresh recipients to check current access.'); },
      });
  }
  protected load(): void {
    if (!this.ownProfileId || this.reading || this.busy() || this.updating || document.hidden) return;
    this.reading = true;
    const generation = this.generation;
    const requests = forkJoin({ mine: this.http.get<SharingSessionView[]>('/api/sharing-sessions/mine'),
      received: this.summaryOnly ? of([] as SharingSessionView[]) : this.http.get<SharingSessionView[]>('/api/sharing-sessions/received') });
    requests.pipe(takeUntil(this.changed), takeUntil(this.readsChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: data => {
        if (generation !== this.generation) return;
        this.reading = false; this.mine.set(data.mine); this.received.set(data.received);
        this.state.set('ready'); this.expire();
        if (this.watching() && !this.mine().some(s => s.sharingSessionId === this.watching())) this.stopWatch();
      },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.reading = false; this.mine.set([]); this.received.set([]); this.stopWatch();
        this.state.set(error.status === 401 ? 'unavailable' : 'error');
      },
    });
  }
  private expire(): void {
    this.now.set(Date.now());
    const active = (s: SharingSessionView) => Date.parse(s.endsAtUtc) > this.now() && s.state === 'Active';
    if (this.mine().some(s => !active(s)) && this.watching()) this.stopWatch();
    this.mine.update(rows => rows.filter(active)); this.received.update(rows => rows.filter(active));
  }
  protected open(): void {
    this.recipientIds = [];
    const end = new Date(Date.now() + 3600000);
    this.endTime = new Date(end.getTime() - end.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
    this.defaultEndTime = this.endTime;
    this.error.set(''); this.formOpen.set(true);
  }
  protected choose(id: string, checked: boolean): void {
    this.recipientIds = checked ? [...this.recipientIds, id] : this.recipientIds.filter(r => r !== id);
  }
  protected valid(): boolean {
    const end = Date.parse(this.endTime);
    return this.eligible() && this.recipientIds.length > 0 && end > this.now() && end <= this.now() + 86400000;
  }
  protected start(): void {
    if (this.presetLoading || !this.valid() || !this.familyId || this.busy()) return;
    this.cancelReads();
    this.busy.set(true); this.error.set('');
    this.http.post<SharingSessionView>('/api/sharing-sessions', {
      familyGroupId: this.familyId, recipientProfileIds: this.recipientIds,
      // The untouched default is exactly one hour from the authoritative server start, not a rounded picker minute.
      endsAtUtc: this.endTime === this.defaultEndTime ? null : new Date(this.endTime).toISOString(),
    }).pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: row => {
        this.busy.set(false); this.formOpen.set(false); this.mine.update(rows => [row, ...rows]);
        this.message.set('Sharing started. Location needs browser permission and may not be available yet.'); this.collect(row);
      },
      error: () => { this.busy.set(false); this.error.set('Sharing could not start. Check recipients and an end time within 24 hours.'); this.load(); },
    });
  }
  protected stop(row: SharingSessionView): void {
    if (this.busy()) return;
    // Cancel browser collection first; an in-flight server update cannot revive a stopped session.
    this.stopWatch(); this.cancelReads(); this.busy.set(true); this.error.set('');
    this.http.post<void>(`/api/sharing-sessions/${row.sharingSessionId}/stop`, { version: row.version })
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => { this.busy.set(false); this.mine.update(rows => rows.filter(s => s.sharingSessionId !== row.sharingSessionId)); this.message.set('Sharing stopped.'); this.load(); },
        error: () => { this.busy.set(false); this.error.set('Sharing could not be stopped. Refresh and retry with the current state.'); this.load(); },
      });
  }
  protected collect(row: SharingSessionView): void {
    this.stopWatch(); this.locationMessage.set('Requesting browser location permission…');
    if (!navigator.geolocation) { this.locationMessage.set('Location unavailable in this browser.'); return; }
    if (document.hidden || Date.parse(row.endsAtUtc) <= Date.now()) return;
    this.watching.set(row.sharingSessionId);
    this.lastAttempt = row.location ? Date.parse(row.location.receivedAtUtc) : 0;
    const generation = this.generation;
    this.watchId = navigator.geolocation.watchPosition(position => {
      if (generation !== this.generation || this.watching() !== row.sharingSessionId || document.hidden) return;
      const current = this.mine().find(s => s.sharingSessionId === row.sharingSessionId);
      if (!current || Date.parse(current.endsAtUtc) <= Date.now()) { this.stopWatch(); return; }
      if (this.updating || this.busy() || Date.now() - this.lastAttempt < 60000) return;
      this.cancelReads();
      this.lastAttempt = Date.now(); this.updating = true;
      this.http.put<SharingSessionView>(`/api/sharing-sessions/${row.sharingSessionId}/location`, {
        latitude: position.coords.latitude, longitude: position.coords.longitude, accuracy: position.coords.accuracy,
        capturedAtUtc: new Date(position.timestamp).toISOString(), version: current.version,
      }).pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: updated => {
          this.updating = false;
          if (this.watching() !== row.sharingSessionId) return;
          this.mine.update(rows => rows.map(s => s.sharingSessionId === updated.sharingSessionId ? updated : s));
          this.locationMessage.set('Location permission granted. Updates are limited to once a minute while this page is active.');
        },
        error: (error: HttpErrorResponse) => {
          this.updating = false;
          this.locationMessage.set('Location update was not accepted. The last accepted point may be older.');
          if (error.status === 401 || error.status === 404) { this.stopWatch(); this.mine.set([]); }
          this.load();
        },
      });
    }, error => {
      if (generation !== this.generation || this.watching() !== row.sharingSessionId) return;
      this.locationMessage.set(error.code === 1 ? 'Location permission denied. Sharing exists, but no new location can be sent.'
        : error.code === 2 ? 'Location unavailable. The browser may retry while this page is active.'
        : 'Location update temporarily failed. The browser may retry while this page is active.');
      if (error.code === 1) this.stopWatch();
    }, { enableHighAccuracy: false, maximumAge: 0, timeout: 15000 });
  }
  private stopWatch(): void {
    if (this.watchId !== null) navigator.geolocation?.clearWatch(this.watchId);
    this.watchId = null; this.watching.set(null);
  }
  private cancelReads(): void { this.readsChanged.next(); this.reading = false; }
  protected stale(row: SharingSessionView): boolean { return !!row.location && this.now() - Date.parse(row.location.capturedAtUtc) > 120000; }
}
