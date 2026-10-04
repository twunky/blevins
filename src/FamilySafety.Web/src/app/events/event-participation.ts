import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, Input, OnChanges, inject, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, takeUntil } from 'rxjs';

export type EventRsvpStatus = 'Pending' | 'Going' | 'Maybe' | 'NotGoing';
export interface EventInvitation {
  invitationId: string; inviteeProfileId: string; createdAtUtc: string; revokedAtUtc: string | null;
  rsvpStatus: EventRsvpStatus; rsvpUpdatedAtUtc: string | null; version: string;
}
interface OrganizerInvitations {
  eligibleViewers: { profileId: string; displayName: string }[];
  invitations: { invitation: EventInvitation; displayName: string }[];
}
export interface PendingEventInvitation {
  invitationId: string; eventId: string; name: string; date: string; time: string;
}
export interface AuthorizedParticipationEvent { eventId: string; familyGroupId: string; organizerProfileId: string }

/** Participation only. App supplies the Event after its protected item read and
 * Family adoption. Every participation endpoint still independently authorizes. */
@Component({
  selector: 'app-event-participation',
  templateUrl: './event-participation.html',
  styleUrl: './event-participation.scss',
})
export class EventParticipation implements OnChanges {
  @Input({ required: true }) event!: AuthorizedParticipationEvent;
  @Input({ required: true }) ownProfileId: string | null = null;
  @Input({ required: true }) currentFamilyId: string | null = null;
  readonly participationChanged = output<void>();
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly contextChanged = new Subject<void>();
  private generation = 0;
  protected readonly status = signal<'loading' | 'ready' | 'error' | 'unavailable'>('loading');
  protected readonly organizer = signal<OrganizerInvitations | null>(null);
  protected readonly own = signal<EventInvitation | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly message = signal('');
  protected readonly needsRefresh = signal(false);
  protected readonly revokePending = signal<string | null>(null);
  protected readonly response = signal<EventRsvpStatus>('Pending');
  protected readonly states: readonly EventRsvpStatus[] = ['Pending', 'Going', 'Maybe', 'NotGoing'];
  protected label(status: EventRsvpStatus): string { return status === 'NotGoing' ? 'Not Going' : status; }
  protected isOrganizer(): boolean { return !!this.ownProfileId && this.event.organizerProfileId === this.ownProfileId; }

  ngOnChanges(): void { this.reload(); }

  protected reload(preserveMessage = false): void {
    ++this.generation;
    this.contextChanged.next();
    this.organizer.set(null);
    this.own.set(null);
    this.response.set('Pending');
    this.revokePending.set(null);
    this.busy.set(false);
    this.error.set('');
    this.needsRefresh.set(false);
    if (!preserveMessage) this.message.set('');
    if (!this.ownProfileId || this.currentFamilyId !== this.event.familyGroupId) {
      this.status.set('unavailable');
      return;
    }
    this.status.set('loading');
    const generation = this.generation;
    const read: Observable<OrganizerInvitations | { invitation: EventInvitation | null }> = this.isOrganizer()
      ? this.http.get<OrganizerInvitations>(`${this.url()}/invitations`)
      : this.http.get<{ invitation: EventInvitation | null }>(`${this.url()}/invitation/mine`);
    read.pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: data => {
        if (generation !== this.generation) return;
        if ('eligibleViewers' in data) this.organizer.set(data);
        else {
          this.own.set(data.invitation);
          this.response.set(data.invitation?.rsvpStatus ?? 'Pending');
        }
        this.status.set('ready');
      },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.status.set(error.status === 404 ? 'unavailable' : 'error');
        this.error.set('Invitations could not be loaded. Please try again.');
      },
    });
  }

  protected canInvite(profileId: string): boolean {
    return profileId !== this.ownProfileId && !this.organizer()?.invitations.some(row =>
      row.invitation.inviteeProfileId === profileId && !row.invitation.revokedAtUtc);
  }

  protected invite(profileId: string): void {
    if (!this.isOrganizer() || !this.canInvite(profileId)) return;
    this.mutate(this.http.post<EventInvitation>(`${this.url()}/invitations`, { inviteeProfileId: profileId }), 'Invitation issued.', true);
  }

  protected confirmRevoke(invitation: EventInvitation): void {
    if (!this.isOrganizer() || this.revokePending() !== invitation.invitationId) return;
    this.mutate(this.http.post<EventInvitation>(`${this.url()}/invitations/${encodeURIComponent(invitation.invitationId)}/revoke`,
      { version: invitation.version }), 'Invitation revoked. Event visibility is unchanged.', true);
  }

  protected submitResponse(): void {
    const invitation = this.own();
    if (!invitation || invitation.revokedAtUtc || this.isOrganizer()) return;
    this.mutate(this.http.put<EventInvitation>(`${this.url()}/invitation/mine/rsvp`, {
      invitationId: invitation.invitationId, version: invitation.version, rsvpStatus: this.response(),
    }), 'Your RSVP was saved.', false);
  }

  private mutate(write: Observable<EventInvitation>, success: string, reloadOrganizer: boolean): void {
    if (this.busy() || this.needsRefresh() || this.status() !== 'ready') return;
    this.busy.set(true);
    this.error.set('');
    this.message.set('');
    const generation = this.generation;
    write.pipe(takeUntil(this.contextChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: invitation => {
        if (generation !== this.generation) return;
        this.busy.set(false);
        this.message.set(success);
        if (reloadOrganizer) this.reload(true);
        else { this.own.set(invitation); this.response.set(invitation.rsvpStatus); }
        this.participationChanged.emit();
      },
      error: (error: HttpErrorResponse) => {
        if (generation !== this.generation) return;
        this.busy.set(false);
        this.needsRefresh.set(true);
        this.revokePending.set(null);
        if (error.status === 404) {
          this.organizer.set(null); this.own.set(null); this.status.set('unavailable');
        }
        this.error.set(error.status === 409
          ? 'This invitation changed. Refresh invitations before trying again.'
          : error.status === 404 ? 'Invitation actions are currently unavailable. Refresh to check current access.'
          : 'Your change could not be saved. Refresh invitations before trying again.');
      },
    });
  }

  private url(): string { return `/api/events/${encodeURIComponent(this.event.eventId)}`; }
}
