import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnChanges, OnInit, SimpleChanges, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AppIcon } from '../icon';
import { AdoptItemFamily, isItemId } from '../item-route';
import { focusHeadingState } from '../navigation';
import { ConcernApi, Concern, ConcernCategory, ConcernEntry, ConcernEntryKind, OriginalConcernCategory, AddConcernEntryRequest, ActiveFamilyMember } from './concern-api';

type InitialKind = 'None' | 'Observation' | 'Unknown';
type ComposerKind = ConcernEntryKind;

@Component({
  selector: 'app-concerns',
  imports: [CommonModule, ReactiveFormsModule, AppIcon, RouterLink],
  templateUrl: './concerns.html',
  styleUrl: './concerns.scss',
})
export class Concerns implements OnInit, OnChanges {
  @Input() currentFamilyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  /** The Concern ID from /concerns/:concernId (REQ-UX-014); null on the Concerns list. */
  @Input() concernId: string | null = null;
  /** The shell's authorize-first Family decision; called only after the Concern read returns 200. */
  @Input() adoptItemFamily: AdoptItemFamily | null = null;

  private readonly api = inject(ConcernApi);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router, { optional: true });

  protected readonly concerns = signal<readonly Concern[]>([]);
  protected readonly selected = signal<Concern | null>(null);
  protected readonly members = signal<readonly ActiveFamilyMember[]>([]);
  protected readonly loadingList = signal(true);
  protected readonly loadingDetail = signal(false);
  protected readonly loadingMembers = signal(false);
  protected readonly saving = signal(false);
  protected readonly listError = signal('');
  protected readonly actionError = signal('');
  /** The routed Concern read failed for a reason other than unavailability; Retry repeats that read. */
  protected readonly detailReadFailed = signal(false);
  protected readonly unavailable = signal(false);
  protected readonly showCreate = signal(false);
  protected readonly initialKind = signal<InitialKind>('None');
  protected readonly initialSourceUnknown = signal(false);
  protected readonly entrySourceUnknown = signal(false);
  protected readonly participantIds = signal<readonly string[]>([]);
  protected readonly managedParticipantIds = signal<readonly string[]>([]);
  protected readonly managingParticipants = signal(false);
  protected readonly initialEntryFailure = signal(false);
  protected readonly initialEntryMessage = signal('');
  protected readonly initialEntrySaving = signal(false);
  protected readonly initialValidation = signal('');
  protected readonly createAmbiguous = signal(false);
  protected readonly createAttempt = signal<{ startedAt: number; subjectId: string; reason: string } | null>(null);
  protected readonly composing = signal<ComposerKind | null>(null);
  protected readonly composerTarget = signal<ConcernEntry | null>(null);
  protected readonly resolving = signal(false);
  protected readonly dateFormat = 'MMM d, y, h:mm a';
  /** Compact timeline label; the full date with year stays available to assistive technology and as a tooltip. */
  protected readonly timelineFormat = 'MMM d, h:mm a';
  protected readonly focusHeadingState = focusHeadingState;

  protected readonly createForm = this.fb.nonNullable.group({
    subjectProfileId: ['', Validators.required],
    reason: ['', [Validators.required, Validators.maxLength(1000)]],
  });
  protected readonly initialForm = this.fb.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(2000)]],
    category: this.fb.nonNullable.control<ConcernCategory>('Reported', Validators.required),
    originalCategory: this.fb.nonNullable.control<OriginalConcernCategory>('Reported', Validators.required),
    sourceDescription: ['', Validators.maxLength(500)],
    eventAt: [''],
  });
  protected readonly entryForm = this.fb.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(2000)]],
    category: this.fb.nonNullable.control<ConcernCategory>('Reported', Validators.required),
    originalCategory: this.fb.nonNullable.control<OriginalConcernCategory>('Reported', Validators.required),
    sourceDescription: ['', Validators.maxLength(500)],
    eventAt: [''],
    explanation: ['', Validators.maxLength(1000)],
  });
  protected readonly resolutionForm = this.fb.nonNullable.group({
    note: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  ngOnInit(): void {
    this.refreshList();
    this.loadMembers();
    if (this.concernId !== null) this.openConcern(this.concernId);
  }

  /** Starts a separate Incident. Navigation state carries IDs only; nothing from the Concern is copied. */
  protected startIncident(concern: Concern): void {
    void this.router?.navigateByUrl('/incidents/new', { state: {
      originatingConcernId: concern.concernId, familyGroupId: concern.familyGroupId, subjectProfileId: concern.subjectProfileId,
    } });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['currentFamilyId'] && !changes['currentFamilyId'].firstChange) this.loadMembers();
    // /concerns and /concerns/:id share this component. A Concern already shown (for example just created) is not read again.
    if (changes['concernId'] && !changes['concernId'].firstChange) {
      if (this.concernId === null) this.resetDetail();
      else if (this.selected()?.concernId !== this.concernId) this.openConcern(this.concernId);
    }
  }

  protected refreshList(): void {
    this.loadingList.set(true);
    this.listError.set('');
    this.api.list().pipe(finalize(() => this.loadingList.set(false))).subscribe({
      next: rows => {
        this.concerns.set(rows);
        this.resolveAmbiguousCreate(rows);
      },
      error: error => {
        this.listError.set(this.isUnauthorized(error)
          ? 'Your session may have ended. Please sign in again.'
          : 'Concerns could not be loaded. Please try again.');
      },
    });
  }

  /**
   * Opens the routed Concern: one item read, the server authorizes it, then the shell decides the Family before it
   * renders (REQ-UX-014). A non-GUID ID is unavailable without a request.
   */
  private openConcern(concernId: string): void {
    this.actionError.set('');
    this.detailReadFailed.set(false);
    this.unavailable.set(false);
    this.selected.set(null);
    if (!isItemId(concernId)) {
      this.showUnavailable();
      return;
    }
    this.loadingDetail.set(true);
    this.api.get(concernId).subscribe({
      next: async concern => {
        const outcome = this.adoptItemFamily ? await this.adoptItemFamily(concern.familyGroupId, 'concern') : 'current';
        if (this.concernId !== concernId) return; // The user has since moved on.
        this.loadingDetail.set(false);
        if (outcome === 'unavailable') this.showUnavailable();
        else this.selected.set(concern);
      },
      error: error => {
        if (this.concernId !== concernId) return;
        this.loadingDetail.set(false);
        if (this.isUnavailable(error)) this.showUnavailable();
        else {
          this.actionError.set(this.isUnauthorized(error)
            ? 'Your session may have ended. Please sign in again.'
            : 'This Concern could not be opened. Please try again.');
          this.detailReadFailed.set(true);
        }
      },
    });
  }

  /** Repeats the same protected item read; the server still authorizes it, and a 404 still shows the unavailable state. */
  protected retryOpenConcern(): void {
    if (this.concernId) this.openConcern(this.concernId);
  }

  /** Returns to the list, and to the list URL when this is a Concern URL. */
  protected backToList(): void {
    this.resetDetail();
    if (this.concernId !== null) void this.router?.navigateByUrl('/concerns');
  }

  /** OD-G: a created Concern opens at its own URL once it is confirmed and no recovery choice is pending. */
  private showCreatedConcern(concernId: string): void {
    void this.router?.navigateByUrl(`/concerns/${concernId}`);
  }

  protected continueToConcern(): void {
    this.initialEntryFailure.set(false);
    this.initialEntryMessage.set('');
    const concern = this.selected();
    if (concern) this.showCreatedConcern(concern.concernId);
  }

  protected resetDetail(): void {
    this.selected.set(null);
    this.loadingDetail.set(false);
    this.unavailable.set(false);
    this.actionError.set('');
    this.detailReadFailed.set(false);
    this.managingParticipants.set(false);
    this.composing.set(null);
    this.resolving.set(false);
  }

  protected openCreate(): void {
    this.showCreate.set(true);
    this.actionError.set('');
    this.detailReadFailed.set(false);
    this.focusById('create-concern-heading');
  }

  protected openResolve(): void {
    this.resolving.set(true);
    this.focusById('resolution-note');
  }

  protected loadMembers(): void {
    const groupId = this.currentFamilyId;
    if (!groupId) {
      this.members.set([]);
      return;
    }
    this.loadingMembers.set(true);
    this.api.members(groupId).pipe(finalize(() => this.loadingMembers.set(false))).subscribe({
      next: members => this.members.set(members),
      error: () => this.members.set([]),
    });
  }

  protected subjectChoices(): readonly ActiveFamilyMember[] {
    const ownId = this.ownProfileId;
    const others = this.members().filter(member => member.profileId !== ownId);
    if (!ownId) return others;
    const own = this.members().find(member => member.profileId === ownId);
    return own ? [{ ...own, displayName: `${own.displayName} (you)` }, ...others]
      : [{ profileId: ownId, displayName: 'You (self-Concern)' }, ...others];
  }

  protected participantChoices(): readonly ActiveFamilyMember[] {
    return this.members().filter(member => member.profileId !== this.ownProfileId);
  }

  protected toggleCreateParticipant(profileId: string, checked: boolean): void {
    const next = new Set(this.participantIds());
    if (checked) next.add(profileId); else next.delete(profileId);
    this.participantIds.set([...next]);
  }

  protected isCreateParticipantSelected(profileId: string): boolean {
    return this.participantIds().includes(profileId);
  }

  protected setInitialKind(value: InitialKind): void {
    this.initialKind.set(value);
    this.initialValidation.set('');
    if (value === 'Unknown') this.initialForm.controls.category.setValue('Unknown');
    else if (value === 'Observation' && this.initialForm.controls.category.value === 'Unknown')
      this.initialForm.controls.category.setValue('Reported');
  }

  protected setCategory(kind: 'initial' | 'entry', value: ConcernCategory): void {
    const form = kind === 'initial' ? this.initialForm : this.entryForm;
    form.controls.category.setValue(value);
    if (value === 'Confirmed') {
      if (kind === 'initial') this.initialSourceUnknown.set(false);
      else this.entrySourceUnknown.set(false);
    }
  }

  protected createConcern(): void {
    if (this.saving() || this.createAmbiguous()) return;
    this.createForm.markAllAsTouched();
    const { subjectProfileId, reason } = this.createForm.getRawValue();
    if (!this.currentFamilyId || !subjectProfileId || !reason.trim() || reason.trim().length > 1000) {
      this.actionError.set('Choose a subject and enter a reason of 1–1,000 characters.');
      return;
    }
    let initialRequest: AddConcernEntryRequest | null = null;
    if (this.initialKind() !== 'None') {
      this.initialForm.markAllAsTouched();
      const candidateKind = this.initialKind() as 'Observation' | 'Unknown';
      const validation = this.validateEntryForm(this.initialForm, candidateKind, this.initialSourceUnknown(), false);
      if (validation) {
        this.initialValidation.set(validation);
        return;
      }
      initialRequest = this.makeEntryRequest(this.initialForm, candidateKind, this.initialSourceUnknown(), null);
    }
    this.actionError.set('');
    this.initialEntryFailure.set(false);
    this.initialEntryMessage.set('');
    this.saving.set(true);
    this.createAttempt.set({ startedAt: Date.now(), subjectId: subjectProfileId, reason: reason.trim() });
    this.api.create(this.currentFamilyId, {
      subjectProfileId,
      reason: reason.trim(),
      participantProfileIds: [...this.participantIds()],
    }).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: created => {
        this.upsert(created);
        this.selected.set(created);
        this.showCreate.set(false);
        this.createAmbiguous.set(false);
        this.createAttempt.set(null);
        this.createForm.reset({ subjectProfileId: '', reason: '' });
        this.participantIds.set([]);
        this.initialKind.set('None');
        if (initialRequest) this.submitInitialEntry(created.concernId, initialRequest);
        else this.showCreatedConcern(created.concernId);
      },
      error: error => {
        if (this.isUnavailable(error)) {
          this.actionError.set('The Concern could not be created with the selected information. Refresh your Family members and try again.');
        } else if (this.isValidation(error)) {
          this.actionError.set('Review the subject, audience, and reason, then try again.');
        } else if (!this.isUnauthorized(error)) {
          this.createAmbiguous.set(true);
          this.actionError.set('We could not confirm whether the Concern was created. Refresh the list before trying again.');
        } else {
          this.actionError.set('Your session may have ended. Please sign in again.');
        }
      },
    });
  }

  protected checkAmbiguousCreate(): void {
    if (!this.createAmbiguous()) return;
    this.refreshList();
  }

  private resolveAmbiguousCreate(rows: readonly Concern[]): void {
    const attempt = this.createAttempt();
    if (!this.createAmbiguous() || !attempt) return;
    const match = rows.find(item => item.creatorProfileId === this.ownProfileId
      && item.subjectProfileId === attempt.subjectId
      && item.creationReason === attempt.reason
      && new Date(item.createdAt).getTime() >= attempt.startedAt - 60_000);
    this.createAmbiguous.set(false);
    this.createAttempt.set(null);
    if (match) {
      this.selected.set(match);
      this.showCreate.set(false);
      this.actionError.set('A matching Concern was found in your authorized list. Review it before creating another.');
      this.showCreatedConcern(match.concernId);
    } else {
      // Show the refreshed list; the create draft is kept if the user reopens the form.
      this.showCreate.set(false);
      this.actionError.set('No matching Concern appeared in the refreshed list. Review the list before deciding whether to submit again.');
    }
  }

  private submitInitialEntry(concernId: string, request: AddConcernEntryRequest): void {
    this.initialEntrySaving.set(true);
    this.api.addEntry(concernId, request).pipe(finalize(() => this.initialEntrySaving.set(false))).subscribe({
      next: concern => {
        this.upsert(concern);
        this.selected.set(concern);
        this.initialEntryFailure.set(false);
        this.initialEntryMessage.set('The Concern and initial entry were created.');
        this.showCreatedConcern(concern.concernId);
      },
      error: error => {
        if (this.isUnavailable(error)) {
          this.showUnavailable();
          return;
        }
        this.initialEntryFailure.set(true);
        this.initialEntryMessage.set('The Concern was created, but the initial entry was not saved. Continue to the Concern or retry only this entry.');
        this.pendingInitial = { concernId, request };
      },
    });
  }

  private pendingInitial: { concernId: string; request: AddConcernEntryRequest } | null = null;

  protected retryInitialEntry(): void {
    if (!this.pendingInitial || this.initialEntrySaving()) return;
    const pending = this.pendingInitial;
    this.initialEntrySaving.set(true);
    this.api.addEntry(pending.concernId, pending.request)
      .pipe(finalize(() => this.initialEntrySaving.set(false))).subscribe({
        next: concern => {
          this.pendingInitial = null;
          this.initialEntryFailure.set(false);
          this.initialEntryMessage.set('The initial entry was added.');
          this.upsert(concern);
          this.selected.set(concern);
          this.showCreatedConcern(concern.concernId);
        },
        error: error => {
          if (this.isUnavailable(error)) this.showUnavailable();
          else this.initialEntryMessage.set('The entry is still not saved. Your draft is preserved; you can retry it again.');
        },
      });
  }

  protected openComposer(kind: ComposerKind, target: ConcernEntry | null = null): void {
    this.composerTarget.set(target);
    this.composing.set(kind);
    this.entrySourceUnknown.set(false);
    this.entryForm.reset({
      text: '', category: kind === 'Unknown' ? 'Unknown' : (target?.category ?? 'Reported'),
      originalCategory: target?.category === 'Unknown' ? 'Reported' : (target?.category as OriginalConcernCategory ?? 'Reported'),
      sourceDescription: '', eventAt: '', explanation: '',
    });
    this.actionError.set('');
    this.focusById('entry-composer-heading');
  }

  protected cancelComposer(): void {
    this.composing.set(null);
    this.composerTarget.set(null);
    this.entryForm.reset({ text: '', category: 'Reported', originalCategory: 'Reported', sourceDescription: '', eventAt: '', explanation: '' });
    this.actionError.set('');
  }

  protected submitEntry(): void {
    const concern = this.selected();
    const kind = this.composing();
    if (!concern || !kind || this.saving()) return;
    const validation = this.validateEntryForm(this.entryForm, kind, this.entrySourceUnknown(), kind === 'Correction' || kind === 'Challenge');
    if (validation) {
      this.actionError.set(validation);
      this.entryForm.markAllAsTouched();
      return;
    }
    const request = this.makeEntryRequest(this.entryForm, kind, this.entrySourceUnknown(), this.composerTarget()?.entryId ?? null);
    this.saving.set(true);
    this.actionError.set('');
    this.api.addEntry(concern.concernId, request).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: updated => {
        this.upsert(updated);
        this.selected.set(updated);
        this.cancelComposer();
      },
      error: error => this.handleWriteError(error, 'The entry could not be saved.'),
    });
  }

  protected openParticipantManager(): void {
    const concern = this.selected();
    if (!concern || concern.state !== 'Open' || !this.isCreator(concern)) return;
    this.managingParticipants.set(true);
    this.managedParticipantIds.set(concern.participants.map(person => person.profileId));
    this.actionError.set('');
    this.api.members(concern.familyGroupId).subscribe({
      next: people => this.members.set(people),
      error: error => {
        this.managingParticipants.set(false);
        this.actionError.set(this.isUnauthorized(error) ? 'Your session may have ended. Please sign in again.' : 'Eligible participants could not be loaded. Try again.');
      },
    });
  }

  protected toggleManagedParticipant(profileId: string, checked: boolean): void {
    const next = new Set(this.managedParticipantIds());
    if (checked) next.add(profileId); else next.delete(profileId);
    this.managedParticipantIds.set([...next]);
  }

  protected isManagedParticipant(profileId: string): boolean {
    return this.managedParticipantIds().includes(profileId);
  }

  protected saveParticipants(): void {
    const concern = this.selected();
    if (!concern || !this.isCreator(concern) || concern.state !== 'Open' || this.saving()) return;
    this.saving.set(true);
    this.api.replaceParticipants(concern.concernId, { participantProfileIds: [...this.managedParticipantIds()] })
      .pipe(finalize(() => this.saving.set(false))).subscribe({
        next: updated => {
          this.upsert(updated);
          this.selected.set(updated);
          this.managingParticipants.set(false);
        },
        error: error => this.handleWriteError(error, 'The participant selection could not be saved. Refresh and review it again.'),
      });
  }

  protected cancelParticipantManagement(): void {
    this.managingParticipants.set(false);
    this.managedParticipantIds.set([]);
  }

  protected resolveConcern(): void {
    const concern = this.selected();
    this.resolutionForm.markAllAsTouched();
    const note = this.resolutionForm.controls.note.value.trim();
    if (!concern || !this.isCreator(concern) || concern.state !== 'Open' || !note || note.length > 2000 || this.saving()) {
      this.actionError.set('Enter a resolution note of 1–2,000 characters.');
      return;
    }
    this.saving.set(true);
    this.api.resolve(concern.concernId, { note }).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: updated => {
        this.upsert(updated);
        this.selected.set(updated);
        this.resolving.set(false);
        this.resolutionForm.reset({ note: '' });
      },
      error: error => this.handleWriteError(error, 'The Concern could not be resolved.'),
    });
  }

  protected isCreator(concern = this.selected()): boolean {
    return !!concern && !!this.ownProfileId && concern.creatorProfileId === this.ownProfileId;
  }

  protected isParticipant(concern = this.selected()): boolean {
    return !!concern && !!this.ownProfileId
      && concern.participants.some(person => person.profileId === this.ownProfileId);
  }

  protected canContribute(concern = this.selected()): boolean {
    return !!concern && concern.state === 'Open' && (this.isCreator(concern) || this.isParticipant(concern));
  }

  protected canCorrect(entry: ConcernEntry): boolean {
    return this.canContribute() && !!this.ownProfileId && entry.contributorProfileId === this.ownProfileId;
  }

  protected canChallenge(entry: ConcernEntry): boolean {
    return this.canContribute() && !!this.ownProfileId && entry.contributorProfileId !== this.ownProfileId;
  }

  protected subjectIsParticipant(concern: Concern): boolean {
    return concern.participants.some(person => person.profileId === concern.subjectProfileId);
  }

  protected relationText(entry: ConcernEntry): string {
    if (!entry.relatedEntryId) return '';
    return entry.kind === 'Correction' ? 'Correction to original entry' : 'Another report about this entry';
  }

  protected stateDescription(state: Concern['state']): string {
    if (state === 'Open') return 'Coordination is open';
    if (state === 'Frozen') return 'Coordination has ended; this record is read-only';
    return 'Coordination on this record has ended';
  }

  protected categoryText(entry: ConcernEntry): string {
    return entry.isOutdated ? `${entry.category} · Outdated` : entry.category;
  }

  protected entryKindText(kind: ConcernEntryKind): string {
    return ({ Observation: 'Observation', Unknown: 'Unknown', Correction: 'Correction', Challenge: 'Another report' } as const)[kind];
  }

  protected initials(name: string): string {
    return name.split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0].toUpperCase()).join('');
  }

  protected closeCreate(): void {
    this.showCreate.set(false);
    this.actionError.set('');
  }

  /** Moves focus to a newly revealed heading or field after Angular renders it. */
  private focusById(id: string): void {
    setTimeout(() => document.getElementById(id)?.focus());
  }

  private validateEntryForm(
    form: typeof this.entryForm | typeof this.initialForm,
    kind: ComposerKind,
    sourceUnknown: boolean,
    needsExplanation: boolean,
  ): string {
    const values = form.getRawValue();
    const text = values.text.trim();
    if (!text || text.length > 2000) return 'Enter information of 1–2,000 characters.';
    const category = kind === 'Unknown' ? 'Unknown' : values.category;
    if (kind === 'Observation' && category === 'Unknown') return 'Choose Unknown as an explicit Unknown entry.';
    const source = values.sourceDescription.trim();
    if (sourceUnknown === !!source) return 'Enter a source or mark the source as unknown.';
    if (source.length > 500) return 'Source descriptions can be up to 500 characters.';
    if (category === 'Confirmed' && sourceUnknown) return 'Confirmed information needs a described verification source.';
    if (category === 'Outdated' && !['Confirmed', 'Reported', 'Assumption'].includes(values.originalCategory))
      return 'Choose the earlier classification for this outdated information.';
    if (needsExplanation) {
      const explanation = this.entryForm.controls.explanation.value.trim();
      if (!explanation || explanation.length > 1000) return 'Enter an explanation of 1–1,000 characters.';
    }
    return '';
  }

  private makeEntryRequest(
    form: typeof this.entryForm | typeof this.initialForm,
    kind: ComposerKind,
    sourceUnknown: boolean,
    relatedEntryId: string | null,
  ): AddConcernEntryRequest {
    const values = form.getRawValue();
    const category: ConcernCategory = kind === 'Unknown' ? 'Unknown' : values.category;
    const original = category === 'Outdated' ? values.originalCategory : null;
    const eventAt = values.eventAt ? new Date(values.eventAt).toISOString() : null;
    return {
      kind,
      category,
      text: values.text.trim(),
      sourceDescription: sourceUnknown ? null : values.sourceDescription.trim(),
      sourceUnknown,
      eventAt,
      relatedEntryId,
      explanation: kind === 'Correction' || kind === 'Challenge' ? this.entryForm.controls.explanation.value.trim() : null,
      isOutdated: category === 'Outdated',
      originalCategory: original,
    };
  }

  private handleWriteError(error: unknown, fallback: string): void {
    if (this.isUnavailable(error)) {
      this.showUnavailable();
    } else if (this.isUnauthorized(error)) {
      this.actionError.set('Your session may have ended. Please sign in again.');
    } else if (this.isValidation(error)) {
      this.actionError.set('The server could not accept those details. Review the fields and try again.');
    } else {
      this.actionError.set(fallback);
    }
  }

  private showUnavailable(): void {
    this.selected.set(null);
    this.unavailable.set(true);
    this.managingParticipants.set(false);
    this.composing.set(null);
    this.resolving.set(false);
    // The unavailable card is the single message; focusing its heading announces it.
    this.actionError.set('');
    this.focusById('unavailable-heading');
    this.refreshList();
  }

  private upsert(concern: Concern): void {
    const current = this.concerns().filter(item => item.concernId !== concern.concernId);
    const next = [...current, concern].sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt));
    this.concerns.set(next);
  }

  private isUnavailable(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 404;
  }

  private isUnauthorized(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 401;
  }

  private isValidation(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 400;
  }
}
