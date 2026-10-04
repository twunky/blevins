import { Component, DestroyRef, Input, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter, finalize } from 'rxjs';
import { Incident, IncidentApi, IncidentSummary } from './incident-api';
import { IncidentCreate, IncidentOrigin } from './incident-create';
import { IncidentDetail } from './incident-detail';
import { IncidentList } from './incident-list';
import { isStatus, sessionEndedMessage } from './incident-shared';
import { AdoptItemFamily, isItemId } from '../item-route';

type Screen = { kind: 'list' } | { kind: 'new'; origin: IncidentOrigin | null } | { kind: 'detail'; id: string };

/** Reads the Start-an-Incident handoff from router navigation state. IDs only; anything malformed is ignored. */
export function readOrigin(state: unknown): IncidentOrigin | null {
  const value = state as Partial<Record<keyof IncidentOrigin, unknown>> | null | undefined;
  const { originatingConcernId, familyGroupId, subjectProfileId } = value ?? {};
  return [originatingConcernId, familyGroupId, subjectProfileId].every(isItemId)
    ? { originatingConcernId, familyGroupId, subjectProfileId } as IncidentOrigin
    : null;
}

/**
 * The `incidents` view for /incidents, /incidents/new, and /incidents/:incidentId. One route
 * definition serves all three, so moving between them never reloads the shell.
 */
@Component({
  selector: 'app-incidents',
  imports: [IncidentList, IncidentCreate, IncidentDetail],
  template: `
    @switch (screen().kind) {
      @case ('list') {
        <app-incident-list [rows]="rows()" [loading]="loadingList()" [error]="listError()" [ownProfileId]="ownProfileId"
          [showFamilyName]="families.length > 1" [canCreate]="!!currentFamilyId"
          (create)="go('/incidents/new')" (refresh)="refreshList()" />
      }
      @case ('new') {
        @for (key of [createKey()]; track key) {
          <app-incident-create [familyGroupId]="createFamilyId()" [familyName]="createFamilyName()" [ownProfileId]="ownProfileId"
            [origin]="origin()" (created)="created($event)" (openIncident)="go('/incidents/' + $event)" (showList)="go('/incidents')" />
        }
      }
      @case ('detail') {
        @for (id of [detailId()]; track id) {
          <app-incident-detail [incidentId]="id" [ownProfileId]="ownProfileId" [initial]="justCreated()"
            initialMessage="Incident created." [adoptItemFamily]="adoptItemFamily"
            (back)="go('/incidents')" (unavailableShown)="refreshList()" />
        }
      }
    }
  `,
  styles: [':host { display: grid; gap: var(--space-4); min-width: 0; }'],
})
export class Incidents implements OnInit {
  @Input() currentFamilyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  @Input() families: readonly { familyGroupId: string; name: string }[] = [];
  /** The shell's authorize-first Family decision (REQ-UX-014), passed to the detail. */
  @Input() adoptItemFamily: AdoptItemFamily | null = null;

  private readonly router = inject(Router);
  private readonly api = inject(IncidentApi);
  private readonly destroyRef = inject(DestroyRef);
  private createSequence = 0;

  protected readonly screen = signal<Screen>({ kind: 'list' });
  protected readonly rows = signal<readonly IncidentSummary[]>([]);
  protected readonly loadingList = signal(false);
  protected readonly listError = signal('');
  protected readonly justCreated = signal<Incident | null>(null);
  protected readonly createKey = signal(0);

  ngOnInit(): void {
    this.applyUrl();
    this.router.events.pipe(filter(event => event instanceof NavigationEnd), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.applyUrl());
  }

  protected origin(): IncidentOrigin | null {
    const screen = this.screen();
    return screen.kind === 'new' ? screen.origin : null;
  }

  protected detailId(): string {
    const screen = this.screen();
    return screen.kind === 'detail' ? screen.id : '';
  }

  /** From a Concern, the Concern's Family Group; otherwise the shell's current Family. */
  protected createFamilyId(): string | null {
    return this.origin()?.familyGroupId ?? this.currentFamilyId;
  }

  protected createFamilyName(): string {
    const id = this.createFamilyId();
    return this.families.find(family => family.familyGroupId === id)?.name ?? '';
  }

  protected go(url: string): void {
    void this.router.navigateByUrl(url);
  }

  protected created(incident: Incident): void {
    this.justCreated.set(incident);
    this.go(`/incidents/${incident.incidentId}`);
  }

  protected refreshList(): void {
    this.loadingList.set(true);
    this.listError.set('');
    this.api.list().pipe(finalize(() => this.loadingList.set(false))).subscribe({
      next: rows => this.rows.set(rows),
      error: error => this.listError.set(isStatus(error, 401) ? sessionEndedMessage : 'Incidents could not be loaded. Please try again.'),
    });
  }

  private applyUrl(): void {
    const segments = this.router.url.split(/[?#]/)[0].split('/').filter(Boolean);
    if (segments[0] !== 'incidents') return;
    const next = segments[1];
    const current = this.screen();
    if (!next) {
      this.justCreated.set(null);
      this.screen.set({ kind: 'list' });
      this.refreshList();
    } else if (next === 'new') {
      const navigation = this.router.currentNavigation() ?? this.router.lastSuccessfulNavigation();
      this.justCreated.set(null);
      this.createKey.set(++this.createSequence);
      this.screen.set({ kind: 'new', origin: readOrigin(navigation?.extras.state) });
    } else if (current.kind !== 'detail' || current.id !== next) {
      if (this.justCreated()?.incidentId !== next) this.justCreated.set(null);
      this.screen.set({ kind: 'detail', id: next });
    }
  }
}
