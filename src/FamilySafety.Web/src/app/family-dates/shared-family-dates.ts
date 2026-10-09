import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AppIcon } from '../icon';
import { focusHeadingState } from '../navigation';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, takeUntil } from 'rxjs';
import { displayDate } from '../date-format';

export interface FamilyDatePerson { profileId: string; displayName: string }
export interface SharedFamilyDate {
  dateId: string; title: string; description: string; date: string; creatorProfileId: string;
  associatedPeople: FamilyDatePerson[]; canEdit: boolean; canManageAssociations: boolean; canDelete: boolean; version: string;
}
export interface SharedFamilyDatesView { canCreate: boolean; dates: SharedFamilyDate[] }

/** Owns the date workflow; selected Family is navigation, and the server supplies all permissions. */
@Component({
  selector: 'app-shared-family-dates', imports: [FormsModule, RouterLink, AppIcon],
  templateUrl: './shared-family-dates.html', styleUrls: ['./shared-family-dates.scss', '../family-summary.scss'],
})
export class SharedFamilyDates implements OnChanges {
  @Input() summary = false;
  protected readonly focusHeadingState = focusHeadingState;
  protected initials(name: string): string { return name.trim().split(/\s+/).slice(0, 2).map(part => part[0]).join('').toLocaleUpperCase(); }
  @Input({ required: true }) familyId!: string;
  @Input({ required: true }) ownProfileId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly contextChanged = new Subject<void>();
  private generation = 0;
  protected readonly status = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly data = signal<SharedFamilyDatesView | null>(null);
  protected readonly people = signal<FamilyDatePerson[]>([]);
  protected readonly peopleStatus = signal<'loading' | 'ready' | 'error'>('ready');
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly message = signal('');
  protected readonly needsRefresh = signal(false);
  protected readonly editing = signal<SharedFamilyDate | null>(null);
  protected readonly formOpen = signal(false);
  protected readonly deleting = signal<string | null>(null);
  protected readonly displayDate = displayDate;
  protected title = '';
  protected description = '';
  protected date = '';
  protected associatedIds: string[] = [];

  ngOnChanges(): void { this.refresh(); }
  refresh(preserveMessage = false): void {
    ++this.generation; this.contextChanged.next();
    this.data.set(null); this.people.set([]); this.busy.set(false); this.error.set(''); this.needsRefresh.set(false);
    this.cancel(); this.deleting.set(null);
    if (!preserveMessage) this.message.set('');
    if (!this.familyId || !this.ownProfileId) { this.status.set('unavailable'); return; }
    this.status.set('loading');
    const generation = this.generation;
    this.http.get<SharedFamilyDatesView>(this.url()).pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: view => {
        if (generation !== this.generation) return;
        if (!Array.isArray(view?.dates) || typeof view.canCreate !== 'boolean') { this.status.set('error'); return; }
        this.data.set(view); this.status.set('ready');
      },
      error: (error: HttpErrorResponse) => {
        if (generation === this.generation) this.status.set(error.status === 404 || error.status === 401 ? 'unavailable' : 'error');
      },
    });
  }
  protected summaryDates(): SharedFamilyDate[] {
    const today = new Date();
    const key = [today.getFullYear(), String(today.getMonth() + 1).padStart(2, '0'), String(today.getDate()).padStart(2, '0')].join('-');
    const dates = [...(this.data()?.dates ?? [])];
    const upcoming = dates.filter(date => date.date.slice(0, 10) >= key).sort((a, b) => a.date.localeCompare(b.date));
    const recent = dates.filter(date => date.date.slice(0, 10) < key).sort((a, b) => b.date.localeCompare(a.date));
    return [...upcoming, ...recent].slice(0, 4);
  }
  protected create(): void { this.cancel(); this.formOpen.set(true); this.loadPeople(); }
  protected edit(entry: SharedFamilyDate): void {
    this.deleting.set(null); this.editing.set(entry); this.formOpen.set(true);
    this.title = entry.title; this.description = entry.description; this.date = entry.date;
    this.associatedIds = entry.associatedPeople.map(p => p.profileId);
    if (entry.canManageAssociations) this.loadPeople();
  }
  protected cancel(): void {
    this.editing.set(null); this.formOpen.set(false); this.title = ''; this.description = ''; this.date = ''; this.associatedIds = [];
  }
  protected associate(profileId: string, checked: boolean): void {
    this.associatedIds = checked ? [...this.associatedIds, profileId] : this.associatedIds.filter(id => id !== profileId);
  }
  protected loadPeople(): void {
    this.peopleStatus.set('loading');
    const generation = this.generation;
    // Member choices are needed only for a creator's open form, not for ordinary date reads.
    this.http.get<FamilyDatePerson[]>(`/api/family-groups/${encodeURIComponent(this.familyId)}/members`)
      .pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: people => {
          if (generation !== this.generation) return;
          this.people.set(people.filter(p => p.profileId !== this.ownProfileId)); this.peopleStatus.set('ready');
        },
        error: (error: HttpErrorResponse) => {
          if (generation !== this.generation) return;
          this.peopleStatus.set('error');
          if (error.status === 404 || error.status === 401) {
            this.data.set(null); this.people.set([]); this.cancel(); this.status.set('unavailable');
          }
        },
      });
  }
  protected valid(): boolean {
    if ((!this.editing() || this.editing()?.canManageAssociations) && this.peopleStatus() !== 'ready') return false;
    return !!this.title.trim() && this.title.trim().length <= 100 && !!this.description.trim()
      && this.description.trim().length <= 1000 && /^\d{4}-\d{2}-\d{2}$/.test(this.date);
  }
  protected save(): void {
    const entry = this.editing();
    if (!this.valid() || (entry ? !entry.canEdit : !this.data()?.canCreate)) return;
    const fields = { title: this.title.trim(), description: this.description.trim(), date: this.date };
    this.mutate(entry ? this.http.put(`${this.url()}/${entry.dateId}`, {
      ...fields, version: entry.version, ...(entry.canManageAssociations ? { associatedProfileIds: this.associatedIds } : {}),
    }) : this.http.post(this.url(), { ...fields, associatedProfileIds: this.associatedIds }), entry ? 'Date saved.' : 'Date created.');
  }
  protected remove(entry: SharedFamilyDate): void {
    if (!entry.canDelete) return;
    this.mutate(this.http.delete(`${this.url()}/${entry.dateId}`, { params: { version: entry.version } }), 'Date deleted.');
  }
  private mutate(write: Observable<unknown>, success: string): void {
    if (this.busy() || this.needsRefresh() || this.status() !== 'ready') return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    const generation = this.generation;
    write.pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { if (generation === this.generation) { this.message.set(success); this.refresh(true); } },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.busy.set(false); this.deleting.set(null);
        if (error.status === 400) { this.error.set('Enter a title, description and valid date including its year.'); return; }
        this.needsRefresh.set(true);
        if (error.status === 404 || error.status === 401) { this.data.set(null); this.people.set([]); this.cancel(); this.status.set('unavailable'); }
        this.error.set(error.status === 409 ? 'This date changed. Refresh dates before trying again.'
          : 'Your change could not be saved. Refresh dates to check current state and access.');
      },
    });
  }
  private url(): string { return `/api/family-groups/${encodeURIComponent(this.familyId)}/dates`; }
}
