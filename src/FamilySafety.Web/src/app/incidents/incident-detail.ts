import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Incident, IncidentApi, IncidentEntry } from './incident-api';
import { ComposerKind, ComposerRequest, IncidentComposer } from './incident-composer';
import { IncidentCoordination } from './incident-coordination';
import { IncidentPeople } from './incident-people';
import { IncidentTasks } from './incident-tasks';
import { IncidentTimeline } from './incident-timeline';
import {
  categoryText, dateOnlyFormat, dateTimeFormat, focusById, initials, isStatus, sessionEndedMessage, sourceText, stateDescription,
} from './incident-shared';
import { AdoptItemFamily, isItemId } from '../item-route';

/**
 * Incident detail as cards (no tabs): header, resolution, timeline, people, contribute, tasks, and
 * coordination. Controls are display hints from returned data and the current profile only.
 */
@Component({
  selector: 'app-incident-detail',
  imports: [DatePipe, RouterLink, IncidentTimeline, IncidentComposer, IncidentPeople, IncidentTasks, IncidentCoordination],
  templateUrl: './incident-detail.html',
  styleUrls: ['./incident-common.scss', './incident-detail.scss'],
})
export class IncidentDetail implements OnInit {
  @Input({ required: true }) incidentId!: string;
  @Input() ownProfileId: string | null = null;
  /** The 201 response from creation, shown without an extra request. */
  @Input() initial: Incident | null = null;
  @Input() initialMessage = '';
  /** REQ-UX-014: after the item read succeeds and before it renders, the shell decides the Family context. */
  @Input() adoptItemFamily: AdoptItemFamily | null = null;
  @Output() readonly back = new EventEmitter<void>();
  @Output() readonly unavailableShown = new EventEmitter<void>();

  private readonly api = inject(IncidentApi);

  protected readonly incident = signal<Incident | null>(null);
  protected readonly loading = signal(false);
  protected readonly unavailable = signal(false);
  protected readonly loadError = signal('');
  protected readonly announcement = signal('');
  protected readonly refreshedNotice = signal('');
  protected readonly composer = signal<ComposerRequest | null>(null);

  protected readonly dateTimeFormat = dateTimeFormat;
  protected readonly dateOnlyFormat = dateOnlyFormat;
  protected readonly initials = initials;
  protected readonly categoryText = categoryText;
  protected readonly sourceText = sourceText;
  protected readonly stateDescription = stateDescription;

  ngOnInit(): void {
    if (this.initial && this.initial.incidentId === this.incidentId) {
      this.incident.set(this.initial);
      this.announcement.set(this.initialMessage);
      focusById('incident-detail-heading');
    } else {
      this.load();
    }
  }

  protected load(): void {
    // A non-GUID ID is unavailable without a request (REQ-PRIV-027).
    if (!isItemId(this.incidentId)) {
      this.showUnavailable();
      return;
    }
    this.loading.set(true);
    this.loadError.set('');
    this.api.get(this.incidentId).subscribe({
      next: async incident => {
        const outcome = this.adoptItemFamily ? await this.adoptItemFamily(incident.familyGroupId, 'incident') : 'current';
        this.loading.set(false);
        if (outcome === 'unavailable') this.showUnavailable();
        else this.incident.set(incident);
      },
      error: error => {
        this.loading.set(false);
        if (isStatus(error, 404)) this.showUnavailable();
        else this.loadError.set(isStatus(error, 401) ? sessionEndedMessage : 'This Incident could not be opened. Please try again.');
      },
    });
  }

  protected isCoordinator(incident: Incident): boolean {
    return !!this.ownProfileId && incident.coordinatorProfileId === this.ownProfileId;
  }

  protected isParticipant(incident: Incident): boolean {
    return !!this.ownProfileId && incident.participants.some(person => person.profileId === this.ownProfileId);
  }

  protected canWrite(incident: Incident): boolean {
    return incident.state === 'Open' && this.isParticipant(incident);
  }

  protected summaries(incident: Incident): IncidentEntry[] {
    return incident.entries.filter(entry => entry.kind === 'Summary')
      .sort((a, b) => Date.parse(a.recordedAt) - Date.parse(b.recordedAt));
  }

  /** The first summary is the creation reason. */
  protected reason(incident: Incident): IncidentEntry | undefined {
    return this.summaries(incident)[0];
  }

  /** The latest summary, shown as current only when it is newer than the reason. */
  protected currentSummary(incident: Incident): IncidentEntry | undefined {
    const all = this.summaries(incident);
    return all.length > 1 ? all[all.length - 1] : undefined;
  }

  protected openComposer(kind: ComposerKind, target: IncidentEntry | null, returnFocusId: string): void {
    this.refreshedNotice.set('');
    this.composer.set({ kind, target, returnFocusId });
  }

  protected composerClosed(returnFocusId: string): void {
    this.composer.set(null);
    if (returnFocusId) focusById(returnFocusId);
  }

  protected applyChange(change: { incident: Incident; message: string; focusId?: string }): void {
    this.refreshedNotice.set('');
    this.incident.set(change.incident);
    this.announcement.set(change.message);
    if (change.focusId) focusById(change.focusId);
  }

  /**
   * Refresh-on-denial: a 404 on a write does not by itself mean access ended. Reload; if the
   * Incident still loads, redraw it with one neutral message; otherwise show the unavailable state.
   */
  protected writeDenied(): void {
    this.composer.set(null);
    this.announcement.set('');
    this.api.get(this.incidentId).subscribe({
      next: incident => {
        this.incident.set(incident);
        this.refreshedNotice.set('That change could not be made. This Incident has been refreshed with its current details.');
        focusById('incident-refreshed-notice');
      },
      error: error => {
        if (isStatus(error, 404)) this.showUnavailable();
        else this.refreshedNotice.set(isStatus(error, 401) ? sessionEndedMessage
          : 'That change could not be made, and the Incident could not be refreshed. Please try again.');
      },
    });
  }

  private showUnavailable(): void {
    this.incident.set(null);
    this.composer.set(null);
    this.unavailable.set(true);
    focusById('incident-unavailable-heading');
    this.unavailableShown.emit();
  }
}
