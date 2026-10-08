import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, takeUntil } from 'rxjs';

export interface HouseholdMember { profileId: string; displayName: string }
export interface Household { householdId: string; name: string; version: string; members: HouseholdMember[] }
export interface HouseholdStructure { canManage: boolean; households: Household[]; unassignedMembers: HouseholdMember[] }

@Component({ selector: 'app-households', imports: [FormsModule], templateUrl: './households.html', styleUrl: './households.scss' })
export class Households implements OnChanges {
  @Input() familyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changed = new Subject<void>();
  protected readonly state = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly structure = signal<HouseholdStructure | null>(null);
  protected readonly busy = signal(false);
  protected readonly formOpen = signal(false);
  protected readonly editing = signal<Household | null>(null);
  protected readonly removing = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly memberSelections = signal<Record<string, string>>({});
  protected name = '';

  ngOnChanges(): void { this.refresh(); }
  protected get url(): string { return '/api/family-groups/' + encodeURIComponent(this.familyId!) + '/households'; }

  protected refresh(preserveMessage = false): void {
    this.changed.next(); this.cancel(); this.busy.set(false); this.structure.set(null); this.error.set('');
    if (!preserveMessage) this.message.set('');
    if (!this.familyId || !this.ownProfileId) { this.state.set('unavailable'); return; }
    this.state.set('loading');
    this.http.get<HouseholdStructure>(this.url).pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: data => { this.structure.set(data); this.state.set('ready'); },
      error: (e: HttpErrorResponse) => this.state.set(e.status === 401 || e.status === 404 ? 'unavailable' : 'error'),
    });
  }

  protected cancel(): void { this.formOpen.set(false); this.editing.set(null); this.removing.set(null); this.memberSelections.set({}); this.name = ''; }
  private canMutate(): boolean { return this.state() === 'ready' && !!this.structure()?.canManage && !this.busy(); }
  protected open(row: Household | null = null): void {
    if (!this.canMutate()) return;
    this.cancel(); this.editing.set(row); this.name = row?.name ?? ''; this.formOpen.set(true); this.error.set('');
  }
  protected valid(): boolean { return this.name.trim().length > 0 && this.name.trim().length <= 100 && !/[\u0000-\u001f\u007f-\u009f]/.test(this.name); }
  protected save(): void {
    if (!this.valid() || !this.formOpen() || !this.canMutate()) return;
    const row = this.editing();
    this.mutate(row ? this.http.put(this.url + '/' + row.householdId, { name: this.name.trim(), version: row.version })
      : this.http.post(this.url, { name: this.name.trim() }), row ? 'Household renamed.' : 'Household created.');
  }

  protected choices(row: Household): { profileId: string; label: string }[] {
    const data = this.structure();
    if (!data) return [];
    return [...data.unassignedMembers.map(m => ({ profileId: m.profileId, label: m.displayName })),
      ...data.households.filter(h => h.householdId !== row.householdId).flatMap(h =>
        h.members.map(m => ({ profileId: m.profileId, label: m.displayName + ' (from ' + h.name + ')' })))];
  }

  protected selectMember(row: Household, profileId: string): void {
    this.memberSelections.update(selections => ({ ...selections, [row.householdId]: profileId }));
  }

  protected selectedSource(row: Household): Household | undefined {
    const profileId = this.memberSelections()[row.householdId];
    return this.structure()?.households.find(h => h.members.some(m => m.profileId === profileId));
  }

  protected addSelectedMember(row: Household): void {
    const profileId = this.memberSelections()[row.householdId];
    if (this.formOpen() || !this.choices(row).some(person => person.profileId === profileId)) return;
    this.assign(row, profileId);
  }

  protected assign(row: Household, profileId: string): void {
    if (!profileId || !this.canMutate()) return;
    const source = this.structure()?.households.find(h => h.members.some(m => m.profileId === profileId));
    const member = [...(this.structure()?.unassignedMembers ?? []), ...(source?.members ?? [])].find(m => m.profileId === profileId);
    this.mutate(this.http.post(this.url + '/' + row.householdId + '/members', {
      version: row.version, profileId, currentHouseholdId: source?.householdId ?? null, sourceVersion: source?.version ?? null,
    }), source ? `${member?.displayName} moved from ${source.name} to ${row.name}.` : `${member?.displayName} added to ${row.name}.`);
  }

  protected unassign(row: Household, profileId: string): void {
    this.mutate(this.http.delete(this.url + '/' + row.householdId + '/members/' + profileId + '?version=' + row.version),
      `${row.members.find(m => m.profileId === profileId)?.displayName} removed from ${row.name}. They remain in this Family and are now unassigned.`);
  }

  protected deleteHousehold(row: Household): void {
    if (this.removing() !== row.householdId) return;
    this.mutate(this.http.delete(this.url + '/' + row.householdId + '?version=' + row.version), 'Household permanently deleted. Its members are now unassigned.');
  }

  private mutate(request: Observable<unknown>, message: string): void {
    if (!this.canMutate()) return;
    this.busy.set(true); this.error.set(''); this.message.set('');
    request.pipe(takeUntil(this.changed), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.message.set(message); this.refresh(true); },
      error: (e: HttpErrorResponse) => {
        this.busy.set(false);
        if (e.status === 400 || e.error?.code === 'DuplicateHouseholdName') {
          this.error.set(e.status === 400 ? 'Choose a plain-text name of 1-100 characters.' : 'This Family already has a Household with this name.'); return;
        }
        this.cancel(); this.structure.set(null);
        this.state.set(e.status === 401 || e.status === 404 ? 'unavailable' : 'error');
        this.error.set(e.status === 409 ? 'Household organization changed. Refresh before trying again.'
          : 'The change could not be confirmed. Refresh to check current organization and access.');
      },
    });
  }
}
