import { CommonModule } from '@angular/common';
import { AppIcon } from '../icon';
import { dateTile, displayDateTime, displayRange } from '../date-format';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AdoptItemFamily, isItemId } from '../item-route';
import { ItemUnavailable } from '../item-unavailable';
import { focusHeadingState } from '../navigation';

interface FamilyGroup { familyGroupId: string; name: string }
interface Person { profileId: string; displayName: string }
interface TripContact {
  contactId: string;
  name: string;
  relationshipRole: string | null;
  phone: string | null;
  email: string | null;
}
type AudienceType = 'FamilyGroup' | 'SelectedPeople';

interface Trip {
  tripId: string;
  familyGroupId: string;
  familyGroupName: string;
  creatorProfileId: string;
  creatorDisplayName: string;
  canEdit: boolean;
  title: string;
  primaryDestination: string;
  startDate: string;
  endDate: string;
  description: string | null;
  transportationNotes: string | null;
  accommodationNotes: string | null;
  expectedArrivalDate: string | null;
  expectedArrivalTime: string | null;
  expectedArrivalTimeZone: string | null;
  audienceType: AudienceType;
  travelers: Person[];
  contacts: TripContact[];
  audienceRecipients: Person[];
}

interface TripNote {
  noteId: string;
  authorProfileId: string;
  authorDisplayName: string;
  content: string;
  createdAt: string;
  updatedAt: string | null;
  canEdit: boolean;
  canDelete: boolean;
  canHide: boolean;
}

interface TripAudiencePreview {
  previewToken: string;
  audienceType: AudienceType;
  audienceRecipients: Person[];
  gainingAccess: Person[];
  losingAccess: Person[];
}

@Component({
  selector: 'app-trips',
  imports: [CommonModule, ReactiveFormsModule, AppIcon, RouterLink, ItemUnavailable],
  templateUrl: './trips.html',
  styleUrl: './trips.scss',
})
export class Trips implements OnInit, OnChanges {
  private readonly destroyRef = inject(DestroyRef);
  @Input() activeGroups: readonly FamilyGroup[] = [];
  @Input() allActiveGroups: readonly FamilyGroup[] = [];
  @Input() currentFamilyId: string | null = null;
  @Input() ownProfileId: string | null = null;
  /** The Trip ID from /trips/:tripId (REQ-UX-014); null on the Trips list. */
  @Input() tripId: string | null = null;
  /** The shell's authorize-first Family decision; called only after the Trip read returns 200. */
  @Input() adoptItemFamily: AdoptItemFamily | null = null;

  private readonly http = inject(HttpClient);
  private readonly formBuilder = inject(FormBuilder);
  private memberRequestVersion = 0;

