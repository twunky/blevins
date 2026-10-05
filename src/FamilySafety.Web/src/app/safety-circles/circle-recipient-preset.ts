import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Component, DestroyRef, Input, OnChanges, Output, EventEmitter, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, takeUntil } from 'rxjs';
import { SafetyCircleView } from './safety-circles';

/** Optional creation convenience. Output contains profile IDs only, never an authorization source. */
@Component({ selector: 'app-circle-recipient-preset', imports: [FormsModule], template: `
  <button type="button" class="button--secondary" (click)="load()" [disabled]="disabled || state() === 'loading'">Use a Safety Circle (optional)</button>
  @if (state() === 'loading') { <p role="status">Loading Safety Circle recipients...</p> }
  @if (state() === 'error') { <p role="alert">Safety Circles are unavailable. Try again or choose recipients manually.</p> }
  @if (state() === 'ready') {
    <label [for]="controlId">Safety Circle preset</label>
    <select [id]="controlId" [ngModel]="selectedId()" [ngModelOptions]="{standalone: true}" [disabled]="disabled" (ngModelChange)="select($event)">
      <option value="">Choose a circle</option>
      @for (circle of circles(); track circle.safetyCircleId) { <option [value]="circle.safetyCircleId">{{ circle.name }}</option> }
    </select>
    @if (!circles().length) { <p>You have no Safety Circles in this Family.</p> }
  }
  <p>Choosing a circle replaces the draft recipients. Review and adjust the recipients below before confirming. Later circle changes do not change this item's recipients.</p>
  `, styles: [':host { display: grid; gap: var(--space-3); } select { width: 100%; min-width: 0; } p { overflow-wrap: anywhere; }'] })
export class CircleRecipientPreset implements OnChanges {
  @Input() familyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  @Input() controlId = 'circle-preset';
  @Input() disabled = false;
  @Output() recipients = new EventEmitter<string[]>();
  @Output() loading = new EventEmitter<boolean>();
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  protected readonly state = signal<'idle' | 'loading' | 'ready' | 'error'>('idle');
  protected readonly circles = signal<SafetyCircleView[]>([]);
  protected readonly selectedId = signal('');
  ngOnChanges(changes: import('@angular/core').SimpleChanges): void {
    if (changes['familyId'] || changes['ownProfileId']) { this.changed.next(); this.circles.set([]); this.selectedId.set(''); this.state.set('idle'); this.loading.emit(false); }
  }
  private get url(): string { return '/api/family-groups/' + encodeURIComponent(this.familyId!) + '/safety-circles'; }
  protected load(): void {
    if (!this.familyId || !this.ownProfileId || this.disabled) return;
    this.changed.next(); this.circles.set([]); this.selectedId.set(''); this.state.set('loading'); this.loading.emit(true);
    this.http.get<SafetyCircleView[]>(this.url).pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: rows => { this.circles.set(rows); this.state.set('ready'); this.loading.emit(false); },
      error: () => { this.state.set('error'); this.loading.emit(false); },
    });
  }
  protected select(id: string): void {
    this.changed.next(); this.selectedId.set(id);
    if (!id) { this.recipients.emit([]); return; }
    this.state.set('loading'); this.loading.emit(true);
    // Fresh owner-authorized read removes stale/ineligible members even if list was loaded earlier.
    this.http.get<SafetyCircleView>(this.url + '/' + encodeURIComponent(id)).pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: row => { this.recipients.emit(row.members.map(m => m.profileId).filter(p => p !== this.ownProfileId)); this.state.set('ready'); this.loading.emit(false); },
      error: () => { this.circles.set([]); this.selectedId.set(''); this.state.set('error'); this.loading.emit(false); },
    });
  }
}
