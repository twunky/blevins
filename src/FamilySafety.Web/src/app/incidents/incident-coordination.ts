import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { Incident, IncidentApi, IncidentParticipant } from './incident-api';
import { IncidentProvenanceFields } from './incident-provenance-fields';
import {
  FieldErrors, focusById, isStatus, mapValidation, provenanceErrors, provenanceGroup, provenanceValue, sessionEndedMessage,
  textError, toInstant,
} from './incident-shared';

type Step = 'idle' | 'choose' | 'confirm' | 'resolve';

const fieldMessages: FieldErrors = {
  summary: 'Enter a resolution summary of 1–2,000 characters.',
  outcome: 'Enter an outcome of 1–2,000 characters.',
  category: 'Choose Confirmed, Reported, Assumption, or Unknown.',
  source: 'Enter a source or mark the source as unknown. Confirmed information needs a described source.',
  restoredAt: 'Enter when contact was restored or this was resolved, or choose Unknown.',
  assistance: 'Assistance can be up to 2,000 characters.',
  followUp: 'Follow-up can be up to 2,000 characters.',
  lessons: 'Lessons can be up to 2,000 characters.',
};

/**
 * Coordinator-only controls while Open: a two-step inline handoff to a current participant, and
 * the resolution form. There is one coordinator; nothing here creates shared coordination.
 */
@Component({
  selector: 'app-incident-coordination',
  imports: [ReactiveFormsModule, IncidentProvenanceFields],
  templateUrl: './incident-coordination.html',
  styleUrls: ['./incident-common.scss', './incident-forms.scss'],
})
export class IncidentCoordination {
  @Input({ required: true }) incident!: Incident;
  @Output() readonly changed = new EventEmitter<{ incident: Incident; message: string; focusId: string }>();
  @Output() readonly denied = new EventEmitter<void>();

  private readonly api = inject(IncidentApi);

  protected readonly step = signal<Step>('idle');
  protected readonly handoffTo = signal<string>('');
  protected readonly saving = signal(false);
  protected readonly errors = signal<FieldErrors>({});
  protected readonly message = signal('');

  protected readonly resolveForm = new FormGroup({
    summary: new FormControl('', { nonNullable: true }),
    outcomeText: new FormControl('', { nonNullable: true }),
    restoredAt: new FormControl('', { nonNullable: true }),
    restoredUnknown: new FormControl(false, { nonNullable: true }),
    assistance: new FormControl('', { nonNullable: true }),
    followUp: new FormControl('', { nonNullable: true }),
    lessons: new FormControl('', { nonNullable: true }),
    provenance: provenanceGroup(),
  });

  /** Current participants other than the coordinator. */
  protected candidates(): readonly IncidentParticipant[] {
    return this.incident.participants.filter(person => !person.isCoordinator && person.profileId !== this.incident.coordinatorProfileId);
  }

  protected chosenName(): string {
    return this.candidates().find(person => person.profileId === this.handoffTo())?.displayName ?? '';
  }

  protected startHandoff(): void {
    this.message.set('');
    this.handoffTo.set('');
    this.step.set('choose');
    focusById('incident-handoff-legend');
  }

  protected continueHandoff(): void {
    if (!this.chosenName()) return;
    this.step.set('confirm');
    focusById('incident-handoff-confirm');
  }

  protected backToChoose(): void {
    this.step.set('choose');
    focusById('incident-handoff-legend');
  }

  protected close(focusId: string): void {
    this.step.set('idle');
    this.errors.set({});
    focusById(focusId);
  }

  /** Escape steps back from confirmation, and closes the choice step. */
  protected onEscape(): void {
    if (this.saving()) return;
    if (this.step() === 'confirm') this.backToChoose();
    else if (this.step() === 'choose') this.close('incident-start-handoff');
  }

  protected handOff(): void {
    const target = this.handoffTo();
    const name = this.chosenName();
    if (!target || this.saving()) return;
    this.saving.set(true);
    this.message.set('');
    this.api.transferCoordinator(this.incident.incidentId, target).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: incident => {
        this.step.set('idle');
        this.changed.emit({ incident, message: `${name} now coordinates this Incident.`, focusId: 'incident-detail-heading' });
      },
      error: error => this.handleError(error, 'Coordination could not be handed off. Try again.'),
    });
  }

  protected startResolve(): void {
    this.message.set('');
    this.step.set('resolve');
    focusById('incident-resolve-heading');
  }

  protected setRestoredUnknown(checked: boolean): void {
    this.resolveForm.controls.restoredUnknown.setValue(checked);
    if (checked) this.resolveForm.controls.restoredAt.setValue('');
  }

  protected resolve(): void {
    if (this.saving()) return;
    const value = this.resolveForm.getRawValue();
    const errors: FieldErrors = provenanceErrors(this.resolveForm.controls.provenance);
    if (textError(value.summary, 2000)) errors['summary'] = fieldMessages['summary'];
    if (textError(value.outcomeText, 2000)) errors['outcome'] = fieldMessages['outcome'];
    if (!value.restoredUnknown === !value.restoredAt) errors['restoredAt'] = fieldMessages['restoredAt'];
    if (value.assistance.trim().length > 2000) errors['assistance'] = fieldMessages['assistance'];
    if (value.followUp.trim().length > 2000) errors['followUp'] = fieldMessages['followUp'];
    if (value.lessons.trim().length > 2000) errors['lessons'] = fieldMessages['lessons'];
    this.errors.set(errors);
    this.message.set('');
    if (Object.keys(errors).length) {
      focusById('incident-resolve-summary');
      return;
    }
    const provenance = provenanceValue(this.resolveForm.controls.provenance);
    this.saving.set(true);
    this.api.resolve(this.incident.incidentId, {
      summary: value.summary.trim(),
      outcomeText: value.outcomeText.trim(),
      outcomeCategory: provenance.category,
      outcomeSourceDescription: provenance.sourceDescription,
      outcomeSourceUnknown: provenance.sourceUnknown,
      contactRestoredAt: toInstant(value.restoredAt),
      contactRestoredTimeUnknown: value.restoredUnknown,
      assistanceProvided: value.assistance.trim() || null,
      unresolvedFollowUp: value.followUp.trim() || null,
      lessons: value.lessons.trim() || null,
    }).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: incident => {
        this.step.set('idle');
        this.changed.emit({ incident, message: 'Incident resolved.', focusId: 'incident-resolution-heading' });
      },
      error: error => {
        if (isStatus(error, 400)) {
          this.errors.set(mapValidation(error, {
            Summary: 'summary', OutcomeText: 'outcome', OutcomeCategory: 'category', OutcomeSourceDescription: 'source',
            ContactRestoredAt: 'restoredAt', AssistanceProvided: 'assistance', UnresolvedFollowUp: 'followUp', Lessons: 'lessons',
          }, fieldMessages));
          focusById('incident-resolve-summary');
        } else {
          this.handleError(error, 'The Incident could not be resolved. Your draft is kept; try again.');
        }
      },
    });
  }

  protected describedBy(...fields: string[]): string | null {
    const ids = fields.filter(field => this.errors()[field]).map(field => `resolve-${field}-error`);
    return ids.length ? ids.join(' ') : null;
  }

  protected hasErrors(): boolean {
    return Object.keys(this.errors()).length > 0;
  }

  private handleError(error: unknown, fallback: string): void {
    if (isStatus(error, 404)) {
      // Drafts stay in memory; the detail refreshes and decides what is still shown.
      this.step.set('idle');
      this.denied.emit();
    } else {
      this.message.set(isStatus(error, 401) ? sessionEndedMessage : fallback);
    }
  }
}
