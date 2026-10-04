import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { FamilyMember, Incident, IncidentApi, IncidentSummary } from './incident-api';
import { IncidentProvenanceFields } from './incident-provenance-fields';
import {
  FieldErrors, focusById, isStatus, mapValidation, provenanceErrors, provenanceGroup, provenanceValue, sessionEndedMessage, textError,
} from './incident-shared';

/** Router navigation state from Concern detail. It carries IDs only and never Concern content. */
export interface IncidentOrigin {
  originatingConcernId: string;
  familyGroupId: string;
  subjectProfileId: string;
}

const fieldMessages: FieldErrors = {
  subject: 'Choose the person this Incident is about.',
  reason: 'Enter a reason of 1–1,000 characters.',
  category: 'Choose Confirmed, Reported, Assumption, or Unknown.',
  source: 'Enter a source or mark the source as unknown. Confirmed information needs a described source.',
  people: 'Review the people with access and try again.',
};

/**
 * Direct creation, or creation started from a Concern. Only the subject is prefilled from a
 * Concern; nothing else is copied, and no Concern participant is preselected or marked.
 */
@Component({
  selector: 'app-incident-create',
  imports: [ReactiveFormsModule, IncidentProvenanceFields],
  templateUrl: './incident-create.html',
  styleUrls: ['./incident-common.scss', './incident-forms.scss', './incident-create.scss'],
})
export class IncidentCreate implements OnInit {
  @Input() familyGroupId: string | null = null;
  @Input() familyName = '';
  @Input() ownProfileId: string | null = null;
  @Input() origin: IncidentOrigin | null = null;
  @Output() readonly created = new EventEmitter<Incident>();
  @Output() readonly openIncident = new EventEmitter<string>();
  @Output() readonly showList = new EventEmitter<void>();

  private readonly api = inject(IncidentApi);

  protected readonly members = signal<readonly FamilyMember[]>([]);
  protected readonly loadingMembers = signal(false);
  protected readonly membersError = signal('');
  protected readonly saving = signal(false);
  protected readonly errors = signal<FieldErrors>({});
  protected readonly message = signal('');
  protected readonly checkingList = signal(false);
  protected readonly matchId = signal<string | null>(null);
  protected readonly unconfirmed = signal(false);
  protected readonly peopleIds = signal<readonly string[]>([]);

  protected readonly form = new FormGroup({
    subjectProfileId: new FormControl('', { nonNullable: true }),
    reason: new FormControl('', { nonNullable: true }),
    provenance: provenanceGroup(),
  });

  ngOnInit(): void {
    this.loadMembers();
    focusById('create-incident-heading');
  }

  protected loadMembers(): void {
    if (!this.familyGroupId) return;
    this.loadingMembers.set(true);
    this.membersError.set('');
    this.api.members(this.familyGroupId).pipe(finalize(() => this.loadingMembers.set(false))).subscribe({
      next: members => {
        this.members.set(members);
        const subject = this.origin?.subjectProfileId;
        // Prefill only when the Concern's subject is an active member returned here; it stays changeable.
        if (subject && !this.form.controls.subjectProfileId.value
          && this.subjectChoices().some(person => person.profileId === subject))
          this.form.controls.subjectProfileId.setValue(subject);
      },
      error: error => this.membersError.set(isStatus(error, 401) ? sessionEndedMessage
        : 'Family members could not be loaded, so an Incident cannot be started yet.'),
    });
  }

  protected subjectChoices(): readonly FamilyMember[] {
    const ownId = this.ownProfileId;
    const others = this.members().filter(person => person.profileId !== ownId);
    if (!ownId) return others;
    const own = this.members().find(person => person.profileId === ownId);
    return [{ profileId: ownId, displayName: `${own?.displayName ?? 'You'} (you)` }, ...others];
  }

  /** Active adults other than the creator: the API grants the creator coordinator access separately. */
  protected peopleChoices(): readonly FamilyMember[] {
    return this.members().filter(person => person.profileId !== this.ownProfileId);
  }

  protected isSelected(profileId: string): boolean {
    return this.peopleIds().includes(profileId);
  }

  protected toggle(profileId: string, checked: boolean): void {
    const next = new Set(this.peopleIds());
    if (checked) next.add(profileId); else next.delete(profileId);
    this.peopleIds.set([...next]);
  }

  protected subjectId(): string {
    return this.form.controls.subjectProfileId.value;
  }

  protected submit(): void {
    if (this.saving() || this.matchId() || !this.familyGroupId) return;
    const { subjectProfileId, reason } = this.form.getRawValue();
    const errors: FieldErrors = { ...provenanceErrors(this.form.controls.provenance) };
    if (!subjectProfileId) errors['subject'] = fieldMessages['subject'];
    const reasonError = textError(reason, 1000, 'a reason');
    if (reasonError) errors['reason'] = reasonError;
    this.errors.set(errors);
    this.message.set('');
    if (Object.keys(errors).length) {
      focusById('create-incident-summary');
      return;
    }
    const provenance = provenanceValue(this.form.controls.provenance);
    const startedAt = Date.now();
    this.saving.set(true);
    this.unconfirmed.set(false);
    this.api.create(this.familyGroupId, {
      subjectProfileId,
      reason: reason.trim(),
      reasonCategory: provenance.category,
      reasonSourceDescription: provenance.sourceDescription,
      reasonSourceUnknown: provenance.sourceUnknown,
      participantProfileIds: [...this.peopleIds()],
      originatingConcernId: this.origin?.originatingConcernId ?? null,
    }).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: incident => this.created.emit(incident),
      error: error => {
        if (isStatus(error, 400)) {
          this.errors.set(mapValidation(error, {
            SubjectProfileId: 'subject', Reason: 'reason', ReasonCategory: 'category',
            ReasonSourceDescription: 'source', ParticipantProfileIds: 'people',
          }, fieldMessages));
          focusById('create-incident-summary');
        } else if (isStatus(error, 404)) {
          this.message.set('The Incident could not be created with the selected information. Refresh your Family members and try again.');
        } else if (isStatus(error, 401)) {
          this.message.set(sessionEndedMessage);
        } else {
          this.checkForMatch(subjectProfileId, startedAt);
        }
      },
    });
  }

  /** An ambiguous failure is never retried automatically; the authorized list is checked for a match instead. */
  private checkForMatch(subjectProfileId: string, startedAt: number): void {
    this.unconfirmed.set(true);
    this.checkingList.set(true);
    this.message.set('We could not confirm whether the Incident was created. Checking your Incident list…');
    this.api.list().pipe(finalize(() => this.checkingList.set(false))).subscribe({
      next: rows => {
        const match = rows.find((row: IncidentSummary) => row.subjectProfileId === subjectProfileId
          && row.coordinatorProfileId === this.ownProfileId
          && Date.parse(row.createdAt) >= startedAt - 60_000);
        this.matchId.set(match?.incidentId ?? null);
        this.message.set(match
          ? 'A matching Incident appears in your list. Review it before creating another.'
          : 'No matching Incident appeared in your list. Review the list before creating another.');
      },
      error: () => this.message.set('We could not confirm whether the Incident was created. Review the list before creating another.'),
    });
  }

  protected errorIds(...fields: string[]): string | null {
    const ids = fields.filter(field => this.errors()[field]).map(field => `create-${field}-error`);
    return ids.length ? ids.join(' ') : null;
  }

  protected hasErrors(): boolean {
    return Object.keys(this.errors()).length > 0;
  }
}
