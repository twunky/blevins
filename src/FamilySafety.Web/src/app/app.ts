import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, ElementRef, HostListener, Injector, OnInit, afterNextRender, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Observable, Subject, finalize, forkJoin, switchMap, takeUntil, throwError } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Trips } from './trips/trips';
import { CurrentFamilyContext } from './family-context';
import { Concerns } from './concerns/concerns';
import { Incidents } from './incidents/incidents';
import { AppIcon, IconName } from './icon';
import { dateTile, displayDateTime, displayRange } from './date-format';
import { GroupLanding } from './plans-trips/group-landing';
import { SafetyDashboard } from './safety/safety-dashboard';
import { ItemUnavailable } from './item-unavailable';
import { EventParticipation, PendingEventInvitation } from './events/event-participation';
import { EventNeeds } from './events/event-needs';
import { EventUpdates } from './events/event-updates';
import { SharedFamilyDates } from './family-dates/shared-family-dates';
import { CheckIns } from './check-ins/check-ins';
import { SharingSessions } from './sharing-sessions/sharing-sessions';
import { SafetyCircles } from './safety-circles/safety-circles';
import { Households } from './households/households';
import { TrustedContacts } from './trusted-contacts/trusted-contacts';
import { AdoptItemFamily, ItemKind, isItemId } from './item-route';
import {
  NavigableView, PrimaryDestination, areaForView, focusHeadingState, matchedAreas, matchedPath, pathForView, primaryDestinations, viewForPath,
} from './navigation';

type ProfileFieldVisibility = 'OnlyMe' | 'ActiveFamilyGroup';

interface AdultProfile {
  profileId: string;
  displayName: string;
  contactEmail: string | null;
  contactEmailVisibility: ProfileFieldVisibility;
  contactPhone: string | null;
  contactPhoneVisibility: ProfileFieldVisibility;
  birthdayMonth: number | null;
  birthdayDay: number | null;
  birthdayVisibility: ProfileFieldVisibility;
}

interface AdministeredFamilyGroup {
  familyGroupId: string;
  name: string;
}

interface InvitationPreview {
  familyGroupName: string;
  inviterDisplayName: string;
}

interface ActiveFamilyGroup {
  familyGroupId: string;
  name: string;
}

interface FamilyDirectoryEntry {
  profileId: string;
  displayName: string;
}

type FamilyRelationshipLabel =
  'SpousePartner' | 'Parent' | 'AdultChild' | 'Sibling' | 'OtherFamily';

interface FamilyRelationshipView {
  relationshipId: string;
  proposerProfileId: string;
  proposerDisplayName: string;
  proposerLabel: FamilyRelationshipLabel;
  recipientProfileId: string;
  recipientDisplayName: string;
  recipientLabel: FamilyRelationshipLabel;
  status: 'Pending' | 'Accepted';
  isCurrentUserProposer: boolean;
  isCurrentUserRecipient: boolean;
}

interface SharedAdultProfile {
  profileId: string;
  displayName: string;
  contactEmail: string | null;
  contactPhone: string | null;
  birthdayMonth: number | null;
  birthdayDay: number | null;
}

interface FamilyEventView {
  eventId: string;
  familyGroupId: string;
  name: string;
  description: string;
  date: string;
  time: string;
  location: string;
  organizerProfileId: string;
  organizerDisplayName: string;
  canEdit: boolean;
}

type PlanAudienceType = 'FamilyGroup' | 'SelectedPeople';

interface FamilyPlanView {
  planId: string;
  familyGroupId: string;
  activityOrDestination: string;
  date: string;
  time: string;
  expectedReturnDate: string | null;
  expectedReturnTime: string | null;
  expectedCheckInDate: string | null;
  expectedCheckInTime: string | null;
  audienceType: PlanAudienceType;
  creatorProfileId: string;
  creatorDisplayName: string;
  notes: string | null;
  version: string;
  canCorrectActivityTiming: boolean;
}

interface HomeTripPreview {
  isCompleted: boolean;
  tripId: string;
  familyGroupId: string;
  title: string;
  primaryDestination: string;
  startDate: string;
  endDate: string;
}