  protected readonly trips = signal<readonly Trip[]>([]);
  protected readonly selectedTrip = signal<Trip | null>(null);
  protected readonly notes = signal<readonly TripNote[]>([]);
  protected readonly notesReadState = signal<'idle' | 'loading' | 'loaded' | 'error'>('idle');
  private notesTripId: string | null = null;
  protected readonly noteDraft = this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]);
  protected readonly noteEditDraft = this.formBuilder.control('', [Validators.required, Validators.maxLength(1000)]);
  protected readonly editingNoteId = signal<string | null>(null);
  protected readonly noteSaving = signal(false);
  protected readonly noteError = signal('');
  protected readonly availableMembers = signal<readonly Person[]>([]);
  protected readonly travelerIds = signal<readonly string[]>([]);
  protected readonly recipientIds = signal<readonly string[]>([]);
  protected readonly showingForm = signal(false);
  protected readonly editingTripId = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly tripsLoadError = signal('');
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected readonly detailError = signal('');
  protected readonly itemLoading = signal(false);
  protected readonly unavailable = signal(false);
  protected readonly focusHeadingState = focusHeadingState;
  protected readonly changingAudience = signal(false);
  protected readonly audienceMode = signal<AudienceType>('SelectedPeople');
  protected readonly audienceRecipientIds = signal<readonly string[]>([]);
  protected readonly audiencePreview = signal<TripAudiencePreview | null>(null);
  protected readonly audienceError = signal('');
  protected readonly audienceBusy = signal(false);
  protected readonly timeZones = supportedTimeZones();

  protected currentTrips(): readonly Trip[] {
    return this.trips().filter(trip => trip.familyGroupId === this.currentFamilyId);
  }

  // Creator access survives membership loss; these items are separate from the
  // selected Family and never provide a directory or group-membership shortcut.
  protected retainedCreatorTrips(): readonly Trip[] {
    return this.trips().filter(trip => trip.creatorProfileId === this.ownProfileId
      && !this.allActiveGroups.some(group => group.familyGroupId === trip.familyGroupId));
  }

  protected readonly tripForm = this.formBuilder.group({
    familyGroupId: ['', Validators.required],
    title: ['', [Validators.required, Validators.maxLength(100)]],
    primaryDestination: ['', [Validators.required, Validators.maxLength(200)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    description: ['', Validators.maxLength(1000)],
    transportationNotes: ['', Validators.maxLength(1000)],
    accommodationNotes: ['', Validators.maxLength(1000)],
    expectedArrivalDate: [''],
    expectedArrivalTime: [''],
    expectedArrivalTimeZone: [defaultTimeZone()],
    audienceType: this.formBuilder.nonNullable.control<AudienceType | ''>('', Validators.required),
    contacts: this.formBuilder.array<ReturnType<Trips['createContactForm']>>([]),
  });

  ngOnInit(): void {
    if (this.tripId === null) this.loadTrips();
    else this.openRoutedTrip(this.tripId);
  }

  ngOnChanges(): void {
    if (!this.tripForm.controls.familyGroupId.value && this.activeGroups.length > 0) {
      this.tripForm.controls.familyGroupId.setValue(this.activeGroups[0].familyGroupId);
    }
  }

  protected get contactForms(): FormArray<ReturnType<Trips['createContactForm']>> {
    return this.tripForm.controls.contacts;
  }

  private createContactForm(contact?: Partial<TripContact>) {
    return this.formBuilder.group({
      name: [contact?.name ?? '', [Validators.required, Validators.maxLength(100)]],
      relationshipRole: [contact?.relationshipRole ?? '', Validators.maxLength(100)],
      phone: [contact?.phone ?? '', Validators.maxLength(32)],
      email: [contact?.email ?? '', [Validators.email, Validators.maxLength(254)]],
    });
  }

  protected addContact(): void { this.contactForms.push(this.createContactForm()); }
  protected removeContact(index: number): void { this.contactForms.removeAt(index); }

  protected openCreate(): void {
    this.error.set('');
    this.editingTripId.set(null);
    this.selectedTrip.set(null);
    this.resetForm();
    this.showingForm.set(true);
  }

  protected beginEdit(): void {
    const trip = this.selectedTrip();
    if (!trip?.canEdit) return;
    this.error.set('');
    this.editingTripId.set(trip.tripId);
    this.tripForm.reset({
      familyGroupId: trip.familyGroupId,
      title: trip.title,
      primaryDestination: trip.primaryDestination,
      startDate: trip.startDate.slice(0, 10),
      endDate: trip.endDate.slice(0, 10),
      description: trip.description ?? '',
      transportationNotes: trip.transportationNotes ?? '',
      accommodationNotes: trip.accommodationNotes ?? '',
      expectedArrivalDate: trip.expectedArrivalDate?.slice(0, 10) ?? '',
      expectedArrivalTime: trip.expectedArrivalTime?.slice(0, 5) ?? '',
      expectedArrivalTimeZone: trip.expectedArrivalTimeZone ?? defaultTimeZone(),
      audienceType: trip.audienceType,
    });
    this.travelerIds.set(trip.travelers.map(person => person.profileId));
    this.recipientIds.set(trip.audienceRecipients.map(person => person.profileId));
    this.contactForms.clear();
    trip.contacts.forEach(contact => this.contactForms.push(this.createContactForm(contact)));
    this.loadMembers();
    this.showingForm.set(true);
  }

  protected cancelForm(): void {
    this.showingForm.set(false);
    this.error.set('');
  }

  /**
   * /trips/:tripId: read the Trip, let the server authorize it, then the shell decides the Family (REQ-UX-014). A Trip
   * whose Family is not selectable after one refresh still renders for its retained creator, with no switch (OD-A).
   */
  private openRoutedTrip(tripId: string): void {
    this.detailError.set('');
    if (!isItemId(tripId)) {
      this.unavailable.set(true);
      return;
    }
    this.itemLoading.set(true);
    this.http.get<Trip>(`/api/trips/${encodeURIComponent(tripId)}`).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: async trip => {
        const outcome = this.adoptItemFamily ? await this.adoptItemFamily(trip.familyGroupId, 'trip') : 'current';
        this.itemLoading.set(false);
        if (outcome === 'unavailable') {
          this.unavailable.set(true);
          return;
        }
        this.selectedTrip.set(trip);
        this.showingForm.set(false);
        this.loadNotes(trip.tripId);
      },
      error: (response: HttpErrorResponse) => {
        this.itemLoading.set(false);
        // Missing and unauthorized are the same 404 and keep the generic unavailable card. Any other failure is a
        // read problem, not a statement about the Trip, so it gets its own message and a Retry.
        if (response.status === 404) this.unavailable.set(true);
        else this.detailError.set(response.status === 401
          ? 'Your session may have ended. Please sign in again.'
          : 'This Trip could not be opened. Please try again.');
      },
    });
  }

  /** Repeats the same authorize-first Trip read; the Family is still decided only after a 200. */
  protected retryRoutedTrip(): void {
    if (this.tripId !== null) this.openRoutedTrip(this.tripId);
  }

  protected closeDetail(): void {
    this.selectedTrip.set(null);
    this.notes.set([]);
    this.notesTripId = null;
    this.notesReadState.set('idle');
  }

  protected readonly dateTile = dateTile;
  protected readonly displayRange = displayRange;
  protected readonly displayDateTime = displayDateTime;

  protected beginAudienceChange(): void {
    const trip = this.selectedTrip();
    if (!trip?.canEdit) return;
    const canBrowse = this.canBrowseTripGroup(trip);
    this.audienceMode.set(canBrowse ? trip.audienceType : 'SelectedPeople');
    this.audienceRecipientIds.set(trip.audienceRecipients.map(person => person.profileId));
    this.audiencePreview.set(null);
    this.audienceError.set('');
    this.changingAudience.set(true);
    if (canBrowse) this.loadMembers(trip.familyGroupId);
  }

  protected cancelAudienceChange(): void {
    this.changingAudience.set(false);
    this.audiencePreview.set(null);
    this.audienceError.set('');
  }

  protected canBrowseTripGroup(trip = this.selectedTrip()): boolean {
    return !!trip && this.activeGroups.some(group => group.familyGroupId === trip.familyGroupId);
  }

  protected audienceChoices(): readonly Person[] {
    const trip = this.selectedTrip();
    if (!trip) return [];
    if (this.canBrowseTripGroup(trip)) return this.availableMembers();
    return trip.audienceRecipients;
  }

  protected changeAudienceMode(mode: AudienceType): void {
    const trip = this.selectedTrip();
    if (!trip?.canEdit || (!this.canBrowseTripGroup(trip) && mode !== 'SelectedPeople')) return;
    this.audienceMode.set(mode);
    if (mode === 'FamilyGroup') this.audienceRecipientIds.set([]);
    else if (this.audienceRecipientIds().length === 0)
      this.audienceRecipientIds.set(trip.audienceRecipients.map(person => person.profileId));
    this.audiencePreview.set(null);
    this.audienceError.set('');
  }

  protected toggleAudienceRecipient(profileId: string, selected: boolean): void {
    this.audienceRecipientIds.set(toggleId(this.audienceRecipientIds(), profileId, selected));
    this.audiencePreview.set(null);
    this.audienceError.set('');
  }

  protected previewAudience(): void {
    const trip = this.selectedTrip();
    if (!trip?.canEdit) return;
    if (this.audienceMode() === 'SelectedPeople'
      && !this.audienceRecipientIds().some(id => id !== this.ownProfileId)) {
      this.audienceError.set('Select at least one eligible person other than you.');
      return;
    }
    this.requestAudiencePreview(trip, false);
  }

  protected confirmAudience(): void {
    const trip = this.selectedTrip();
    const preview = this.audiencePreview();
    if (!trip?.canEdit || !preview || this.audienceBusy()) return;
    this.audienceBusy.set(true);
    this.audienceError.set('');
    this.http.post<Trip>(`/api/trips/${encodeURIComponent(trip.tripId)}/audience/confirm`, {
      audienceType: this.audienceMode(),
      audienceProfileIds: this.audienceMode() === 'SelectedPeople' ? this.audienceRecipientIds() : [],
      previewToken: preview.previewToken,
    }).pipe(finalize(() => this.audienceBusy.set(false))).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => {
        this.selectedTrip.set(updated);
        this.changingAudience.set(false);
        this.audiencePreview.set(null);
        this.loadTrips();
        this.loadNotes(updated.tripId);
      },
      error: (response: HttpErrorResponse) => {
        if (response.status === 409) {
          this.audienceError.set('The audience changed after this preview. Review the refreshed changes and confirm again.');
          this.requestAudiencePreview(trip, true);
          return;
        }
        this.audiencePreview.set(null);
        const validation = response.error?.errors as Record<string, string[]> | undefined;
        this.audienceError.set(validation ? Object.values(validation).flat().join(' ')
          : 'The audience could not be changed. Refresh the Trip and try again.');
      },
    });
  }

  private requestAudiencePreview(trip: Trip, staleRetry: boolean): void {
    this.audienceBusy.set(true);
    this.audienceError.set(staleRetry ? this.audienceError() : '');
    this.http.post<TripAudiencePreview>(`/api/trips/${encodeURIComponent(trip.tripId)}/audience/preview`, {
      audienceType: this.audienceMode(),
      audienceProfileIds: this.audienceMode() === 'SelectedPeople' ? this.audienceRecipientIds() : [],
    }).pipe(finalize(() => this.audienceBusy.set(false))).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: preview => this.audiencePreview.set(preview),
      error: (response: HttpErrorResponse) => {
        const validation = response.error?.errors as Record<string, string[]> | undefined;
        this.audiencePreview.set(null);
        this.audienceError.set(validation ? Object.values(validation).flat().join(' ')
          : 'An audience preview could not be created. Refresh the Trip and try again.');
      },
    });
  }

  private loadNotes(tripId: string): void {
    if (this.notesTripId !== tripId) {
      this.notes.set([]);
      this.notesTripId = tripId;
    }
    this.notesReadState.set('loading');
    this.http.get<TripNote[]>(`/api/trips/${encodeURIComponent(tripId)}/notes`).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: notes => {
        if (this.selectedTrip()?.tripId !== tripId) return;
        this.notes.set(notes);
        this.notesReadState.set('loaded');
      },
      error: () => {
        if (this.selectedTrip()?.tripId === tripId) {
          this.notesReadState.set('error');
        }
      },
    });
  }

  protected retryNotes(): void {
    const trip = this.selectedTrip();
    if (trip && this.notesReadState() === 'error') this.loadNotes(trip.tripId);
  }

  protected addNote(): void {
    const trip = this.selectedTrip();
    const content = this.noteDraft.value?.trim();
    if (!trip || !content || content.length > 1000) {
      this.noteError.set('Enter 1 to 1,000 characters.'); return;
    }
    this.sendNote(this.http.post<TripNote>(`/api/trips/${encodeURIComponent(trip.tripId)}/notes`, { content }),
      () => this.noteDraft.reset(''));
  }

  protected beginNoteEdit(note: TripNote): void {
    if (!note.canEdit) return;
    this.editingNoteId.set(note.noteId);
    this.noteEditDraft.setValue(note.content);
    this.noteError.set('');
  }

  protected cancelNoteEdit(): void { this.editingNoteId.set(null); }

  protected saveNoteEdit(note: TripNote): void {
    const trip = this.selectedTrip();
    const content = this.noteEditDraft.value?.trim();
    if (!trip || !note.canEdit || !content || content.length > 1000) {
      this.noteError.set('Enter 1 to 1,000 characters.'); return;
    }
    this.sendNote(this.http.put<TripNote>(
      `/api/trips/${encodeURIComponent(trip.tripId)}/notes/${encodeURIComponent(note.noteId)}`, { content }),
      () => this.editingNoteId.set(null));
  }

  protected deleteNote(note: TripNote): void { this.removeNote(note, 'delete'); }
  protected hideNote(note: TripNote): void { this.removeNote(note, 'hide'); }

  private removeNote(note: TripNote, action: 'delete' | 'hide'): void {
    const trip = this.selectedTrip();
    if (!trip || (action === 'delete' && !note.canDelete) || (action === 'hide' && !note.canHide)) return;
    const url = `/api/trips/${encodeURIComponent(trip.tripId)}/notes/${encodeURIComponent(note.noteId)}`;
    const request = action === 'delete' ? this.http.delete<void>(url) : this.http.post<void>(`${url}/hide`, {});
    this.sendNote(request);
  }

  private sendNote(request: ReturnType<HttpClient['post']>, afterSuccess?: () => void): void {
    const tripId = this.selectedTrip()?.tripId;
    this.noteSaving.set(true);
    this.noteError.set('');
    request.pipe(finalize(() => this.noteSaving.set(false))).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { afterSuccess?.(); if (tripId) this.loadNotes(tripId); },
      error: (response: HttpErrorResponse) => {
        const validation = response.error?.errors as Record<string, string[]> | undefined;
        this.noteError.set(validation ? Object.values(validation).flat().join(' ') : 'This Note could not be changed. Refresh the Trip and try again.');
      },
    });
  }

  protected changeAudience(): void {
    if (this.tripForm.controls.audienceType.value !== 'SelectedPeople') this.recipientIds.set([]);
  }

  protected toggleTraveler(profileId: string, selected: boolean): void {
    this.travelerIds.set(toggleId(this.travelerIds(), profileId, selected));
  }

  protected toggleRecipient(profileId: string, selected: boolean): void {
    this.recipientIds.set(toggleId(this.recipientIds(), profileId, selected));
  }

  protected travelerChoices(): readonly Person[] {
    const known = this.availableMembers();
    const current = this.selectedTrip()?.travelers ?? [];
    return [...known, ...current.filter(person => !known.some(member => member.profileId === person.profileId))];
  }

  protected saveTrip(): void {
    this.tripForm.markAllAsTouched();
    this.error.set('');
    const fields = this.tripForm.getRawValue();
    const editingId = this.editingTripId();
    const arrivalParts = [fields.expectedArrivalDate, fields.expectedArrivalTime, fields.expectedArrivalTimeZone];
    const hasArrival = !!fields.expectedArrivalDate || !!fields.expectedArrivalTime;
    if (this.tripForm.invalid || !fields.familyGroupId || !fields.title?.trim()
      || !fields.primaryDestination?.trim() || !fields.startDate || !fields.endDate
      || fields.endDate < fields.startDate || this.travelerIds().length === 0
      || (hasArrival && arrivalParts.some(part => !part?.trim()))
      || (!editingId && fields.audienceType === 'SelectedPeople' && this.recipientIds().length === 0)) {
      this.error.set('Complete the required fields, dates, travelers, and audience. Expected arrival needs date, time, and time zone together.');
      return;
    }

    const core = {
      title: fields.title.trim(),
      primaryDestination: fields.primaryDestination.trim(),
      startDate: fields.startDate,
      endDate: fields.endDate,
      description: emptyToNull(fields.description),
      transportationNotes: emptyToNull(fields.transportationNotes),
      accommodationNotes: emptyToNull(fields.accommodationNotes),
      expectedArrivalDate: hasArrival ? fields.expectedArrivalDate : null,
      expectedArrivalTime: hasArrival ? fields.expectedArrivalTime : null,
      expectedArrivalTimeZone: hasArrival ? fields.expectedArrivalTimeZone?.trim() : null,
      travelerProfileIds: this.travelerIds(),
      contacts: fields.contacts.map(contact => ({
        name: contact.name?.trim(),
        relationshipRole: emptyToNull(contact.relationshipRole),
        phone: emptyToNull(contact.phone),
        email: emptyToNull(contact.email),
      })),
    };
    this.saving.set(true);
    const request = editingId
      ? this.http.put<Trip>(`/api/trips/${encodeURIComponent(editingId)}`, core)
      : this.http.post<Trip>(
          `/api/family-groups/${encodeURIComponent(fields.familyGroupId)}/trips`,
          { ...core, audienceType: fields.audienceType, audienceProfileIds: fields.audienceType === 'SelectedPeople' ? this.recipientIds() : [] },
        );
    request.pipe(finalize(() => this.saving.set(false))).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: trip => {
        this.showingForm.set(false);
        this.selectedTrip.set(trip);
        this.loadNotes(trip.tripId);
        this.loadTrips();
      },
      error: (response: HttpErrorResponse) => {
        const validation = response.error?.errors as Record<string, string[]> | undefined;
        this.error.set(validation ? Object.values(validation).flat().join(' ') : 'The trip could not be saved. Please try again.');
      },
    });
  }

  private resetForm(): void {
    this.tripForm.reset({
      familyGroupId: this.activeGroups[0]?.familyGroupId ?? '',
      title: '', primaryDestination: '', startDate: '', endDate: '',
      description: '', transportationNotes: '', accommodationNotes: '',
      expectedArrivalDate: '', expectedArrivalTime: '', expectedArrivalTimeZone: defaultTimeZone(),
      audienceType: '',
    });
    this.contactForms.clear();
    this.travelerIds.set([]);
    this.recipientIds.set([]);
    this.loadMembers();
  }

  private loadTrips(): void {
    this.loading.set(true);
    this.tripsLoadError.set('');
    this.http.get<Trip[]>('/api/trips')
      .pipe(finalize(() => this.loading.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: trips => this.trips.set(trips),
        error: () => {
          this.trips.set([]);
          this.tripsLoadError.set('Trips could not be loaded. Please try again.');
        },
      });
  }

  protected retryTrips(): void { this.loadTrips(); }

  private loadMembers(requestedGroupId?: string): void {
    const groupId = requestedGroupId ?? this.tripForm.controls.familyGroupId.value;
    if (!groupId || !this.activeGroups.some(group => group.familyGroupId === groupId)) {
      this.availableMembers.set([]);
      return;
    }
    const requestVersion = ++this.memberRequestVersion;
    this.http.get<Person[]>(`/api/family-groups/${encodeURIComponent(groupId)}/members`).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: members => {
        const stillRelevant = requestedGroupId
          ? this.changingAudience() && this.selectedTrip()?.familyGroupId === groupId
          : groupId === this.tripForm.controls.familyGroupId.value;
        if (requestVersion === this.memberRequestVersion && stillRelevant)
          this.availableMembers.set(members);
      },
      error: () => {
        const stillRelevant = requestedGroupId
          ? this.changingAudience() && this.selectedTrip()?.familyGroupId === groupId
          : groupId === this.tripForm.controls.familyGroupId.value;
        if (requestVersion === this.memberRequestVersion && stillRelevant) this.availableMembers.set([]);
      },
    });
  }
}

function toggleId(ids: readonly string[], id: string, selected: boolean): readonly string[] {
  const updated = new Set(ids);
  if (selected) updated.add(id); else updated.delete(id);
  return [...updated];
}

function emptyToNull(value: string | null | undefined): string | null {
  return value?.trim() || null;
}

function defaultTimeZone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';
}

function supportedTimeZones(): readonly string[] {
  const available = (Intl as typeof Intl & { supportedValuesOf?: (key: string) => string[] })
    .supportedValuesOf?.('timeZone') ?? [];
  return [...new Set(['UTC', defaultTimeZone(), ...available])].sort();
}
