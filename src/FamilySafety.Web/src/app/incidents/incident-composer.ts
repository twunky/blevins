import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { Incident, IncidentApi, IncidentCategory, IncidentContactMethod, IncidentContactOutcome, IncidentEntry } from './incident-api';
import { IncidentProvenanceFields } from './incident-provenance-fields';
import {
  FieldErrors, contactMethods, contactOutcomes, dateTimeFormat, focusById, isStatus, mapValidation, provenanceErrors,
  provenanceGroup, provenanceValue, sessionEndedMessage, textError, toInstant,
} from './incident-shared';

export type ComposerKind = 'Observation' | 'Unknown' | 'Summary' | 'Correction' | 'Challenge' | 'ContactAttempt';

export interface ComposerRequest {
  kind: ComposerKind;
  target: IncidentEntry | null;
  /** The control that opened the composer; focus returns there on close. */
  returnFocusId: string;
}

const copy: Record<ComposerKind, { heading: string; text: string; submit: string; done: string }> = {
  Observation: { heading: 'Add a finding', text: 'Information', submit: 'Add finding', done: 'Finding added.' },
  Unknown: { heading: 'Record an Unknown', text: 'What is unknown', submit: 'Record Unknown', done: 'Unknown recorded.' },
  Summary: { heading: 'Post a summary update', text: 'Summary', submit: 'Post summary', done: 'Summary posted.' },
  Correction: { heading: 'Add a correction', text: 'Corrected information', submit: 'Add correction', done: 'Correction added.' },
  Challenge: { heading: 'Add another report', text: 'Your report', submit: 'Add report', done: 'Report added.' },
  ContactAttempt: { heading: 'Record a contact attempt', text: 'Notes (optional)', submit: 'Record contact attempt', done: 'Contact attempt recorded.' },
};

const fieldMessages: FieldErrors = {
  text: 'Enter information of 1–2,000 characters.',
  explanation: 'Enter an explanation of 1–1,000 characters.',
  category: 'Choose how this information is known.',
  source: 'Enter a source or mark the source as unknown. Confirmed information needs a described source.',
  eventAt: 'Enter when this happened, or choose Time unknown.',
  target: 'Enter who was contacted, up to 200 characters.',
  attemptedBy: 'Enter who attempted contact, up to 200 characters.',
  method: 'Choose how contact was attempted.',
  outcome: 'Choose the outcome of the attempt.',
};

/**
 * Every contribution form posts AddIncidentEntryRequest. Only one composer is open at a time. A
 * contact attempt is a record only: no profile or contact details are suggested, and nothing is sent.
 */
@Component({
  selector: 'app-incident-composer',
  imports: [DatePipe, ReactiveFormsModule, IncidentProvenanceFields],
  templateUrl: './incident-composer.html',
  styleUrls: ['./incident-common.scss', './incident-forms.scss'],
})
export class IncidentComposer implements OnChanges {
  @Input({ required: true }) incident!: Incident;
  @Input() request: ComposerRequest | null = null;
  @Output() readonly changed = new EventEmitter<{ incident: Incident; message: string }>();
  @Output() readonly denied = new EventEmitter<void>();
  @Output() readonly closed = new EventEmitter<string>();

  private readonly api = inject(IncidentApi);
  /** Drafts kept in memory when a save is denied, keyed by kind and target. */
  private readonly drafts = new Map<string, ReturnType<IncidentComposer['form']['getRawValue']>>();

  protected readonly copy = copy;
  protected readonly methods = contactMethods;
  protected readonly outcomes = contactOutcomes;
  protected readonly dateTimeFormat = dateTimeFormat;
  protected readonly saving = signal(false);
  protected readonly errors = signal<FieldErrors>({});
  protected readonly message = signal('');

  protected readonly form = new FormGroup({
    text: new FormControl('', { nonNullable: true }),
    explanation: new FormControl('', { nonNullable: true }),
    eventAt: new FormControl('', { nonNullable: true }),
    timeUnknown: new FormControl(false, { nonNullable: true }),
    attemptedBy: new FormControl<'me' | 'someone'>('me', { nonNullable: true }),
    attemptedByDescription: new FormControl('', { nonNullable: true }),
    targetLabel: new FormControl('', { nonNullable: true }),
    method: new FormControl<IncidentContactMethod | ''>('', { nonNullable: true }),
    outcome: new FormControl<IncidentContactOutcome | ''>('', { nonNullable: true }),
    provenance: provenanceGroup(),
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['request'] && this.request) this.prepare(this.request);
  }

  protected categories(kind: ComposerKind): readonly IncidentCategory[] {
    if (kind === 'Summary') return ['Confirmed', 'Reported', 'Assumption', 'Unknown', 'Outdated'];
    if (kind === 'ContactAttempt') return ['Confirmed', 'Reported', 'Assumption', 'Unknown'];
    return ['Confirmed', 'Reported', 'Assumption', 'Outdated'];
  }

  protected setAttemptedBy(value: 'me' | 'someone'): void {
    this.form.controls.attemptedBy.setValue(value);
    if (value === 'someone') this.form.controls.provenance.controls.category.setValue('Reported');
  }

  protected setTimeUnknown(checked: boolean): void {
    this.form.controls.timeUnknown.setValue(checked);
    if (checked) this.form.controls.eventAt.setValue('');
  }