@Component({
  selector: 'app-workspace',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, Trips, Concerns, Incidents, AppIcon, GroupLanding, SafetyDashboard, ItemUnavailable, EventParticipation, EventNeeds, EventUpdates, SharedFamilyDates, CheckIns, SharingSessions, TrustedContacts, SafetyCircles, Households],
  styleUrls: ['./app.scss', './home-shell.scss', './home.scss', './family.scss', './family-settings.scss', './events.scss', './events-dashboard.scss', './plans.scss'],
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router, { optional: true });
  private readonly activatedRoute = inject(ActivatedRoute, { optional: true });
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly injector = inject(Injector);
  /** The navigation that created this shell. The router creates the shell while activating a route, before that navigation ends. */
  private readonly creatingNavigation = this.router?.currentNavigation() ?? null;
  private readonly familyContext = inject(CurrentFamilyContext);
  protected readonly currentFamilyId = this.familyContext.familyId;
  protected readonly view = signal<NavigableView | 'invitation' | 'handoff'>('home');
  protected readonly primaryDestinations = primaryDestinations;
  protected readonly focusHeadingState = focusHeadingState;
  /** Events dashboard Quick Links (WI-009-008 D8): real destinations only. Navigation only; each page still decides access. */
  protected readonly eventQuickLinks: readonly { id: string; label: string; detail: string; icon: IconName; path: string }[] = [
    { id: 'create', label: 'Create an event', detail: 'Share a gathering with family', icon: 'plus', path: '/events/new' },
    { id: 'all', label: 'All events', detail: 'Upcoming and past events', icon: 'calendar', path: '/events/all' },
    { id: 'trips', label: 'Trips', detail: 'Travel details you can see', icon: 'plane', path: '/trips' },
    { id: 'plans', label: 'Plans', detail: 'Lightweight shared plans', icon: 'clipboard', path: '/plans' },
    { id: 'family', label: 'Family members', detail: 'See who is in your family', icon: 'users', path: '/family' },
    { id: 'profile', label: 'Your profile', detail: 'Profile and sharing choices', icon: 'user', path: '/me' },
  ];
  /** The primary area shown as selected. Derived only from the routed page; never from data or permissions. */
  protected readonly currentArea = computed(() => {
    const view = this.view();
    return view === 'invitation' || view === 'handoff' ? undefined : areaForView[view];
  });
  /** The item ID in an item URL (REQ-UX-014), or `new` on /incidents/new. Only ever sent to that item's own read. */
  protected readonly routeItemId = signal<string | null>(null);
  /** An item page renders its host whether or not a Family is selected; the item read decides, never the selection. */
  protected readonly isItemView = computed(() => {
    const view = this.view();
    return view === 'eventItem' || view === 'planItem' || view === 'tripItem' || view === 'concernItem'
      || (view === 'incidents' && !!this.routeItemId() && this.routeItemId() !== 'new');
  });
  /** The shared "Family context required" prompt. Item pages show it only for a membership error (REQ-UX-014). */
  protected readonly familyContextPromptShown = computed(() => {
    const view = this.view();
    if (this.isItemView()) return !!this.membershipError();
    return view !== 'home' && view !== 'profile' && view !== 'invitation' && view !== 'handoff'
      && view !== 'plansTrips' && view !== 'safety' && !this.selectedFamily()
      && !(view === 'trips' && this.familyGroupsLoaded() && !this.membershipError() && this.activeFamilyGroups().length === 0);
  });
  /** WI-009-010 O11: the shared prompt is showing for a person with no Family, so the page's own no-Family copy is
   * redundant. Events, Plans, and the Family directory leave it out; the page has one Family-required message. */
  protected readonly sharedNoFamilyPromptShown = computed(() =>
    this.familyContextPromptShown() && this.familyGroupsLoaded() && this.activeFamilyGroups().length === 0);
  protected readonly itemEvent = signal<FamilyEventView | null>(null);
  protected readonly itemPlan = signal<FamilyPlanView | null>(null);
  protected readonly itemStatus = signal<'loading' | 'ready' | 'unavailable' | 'error'>('loading');
  protected readonly itemError = signal('');
  /** OD-H: set only when an item link actually switches the selected Family. The name is from the caller's own list. */
  protected readonly familySwitchAnnouncement = signal('');
  private membershipWaiters: (() => void)[] = [];
  protected readonly profileReview = signal(false);
  protected readonly handoffBusy = signal(false);
  protected readonly handoffError = signal('');
  private acceptedFamilyId: string | null = null;
  /** WI-009-010 O2: consumed by the first membership load of the Home shell created by the invitation arrival. */
  private pendingJoinedFamilyId: string | null = this.arrivedFromInvitation();
  private membershipLoadVersion = 0;
  private readonly familyScopeChanged = new Subject<void>();
  protected selectedFamily(): ActiveFamilyGroup | undefined {
    return this.activeFamilyGroups().find(group => group.familyGroupId === this.currentFamilyId());
  }

  protected canAdministerCurrentFamily(): boolean {
    const familyId = this.currentFamilyId();
    return !!familyId && this.administeredGroups().some(group => group.familyGroupId === familyId);
  }

  protected hasRelationshipSuggestions(): boolean {
    return this.familyRelationships().some(item => item.status === 'Pending' && item.isCurrentUserRecipient);
  }

  protected homeAttentionSummary(): string {
    if (this.relationshipLoading() || this.pendingEventInvitationsLoading()) return 'Checking attention…';
    if (this.relationshipError() || this.pendingEventInvitationsError()) return 'Some attention could not be checked';
    const count = this.pendingEventInvitations().length + (this.hasRelationshipSuggestions() ? 1 : 0);
    return count === 0 ? 'No items in these checks' : `${count} ${count === 1 ? 'item' : 'items'}`;
  }

  /** Family dashboard and Settings groupings of the relationships the server already returns; no new request or rule. */
  protected readonly incomingRelationshipSuggestions = computed(() =>
    this.familyRelationships().filter(item => item.status === 'Pending' && item.isCurrentUserRecipient));
  protected readonly outgoingRelationshipSuggestions = computed(() =>
    this.familyRelationships().filter(item => item.status === 'Pending' && !item.isCurrentUserRecipient));
  protected readonly acceptedRelationships = computed(() =>
    this.familyRelationships().filter(item => item.status === 'Accepted'));

  /** Accepted relationships that include this member. Accepted relationships are visible to every active adult. */
  protected acceptedRelationshipsFor(profileId: string): readonly FamilyRelationshipView[] {
    return this.acceptedRelationships().filter(item => item.proposerProfileId === profileId || item.recipientProfileId === profileId);
  }

  /** D5: a directory tile's second line, only the member's side of an accepted relationship with the signed-in adult; otherwise none. */
  protected memberTileNote(profileId: string): string {
    const ownId = this.ownerProfile()?.profileId;
    if (!ownId || profileId === ownId) return '';
    const shared = this.acceptedRelationshipsFor(profileId).find(item => item.isCurrentUserProposer || item.isCurrentUserRecipient);
    if (!shared) return '';
    return this.relationshipLabelText(shared.proposerProfileId === profileId ? shared.proposerLabel : shared.recipientLabel);
  }
  protected homeEvents(): readonly FamilyEventView[] {
    return this.upcomingEvents(3);
  }

  /** Events dashboard summary (WI-009-008): the same upcoming selection as Home, a few more rows. /events/all keeps every Event. */
  protected dashboardEvents(): readonly FamilyEventView[] {
    return this.upcomingEvents(5);
  }

  private upcomingEvents(limit: number): readonly FamilyEventView[] {
    const today = localDateString(new Date());
    return this.familyEvents()
      .filter(event => event.familyGroupId === this.currentFamilyId() && event.date.slice(0, 10) >= today)
      .slice()
      .sort((left, right) => compareDateTime(left.date, left.time, right.date, right.time)
        || left.eventId.localeCompare(right.eventId))
      .slice(0, limit);
  }

  protected homePlans(): readonly FamilyPlanView[] {
    const today = localDateString(new Date());
    return this.familyPlans()
      // Calendar-date summary freshness, not inferred Plan completion (owner D2).
      .filter(plan => plan.familyGroupId === this.currentFamilyId()
        && (plan.expectedReturnDate ?? plan.date).slice(0, 10) >= today)
      .slice()
      .sort((left, right) => compareDateTime(left.date, left.time, right.date, right.time)
        || left.planId.localeCompare(right.planId))
      .slice(0, 3);
  }

  protected homeTrips(): readonly HomeTripPreview[] {
    const today = localDateString(new Date());
    return this.homeTripResults()
      // Summary freshness only: passing the end date never completes a Trip (REQ-TRIP-011).
      .filter(trip => trip.familyGroupId === this.currentFamilyId() && !trip.isCompleted && trip.endDate.slice(0, 10) >= today)
      .slice()
      .sort((left, right) => left.startDate.localeCompare(right.startDate)
        || left.tripId.localeCompare(right.tripId))
      .slice(0, 3);
  }

  protected clearDirectoryFilter(): void {
    this.directoryForm.controls.search.setValue('');
    this.searchDirectory();
  }
  protected readonly today = new Date();

  /** Home greeting from the local time and the already-loaded own profile name; no extra request. */
  protected readonly homeGreeting = computed(() => {
    const hour = this.today.getHours();
    const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening';
    const name = this.ownerProfile()?.displayName.trim();
    return name ? `${greeting}, ${name}!` : `${greeting}!`;
  });

  protected readonly pageTitle = computed(() => ({
    home: this.selectedFamily() ? this.homeGreeting() : 'Home',
    profile: this.profileReview() ? 'Review your profile' : 'Your profile',
    family: 'Family', familySettings: 'Family settings', events: 'Events', eventCreate: 'Create event', eventsAll: 'All events',
    eventItem: 'Events', plans: 'Plans', planItem: 'Plans',
    trips: 'Trips', tripItem: 'Trips', plansTrips: 'Plans & Trips', safety: 'Safety', concerns: 'Concerns', concernItem: 'Concerns',
    incidents: 'Safety Incidents', invitation: 'Family invitation', handoff: 'Joining your family',
  })[this.view()]);

  /** One-line description shown under the page title; each page has a single visual title. */
  protected readonly pageSubtitle = computed(() => ({
    home: this.selectedFamily() ? `Here's what's happening with ${this.selectedFamily()!.name} today.` : '',
    profile: 'Your profile and your choices for who can see each shared profile field.',
    family: this.selectedFamily() ? `Members, relationships, and coordination for ${this.selectedFamily()!.name}.` : '',
    familySettings: this.selectedFamily() ? `${this.selectedFamily()!.name}. Your membership, relationships, and the Family actions available to you.` : '',
    events: this.selectedFamily() ? `Upcoming events and trips shared with you in ${this.selectedFamily()!.name}.` : '',
    eventCreate: 'Create an event and choose the active adult family members who can see it. Event sharing does not send an invitation or start location sharing.',
    eventsAll: this.selectedFamily() ? `Every Event shared with you in ${this.selectedFamily()!.name}, including past Events.` : '',
    eventItem: '', planItem: '',
    plans: 'Share a lightweight plan for a future activity or destination with the people you choose.',
    trips: 'Plan and share travel details with your chosen family audience.',
    tripItem: 'Plan and share travel details with your chosen family audience.',
    plansTrips: 'Choose Plans or Trips.',
    safety: 'Concerns and Safety Incidents shared with you.',
    concerns: 'Keep uncertainty and shared information clear while coordinating with selected people.',
    concernItem: 'Keep uncertainty and shared information clear while coordinating with selected people.',
    incidents: 'Coordinate a deliberate family response with the people you choose.',
    invitation: '', handoff: '',
  })[this.view()]);

  protected readonly dateTile = dateTile;
  protected readonly displayDateTime = displayDateTime;
  protected readonly displayRange = displayRange;

  protected initials(name: string | null | undefined): string {
    return (name ?? '').split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0].toUpperCase()).join('');
  }

  /** D3: "page" on the area's own page, "true" on one of its child pages, otherwise no attribute. */
  protected ariaCurrent(destination: PrimaryDestination): 'page' | 'true' | null {
    if (this.currentArea() !== destination.area) return null;
    return this.view() === destination.view ? 'page' : 'true';
  }

  protected navigate(view: NavigableView): void {
    this.view.set(view);
    const path = pathForView[view];
    // The Concern and Incident routes each share one matcher-based definition (app.routes.ts); they have no path.
    if (this.router?.config.some(route => route.path === path || (matchedAreas.includes(path) && !!route.matcher)))
      void this.router.navigateByUrl(`/${path}`);
  }

  protected switchCurrentFamily(familyId: string): void {
    if (!this.activeFamilyGroups().some(group => group.familyGroupId === familyId)
      || familyId === this.currentFamilyId()) return;
    // WI-009-010 O6: choosing a Family, whether the first choice or a switch, keeps the current page; the page reloads
    // its data for the newly selected Family.
    this.familySwitchAnnouncement.set('');
    this.clearFamilyScopedState();
    this.familyContext.familyId.set(familyId);
    this.loadSelectedFamily(familyId);
  }

  /**
   * WI-009-010 O1–O3: both profile-review actions finish the invitation arrival on Home. The accepted Family is
   * already selected from the server-confirmed membership list; Home announces it once (O2).
   */
  protected continueToHome(): void {
    // Skipping discards unsaved edits. Membership was already established.
    const profile = this.ownerProfile();
    if (profile) this.applyProfile(profile);
    this.profileReview.set(false);
    const joinedFamilyId = this.acceptedFamilyId;
    if (this.router?.config.some(route => route.path === 'home')) {
      void this.router.navigateByUrl('/home', { state: { ...focusHeadingState, joinedFamilyId } });
      return;
    }
    // Without a router (component tests), stay in this shell.
    this.view.set('home');
    this.announceJoinedFamily(joinedFamilyId);
  }

  /** O2: "You joined {Family}." only when that Family is the selected one in the caller's own membership list. */
  private announceJoinedFamily(familyGroupId: string | null): void {
    const family = familyGroupId ? this.selectedFamily() : undefined;
    if (family && family.familyGroupId === familyGroupId) this.familySwitchAnnouncement.set(`You joined ${family.name}.`);
  }

  @HostListener('window:hashchange')
  protected openInvitationLink(): void {
    const token = readInvitationToken();
    if (!location.hash.startsWith('#invite=')) return;
    this.invitationPreview.set(null);
    this.invitationError.set('');
    this.invitationMessage.set('');
    this.invitationToken.set(token);
    this.view.set('invitation');
    if (token) this.previewInvitation(token);
    else this.invitationError.set('This invitation is unavailable.');
  }

  private readonly formBuilder = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private relationshipRequestVersion = 0;
  private directoryRequestVersion = 0;
  private eventAudienceRequestVersion = 0;

  protected readonly visibilityOptions: ReadonlyArray<{
    value: ProfileFieldVisibility;
    label: string;
  }> = [
    { value: 'OnlyMe', label: 'Only me' },
    { value: 'ActiveFamilyGroup', label: 'Active family group' },
  ];

  protected readonly profileForm = this.formBuilder.group(
    {
      displayName: ['', [Validators.required, Validators.maxLength(100)]],
      contactEmail: ['', [Validators.email, Validators.maxLength(254)]],
      contactEmailVisibility:
        this.formBuilder.nonNullable.control<ProfileFieldVisibility>('OnlyMe'),
      contactPhone: ['', Validators.maxLength(32)],
      contactPhoneVisibility:
        this.formBuilder.nonNullable.control<ProfileFieldVisibility>('OnlyMe'),
      birthdayMonth: this.formBuilder.control<number | null>(null),
      birthdayDay: this.formBuilder.control<number | null>(null),
      birthdayVisibility: this.formBuilder.nonNullable.control<ProfileFieldVisibility>('OnlyMe'),
    },
    { validators: birthdayValidator },
  );

  protected readonly invitationForm = this.formBuilder.group({
    familyGroupId: ['', Validators.required],
    recipientEmail: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
  });

  protected readonly directoryForm = this.formBuilder.group({
    familyGroupId: [''],
    search: [''],
  });

  protected readonly relationshipForm = this.formBuilder.group({
    recipientProfileId: ['', Validators.required],
    proposerLabel: this.formBuilder.nonNullable.control<FamilyRelationshipLabel>(
      'OtherFamily',
      Validators.required,
    ),
  });

  protected readonly eventForm = this.formBuilder.group({
    familyGroupId: ['', Validators.required],
    name: ['', Validators.required],
    description: ['', Validators.required],
    date: ['', Validators.required],
    time: ['', Validators.required],
    location: ['', Validators.required],
  });

  protected readonly planForm = this.formBuilder.group({
    familyGroupId: ['', Validators.required],
    activityOrDestination: ['', [Validators.required, Validators.maxLength(200)]],
    date: ['', Validators.required],
    time: ['', Validators.required],
    expectedReturnDate: [''],
    expectedReturnTime: [''],
    audienceType: this.formBuilder.nonNullable.control<PlanAudienceType | ''>(
      '',
      Validators.required,
    ),
    expectedCheckInDate: [''],
    expectedCheckInTime: [''],
    notes: ['', Validators.maxLength(1000)],
  });
  protected readonly planCorrectionForm = this.formBuilder.group({
    activityOrDestination: ['', [Validators.required, Validators.maxLength(200)]],
    date: ['', Validators.required], time: ['', Validators.required],
    expectedReturnDate: [''], expectedReturnTime: [''],
    expectedCheckInDate: [''], expectedCheckInTime: [''],
  });
  protected readonly editingPlanFactsId = signal<string | null>(null);
  protected readonly planCorrectionSaving = signal(false);
  protected readonly planCorrectionError = signal('');
  protected readonly planCorrectionMessage = signal('');
  protected readonly planCorrectionConflict = signal(false);
  private planCorrectionVersion: string | null = null;

  protected readonly planNoteForm = this.formBuilder.group({
    notes: ['', Validators.maxLength(1000)],
  });

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal('');
  /** The own-profile read failed (Retry applies), as opposed to a profile save failing. */
  protected readonly profileLoadFailed = signal(false);
  protected readonly profileReloading = signal(false);
  protected readonly successMessage = signal('');
  protected readonly administeredGroups = signal<readonly AdministeredFamilyGroup[]>([]);
  protected readonly invitationBusy = signal(false);
  protected readonly invitationMessage = signal('');
  protected readonly invitationError = signal('');
  protected readonly invitationToken = signal('');
  protected readonly invitationPreview = signal<InvitationPreview | null>(null);
  protected readonly ownerProfile = signal<AdultProfile | null>(null);
  protected readonly activeFamilyGroups = signal<readonly ActiveFamilyGroup[]>([]);
  protected readonly familyGroupsLoaded = signal(false);
  protected readonly membershipError = signal('');
  protected readonly directoryMembers = signal<readonly FamilyDirectoryEntry[]>([]);
  protected readonly directoryLoading = signal(false);
  protected readonly directoryError = signal('');
  /** The member list read itself failed (Retry applies), as opposed to a single member's profile being unavailable. */
  protected readonly directoryLoadFailed = signal(false);
  protected readonly selectedMember = signal<SharedAdultProfile | null>(null);
  protected readonly familyRelationships = signal<readonly FamilyRelationshipView[]>([]);
  protected readonly relationshipLoading = signal(false);
  protected readonly relationshipSubmitting = signal(false);
  protected readonly relationshipError = signal('');
  /** The relationship list read itself failed (Retry applies), as opposed to a suggestion or response action failing. */
  protected readonly relationshipsLoadFailed = signal(false);
  protected readonly relationshipMessage = signal('');
  protected readonly relationshipBusyId = signal('');
  protected readonly relationshipRemovalPendingId = signal('');
  protected readonly familyEvents = signal<readonly FamilyEventView[]>([]);
  protected readonly pendingEventInvitations = signal<readonly PendingEventInvitation[]>([]);
  protected readonly pendingEventInvitationsLoading = signal(false);
  protected readonly pendingEventInvitationsError = signal('');
  private pendingEventInvitationsVersion = 0;
  protected readonly eventAudienceMembers = signal<readonly FamilyDirectoryEntry[]>([]);
  protected readonly eventAudienceProfileIds = signal<readonly string[]>([]);
  protected readonly eventEditorProfileIds = signal<readonly string[]>([]);
  protected readonly eventAudienceSearch = signal('');
  protected readonly eventsLoading = signal(false);
  protected readonly eventSubmitting = signal(false);
  protected readonly eventError = signal('');
  protected readonly eventsLoadError = signal('');
  protected readonly eventMessage = signal('');
  protected readonly eventEditError = signal('');
  protected readonly editingEventId = signal<string | null>(null);
  protected readonly eventUpdating = signal(false);
  protected readonly familyPlans = signal<readonly FamilyPlanView[]>([]);
  protected readonly planAudienceMembers = signal<readonly FamilyDirectoryEntry[]>([]);
  protected readonly planAudienceProfileIds = signal<readonly string[]>([]);
  protected readonly planAudienceSearch = signal('');
  protected readonly plansLoading = signal(false);
  protected readonly planSubmitting = signal(false);
  protected readonly planError = signal('');
  protected readonly plansLoadError = signal('');
  protected readonly planMessage = signal('');
  protected readonly editingPlanNoteId = signal<string | null>(null);
  protected readonly planNoteClearPendingId = signal<string | null>(null);
  protected readonly planNoteSaving = signal(false);
  protected readonly planNoteError = signal('');
  protected readonly planNoteMessage = signal('');
  protected readonly homeTripResults = signal<readonly HomeTripPreview[]>([]);
  protected readonly homeTripsLoading = signal(false);
  protected readonly homeTripsError = signal('');
  private homeTripsRequestVersion = 0;
  private planAudienceRequestVersion = 0;
  private plansRequestVersion = 0;
  protected readonly eventEditForm = this.formBuilder.group({
    name: ['', Validators.required],
    description: ['', Validators.required],
    date: ['', Validators.required],
    time: ['', Validators.required],
    location: ['', Validators.required],
  });
  private eventsRequestVersion = 0;

  protected readonly relationshipLabelOptions: ReadonlyArray<{
    value: FamilyRelationshipLabel;
    label: string;
  }> = [
    { value: 'SpousePartner', label: 'Spouse or partner' },
    { value: 'Parent', label: 'Parent' },
    { value: 'AdultChild', label: 'Adult child' },
    { value: 'Sibling', label: 'Sibling' },
    { value: 'OtherFamily', label: 'Other family' },
  ];

  ngOnInit(): void {
    if (!location.hash.startsWith('#invite=')) {
      const routeConfig = this.activatedRoute?.snapshot.routeConfig;
      if (routeConfig?.matcher && this.activatedRoute) {
        // List and item share this shell (/concerns ↔ /concerns/:id, /incidents ↔ /incidents/:id), so follow URL changes.
        this.activatedRoute.url.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(url => {
          const segments = url.map(segment => segment.path);
          const view = viewForPath(matchedPath(segments) ?? '');
          if (!view) return;
          this.view.set(view);
          this.routeItemId.set(segments[1] ?? null);
          this.familySwitchAnnouncement.set('');
        });
      } else {
        const path = routeConfig?.path ?? location.pathname.replace(/^\//, '').split('/')[0];
        const view = viewForPath(path);
        if (view) {
          this.view.set(view);
          const params = this.activatedRoute?.snapshot.paramMap;
          this.routeItemId.set(params?.get('eventId') ?? params?.get('planId') ?? params?.get('tripId') ?? null);
        } else if (path === '' && this.router?.config.some(route => route.path === 'home')) {
          void this.router.navigateByUrl('/home', { replaceUrl: true });
        }
      }
    }
    this.loadProfile();
    this.http.get<AdministeredFamilyGroup[]>('/api/family-groups/administered').pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (groups) => {
        this.administeredGroups.set(groups);
        if (this.canAdministerCurrentFamily())
          this.invitationForm.controls.familyGroupId.setValue(this.currentFamilyId()!);
      },
      error: () => this.administeredGroups.set([]),
    });
    this.loadMemberships();
    this.openRoutedItem();
    this.openInvitationLink();
    this.focusHeadingAfterUserNavigation();
  }

  /**
   * REQ-UX-014 step 2 (DEC-074 D4; DEC-075 OD-A, OD-B, OD-F, OD-H). Call only with the FamilyGroupId of an item the
   * server has just returned (200). It waits for the caller's own Family list, refreshes it at most once, and only
   * selects among Families that list returned; it never treats the list, the URL, or the selection as authorization.
   * If the list cannot be loaded, the existing membership error and Retry show, and the decision waits for a
   * successful Retry: nothing renders and nothing switches meanwhile.
   */
  protected readonly adoptAuthorizedItemFamily: AdoptItemFamily = async (familyGroupId, kind) => {
    await this.familiesLoaded();
    if (!this.isActiveFamily(familyGroupId)) {
      this.loadMemberships(true);
      await this.familiesLoaded();
    }
    if (!this.isActiveFamily(familyGroupId)) return kind === 'trip' ? 'retained' : 'unavailable';
    if (familyGroupId === this.currentFamilyId()) return 'current';
    this.clearFamilyScopedState();
    this.familyContext.familyId.set(familyGroupId);
    this.loadSelectedFamily(familyGroupId);
    this.familySwitchAnnouncement.set(`Switched to ${this.selectedFamily()!.name}.`);
    return 'switched';
  };

  private isActiveFamily(familyGroupId: string): boolean {
    return this.activeFamilyGroups().some(group => group.familyGroupId === familyGroupId);
  }

  /** Resolves once the Family list has loaded without error; a failed load waits for the user's Retry. */
  private familiesLoaded(): Promise<void> {
    if (this.familyGroupsLoaded() && !this.membershipError()) return Promise.resolve();
    return new Promise(resolve => this.membershipWaiters.push(resolve));
  }

  /** Event and Plan item pages (OD-C): read the item by ID, let the server authorize it, decide the Family, then render. */
  private openRoutedItem(): void {
    const view = this.view();
    if (view !== 'eventItem' && view !== 'planItem') return;
    const kind: ItemKind = view === 'eventItem' ? 'event' : 'plan';
    const id = this.routeItemId();
    this.cancelPlanNoteEdit();
    this.planNoteMessage.set('');
    this.cancelPlanCorrection();
    this.planCorrectionMessage.set('');
    if (!isItemId(id)) {
      this.itemStatus.set('unavailable');
      return;
    }
    this.itemStatus.set('loading');
    const read: Observable<FamilyEventView | FamilyPlanView> = kind === 'event'
      ? this.http.get<FamilyEventView>(`/api/events/${encodeURIComponent(id)}`)
      : this.http.get<FamilyPlanView>(`/api/plans/${encodeURIComponent(id)}`);
    read.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: async (item: FamilyEventView | FamilyPlanView) => {
        const outcome = await this.adoptAuthorizedItemFamily(item.familyGroupId, kind);
        if (outcome === 'unavailable') {
          this.itemStatus.set('unavailable');
          return;
        }
        if (kind === 'event') {
          this.itemEvent.set(item as FamilyEventView);
          if (this.arrivedFromEventCreate()) this.eventMessage.set('Event created and shared with the selected family members.');
        }
        else this.itemPlan.set(item as FamilyPlanView);
        this.itemStatus.set('ready');
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 404) {
          this.itemStatus.set('unavailable');
          return;
        }
        this.itemError.set(error.status === 401 ? 'Your session may have ended. Please sign in again.'
          : `This ${kind === 'event' ? 'Event' : 'Plan'} could not be opened. Please try again.`);
        this.itemStatus.set('error');
      },
    });
  }

  /**
   * D4: after a user follows a main-navigation or landing link, move focus once to the new page's h1.
   * Those links carry focusHeadingState. History state is restored on refresh and on back/forward, so
   * the router's initial navigation and popstate navigations are excluded. Data loads, Family switches,
   * and Incident sub-screen changes do not re-create the shell, so they never reach this.
   */
  /**
   * WI-009-010 O2: the Family ID carried only by the navigation from the invitation profile review to Home, so the join
   * announcement shows once and never on refresh or back/forward. It names nothing by itself: the announcement waits
   * for the caller's own membership list and the selection made from it.
   */
  private arrivedFromInvitation(): string | null {
    const navigation = this.creatingNavigation;
    const joined = navigation?.extras.state?.['joinedFamilyId'];
    return navigation && navigation.trigger === 'imperative' && navigation.previousNavigation && typeof joined === 'string'
      ? joined : null;
  }

  /** D10: the create success message follows the new Event only on the navigation from /events/new, never on refresh or back/forward. */
  private arrivedFromEventCreate(): boolean {
    const navigation = this.creatingNavigation;
    return !!navigation && navigation.trigger === 'imperative' && !!navigation.previousNavigation
      && navigation.extras.state?.['eventCreated'] === true;
  }

  private focusHeadingAfterUserNavigation(): void {
    const navigation = this.creatingNavigation;
    if (!navigation || navigation.trigger !== 'imperative' || !navigation.previousNavigation
      || navigation.extras.state?.['focusHeading'] !== true) return;
    afterNextRender(() => {
      if (this.view() === 'invitation' || this.view() === 'handoff') return;
      this.host.nativeElement.querySelector<HTMLElement>('main h1')?.focus();
    }, { injector: this.injector });
  }

  protected retryMemberships(): void { this.loadMemberships(); }

  private loadProfile(): void {
    this.http.get<AdultProfile>('/api/profile')
      .pipe(finalize(() => this.profileReloading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (profile) => {
          this.ownerProfile.set(profile);
          this.applyProfile(profile);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Your profile could not be loaded. Please try again.');
          this.profileLoadFailed.set(true);
          this.loading.set(false);
        },
      });
  }

  /**
   * Repeats only the profile read. The page stays in place with its own loading status: re-entering the initial
   * `loading` state would re-create every embedded page and repeat their reads.
   */
  protected retryProfile(): void {
    this.errorMessage.set('');
    this.profileLoadFailed.set(false);
    this.profileReloading.set(true);
    this.loadProfile();
  }

  // Retry for a failed read repeats only that read, through its existing loader and the same server authorization.
  protected retryEvents(): void { this.loadEvents(); }
  protected retryPlans(): void { this.loadPlans(); }
  protected retryDirectory(): void { this.loadDirectory(); }
  protected retryRelationships(): void { this.loadRelationships(); }
  protected retryHomeTrips(): void {
    const familyId = this.currentFamilyId();
    if (familyId) this.loadHomeTrips(familyId);
  }
  /** Repeats the authorize-first item read (OD-C): the item is read by ID before any Family decision. */
  protected retryRoutedItem(): void { this.openRoutedItem(); }

  /** `refresh` (OD-B) keeps the current selection's loaded state when the refreshed list still selects it. */
  private loadMemberships(refresh = false): void {
    const membershipVersion = ++this.membershipLoadVersion;
    this.familyGroupsLoaded.set(false);
    this.membershipError.set('');
    this.http.get<ActiveFamilyGroup[]>('/api/family-groups/mine').pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (groups) => {
        if (membershipVersion !== this.membershipLoadVersion) return;
        this.activeFamilyGroups.set(groups);
        this.familyGroupsLoaded.set(true);
        const priorChoice = this.currentFamilyId();
        const familyId = groups.length === 1 ? groups[0].familyGroupId
          : groups.some(group => group.familyGroupId === priorChoice) ? priorChoice : null;
        if (!refresh || familyId !== priorChoice) {
          this.clearFamilyScopedState(this.view() === 'invitation' || this.view() === 'handoff');
          this.familyContext.familyId.set(familyId);
          if (familyId) this.loadSelectedFamily(familyId);
        }
        if (this.pendingJoinedFamilyId) {
          this.announceJoinedFamily(this.pendingJoinedFamilyId);
          this.pendingJoinedFamilyId = null;
        }
        const waiters = this.membershipWaiters;
        this.membershipWaiters = [];
        waiters.forEach(resolve => resolve());
      },
      error: () => {
        if (membershipVersion !== this.membershipLoadVersion) return;
        this.familyGroupsLoaded.set(true);
        this.membershipError.set('Your Family memberships could not be loaded. Please try again.');
      },
    });
  }

  private loadSelectedFamily(familyId: string): void {
    this.directoryForm.controls.familyGroupId.setValue(familyId);
    this.eventForm.controls.familyGroupId.setValue(familyId);
    this.planForm.controls.familyGroupId.setValue(familyId);
    if (this.canAdministerCurrentFamily())
      this.invitationForm.controls.familyGroupId.setValue(familyId);
    this.loadDirectory();
    this.loadRelationships();
    this.loadEvents();
    this.loadEventAudienceMembers();
    if (this.view() === 'home' || this.view() === 'events') this.loadPendingEventInvitations();
    this.loadPlans();
    this.loadPlanAudienceMembers();
    // Home and the Events dashboard Trips card (WI-009-008 D3) share one read and one current-Family filter.
    if (this.view() === 'home' || this.view() === 'events') this.loadHomeTrips(familyId);
  }

  private clearFamilyScopedState(preserveInvitation = false): void {
    this.familyScopeChanged.next();
    ++this.directoryRequestVersion;
    ++this.relationshipRequestVersion;
    ++this.eventsRequestVersion;
    ++this.pendingEventInvitationsVersion;
    this.pendingEventInvitations.set([]);
    this.pendingEventInvitationsLoading.set(false);
    this.pendingEventInvitationsError.set('');
    ++this.plansRequestVersion;
    ++this.eventAudienceRequestVersion;
    ++this.planAudienceRequestVersion;
    this.directoryMembers.set([]);
    this.directoryLoading.set(false);
    this.directoryError.set('');
    this.directoryLoadFailed.set(false);
    this.clearDirectorySelection();
    this.familyRelationships.set([]);
    this.relationshipLoading.set(false);
    this.relationshipError.set('');
    this.relationshipsLoadFailed.set(false);
    this.relationshipMessage.set('');
    this.relationshipRemovalPendingId.set('');
    this.relationshipBusyId.set('');
    this.relationshipSubmitting.set(false);
    this.relationshipForm.reset({ recipientProfileId: '', proposerLabel: 'OtherFamily' });
    this.familyEvents.set([]);
    this.eventsLoadError.set('');
    this.eventAudienceMembers.set([]);
    this.eventAudienceProfileIds.set([]);
    this.eventEditorProfileIds.set([]);
    this.eventAudienceSearch.set('');
    this.eventsLoading.set(false);
    this.eventError.set('');
    this.eventsLoadError.set('');
    this.eventMessage.set('');
    this.eventEditError.set('');
    this.editingEventId.set(null);
    this.eventEditForm.reset();
    this.eventSubmitting.set(false);
    this.eventUpdating.set(false);
    this.familyPlans.set([]);
    this.planAudienceMembers.set([]);
    this.planAudienceProfileIds.set([]);
    this.planAudienceSearch.set('');
    this.plansLoading.set(false);
    this.planSubmitting.set(false);
    this.planError.set('');
    this.plansLoadError.set('');
    this.planMessage.set('');
    this.cancelPlanNoteEdit();
    this.planNoteMessage.set('');
    this.cancelPlanCorrection();
    this.planCorrectionMessage.set('');
    this.homeTripResults.set([]);
    this.homeTripsLoading.set(false);
    this.homeTripsError.set('');
    ++this.homeTripsRequestVersion;
    this.directoryForm.reset({ familyGroupId: '', search: '' });
    this.eventForm.reset({ familyGroupId: '', name: '', description: '', date: '', time: '', location: '' });
    this.planForm.reset({ familyGroupId: '', activityOrDestination: '', date: '', time: '',
      expectedReturnDate: '', expectedReturnTime: '', audienceType: '',
      expectedCheckInDate: '', expectedCheckInTime: '', notes: '' });
    this.invitationForm.reset({ familyGroupId: '', recipientEmail: '' });
    if (!preserveInvitation) {
      this.invitationError.set('');
      this.invitationMessage.set('');
    }
  }

  protected sendInvitation(): void {
    this.submitAdministratorAction('/api/membership-invitations', 'Invitation sent.');
  }

  protected revokeInvitation(): void {
    this.submitAdministratorAction('/api/membership-invitations/revoke', 'Invitation revoked.');
  }

  protected acceptInvitation(): void {
    const token = this.invitationToken();
    if (!token || this.invitationBusy()) return;
    this.invitationBusy.set(true);
    this.invitationError.set('');
    this.http.post<{ familyGroupId: string }>('/api/membership-invitations/accept', { token })
      .pipe(finalize(() => this.invitationBusy.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (accepted) => {
          this.acceptedFamilyId = accepted.familyGroupId;
          this.clearInvitationToken();
          this.invitationMessage.set('Invitation accepted. Loading your family…');
          this.retryHandoff();
        },
        error: (error: HttpErrorResponse) => this.invitationError.set(error.status === 409
          ? 'This family group has reached its capacity.' : 'This invitation is unavailable.'),
      });
  }

  protected retryHandoff(): void {
    const familyGroupId = this.acceptedFamilyId;
    if (!familyGroupId || this.handoffBusy()) return;
    ++this.membershipLoadVersion; // Ignore an initial membership read still in flight.
    ++this.directoryRequestVersion;
    ++this.relationshipRequestVersion;
    ++this.eventsRequestVersion;
    ++this.plansRequestVersion;
    ++this.eventAudienceRequestVersion;
    ++this.planAudienceRequestVersion;
    this.view.set('handoff');
    this.handoffBusy.set(true);
    this.handoffError.set('');
    this.http.get<ActiveFamilyGroup[]>('/api/family-groups/mine').pipe(
      switchMap(groups => {
        if (!groups.some(group => group.familyGroupId === familyGroupId))
          return throwError(() => new Error('Accepted membership not available.'));
        this.clearFamilyScopedState(true);
        this.familyContext.familyId.set(familyGroupId);
        this.membershipError.set('');
        this.directoryForm.reset({ familyGroupId, search: '' });
        this.eventForm.controls.familyGroupId.setValue(familyGroupId);
        this.planForm.controls.familyGroupId.setValue(familyGroupId);
        this.activeFamilyGroups.set(groups);
        this.familyGroupsLoaded.set(true);
        if (this.canAdministerCurrentFamily())
          this.invitationForm.controls.familyGroupId.setValue(familyGroupId);
        return forkJoin({
          members: this.http.get<FamilyDirectoryEntry[]>(`/api/family-groups/${encodeURIComponent(familyGroupId)}/members`),
          relationships: this.http.get<FamilyRelationshipView[]>(`/api/family-groups/${encodeURIComponent(familyGroupId)}/relationships`),
        });
      }),
      finalize(() => this.handoffBusy.set(false)),
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ members, relationships }) => {
        this.clearDirectorySelection();
        this.directoryMembers.set(members);
        this.familyRelationships.set(relationships);
        this.directoryLoading.set(false);
        this.relationshipLoading.set(false);
        this.directoryError.set('');
        this.relationshipError.set('');
        this.eventAudienceMembers.set(members);
        this.planAudienceMembers.set(members);
        this.loadEvents();
        this.loadPlans();
        this.invitationMessage.set('');
        this.profileReview.set(true);
        this.view.set('profile');
      },
      error: () => this.handoffError.set('Your invitation was accepted, but your family could not be loaded. Retry to continue.'),
    });
  }

  private clearInvitationToken(): void {
    this.invitationPreview.set(null);
    this.invitationToken.set('');
    history.replaceState(null, '', location.pathname + location.search);
  }

  protected declineInvitation(): void {
    this.submitRecipientAction('/api/membership-invitations/decline', 'Invitation declined.');
  }

  protected eventAccessForMember(profileId: string): 'none' | 'view' | 'edit' {
    if (this.eventEditorProfileIds().includes(profileId)) return 'edit';
    if (this.eventAudienceProfileIds().includes(profileId)) return 'view';
    return 'none';
  }

  protected setEventMemberAccess(profileId: string, access: 'none' | 'view' | 'edit'): void {
    const audience = new Set(this.eventAudienceProfileIds());
    const editors = new Set(this.eventEditorProfileIds());
    if (access === 'none') {
      audience.delete(profileId);
      editors.delete(profileId);
    } else {
      audience.add(profileId);
      if (access === 'edit') editors.add(profileId);
      else editors.delete(profileId);
    }
    this.eventAudienceProfileIds.set([...audience]);
    this.eventEditorProfileIds.set([...editors]);
    this.eventError.set('');
  }

  protected createEvent(): void {
    this.eventForm.markAllAsTouched();
    this.eventError.set('');
    this.eventMessage.set('');
    const familyGroupId = this.eventForm.controls.familyGroupId.value;
    if (this.eventForm.invalid || !familyGroupId || this.eventAudienceProfileIds().length === 0) {
      this.eventError.set(
        this.eventAudienceProfileIds().length === 0
          ? 'Choose at least one active adult family member to share this event with.'
          : 'Complete each required event detail before saving.',
      );
      return;
    }

    this.eventSubmitting.set(true);
    const formValue = this.eventForm.getRawValue();
    this.http
      .post<FamilyEventView>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/events`,
        {
          name: formValue.name,
          description: formValue.description,
          date: formValue.date,
          time: formValue.time,
          location: formValue.location,
          audienceProfileIds: this.eventAudienceProfileIds(),
          editorProfileIds: this.eventEditorProfileIds(),
        },
      )
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.eventSubmitting.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (created) => {
          // D10: open the new Event at its item URL; the item page reads it through the authorize-first item route.
          if (this.router?.config.some(route => route.path === 'events/:eventId')) {
            void this.router.navigate(['/events', created.eventId], { state: { ...focusHeadingState, eventCreated: true } });
            return;
          }
          this.eventForm.reset({
            familyGroupId,
            name: '',
            description: '',
            date: '',
            time: '',
            location: '',
          });
          this.eventAudienceProfileIds.set([]);
          this.eventEditorProfileIds.set([]);
          this.eventAudienceSearch.set('');
          this.eventMessage.set('Event created and shared with the selected family members.');
          this.loadEvents();
        },
        error: () => this.eventError.set('The event could not be created. Please try again.'),
      });
  }

  protected availablePlanAudienceMembers(): readonly FamilyDirectoryEntry[] {
    const ownProfileId = this.ownerProfile()?.profileId;
    return this.planAudienceMembers().filter(member => member.profileId !== ownProfileId);
  }

  protected visiblePlanAudienceMembers(): readonly FamilyDirectoryEntry[] {
    const search = this.planAudienceSearch().trim().toLocaleLowerCase();
    return this.availablePlanAudienceMembers().filter(member => member.displayName.toLocaleLowerCase().includes(search));
  }

  protected isPlanAudienceSelected(profileId: string): boolean {
    return this.planAudienceProfileIds().includes(profileId);
  }

  protected setPlanAudienceMember(profileId: string, selected: boolean): void {
    const audience = new Set(this.planAudienceProfileIds());
    if (selected) audience.add(profileId);
    else audience.delete(profileId);
    this.planAudienceProfileIds.set([...audience]);
    this.planError.set('');
  }

  protected onPlanAudienceTypeChange(): void {
    this.planError.set('');
    if (this.planForm.controls.audienceType.value !== 'SelectedPeople') {
      this.planAudienceProfileIds.set([]);
      this.planAudienceSearch.set('');
    }
  }

  protected createPlan(): void {
    this.planForm.markAllAsTouched();
    this.planError.set('');
    this.planMessage.set('');
    const formValue = this.planForm.getRawValue();
    const familyGroupId = formValue.familyGroupId;
    const audienceType = formValue.audienceType;
    const expectedReturnIncomplete = Boolean(formValue.expectedReturnDate) !== Boolean(formValue.expectedReturnTime);
    const expectedCheckInIncomplete = Boolean(formValue.expectedCheckInDate) !== Boolean(formValue.expectedCheckInTime);
    if (this.planForm.invalid || !familyGroupId || !audienceType || expectedReturnIncomplete || expectedCheckInIncomplete) {
      this.planError.set(
        expectedReturnIncomplete || expectedCheckInIncomplete
          ? 'Enter both a date and time for an expected return or check-in, or leave both empty.'
          : 'Complete the plan details and choose who can see this plan.',
      );
      return;
    }

    if (audienceType === 'SelectedPeople' && this.planAudienceProfileIds().length === 0) {
      this.planError.set('Choose at least one active adult family member to share this plan with.');
      return;
    }

    this.planSubmitting.set(true);
    this.http
      .post<FamilyPlanView>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/plans`,
        {
          activityOrDestination: formValue.activityOrDestination,
          date: formValue.date,
          time: formValue.time,
          expectedReturnDate: formValue.expectedReturnDate || null,
          expectedReturnTime: formValue.expectedReturnTime || null,
          expectedCheckInDate: formValue.expectedCheckInDate || null,
          expectedCheckInTime: formValue.expectedCheckInTime || null,
          notes: formValue.notes?.trim() ? formValue.notes : null,
          audienceType,
          audienceProfileIds: audienceType === 'SelectedPeople' ? this.planAudienceProfileIds() : [],
        },
      )
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.planSubmitting.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.planForm.reset({
            familyGroupId,
            activityOrDestination: '',
            date: '',
            time: '',
            expectedReturnDate: '',
            expectedReturnTime: '',
            audienceType: '',
            expectedCheckInDate: '',
            expectedCheckInTime: '',
            notes: '',
          });
          this.planAudienceProfileIds.set([]);
          this.planAudienceSearch.set('');
          this.planMessage.set('Plan created with the selected audience.');
          this.loadPlans();
        },
        error: () => this.planError.set('The plan could not be created. Please try again.'),
      });
  }

  protected canCorrectPlan(plan: FamilyPlanView): boolean {
    return plan.canCorrectActivityTiming && plan.creatorProfileId === this.ownerProfile()?.profileId
      && this.activeFamilyGroups().some(family => family.familyGroupId === plan.familyGroupId);
  }

  protected beginPlanCorrection(plan: FamilyPlanView): void {
    if (!this.canCorrectPlan(plan) || this.planCorrectionSaving()) return;
    this.planCorrectionError.set('');
    this.planCorrectionMessage.set('');
    this.planCorrectionConflict.set(false);
    this.planCorrectionVersion = plan.version;
    this.planCorrectionForm.reset({
      activityOrDestination: plan.activityOrDestination, date: plan.date.slice(0, 10), time: plan.time,
      expectedReturnDate: plan.expectedReturnDate?.slice(0, 10) ?? '', expectedReturnTime: plan.expectedReturnTime ?? '',
      expectedCheckInDate: plan.expectedCheckInDate?.slice(0, 10) ?? '', expectedCheckInTime: plan.expectedCheckInTime ?? '',
    });
    this.editingPlanFactsId.set(plan.planId);
  }

  protected cancelPlanCorrection(): void {
    this.editingPlanFactsId.set(null);
    this.planCorrectionVersion = null;
    this.planCorrectionError.set('');
    this.planCorrectionConflict.set(false);
    this.planCorrectionForm.reset();
  }

  protected savePlanCorrection(plan: FamilyPlanView): void {
    if (!this.canCorrectPlan(plan) || this.editingPlanFactsId() !== plan.planId
      || this.planCorrectionSaving() || this.planNoteSaving() || !this.planCorrectionVersion) return;
    this.planCorrectionForm.markAllAsTouched();
    this.planCorrectionError.set('');
    const value = this.planCorrectionForm.getRawValue();
    const incompleteReturn = !!value.expectedReturnDate !== !!value.expectedReturnTime;
    const incompleteCheckIn = !!value.expectedCheckInDate !== !!value.expectedCheckInTime;
    if (this.planCorrectionForm.invalid || !value.activityOrDestination?.trim()
      || incompleteReturn || incompleteCheckIn) {
      this.planCorrectionError.set(incompleteReturn || incompleteCheckIn
        ? 'Enter both date and time for each optional timing, or leave both empty.'
        : 'Enter activity / destination (up to 200 characters), date and time.');
      return;
    }
    this.planCorrectionSaving.set(true);
    this.http.put<FamilyPlanView>(`/api/plans/${encodeURIComponent(plan.planId)}/activity-timing`, {
      activityOrDestination: value.activityOrDestination.trim(), date: value.date, time: value.time,
      expectedReturnDate: value.expectedReturnDate || null, expectedReturnTime: value.expectedReturnTime || null,
      expectedCheckInDate: value.expectedCheckInDate || null, expectedCheckInTime: value.expectedCheckInTime || null,
      version: this.planCorrectionVersion,
    }).pipe(takeUntil(this.familyScopeChanged), finalize(() => this.planCorrectionSaving.set(false)),
      takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => {
        if (this.view() !== 'planItem' || this.routeItemId() !== updated.planId) return;
        this.itemPlan.set(updated);
        this.familyPlans.update(plans => plans.map(current => current.planId === updated.planId ? updated : current));
        this.cancelPlanCorrection();
        this.planCorrectionMessage.set('Plan activity and timing saved.');
      },
      error: (error: HttpErrorResponse) => {
        if (this.view() !== 'planItem' || this.routeItemId() !== plan.planId) return;
        if (error.status === 404) {
          this.itemPlan.set(null);
          this.itemStatus.set('unavailable');
          this.cancelPlanCorrection();
          return;
        }
        this.planCorrectionConflict.set(error.status === 409);
        this.planCorrectionError.set(error.status === 409
          ? 'The Plan changed. Reload the Plan and review its current values before correcting it again.'
          : error.status === 401 ? 'Your session may have ended. Please sign in again.'
          : error.status === 400 ? 'Check the activity and timing values before saving.'
          : 'The Plan could not be corrected. Please try again.');
      },
    });
  }

  protected beginPlanNoteEdit(plan: FamilyPlanView): void {
    if (plan.creatorProfileId !== this.ownerProfile()?.profileId || this.planCorrectionSaving()) return;
    this.planNoteClearPendingId.set(null);
    this.planNoteError.set('');
    this.planNoteMessage.set('');
    this.planNoteForm.reset({ notes: plan.notes ?? '' });
    this.editingPlanNoteId.set(plan.planId);
  }

  protected cancelPlanNoteEdit(): void {
    this.editingPlanNoteId.set(null);
    this.planNoteClearPendingId.set(null);
    this.planNoteError.set('');
    this.planNoteForm.reset({ notes: '' });
  }

  protected requestPlanNoteClear(plan: FamilyPlanView): void {
    if (plan.creatorProfileId !== this.ownerProfile()?.profileId || !plan.notes || this.planNoteSaving() || this.planCorrectionSaving()) return;
    this.planNoteClearPendingId.set(plan.planId);
  }

  protected cancelPlanNoteClear(): void {
    this.planNoteClearPendingId.set(null);
  }

  protected confirmPlanNoteClear(plan: FamilyPlanView): void {
    if (this.planNoteClearPendingId() !== plan.planId || !plan.notes) return;
    this.planNoteClearPendingId.set(null);
    this.savePlanNote(plan, true);
  }

  protected savePlanNote(plan: FamilyPlanView, clear = false): void {
    if (plan.creatorProfileId !== this.ownerProfile()?.profileId || this.planNoteSaving() || this.planCorrectionSaving()) return;
    if (!clear) this.planNoteForm.markAllAsTouched();
    this.planNoteError.set('');
    if (!clear && this.planNoteForm.invalid) {
      this.planNoteError.set('Notes cannot exceed 1,000 characters.');
      return;
    }

    const value = clear ? null : this.planNoteForm.controls.notes.value;
    this.planNoteSaving.set(true);
    this.http.put<FamilyPlanView>(`/api/plans/${encodeURIComponent(plan.planId)}/notes`, {
      notes: value?.trim() ? value : null,
    })
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.planNoteSaving.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: updated => {
          if (this.routeItemId() !== updated.planId) return;
          this.itemPlan.set(updated);
          this.familyPlans.update(plans => plans.map(current => current.planId === updated.planId ? updated : current));
          this.cancelPlanNoteEdit();
          this.planNoteMessage.set(updated.notes ? 'Plan note saved.' : 'Plan note cleared.');
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.itemPlan.set(null);
            this.itemStatus.set('unavailable');
            return;
          }
          this.planNoteError.set(error.status === 401
            ? 'Your session may have ended. Please sign in again.'
            : 'The Plan note could not be saved. Please try again.');
        },
      });
  }

  protected beginEventEdit(event: FamilyEventView): void {
    if (!event.canEdit) return;
    this.eventEditError.set('');
    this.editingEventId.set(event.eventId);
    this.eventEditForm.reset({
      name: event.name,
      description: event.description,
      date: event.date.slice(0, 10),
      time: event.time.slice(0, 5),
      location: event.location,
    });
  }

  protected cancelEventEdit(): void {
    this.editingEventId.set(null);
    this.eventEditError.set('');
    this.eventEditForm.reset();
  }

  protected saveEventEdits(event: FamilyEventView): void {
    this.eventEditForm.markAllAsTouched();
    this.eventEditError.set('');
    if (!event.canEdit || this.eventEditForm.invalid) {
      this.eventEditError.set('Complete each required event detail before saving.');
      return;
    }

    this.eventUpdating.set(true);
    const formValue = this.eventEditForm.getRawValue();
    this.http
      .put<FamilyEventView>(
        `/api/family-groups/${encodeURIComponent(event.familyGroupId)}/events/${encodeURIComponent(event.eventId)}`,
        {
          name: formValue.name,
          description: formValue.description,
          date: formValue.date,
          time: formValue.time,
          location: formValue.location,
        },
      )
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.eventUpdating.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (updatedEvent) => {
          this.editingEventId.set(null);
          this.eventEditForm.reset();
          this.eventMessage.set('Event details updated.');
          if (this.itemEvent()?.eventId === updatedEvent.eventId) this.itemEvent.set(updatedEvent);
          this.familyEvents.update((events) =>
            events.map((current) =>
              current.eventId === updatedEvent.eventId ? updatedEvent : current,
            ),
          );
        },
        error: () => this.eventEditError.set('The event could not be updated. Please try again.'),
      });
  }

  protected searchDirectory(): void {
    this.clearDirectorySelection();
    this.loadDirectory();
  }

  protected availableRelationshipMembers(): readonly FamilyDirectoryEntry[] {
    const ownProfileId = this.ownerProfile()?.profileId;
    return this.directoryMembers().filter((member) => member.profileId !== ownProfileId);
  }

  protected availableEventAudienceMembers(): readonly FamilyDirectoryEntry[] {
    const ownProfileId = this.ownerProfile()?.profileId;
    return this.eventAudienceMembers().filter((member) => member.profileId !== ownProfileId);
  }

  protected visibleEventAudienceMembers(): readonly FamilyDirectoryEntry[] {
    const search = this.eventAudienceSearch().trim().toLocaleLowerCase();
    return this.availableEventAudienceMembers().filter((member) =>
      member.displayName.toLocaleLowerCase().includes(search),
    );
  }

  protected relationshipLabelText(label: FamilyRelationshipLabel): string {
    return this.relationshipLabelOptions.find((option) => option.value === label)?.label ?? '';
  }

  protected proposeRelationship(): void {
    this.relationshipForm.markAllAsTouched();
    this.relationshipError.set('');
    this.relationshipMessage.set('');
    if (this.relationshipForm.invalid) {
      this.relationshipError.set('Choose an active adult family member and a relationship.');
      return;
    }

    const familyGroupId = this.directoryForm.controls.familyGroupId.value;
    if (!familyGroupId) return;
    this.relationshipSubmitting.set(true);
    const request = this.relationshipForm.getRawValue();
    this.http
      .post(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/relationship-proposals`,
        request,
      )
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.relationshipSubmitting.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.relationshipForm.reset({ recipientProfileId: '', proposerLabel: 'OtherFamily' });
          this.relationshipMessage.set(
            'Relationship suggestion sent. Only you and the recipient can see it until they accept.',
          );
          this.loadRelationships();
        },
        error: (error: HttpErrorResponse) => {
          this.relationshipError.set(
            error.status === 409
              ? 'A pending or accepted relationship already exists for these family members.'
              : error.status === 400
                ? 'Choose an active adult family member and a supported relationship.'
                : 'The relationship suggestion could not be sent. Please try again.',
          );
        },
      });
  }

  protected respondToRelationship(
    relationship: FamilyRelationshipView,
    action: 'accept' | 'decline' | 'withdraw' | 'remove',
  ): void {
    this.relationshipError.set('');
    this.relationshipMessage.set('');
    this.relationshipBusyId.set(relationship.relationshipId);
    const url = `/api/family-groups/relationships/${encodeURIComponent(relationship.relationshipId)}`;
    const request =
      action === 'remove' ? this.http.delete(url) : this.http.post(`${url}/${action}`, {});

    request.pipe(takeUntil(this.familyScopeChanged), finalize(() => this.relationshipBusyId.set(''))).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        const messages = {
          accept: 'Relationship confirmed. Active adults in this family group can now see it.',
          decline: 'Relationship suggestion declined.',
          withdraw: 'Relationship suggestion withdrawn.',
          remove: 'Relationship removed.',
        };
        if (action === 'remove') this.relationshipRemovalPendingId.set('');
        this.relationshipMessage.set(messages[action]);
        this.loadRelationships();
      },
      error: () =>
        this.relationshipError.set('This relationship is unavailable or could not be updated.'),
    });
  }

  protected requestRelationshipRemoval(relationshipId: string): void {
    this.relationshipRemovalPendingId.set(relationshipId);
  }

  protected cancelRelationshipRemoval(): void {
    this.relationshipRemovalPendingId.set('');
  }

  protected selectDirectoryMember(member: FamilyDirectoryEntry): void {
    this.directoryError.set('');
    const owner = this.ownerProfile();
    if (owner?.profileId === member.profileId) {
      this.selectedMember.set(toSharedProfile(owner));
      return;
    }

    this.directoryLoading.set(true);
    this.http
      .get<SharedAdultProfile>(
        `/api/family-members/${encodeURIComponent(member.profileId)}/profile`,
      )
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.directoryLoading.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (profile) => this.selectedMember.set(profile),
        error: () => {
          this.selectedMember.set(null);
          this.directoryError.set('This family member is unavailable.');
        },
      });
  }

  protected save(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
    this.profileForm.markAllAsTouched();

    if (this.profileForm.invalid) {
      this.errorMessage.set('Review the highlighted profile fields before saving.');
      return;
    }

    this.saving.set(true);
    const value = this.profileForm.getRawValue();
    this.http
      .put<AdultProfile>('/api/profile', {
        displayName: value.displayName,
        contactEmail: emptyToNull(value.contactEmail),
        contactEmailVisibility: value.contactEmailVisibility,
        contactPhone: emptyToNull(value.contactPhone),
        contactPhoneVisibility: value.contactPhoneVisibility,
        birthdayMonth: value.birthdayMonth,
        birthdayDay: value.birthdayDay,
        birthdayVisibility: value.birthdayVisibility,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (profile) => {
          this.ownerProfile.set(profile);
          this.applyProfile(profile);
          this.profileLoadFailed.set(false);
          this.successMessage.set('Profile saved.');
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            error.status === 400
              ? 'Review the highlighted profile fields before saving.'
              : 'Your profile could not be saved. Please try again.',
          );
        },
      });
  }

  private applyProfile(profile: AdultProfile): void {
    this.profileForm.reset({
      displayName: profile.displayName,
      contactEmail: profile.contactEmail ?? '',
      contactEmailVisibility: profile.contactEmailVisibility,
      contactPhone: profile.contactPhone ?? '',
      contactPhoneVisibility: profile.contactPhoneVisibility,
      birthdayMonth: profile.birthdayMonth,
      birthdayDay: profile.birthdayDay,
      birthdayVisibility: profile.birthdayVisibility,
    });
  }

  private submitAdministratorAction(url: string, success: string): void {
    this.invitationForm.markAllAsTouched();
    this.invitationMessage.set('');
    this.invitationError.set('');
    if (this.invitationForm.invalid) {
      this.invitationError.set('Choose a family group and enter a valid email address.');
      return;
    }

    this.invitationBusy.set(true);
    this.http
      .post(url, this.invitationForm.getRawValue())
      .pipe(takeUntil(this.familyScopeChanged), finalize(() => this.invitationBusy.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => this.invitationMessage.set(success),
        error: (error: HttpErrorResponse) =>
          this.invitationError.set(
            error.status === 409
              ? 'This family group has reached its capacity.'
              : error.status === 400
                ? 'Enter a valid email address.'
                : 'The invitation action could not be completed.',
          ),
      });
  }

  private loadDirectory(): void {
    const requestVersion = ++this.directoryRequestVersion;
    const familyGroupId = this.directoryForm.controls.familyGroupId.value;
    if (!familyGroupId) return;
    const search = this.directoryForm.controls.search.value?.trim() ?? '';
    const query = search ? `?search=${encodeURIComponent(search)}` : '';
    this.directoryLoading.set(true);
    this.directoryError.set('');
    this.directoryLoadFailed.set(false);
    this.http
      .get<FamilyDirectoryEntry[]>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/members${query}`,
      )
      .pipe(finalize(() => {
        if (requestVersion === this.directoryRequestVersion) this.directoryLoading.set(false);
      }))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (members) => {
          if (requestVersion !== this.directoryRequestVersion) return;
          this.directoryMembers.set(members);
          if (familyGroupId === this.eventForm.controls.familyGroupId.value) {
            this.eventAudienceMembers.set(members);
          }
          if (familyGroupId === this.planForm.controls.familyGroupId.value) {
            this.planAudienceMembers.set(members);
          }
        },
        error: () => {
          if (requestVersion !== this.directoryRequestVersion) return;
          this.directoryMembers.set([]);
          this.directoryError.set('Your family directory could not be loaded. Please try again.');
          this.directoryLoadFailed.set(true);
        },
      });
  }

  protected loadPendingEventInvitations(): void {
    const familyId = this.currentFamilyId();
    if (!familyId || (this.view() !== 'home' && this.view() !== 'events')) return;
    const version = ++this.pendingEventInvitationsVersion;
    this.pendingEventInvitations.set([]);
    this.pendingEventInvitationsLoading.set(true);
    this.pendingEventInvitationsError.set('');
    this.http.get<PendingEventInvitation[]>(`/api/family-groups/${encodeURIComponent(familyId)}/event-invitations/mine`)
      .pipe(takeUntil(this.familyScopeChanged), takeUntilDestroyed(this.destroyRef)).subscribe({
        next: rows => {
          if (version !== this.pendingEventInvitationsVersion || familyId !== this.currentFamilyId()) return;
          this.pendingEventInvitations.set(rows);
          this.pendingEventInvitationsLoading.set(false);
        },
        error: () => {
          if (version !== this.pendingEventInvitationsVersion || familyId !== this.currentFamilyId()) return;
          this.pendingEventInvitationsLoading.set(false);
          this.pendingEventInvitationsError.set('Your pending Event invitations could not be loaded. Please try again.');
        },
      });
  }

  private loadEvents(): void {
    const familyGroupId = this.eventForm.controls.familyGroupId.value;
    if (!familyGroupId) {
      this.familyEvents.set([]);
      return;
    }

    const requestVersion = ++this.eventsRequestVersion;
    this.eventsLoading.set(true);
    this.eventsLoadError.set('');
    this.http
      .get<FamilyEventView[]>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/events`,
      )
      .pipe(finalize(() => {
        if (requestVersion === this.eventsRequestVersion) this.eventsLoading.set(false);
      }))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (events) => {
          if (
            requestVersion === this.eventsRequestVersion &&
            familyGroupId === this.eventForm.controls.familyGroupId.value
          ) {
            this.familyEvents.set(events);
          }
        },
        error: () => {
          if (
            requestVersion === this.eventsRequestVersion &&
            familyGroupId === this.eventForm.controls.familyGroupId.value
          ) {
            this.familyEvents.set([]);
            this.eventsLoadError.set('Events could not be loaded. Please try again.');
          }
        },
      });
  }

  private loadEventAudienceMembers(): void {
    const familyGroupId = this.eventForm.controls.familyGroupId.value;
    if (!familyGroupId) {
      this.eventAudienceMembers.set([]);
      return;
    }

    if (familyGroupId === this.directoryForm.controls.familyGroupId.value) {
      this.eventAudienceMembers.set(this.directoryMembers());
      return;
    }

    const requestVersion = ++this.eventAudienceRequestVersion;
    this.http
      .get<FamilyDirectoryEntry[]>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/members`,
      )
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (members) => {
          if (
            requestVersion === this.eventAudienceRequestVersion &&
            familyGroupId === this.eventForm.controls.familyGroupId.value
          ) {
            this.eventAudienceMembers.set(members);
          }
        },
        error: () => {
          if (requestVersion === this.eventAudienceRequestVersion) {
            this.eventAudienceMembers.set([]);
          }
        },
      });
  }

  private loadPlans(): void {
    const familyGroupId = this.planForm.controls.familyGroupId.value;
    if (!familyGroupId) {
      this.familyPlans.set([]);
      return;
    }

    const requestVersion = ++this.plansRequestVersion;
    this.plansLoading.set(true);
    this.plansLoadError.set('');
    this.http
      .get<FamilyPlanView[]>(`/api/family-groups/${encodeURIComponent(familyGroupId)}/plans`)
      .pipe(finalize(() => {
        if (requestVersion === this.plansRequestVersion) this.plansLoading.set(false);
      }))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (plans) => {
          if (requestVersion === this.plansRequestVersion && familyGroupId === this.planForm.controls.familyGroupId.value) {
            this.familyPlans.set(plans);
          }
        },
        error: () => {
          if (requestVersion === this.plansRequestVersion && familyGroupId === this.planForm.controls.familyGroupId.value) {
            this.familyPlans.set([]);
            this.plansLoadError.set('Plans could not be loaded. Please try again.');
          }
        },
      });
  }

  private loadHomeTrips(familyGroupId: string): void {
    const requestVersion = ++this.homeTripsRequestVersion;
    this.homeTripsLoading.set(true);
    this.homeTripsError.set('');
    this.http.get<HomeTripPreview[]>('/api/trips')
      .pipe(finalize(() => {
        if (requestVersion === this.homeTripsRequestVersion) this.homeTripsLoading.set(false);
      }))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: trips => {
          if (requestVersion === this.homeTripsRequestVersion && familyGroupId === this.currentFamilyId())
            this.homeTripResults.set(trips.filter(trip => trip.familyGroupId === familyGroupId));
        },
        error: () => {
          if (requestVersion === this.homeTripsRequestVersion && familyGroupId === this.currentFamilyId()) {
            this.homeTripResults.set([]);
            this.homeTripsError.set('Trips could not be loaded. Please try again.');
          }
        },
      });
  }

  private loadPlanAudienceMembers(): void {
    const familyGroupId = this.planForm.controls.familyGroupId.value;
    if (!familyGroupId) {
      this.planAudienceMembers.set([]);
      return;
    }

    if (familyGroupId === this.directoryForm.controls.familyGroupId.value) {
      this.planAudienceMembers.set(this.directoryMembers());
      return;
    }

    const requestVersion = ++this.planAudienceRequestVersion;
    this.http
      .get<FamilyDirectoryEntry[]>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/members`,
      )
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (members) => {
          if (requestVersion === this.planAudienceRequestVersion && familyGroupId === this.planForm.controls.familyGroupId.value) {
            this.planAudienceMembers.set(members);
          }
        },
        error: () => {
          if (requestVersion === this.planAudienceRequestVersion) {
            this.planAudienceMembers.set([]);
          }
        },
      });
  }

  private loadRelationships(): void {
    const familyGroupId = this.directoryForm.controls.familyGroupId.value;
    if (!familyGroupId) return;
    const requestVersion = ++this.relationshipRequestVersion;
    this.relationshipLoading.set(true);
    this.relationshipError.set('');
    this.relationshipsLoadFailed.set(false);
    this.http
      .get<FamilyRelationshipView[]>(
        `/api/family-groups/${encodeURIComponent(familyGroupId)}/relationships`,
      )
      .pipe(
        finalize(() => {
          if (requestVersion === this.relationshipRequestVersion) {
            this.relationshipLoading.set(false);
          }
        }),
      )
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (relationships) => {
          if (
            requestVersion === this.relationshipRequestVersion &&
            familyGroupId === this.directoryForm.controls.familyGroupId.value
          ) {
            this.familyRelationships.set(relationships);
          }
        },
        error: () => {
          if (
            requestVersion === this.relationshipRequestVersion &&
            familyGroupId === this.directoryForm.controls.familyGroupId.value
          ) {
            this.familyRelationships.set([]);
            this.relationshipError.set(
              'Family relationships could not be loaded. Please try again.',
            );
            this.relationshipsLoadFailed.set(true);
          }
        },
      });
  }

  private clearDirectorySelection(): void {
    this.selectedMember.set(null);
    this.directoryMembers.set([]);
  }

  private previewInvitation(token: string): void {
    this.invitationBusy.set(true);
    this.http
      .post<InvitationPreview>('/api/membership-invitations/preview', { token })
      .pipe(finalize(() => this.invitationBusy.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (preview) => this.invitationPreview.set(preview),
        error: () => this.invitationError.set('This invitation is unavailable.'),
      });
  }

  private submitRecipientAction(url: string, success: string): void {
    const token = this.invitationToken();
    if (!token || this.invitationBusy()) return;
    this.invitationBusy.set(true);
    this.invitationError.set('');
    this.http
      .post(url, { token })
      .pipe(finalize(() => this.invitationBusy.set(false)))
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.clearInvitationToken();
          this.invitationMessage.set(success);
        },
        error: (error: HttpErrorResponse) =>
          this.invitationError.set(
            error.status === 409
              ? 'This family group has reached its capacity.'
              : 'This invitation is unavailable.',
          ),
      });
  }
}

function toSharedProfile(profile: AdultProfile): SharedAdultProfile {
  return {
    profileId: profile.profileId,
    displayName: profile.displayName,
    contactEmail: profile.contactEmail,
    contactPhone: profile.contactPhone,
    birthdayMonth: profile.birthdayMonth,
    birthdayDay: profile.birthdayDay,
  };
}

function readInvitationToken(): string {
  const prefix = '#invite=';
  try {
    return location.hash.startsWith(prefix) ? decodeURIComponent(location.hash.slice(prefix.length)).trim() : '';
  } catch { return ''; }
}

function emptyToNull(value: string | null): string | null {
  const normalized = value?.trim();
  return normalized ? normalized : null;
}

function localDateString(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function compareDateTime(leftDate: string, leftTime: string, rightDate: string, rightTime: string): number {
  return `${leftDate.slice(0, 10)}T${leftTime}`.localeCompare(`${rightDate.slice(0, 10)}T${rightTime}`);
}

function birthdayValidator(control: AbstractControl): ValidationErrors | null {
  const month = control.get('birthdayMonth')?.value as number | null;
  const day = control.get('birthdayDay')?.value as number | null;

  if (month === null && day === null) {
    return null;
  }

  if (month === null || day === null) {
    return { birthdayPair: true };
  }

  const date = new Date(Date.UTC(2000, month - 1, day));
  return date.getUTCMonth() === month - 1 && date.getUTCDate() === day
    ? null
    : { birthdayDate: true };
}
