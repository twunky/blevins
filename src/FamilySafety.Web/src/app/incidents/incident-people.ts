import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { FamilyMember, Incident, IncidentApi, IncidentParticipant } from './incident-api';
import { focusById, initials, isStatus, sessionEndedMessage } from './incident-shared';

/**
 * "People with access": the coordinator first, then current participants. The subject has no access
 * unless added. The coordinator is never offered in the editor and never sent in the replacement set.
 */
@Component({
  selector: 'app-incident-people',
  templateUrl: './incident-people.html',
  styleUrls: ['./incident-common.scss', './incident-forms.scss', './incident-people.scss'],
})
export class IncidentPeople {
  @Input({ required: true }) incident!: Incident;
  @Input() ownProfileId: string | null = null;
  @Input() isCoordinator = false;
  @Output() readonly changed = new EventEmitter<{ incident: Incident; message: string }>();
  @Output() readonly denied = new EventEmitter<void>();

  private readonly api = inject(IncidentApi);

  protected readonly initials = initials;
  protected readonly managing = signal(false);
  protected readonly loadingMembers = signal(false);
  protected readonly members = signal<readonly FamilyMember[]>([]);
  protected readonly selectedIds = signal<readonly string[]>([]);
  protected readonly saving = signal(false);
  protected readonly message = signal('');

  protected people(): readonly IncidentParticipant[] {
    return [...this.incident.participants].sort((a, b) => Number(b.isCoordinator) - Number(a.isCoordinator));
  }

  protected subjectHasAccess(): boolean {
    return this.incident.participants.some(person => person.profileId === this.incident.subjectProfileId);
  }

  protected canManage(): boolean {
    return this.isCoordinator && this.incident.state === 'Open';
  }

  /** Active adults from the members endpoint, excluding the coordinator. */
  protected choices(): readonly FamilyMember[] {
    return this.members().filter(person => person.profileId !== this.incident.coordinatorProfileId);
  }

  protected startManaging(): void {
    this.message.set('');
    this.managing.set(true);
    this.loadingMembers.set(true);
    this.api.members(this.incident.familyGroupId).pipe(finalize(() => this.loadingMembers.set(false))).subscribe({
      next: members => {
        this.members.set(members);
        const memberIds = new Set(members.map(person => person.profileId));
        this.selectedIds.set(this.incident.participants
          .filter(person => !person.isCoordinator && memberIds.has(person.profileId))
          .map(person => person.profileId));
        focusById('incident-people-legend');
      },
      error: error => {
        this.managing.set(false);
        this.message.set(isStatus(error, 401) ? sessionEndedMessage : 'Eligible people could not be loaded. Try again.');
      },
    });
  }

  protected isSelected(profileId: string): boolean {
    return this.selectedIds().includes(profileId);
  }

  protected toggle(profileId: string, checked: boolean): void {
    const next = new Set(this.selectedIds());
    if (checked) next.add(profileId); else next.delete(profileId);
    this.selectedIds.set([...next]);
  }

  protected cancel(): void {
    this.managing.set(false);
    focusById('incident-manage-people');
  }

  protected save(): void {
    if (this.saving()) return;
    const ids = this.selectedIds().filter(id => id !== this.incident.coordinatorProfileId);
    this.saving.set(true);
    this.message.set('');
    this.api.replaceParticipants(this.incident.incidentId, ids).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: incident => {
        this.managing.set(false);
        this.changed.emit({ incident, message: 'People with access saved.' });
        focusById('incident-manage-people');
      },
      error: error => {
        if (isStatus(error, 404)) {
          this.managing.set(false);
          this.denied.emit();
        } else {
          this.message.set(isStatus(error, 401) ? sessionEndedMessage
            : isStatus(error, 400) ? 'That selection could not be saved. Review the people and try again.'
            : 'People with access could not be saved. Try again.');
        }
      },
    });
  }
}