  protected cancel(): void {
    const returnFocusId = this.request?.returnFocusId ?? '';
    this.reset();
    this.closed.emit(returnFocusId);
  }

  protected submit(): void {
    const request = this.request;
    if (!request || this.saving()) return;
    const errors = this.validate(request.kind);
    this.errors.set(errors);
    this.message.set('');
    if (Object.keys(errors).length) {
      focusById('incident-composer-summary');
      return;
    }
    this.saving.set(true);
    this.api.addEntry(this.incident.incidentId, this.body(request))
      .pipe(finalize(() => this.saving.set(false))).subscribe({
        next: incident => {
          this.reset();
          this.changed.emit({ incident, message: copy[request.kind].done });
          this.closed.emit(request.returnFocusId);
        },
        error: error => {
          if (isStatus(error, 404)) {
            // Keep the draft in memory, close the form, and let the detail refresh.
            this.drafts.set(this.key(request), this.form.getRawValue());
            this.closed.emit(request.returnFocusId);
            this.denied.emit();
          } else if (isStatus(error, 400)) {
            this.errors.set(mapValidation(error, {
              Text: 'text', Explanation: 'explanation', RelatedEntryId: 'explanation', Category: 'category',
              OriginalCategory: 'category', SourceDescription: 'source', EventAt: 'eventAt', TargetLabel: 'target',
              AttemptedByDescription: 'attemptedBy', ContactMethod: 'method', ContactOutcome: 'outcome',
            }, fieldMessages));
            focusById('incident-composer-summary');
          } else {
            this.message.set(isStatus(error, 401) ? sessionEndedMessage : 'The contribution could not be saved. Your draft is kept; try again.');
          }
        },
      });
  }

  protected describedBy(...fields: string[]): string | null {
    const ids = fields.filter(field => this.errors()[field]).map(field => `composer-${field}-error`);
    return ids.length ? ids.join(' ') : null;
  }

  protected hasErrors(): boolean {
    return Object.keys(this.errors()).length > 0;
  }

  private prepare(request: ComposerRequest): void {
    this.reset();
    const draft = this.drafts.get(this.key(request));
    if (draft) {
      this.form.setValue(draft);
      this.drafts.delete(this.key(request));
    } else if (request.kind === 'Unknown') {
      this.form.controls.provenance.controls.category.setValue('Unknown');
    }
    focusById('incident-composer-heading');
  }

  private reset(): void {
    this.form.reset();
    this.form.controls.provenance.controls.sourceUnknown.enable();
    this.errors.set({});
    this.message.set('');
  }

  private key(request: ComposerRequest): string {
    return `${request.kind}:${request.target?.entryId ?? ''}`;
  }

  private validate(kind: ComposerKind): FieldErrors {
    const value = this.form.getRawValue();
    const errors: FieldErrors = provenanceErrors(this.form.controls.provenance);
    if (kind === 'ContactAttempt') {
      if (value.text.trim().length > 2000) errors['text'] = 'Notes can be up to 2,000 characters.';
      const target = value.targetLabel.trim();
      if (!target || target.length > 200) errors['target'] = fieldMessages['target'];
      const attemptedBy = value.attemptedByDescription.trim();
      if (value.attemptedBy === 'someone' && (!attemptedBy || attemptedBy.length > 200)) errors['attemptedBy'] = fieldMessages['attemptedBy'];
      if (!value.method) errors['method'] = fieldMessages['method'];
      if (!value.outcome) errors['outcome'] = fieldMessages['outcome'];
      if (!value.timeUnknown === !value.eventAt) errors['eventAt'] = 'Enter when the attempt happened, or choose Time unknown.';
    } else {
      const textError_ = textError(value.text, 2000);
      if (textError_) errors['text'] = textError_;
    }
    if (kind === 'Correction' || kind === 'Challenge') {
      const explanation = value.explanation.trim();
      if (!explanation || explanation.length > 1000) errors['explanation'] = fieldMessages['explanation'];
    }
    return errors;
  }

  private body(request: ComposerRequest) {
    const value = this.form.getRawValue();
    const provenance = provenanceValue(this.form.controls.provenance);
    const contact = request.kind === 'ContactAttempt';
    const linked = request.kind === 'Correction' || request.kind === 'Challenge';
    return {
      kind: request.kind,
      category: request.kind === 'Unknown' ? 'Unknown' as const : provenance.category,
      text: value.text.trim() || null,
      sourceDescription: provenance.sourceDescription,
      sourceUnknown: provenance.sourceUnknown,
      eventAt: toInstant(value.eventAt),
      // A blank optional event time is sent as an explicit Unknown.
      eventTimeUnknown: contact ? value.timeUnknown : !value.eventAt,
      relatedEntryId: linked ? request.target?.entryId ?? null : null,
      explanation: linked ? value.explanation.trim() : null,
      originalCategory: provenance.originalCategory,
      attemptedByDescription: contact && value.attemptedBy === 'someone' ? value.attemptedByDescription.trim() : null,
      targetLabel: contact ? value.targetLabel.trim() : null,
      contactMethod: contact ? value.method || null : null,
      contactOutcome: contact ? value.outcome || null : null,
    };
  }
}
