import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Subject, forkJoin, takeUntil } from 'rxjs';
export interface SafetyCircleView {
  safetyCircleId: string; familyGroupId: string; name: string; version: string;
  members: { profileId: string; displayName: string }[];
}
@Component({ selector: 'app-safety-circles', imports: [FormsModule], templateUrl: './safety-circles.html', styleUrl: './safety-circles.scss' })
export class SafetyCircles implements OnChanges {
  @Input() familyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  protected readonly state = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly circles = signal<SafetyCircleView[]>([]);
  protected readonly people = signal<{ profileId: string; displayName: string }[]>([]);
  protected readonly busy = signal(false);
  protected readonly formOpen = signal(false);
  protected readonly editing = signal<SafetyCircleView | null>(null);
  protected readonly removing = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected name = '';
  ngOnChanges(): void { this.refresh(); }
  protected get url(): string { return '/api/family-groups/' + encodeURIComponent(this.familyId!) + '/safety-circles'; }
  protected refresh(preserve = false): void {
    this.changed.next(); this.cancel(); this.busy.set(false); this.circles.set([]); this.people.set([]); this.error.set('');
    if (!preserve) this.message.set('');
    if (!this.familyId || !this.ownProfileId) { this.state.set('unavailable'); return; }
    this.state.set('loading');
    forkJoin({ circles: this.http.get<SafetyCircleView[]>(this.url), people: this.http.get<{ profileId: string; displayName: string }[]>('/api/family-groups/' + encodeURIComponent(this.familyId) + '/members') })
      .pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: data => { this.circles.set(data.circles); this.people.set(data.people); this.state.set('ready'); },
        error: (e: HttpErrorResponse) => this.state.set(e.status === 401 || e.status === 404 ? 'unavailable' : 'error'),
      });
  }
  protected cancel(): void { this.formOpen.set(false); this.editing.set(null); this.removing.set(null); this.name = ''; }
  protected open(row: SafetyCircleView | null = null): void {
    if (this.busy() || this.state() !== 'ready') return;
    this.cancel(); this.editing.set(row); this.name = row?.name ?? ''; this.formOpen.set(true); this.error.set('');
  }
  protected valid(): boolean { return this.name.trim().length > 0 && this.name.trim().length <= 100 && !/[\u0000-\u001f\u007f]/.test(this.name); }
  protected save(): void {
    if (!this.valid() || this.busy() || this.state() !== 'ready') return;
    const row = this.editing();
    this.mutate(row ? this.http.put(this.url + '/' + row.safetyCircleId, { name: this.name.trim(), version: row.version })
      : this.http.post(this.url, { name: this.name.trim() }), row ? 'Safety Circle renamed.' : 'Safety Circle created.');
  }
  protected available(row: SafetyCircleView) { return this.people().filter(p => !row.members.some(m => m.profileId === p.profileId)); }
  protected addMember(row: SafetyCircleView, profile: string): void {
    if (profile) this.mutate(this.http.post(this.url + '/' + row.safetyCircleId + '/members', { version: row.version, memberProfileId: profile }), 'Circle member added.');
  }
  protected removeMember(row: SafetyCircleView, profile: string): void {
    this.mutate(this.http.delete(this.url + '/' + row.safetyCircleId + '/members/' + profile + '?version=' + row.version), 'Circle member removed.');
  }
  protected deleteCircle(row: SafetyCircleView): void {
    if (this.removing() !== row.safetyCircleId) return;
    this.mutate(this.http.delete(this.url + '/' + row.safetyCircleId + '?version=' + row.version), 'Safety Circle permanently deleted.');
  }
  private mutate(request: import('rxjs').Observable<unknown>, message: string): void {
    if (this.busy() || this.state() !== 'ready') return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    request.pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.message.set(message); this.refresh(true); },
      error: (e: HttpErrorResponse) => {
        this.busy.set(false);
        if (e.status === 400 || e.error?.code === 'DuplicateCircleName') {
          this.error.set(e.status === 400 ? 'Choose a plain-text name of 1-100 characters.' : 'You already have a Safety Circle with this name in this Family.'); return;
        }
        this.cancel(); this.circles.set([]); this.people.set([]);
        this.state.set(e.status === 401 || e.status === 404 ? 'unavailable' : 'error');
        this.error.set('The change could not be confirmed. Refresh your circles to check their current state and access.');
      },
    });
  }
}
