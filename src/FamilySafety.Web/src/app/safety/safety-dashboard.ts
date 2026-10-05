import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Concern, ConcernApi } from '../concerns/concern-api';
import { AppIcon } from '../icon';
import { IncidentApi, IncidentSummary } from '../incidents/incident-api';
import { focusHeadingState } from '../navigation';

/** Rows shown per card; the rest stay on the full list behind View all. */
const dashboardRows = 3;

const sessionEndedMessage = 'Your session may have ended. Please sign in again.';

/** Most recent authorized activity first. */
function byLastActivity<T extends { lastAuthorizedMutationAt: string }>(a: T, b: T): number {
  return Date.parse(b.lastAuthorizedMutationAt) - Date.parse(a.lastAuthorizedMutationAt);
}

/**
 * The Safety dashboard at /safety (WI-009-009, DEC-079; references/ux/home-dashboard panel ④).
 *
 * Read only. It shows the Open Concerns and Open Safety Incidents that the existing authorized list reads return,
 * in two separate cards (REQ-UX-013: no combined list, count, timeline, or create menu). The server decides every
 * row; nothing here grants access or implies that an item exists for anyone else (REQ-PRIV-027). Concern reasons,
 * contact attempts, tasks, and timelines stay on the authorized detail pages. Recent Check-Ins links to the standalone
 * workflow below the dashboard (WI-095-005). Nothing here states or infers anyone's real-world safety.
 */
@Component({
  selector: 'app-safety-dashboard',
  imports: [DatePipe, RouterLink, AppIcon],
  templateUrl: './safety-dashboard.html',
  styleUrl: './safety-dashboard.scss',
})
export class SafetyDashboard implements OnInit {
  @Input() ownProfileId: string | null = null;
  /** Family names come from each authorized row; they are shown only when the caller has more than one Family. */
  @Input() showFamilyName = false;

  private readonly concernApi = inject(ConcernApi);
  private readonly incidentApi = inject(IncidentApi);

  protected readonly focusHeadingState = focusHeadingState;
  protected readonly dateFormat = 'MMM d, y, h:mm a';

  private readonly concerns = signal<readonly Concern[]>([]);
  protected readonly concernsLoading = signal(true);
  protected readonly concernsError = signal('');
  private readonly incidents = signal<readonly IncidentSummary[]>([]);
  protected readonly incidentsLoading = signal(true);
  protected readonly incidentsError = signal('');

  private readonly openConcerns = computed(() => this.concerns().filter(row => row.state === 'Open').sort(byLastActivity));
  private readonly openIncidents = computed(() => this.incidents().filter(row => row.state === 'Open').sort(byLastActivity));
  protected readonly concernRows = computed(() => this.openConcerns().slice(0, dashboardRows));
  protected readonly incidentRows = computed(() => this.openIncidents().slice(0, dashboardRows));
  protected readonly moreConcerns = computed(() => this.openConcerns().length > dashboardRows);
  protected readonly moreIncidents = computed(() => this.openIncidents().length > dashboardRows);

  ngOnInit(): void {
    this.loadConcerns();
    this.loadIncidents();
  }

  protected loadConcerns(): void {
    this.concernsLoading.set(true);
    this.concernsError.set('');
    this.concernApi.list().pipe(finalize(() => this.concernsLoading.set(false))).subscribe({
      next: rows => this.concerns.set(rows),
      error: error => this.concernsError.set(this.isSessionEnded(error) ? sessionEndedMessage : 'Concerns could not be loaded. Please try again.'),
    });
  }

  protected loadIncidents(): void {
    this.incidentsLoading.set(true);
    this.incidentsError.set('');
    this.incidentApi.list().pipe(finalize(() => this.incidentsLoading.set(false))).subscribe({
      next: rows => this.incidents.set(rows),
      error: error => this.incidentsError.set(this.isSessionEnded(error) ? sessionEndedMessage : 'Safety Incidents could not be loaded. Please try again.'),
    });
  }

  protected initials(name: string): string {
    return name.split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0].toUpperCase()).join('');
  }

  private isSessionEnded(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 401;
  }
}
