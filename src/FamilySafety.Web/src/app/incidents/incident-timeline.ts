import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Incident, IncidentEntry, IncidentTask, IncidentTimelineItem } from './incident-api';
import {
  categoryText, dateTimeFormat, entryKindLabel, focusEntry, methodLabel, outcomeLabel, sourceText, timelineTimeFormat,
} from './incident-shared';

const typeLabels: Record<Exclude<IncidentTimelineItem['type'], 'Entry'>, string> = {
  IncidentCreated: 'Incident started', ParticipantAdded: 'Person added', CoordinatorHandedOff: 'Coordination handed off',
  TaskCreated: 'Task added', TaskCompleted: 'Task completed', TaskCancelled: 'Task cancelled',
  ParticipantRemoved: 'Person removed', ParticipantAccessEnded: 'Access ended', Frozen: 'Coordination stopped', Resolved: 'Resolved',
};

/**
 * The single chronological history, rendered from `timeline` in server order (never re-sorted).
 * Markers are by item type only. Access-ended and Frozen items never show a reason.
 */
@Component({
  selector: 'app-incident-timeline',
  imports: [DatePipe],
  templateUrl: './incident-timeline.html',
  styleUrls: ['./incident-common.scss', './incident-timeline.scss'],
})
export class IncidentTimeline {
  @Input({ required: true }) incident!: Incident;
  @Input() ownProfileId: string | null = null;
  @Input() canWrite = false;
  @Output() readonly correct = new EventEmitter<IncidentEntry>();
  @Output() readonly challenge = new EventEmitter<IncidentEntry>();

  protected readonly dateTimeFormat = dateTimeFormat;
  protected readonly timelineTimeFormat = timelineTimeFormat;
  protected readonly categoryText = categoryText;
  protected readonly sourceText = sourceText;
  protected readonly goTo = focusEntry;

  protected entryFor(item: IncidentTimelineItem): IncidentEntry | undefined {
    return item.entryId ? this.incident.entries.find(entry => entry.entryId === item.entryId) : undefined;
  }

  protected taskFor(item: IncidentTimelineItem): IncidentTask | undefined {
    return item.taskId ? this.incident.tasks.find(task => task.taskId === item.taskId) : undefined;
  }

  protected typeLabel(item: IncidentTimelineItem): string {
    if (item.type !== 'Entry') return typeLabels[item.type];
    return item.entryKind ? entryKindLabel(item.entryKind) : 'Entry';
  }

  /** Marker by item type only — never by category, danger, or outcome. */
  protected marker(item: IncidentTimelineItem): string {
    if (item.type !== 'Entry') {
      if (item.type.startsWith('Task')) return 'task';
      if (item.type.startsWith('Participant') || item.type === 'CoordinatorHandedOff') return 'people';
      return 'state';
    }
    return ({ Unknown: 'unknown', Summary: 'summary', ContactAttempt: 'contact', TaskResult: 'task' } as Record<string, string>)[item.entryKind ?? ''] ?? 'finding';
  }

  protected title(item: IncidentTimelineItem): string {
    const affected = item.affectedName ?? 'Someone';
    switch (item.type) {
      case 'IncidentCreated': return `${item.actorName} started this Incident about ${this.incident.subjectName}`;
      case 'ParticipantAdded': return `${item.actorName} added ${affected}`;
      case 'ParticipantRemoved': return `${item.actorName} removed ${affected}`;
      case 'ParticipantAccessEnded': return `${affected}'s access ended`;
      case 'CoordinatorHandedOff': return `${item.actorName} handed coordination to ${affected}`;
      case 'TaskCreated': return `${item.actorName} added a task for ${affected}`;
      case 'TaskCompleted': return `${item.actorName} completed a task`;
      case 'TaskCancelled': return `${item.actorName} cancelled a task`;
      case 'Frozen': return 'Coordination stopped. This record is now read-only.';
      case 'Resolved': return `${item.actorName} resolved this Incident`;
      default: return item.actorName;
    }
  }

  protected contactLine(entry: IncidentEntry): string {
    const who = entry.attemptedByDescription || entry.contributorName;
    return `${who} tried to reach ${entry.targetLabel ?? ''} · ${methodLabel(entry.contactMethod)} · ${outcomeLabel(entry.contactOutcome)}`;
  }

  protected related(entry: IncidentEntry): IncidentEntry | undefined {
    return entry.relatedEntryId ? this.incident.entries.find(item => item.entryId === entry.relatedEntryId) : undefined;
  }

  protected relatedTask(entry: IncidentEntry): IncidentTask | undefined {
    return entry.relatedTaskId ? this.incident.tasks.find(task => task.taskId === entry.relatedTaskId) : undefined;
  }

  protected relationPrefix(entry: IncidentEntry): string {
    return entry.kind === 'Correction' ? 'Correction to' : 'Another report about';
  }

  protected canCorrect(entry: IncidentEntry): boolean {
    return this.canWrite && !!this.ownProfileId && entry.contributorProfileId === this.ownProfileId;
  }

  protected canChallenge(entry: IncidentEntry): boolean {
    return this.canWrite && !!this.ownProfileId && entry.contributorProfileId !== this.ownProfileId;
  }
}
