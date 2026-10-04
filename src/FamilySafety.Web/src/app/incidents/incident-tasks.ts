import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Observable, finalize } from 'rxjs';
import { Incident, IncidentApi, IncidentTask } from './incident-api';
import { IncidentProvenanceFields } from './incident-provenance-fields';
import {
  FieldErrors, dateTimeFormat, focusById, focusEntry, isStatus, mapValidation, provenanceErrors, provenanceGroup, provenanceValue,
  sessionEndedMessage, textError, toInstant,
} from './incident-shared';

type TaskMode = { type: 'create' } | { type: 'complete' | 'cancel'; task: IncidentTask };

const fieldMessages: FieldErrors = {
  description: 'Enter a task description of 1–500 characters.',
  assignee: 'Choose who the task is assigned to.',
  result: 'Enter a result of 1–2,000 characters.',
  category: 'Choose Confirmed, Reported, Assumption, or Unknown.',
  source: 'Enter a source or mark the source as unknown. Confirmed information needs a described source.',
  reason: 'Enter a reason of 1–1,000 characters.',
};

/**
 * Incident tasks: immutable description, one assignee, Open / Done / Cancelled, and "Not completed"
 * once the Incident ends. Controls are display hints only; the server enforces every task rule.
 */
@Component({
  selector: 'app-incident-tasks',
  imports: [DatePipe, ReactiveFormsModule, IncidentProvenanceFields],
  templateUrl: './incident-tasks.html',
  styleUrls: ['./incident-common.scss', './incident-forms.scss', './incident-tasks.scss'],
})
export class IncidentTasks {
  @Input({ required: true }) incident!: Incident;
  @Input() ownProfileId: string | null = null;
  @Input() isCoordinator = false;
  @Input() canWrite = false;
  @Output() readonly changed = new EventEmitter<{ incident: Incident; message: string }>();
  @Output() readonly denied = new EventEmitter<void>();

  private readonly api = inject(IncidentApi);
  private returnFocusId = '';

  protected readonly dateTimeFormat = dateTimeFormat;
  protected readonly focusEntry = focusEntry;
  protected readonly mode = signal<TaskMode | null>(null);
  protected readonly saving = signal(false);
  protected readonly errors = signal<FieldErrors>({});
  protected readonly message = signal('');

  protected readonly createForm = new FormGroup({
    description: new FormControl('', { nonNullable: true }),
    assigneeProfileId: new FormControl('', { nonNullable: true }),
  });
  protected readonly completeForm = new FormGroup({
    resultText: new FormControl('', { nonNullable: true }),
    eventAt: new FormControl('', { nonNullable: true }),
    provenance: provenanceGroup(),
  });
  protected readonly cancelForm = new FormGroup({ reason: new FormControl('', { nonNullable: true }) });

  protected statusText(task: IncidentTask): string {
    return task.notCompleted ? 'Not completed' : task.status;
  }

  protected statusClass(task: IncidentTask): string {
    if (task.notCompleted) return 'status-pill incident-pill incident-pill--cancelled';
    return task.status === 'Open' ? 'status-pill incident-pill incident-pill--open-task' : `status-pill incident-pill incident-pill--${task.status.toLowerCase()}`;
  }

  protected canComplete(task: IncidentTask): boolean {
    return this.canWrite && task.status === 'Open' && task.assigneeProfileId === this.ownProfileId;
  }

  /** Coordinator: any Open task. Participant: only a task they created and are assigned. */
  protected canCancel(task: IncidentTask): boolean {
    return this.canWrite && task.status === 'Open' && (this.isCoordinator
      || (task.createdByProfileId === this.ownProfileId && task.assigneeProfileId === this.ownProfileId));
  }

  protected assigneeChoices() {
    return [...this.incident.participants]
      .sort((a, b) => Number(b.isCoordinator) - Number(a.isCoordinator))
      .map(person => ({ profileId: person.profileId, label: person.profileId === this.ownProfileId ? `${person.displayName} (you)` : person.displayName }));
  }

  protected open(mode: TaskMode, triggerId: string): void {
    this.returnFocusId = triggerId;
    this.errors.set({});
    this.message.set('');
    this.mode.set(mode);
    focusById('incident-task-form-heading');
  }

  protected close(): void {
    this.mode.set(null);
    this.errors.set({});
    focusById(this.returnFocusId);
  }

  protected isMode(type: TaskMode['type'], task?: IncidentTask): boolean {
    const mode = this.mode();
    return !!mode && mode.type === type && (!task || ('task' in mode && mode.task.taskId === task.taskId));
  }

  protected submitCreate(): void {
    const { description, assigneeProfileId } = this.createForm.getRawValue();
    const assignee = this.isCoordinator ? assigneeProfileId : this.ownProfileId ?? '';
    const errors: FieldErrors = {};
    if (textError(description, 500)) errors['description'] = fieldMessages['description'];
    if (!assignee) errors['assignee'] = fieldMessages['assignee'];
    this.run(errors, () => this.api.createTask(this.incident.incidentId, description.trim(), assignee), 'Task added.', this.createForm);
  }

  protected submitComplete(task: IncidentTask): void {
    const { resultText, eventAt } = this.completeForm.getRawValue();
    const errors: FieldErrors = provenanceErrors(this.completeForm.controls.provenance);
    if (textError(resultText, 2000)) errors['result'] = fieldMessages['result'];
    const provenance = provenanceValue(this.completeForm.controls.provenance);
    this.run(errors, () => this.api.completeTask(this.incident.incidentId, task.taskId, {
      resultText: resultText.trim(), category: provenance.category, sourceDescription: provenance.sourceDescription,
      sourceUnknown: provenance.sourceUnknown, eventAt: toInstant(eventAt),
    }), 'Task completed.', this.completeForm);
  }

  protected submitCancel(task: IncidentTask): void {
    const { reason } = this.cancelForm.getRawValue();
    const errors: FieldErrors = {};
    if (textError(reason, 1000)) errors['reason'] = fieldMessages['reason'];
    this.run(errors, () => this.api.cancelTask(this.incident.incidentId, task.taskId, reason.trim()), 'Task cancelled.', this.cancelForm);
  }

  protected describedBy(...fields: string[]): string | null {
    const ids = fields.filter(field => this.errors()[field]).map(field => `task-${field}-error`);
    return ids.length ? ids.join(' ') : null;
  }

  protected hasErrors(): boolean {
    return Object.keys(this.errors()).length > 0;
  }

  private run(errors: FieldErrors, call: () => Observable<Incident>, done: string, form: FormGroup): void {
    if (this.saving()) return;
    this.errors.set(errors);
    this.message.set('');
    if (Object.keys(errors).length) {
      focusById('incident-task-summary');
      return;
    }
    this.saving.set(true);
    call().pipe(finalize(() => this.saving.set(false))).subscribe({
      next: incident => {
        form.reset();
        this.mode.set(null);
        this.changed.emit({ incident, message: done });
        focusById(this.returnFocusId);
      },
      error: error => {
        if (isStatus(error, 404)) {
          // The draft stays in its form; the detail refreshes and decides what is still shown.
          this.mode.set(null);
          this.denied.emit();
        } else if (isStatus(error, 400)) {
          this.errors.set(mapValidation(error, {
            Description: 'description', AssigneeProfileId: 'assignee', ResultText: 'result', Category: 'category',
            SourceDescription: 'source', Reason: 'reason',
          }, fieldMessages));
          focusById('incident-task-summary');
        } else {
          this.message.set(isStatus(error, 401) ? sessionEndedMessage : 'The task change could not be saved. Your draft is kept; try again.');
        }
      },
    });
  }
}
