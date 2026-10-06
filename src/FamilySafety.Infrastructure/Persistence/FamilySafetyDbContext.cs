using FamilySafety.Domain.Households;
using FamilySafety.Domain.SafetyCircles;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.TrustedContacts;
using FamilySafety.Domain.CheckIns;
using FamilySafety.Domain.Sharing;
using FamilySafety.Domain.FamilyDates;
using FamilySafety.Domain.Relationships;
using FamilySafety.Domain.Events;
using FamilySafety.Domain.Plans;
using FamilySafety.Domain.Trips;
using FamilySafety.Domain.Concerns;
using FamilySafety.Domain.Incidents;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class FamilySafetyDbContext(DbContextOptions<FamilySafetyDbContext> options)
    : DbContext(options)
{
    public DbSet<FamilyHousehold> FamilyHouseholds => Set<FamilyHousehold>();
    public DbSet<HouseholdAssignment> HouseholdAssignments => Set<HouseholdAssignment>();
    public DbSet<SafetyCircle> SafetyCircles => Set<SafetyCircle>();
    public DbSet<SafetyCircleMember> SafetyCircleMembers => Set<SafetyCircleMember>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<TrustedContact> TrustedContacts => Set<TrustedContact>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<CheckInRecipient> CheckInRecipients => Set<CheckInRecipient>();
    public DbSet<SharingSession> SharingSessions => Set<SharingSession>();
    public DbSet<SharingSessionRecipient> SharingSessionRecipients => Set<SharingSessionRecipient>();
    public DbSet<SharingCurrentLocation> SharingCurrentLocations => Set<SharingCurrentLocation>();

    public DbSet<AdultProfile> AdultProfiles => Set<AdultProfile>();

    public DbSet<FamilyGroup> FamilyGroups => Set<FamilyGroup>();

    public DbSet<FamilyGroupMembership> FamilyGroupMemberships => Set<FamilyGroupMembership>();

    public DbSet<FamilyAdministratorAssignment> FamilyAdministratorAssignments =>
        Set<FamilyAdministratorAssignment>();

    public DbSet<MembershipInvitation> MembershipInvitations => Set<MembershipInvitation>();

    public DbSet<FamilyRelationship> FamilyRelationships => Set<FamilyRelationship>();
    public DbSet<SharedFamilyDate> SharedFamilyDates => Set<SharedFamilyDate>();
    public DbSet<SharedFamilyDateAssociation> SharedFamilyDateAssociations => Set<SharedFamilyDateAssociation>();

    public DbSet<FamilyEvent> FamilyEvents => Set<FamilyEvent>();

    public DbSet<FamilyEventAudienceMember> FamilyEventAudienceMembers => Set<FamilyEventAudienceMember>();
    public DbSet<FamilyEventInvitation> FamilyEventInvitations => Set<FamilyEventInvitation>();
    public DbSet<FamilyEventNeed> FamilyEventNeeds => Set<FamilyEventNeed>();
    public DbSet<EventNeedResponsibilityChange> EventNeedResponsibilityChanges => Set<EventNeedResponsibilityChange>();
    public DbSet<FamilyEventUpdate> FamilyEventUpdates => Set<FamilyEventUpdate>();
    public DbSet<EventUpdateRevision> EventUpdateRevisions => Set<EventUpdateRevision>();

    public DbSet<FamilyPlan> FamilyPlans => Set<FamilyPlan>();

    public DbSet<FamilyPlanAudienceMember> FamilyPlanAudienceMembers => Set<FamilyPlanAudienceMember>();

    public DbSet<FamilyTrip> FamilyTrips => Set<FamilyTrip>();
    public DbSet<FamilyTripTraveler> FamilyTripTravelers => Set<FamilyTripTraveler>();
    public DbSet<FamilyTripAudienceMember> FamilyTripAudienceMembers => Set<FamilyTripAudienceMember>();
    public DbSet<FamilyTripContact> FamilyTripContacts => Set<FamilyTripContact>();
    public DbSet<FamilyTripNote> FamilyTripNotes => Set<FamilyTripNote>();
    public DbSet<Concern> Concerns => Set<Concern>();
    public DbSet<ConcernParticipantGrant> ConcernParticipantGrants => Set<ConcernParticipantGrant>();
    public DbSet<ConcernEntry> ConcernEntries => Set<ConcernEntry>();
    public DbSet<ConcernAuditEvent> ConcernAuditEvents => Set<ConcernAuditEvent>();
    public DbSet<SafetyIncident> Incidents => Set<SafetyIncident>();
    public DbSet<IncidentParticipantGrant> IncidentParticipantGrants => Set<IncidentParticipantGrant>();
    public DbSet<IncidentCoordinatorChange> IncidentCoordinatorChanges => Set<IncidentCoordinatorChange>();
    public DbSet<IncidentEntry> IncidentEntries => Set<IncidentEntry>();
    public DbSet<IncidentTask> IncidentTasks => Set<IncidentTask>();
    public DbSet<IncidentAuditEvent> IncidentAuditEvents => Set<IncidentAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var household = modelBuilder.Entity<FamilyHousehold>();
        household.ToTable("FamilyHouseholds");
        household.HasKey(h => h.Id);
        household.HasAlternateKey(h => new { h.Id, h.FamilyGroupId });
        household.Property(h => h.Name).HasMaxLength(100).IsRequired();
        household.Property(h => h.NormalizedName).HasMaxLength(200).UseCollation("Latin1_General_100_BIN2").IsRequired();
        household.Property(h => h.Version).IsConcurrencyToken();
        household.HasIndex(h => new { h.FamilyGroupId, h.NormalizedName }).IsUnique();
        household.HasOne<FamilyGroup>().WithMany().HasForeignKey(h => h.FamilyGroupId).OnDelete(DeleteBehavior.Restrict);
        var assignment = modelBuilder.Entity<HouseholdAssignment>();
        assignment.ToTable("HouseholdAssignments", t => t.HasTrigger("TR_HouseholdAssignments_Eligibility"));
        assignment.HasKey(a => new { a.FamilyGroupId, a.ProfileId });
        assignment.HasOne<FamilyHousehold>().WithMany().HasForeignKey(a => new { a.HouseholdId, a.FamilyGroupId })
            .HasPrincipalKey(h => new { h.Id, h.FamilyGroupId }).OnDelete(DeleteBehavior.Cascade);
        assignment.HasOne<AdultProfile>().WithMany().HasForeignKey(a => a.ProfileId).OnDelete(DeleteBehavior.Restrict);
        assignment.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(a => a.MembershipId).OnDelete(DeleteBehavior.Restrict);
        var circle = modelBuilder.Entity<SafetyCircle>();
        circle.ToTable("SafetyCircles"); circle.HasKey(c => c.Id);
        circle.Property(c => c.Name).HasMaxLength(100).IsRequired();
        circle.Property(c => c.NormalizedName).HasMaxLength(200).UseCollation("Latin1_General_100_BIN2").IsRequired();
        circle.Property(c => c.Version).IsConcurrencyToken();
        circle.HasIndex(c => new { c.OwnerProfileId, c.FamilyGroupId, c.NormalizedName }).IsUnique();
        circle.HasOne<AdultProfile>().WithMany().HasForeignKey(c => c.OwnerProfileId).OnDelete(DeleteBehavior.Restrict);
        circle.HasOne<FamilyGroup>().WithMany().HasForeignKey(c => c.FamilyGroupId).OnDelete(DeleteBehavior.Restrict);
        circle.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(c => c.OwnerMembershipId).OnDelete(DeleteBehavior.Restrict);
        var circleMember = modelBuilder.Entity<SafetyCircleMember>();
        circleMember.ToTable("SafetyCircleMembers"); circleMember.HasKey(m => new { m.SafetyCircleId, m.MemberProfileId });
        circleMember.HasOne<SafetyCircle>().WithMany().HasForeignKey(m => m.SafetyCircleId).OnDelete(DeleteBehavior.Cascade);
        circleMember.HasOne<AdultProfile>().WithMany().HasForeignKey(m => m.MemberProfileId).OnDelete(DeleteBehavior.Restrict);
        circleMember.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(m => m.MembershipId).OnDelete(DeleteBehavior.Restrict);
        var trustedContact = modelBuilder.Entity<TrustedContact>();
        trustedContact.ToTable("TrustedContacts", t => t.HasCheckConstraint("CK_TrustedContacts_ContactMethod",
            "([PhoneNumber] IS NOT NULL AND [NormalizedPhone] IS NOT NULL) OR ([EmailAddress] IS NOT NULL AND [NormalizedEmail] IS NOT NULL)"));
        trustedContact.HasKey(c => c.Id);
        trustedContact.Property(c => c.DisplayName).HasMaxLength(TrustedContact.NameLimit).IsRequired();
        trustedContact.Property(c => c.PhoneNumber).HasMaxLength(TrustedContact.PhoneLimit);
        trustedContact.Property(c => c.NormalizedPhone).HasMaxLength(16).UseCollation("Latin1_General_100_BIN2");
        trustedContact.Property(c => c.EmailAddress).HasMaxLength(TrustedContact.EmailLimit);
        trustedContact.Property(c => c.NormalizedEmail).HasMaxLength(TrustedContact.EmailLimit).UseCollation("Latin1_General_100_BIN2");
        trustedContact.Property(c => c.RelationshipLabel).HasMaxLength(TrustedContact.LabelLimit);
        trustedContact.Property(c => c.Version).IsConcurrencyToken();
        trustedContact.HasIndex(c => new { c.OwnerProfileId, c.NormalizedPhone }).IsUnique().HasFilter("[NormalizedPhone] IS NOT NULL");
        trustedContact.HasIndex(c => new { c.OwnerProfileId, c.NormalizedEmail }).IsUnique().HasFilter("[NormalizedEmail] IS NOT NULL");
        trustedContact.HasOne<AdultProfile>().WithMany().HasForeignKey(c => c.OwnerProfileId).OnDelete(DeleteBehavior.Cascade);
        var session = modelBuilder.Entity<SharingSession>();
        session.ToTable("SharingSessions", t => t.HasCheckConstraint("CK_SharingSessions_Duration", "[EndsAtUtc] > [StartedAtUtc] AND [EndsAtUtc] <= DATEADD(hour, 24, [StartedAtUtc])"));
        session.HasKey(s => s.Id);
        session.Property(s => s.Version).IsConcurrencyToken();
        session.HasIndex(s => new { s.AuthorProfileId, s.EndsAtUtc });
        session.HasOne<FamilyGroup>().WithMany().HasForeignKey(s => s.FamilyGroupId).OnDelete(DeleteBehavior.Restrict);
        session.HasOne<AdultProfile>().WithMany().HasForeignKey(s => s.AuthorProfileId).OnDelete(DeleteBehavior.Restrict);
        session.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(s => s.AuthorMembershipId).OnDelete(DeleteBehavior.Restrict);
        var recipient = modelBuilder.Entity<SharingSessionRecipient>();
        recipient.ToTable("SharingSessionRecipients");
        recipient.HasKey(r => new { r.SharingSessionId, r.ProfileId });
        recipient.HasIndex(r => new { r.ProfileId, r.IsInvalidated });
        recipient.HasOne<SharingSession>().WithMany().HasForeignKey(r => r.SharingSessionId).OnDelete(DeleteBehavior.Cascade);
        recipient.HasOne<AdultProfile>().WithMany().HasForeignKey(r => r.ProfileId).OnDelete(DeleteBehavior.Restrict);
        recipient.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(r => r.MembershipId).OnDelete(DeleteBehavior.Restrict);
        var location = modelBuilder.Entity<SharingCurrentLocation>();
        location.ToTable("SharingCurrentLocations", t => t.HasCheckConstraint("CK_SharingCurrentLocations_Coordinates", "[Latitude] >= -90 AND [Latitude] <= 90 AND [Longitude] >= -180 AND [Longitude] <= 180 AND [Accuracy] >= 0"));
        location.HasKey(l => l.SharingSessionId);
        location.HasOne<SharingSession>().WithOne().HasForeignKey<SharingCurrentLocation>(l => l.SharingSessionId).OnDelete(DeleteBehavior.Cascade);
        var checkIn = modelBuilder.Entity<CheckIn>();
        checkIn.ToTable("CheckIns", t => t.HasCheckConstraint("CK_CheckIns_Status", "[Status] IN ('ImOkay','Arrived','LeavingNow','Delayed','PlansChanged')"));
        checkIn.HasKey(c => c.Id);
        checkIn.Property(c => c.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        checkIn.Property(c => c.Note).HasMaxLength(CheckIn.NoteLimit);
        checkIn.Property(c => c.CreatedAtUtc).HasColumnType("datetimeoffset(7)");
        checkIn.HasIndex(c => new { c.AuthorProfileId, c.CreatedAtUtc });
        checkIn.HasOne<AdultProfile>().WithMany().HasForeignKey(c => c.AuthorProfileId).OnDelete(DeleteBehavior.Restrict);
        checkIn.HasOne<FamilyGroup>().WithMany().HasForeignKey(c => c.FamilyGroupId).OnDelete(DeleteBehavior.Restrict);
        checkIn.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(c => c.AuthorMembershipId).OnDelete(DeleteBehavior.Restrict);
        var checkInRecipient = modelBuilder.Entity<CheckInRecipient>();
        checkInRecipient.ToTable("CheckInRecipients");
        checkInRecipient.HasKey(r => new { r.CheckInId, r.ProfileId });
        checkInRecipient.HasIndex(r => new { r.ProfileId, r.IsInvalidated });
        checkInRecipient.HasOne<CheckIn>().WithMany().HasForeignKey(r => r.CheckInId).OnDelete(DeleteBehavior.Cascade);
        checkInRecipient.HasOne<AdultProfile>().WithMany().HasForeignKey(r => r.ProfileId).OnDelete(DeleteBehavior.Restrict);
        checkInRecipient.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(r => r.MembershipId).OnDelete(DeleteBehavior.Restrict);
        var account = modelBuilder.Entity<Account>();
        account.ToTable("Accounts");
        account.HasKey(entity => entity.Id);
        account.Property(entity => entity.SignInIdentifier).HasMaxLength(254).IsRequired();
        account.HasIndex(entity => entity.SignInIdentifier).IsUnique();

        var profile = modelBuilder.Entity<AdultProfile>();
        profile.ToTable("AdultProfiles");
        profile.HasKey(entity => entity.Id);
        profile.Property(entity => entity.DisplayName).HasMaxLength(100).IsRequired();
        profile.Property(entity => entity.ContactEmail).HasMaxLength(254);
        profile.Property(entity => entity.ContactPhone).HasMaxLength(32);
        profile.Property(entity => entity.ContactEmailVisibility).HasConversion<string>().HasMaxLength(32);
        profile.Property(entity => entity.ContactPhoneVisibility).HasConversion<string>().HasMaxLength(32);
        profile.Property(entity => entity.BirthdayVisibility).HasConversion<string>().HasMaxLength(32);
        profile.HasIndex(entity => entity.AccountId).IsUnique();
        profile.HasOne<Account>()
            .WithOne()
            .HasForeignKey<AdultProfile>(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        var familyGroup = modelBuilder.Entity<FamilyGroup>();
        familyGroup.ToTable("FamilyGroups");
        familyGroup.HasKey(entity => entity.Id);
        familyGroup.Property(entity => entity.Name).HasMaxLength(100).IsRequired();
        familyGroup.Property(entity => entity.CapacityLimit).IsRequired();

        var membership = modelBuilder.Entity<FamilyGroupMembership>();
        membership.ToTable("FamilyGroupMemberships", table => {
            table.HasTrigger("TR_EventNeeds_MembershipLoss");
            table.HasTrigger("TR_SharedFamilyDates_MembershipLoss");
            table.HasTrigger("TR_CheckIns_MembershipLoss");
            table.HasTrigger("TR_SharingSessions_MembershipLoss");
            table.HasTrigger("TR_SafetyCircles_MembershipLoss");
            table.HasTrigger("TR_Households_MembershipLoss");
        });
        membership.HasKey(entity => entity.Id);
        membership.HasIndex(entity => new { entity.AccountId, entity.FamilyGroupId }).IsUnique();
        membership.HasIndex(entity => new { entity.FamilyGroupId, entity.IsActive });
        membership.Property(entity => entity.AccessVersion).HasDefaultValue(0);
        membership.Property(entity => entity.EndedAt).HasColumnType("datetimeoffset(7)");
        membership.HasOne<Account>()
            .WithMany()
            .HasForeignKey(entity => entity.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
        membership.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.AdultProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        membership.HasOne<FamilyGroup>()
            .WithMany()
            .HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        var administrator = modelBuilder.Entity<FamilyAdministratorAssignment>();
        administrator.ToTable("FamilyAdministratorAssignments");
        administrator.HasKey(entity => entity.Id);
        administrator.HasIndex(entity => entity.MembershipId).IsUnique();
        administrator.HasOne<FamilyGroupMembership>()
            .WithOne()
            .HasForeignKey<FamilyAdministratorAssignment>(entity => entity.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        var invitation = modelBuilder.Entity<MembershipInvitation>();
        invitation.ToTable("MembershipInvitations");
        invitation.HasKey(entity => entity.Id);
        invitation.Property(entity => entity.RecipientEmail).HasMaxLength(254).IsRequired();
        invitation.Property(entity => entity.NormalizedRecipientEmail).HasMaxLength(254).IsRequired();
        invitation.Property(entity => entity.TokenHash).HasMaxLength(64).IsRequired();
        invitation.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        invitation.Property(entity => entity.RowVersion).IsRowVersion();
        invitation.HasIndex(entity => entity.TokenHash).IsUnique();
        invitation.HasIndex(entity => new { entity.FamilyGroupId, entity.NormalizedRecipientEmail })
            .IsUnique()
            .HasFilter("[Status] = N'Pending'");
        invitation.HasOne<FamilyGroup>()
            .WithMany()
            .HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        invitation.HasOne<FamilyGroupMembership>()
            .WithMany()
            .HasForeignKey(entity => entity.IssuedByMembershipId)
            .OnDelete(DeleteBehavior.Restrict);
        invitation.HasOne<Account>()
            .WithMany()
            .HasForeignKey(entity => entity.AcceptedByAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        var relationship = modelBuilder.Entity<FamilyRelationship>();
        relationship.ToTable("FamilyRelationships");
        relationship.HasKey(entity => entity.Id);
        relationship.Property(entity => entity.ProposerLabel).HasConversion<string>().HasMaxLength(32).IsRequired();
        relationship.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        relationship.HasIndex(entity => new
        {
            entity.FamilyGroupId,
            entity.FirstProfileId,
            entity.SecondProfileId,
        }).IsUnique();
        relationship.HasIndex(entity => new { entity.FamilyGroupId, entity.Status });
        relationship.HasOne<FamilyGroup>()
            .WithMany()
            .HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        relationship.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.ProposerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        relationship.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.RecipientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var familyEvent = modelBuilder.Entity<FamilyEvent>();
        familyEvent.ToTable("FamilyEvents");
        familyEvent.HasKey(entity => entity.Id);
        familyEvent.Property(entity => entity.Name).IsRequired();
        familyEvent.Property(entity => entity.Description).IsRequired();
        familyEvent.Property(entity => entity.Location).IsRequired();
        familyEvent.Property(entity => entity.Date).HasColumnType("date");
        familyEvent.Property(entity => entity.Time).HasColumnType("time");
        familyEvent.HasIndex(entity => new { entity.FamilyGroupId, entity.Date });
        familyEvent.HasOne<FamilyGroup>()
            .WithMany()
            .HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        familyEvent.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.CreatorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var eventAudienceMember = modelBuilder.Entity<FamilyEventAudienceMember>();
        eventAudienceMember.ToTable("FamilyEventAudienceMembers", table => table.HasTrigger("TR_EventNeeds_VisibilityLoss"));
        eventAudienceMember.HasKey(entity => new { entity.EventId, entity.ProfileId });
        eventAudienceMember.Property(entity => entity.CanEdit).HasDefaultValue(false);
        eventAudienceMember.HasOne<FamilyEvent>()
            .WithMany()
            .HasForeignKey(entity => entity.EventId)
            .OnDelete(DeleteBehavior.Restrict);
        eventAudienceMember.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var eventInvitation = modelBuilder.Entity<FamilyEventInvitation>();
        eventInvitation.ToTable("FamilyEventInvitations", table => table.HasCheckConstraint(
            "CK_FamilyEventInvitations_RsvpStatus", "[RsvpStatus] IN ('Pending', 'Going', 'Maybe', 'NotGoing')"));
        eventInvitation.ToTable(table => table.HasTrigger("TR_EventNeeds_InvitationEligibility"));
        eventInvitation.HasKey(entity => entity.Id);
        eventInvitation.Property(entity => entity.RsvpStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        eventInvitation.Property(entity => entity.Version).IsConcurrencyToken();
        eventInvitation.HasIndex(entity => entity.EventId);
        eventInvitation.HasIndex(entity => new { entity.EventId, entity.InviteeProfileId })
            .IsUnique().HasFilter("[RevokedAtUtc] IS NULL");
        eventInvitation.HasIndex(entity => new { entity.InviteeProfileId, entity.RsvpStatus, entity.EventId })
            .HasFilter("[RevokedAtUtc] IS NULL");
        eventInvitation.HasOne<FamilyEvent>().WithMany().HasForeignKey(entity => entity.EventId)
            .OnDelete(DeleteBehavior.Restrict);
        eventInvitation.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.InviteeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var need = modelBuilder.Entity<FamilyEventNeed>();
        need.ToTable("FamilyEventNeeds", table =>
        {
            table.HasCheckConstraint("CK_EventNeeds_Status", "[Status] IN ('Open', 'Claimed', 'Assigned', 'Completed')");
            table.HasCheckConstraint("CK_EventNeeds_Responsibility", "([Status] = 'Open' AND [ResponsibleProfileId] IS NULL) OR ([Status] <> 'Open' AND [ResponsibleProfileId] IS NOT NULL)");
            table.HasCheckConstraint("CK_EventNeeds_Category", "[Category] IS NULL OR [Category] IN ('Food', 'Supplies', 'Help', 'Transport', 'Other')");
        });
        need.HasKey(n => n.Id);
        need.Property(n => n.Title).HasMaxLength(200).IsRequired();
        need.Property(n => n.Detail).HasMaxLength(1000);
        need.Property(n => n.Status).HasConversion<string>().HasMaxLength(16);
        need.Property(n => n.Category).HasConversion<string>().HasMaxLength(16);
        need.Property(n => n.Version).IsConcurrencyToken();
        need.HasIndex(n => new { n.EventId, n.ResponsibleProfileId, n.Status });
        need.HasOne<FamilyEvent>().WithMany().HasForeignKey(n => n.EventId).OnDelete(DeleteBehavior.Restrict);
        need.HasOne<AdultProfile>().WithMany().HasForeignKey(n => n.ResponsibleProfileId).OnDelete(DeleteBehavior.Restrict);

        var change = modelBuilder.Entity<EventNeedResponsibilityChange>();
        change.ToTable("EventNeedResponsibilityChanges");
        change.HasKey(c => c.Id);
        change.Property(c => c.PriorStatus).HasConversion<string>().HasMaxLength(16);
        change.Property(c => c.Status).HasConversion<string>().HasMaxLength(16);
        change.Property(c => c.Reason).HasMaxLength(40).IsRequired();
        change.HasIndex(c => new { c.NeedId, c.OccurredAtUtc });
        change.HasOne<FamilyEventNeed>().WithMany().HasForeignKey(c => c.NeedId).OnDelete(DeleteBehavior.Cascade);
        change.HasOne<AdultProfile>().WithMany().HasForeignKey(c => c.ProfileId).OnDelete(DeleteBehavior.Restrict);

        var update = modelBuilder.Entity<FamilyEventUpdate>();
        update.ToTable("FamilyEventUpdates");
        update.HasKey(u => u.Id);
        update.Property(u => u.Text).HasMaxLength(FamilyEventUpdate.TextLimit).IsRequired();
        update.Property(u => u.Version).IsConcurrencyToken();
        update.HasIndex(u => new { u.EventId, u.HiddenAtUtc, u.CreatedAtUtc });
        update.HasOne<FamilyEvent>().WithMany().HasForeignKey(u => u.EventId).OnDelete(DeleteBehavior.Restrict);
        update.HasOne<AdultProfile>().WithMany().HasForeignKey(u => u.AuthorProfileId).OnDelete(DeleteBehavior.Restrict);

        var revision = modelBuilder.Entity<EventUpdateRevision>();
        revision.ToTable("EventUpdateRevisions");
        revision.HasKey(r => r.Id);
        revision.Property(r => r.Text).HasMaxLength(FamilyEventUpdate.TextLimit).IsRequired();
        revision.HasIndex(r => new { r.UpdateId, r.RevisedAtUtc });
        revision.HasOne<FamilyEventUpdate>().WithMany().HasForeignKey(r => r.UpdateId).OnDelete(DeleteBehavior.Cascade);
        revision.HasOne<AdultProfile>().WithMany().HasForeignKey(r => r.EditorProfileId).OnDelete(DeleteBehavior.Restrict);

        var familyPlan = modelBuilder.Entity<FamilyPlan>();
        familyPlan.ToTable("FamilyPlans");
        familyPlan.HasKey(entity => entity.Id);
        familyPlan.Property(entity => entity.ActivityOrDestination).HasMaxLength(200).IsRequired();
        familyPlan.Property(entity => entity.Date).HasColumnType("date");
        familyPlan.Property(entity => entity.Time).HasColumnType("time");
        familyPlan.Property(entity => entity.ExpectedReturnDate).HasColumnType("date");
        familyPlan.Property(entity => entity.ExpectedReturnTime).HasColumnType("time");
        familyPlan.Property(entity => entity.ExpectedCheckInDate).HasColumnType("date");
        familyPlan.Property(entity => entity.ExpectedCheckInTime).HasColumnType("time");
        familyPlan.Property(entity => entity.AudienceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        familyPlan.Property(entity => entity.Notes).HasMaxLength(1000);
        familyPlan.HasIndex(entity => new { entity.FamilyGroupId, entity.Date });
        familyPlan.HasOne<FamilyGroup>()
            .WithMany()
            .HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        familyPlan.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.CreatorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var planAudienceMember = modelBuilder.Entity<FamilyPlanAudienceMember>();
        planAudienceMember.ToTable("FamilyPlanAudienceMembers");
        planAudienceMember.HasKey(entity => new { entity.PlanId, entity.ProfileId });
        planAudienceMember.HasOne<FamilyPlan>()
            .WithMany()
            .HasForeignKey(entity => entity.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        planAudienceMember.HasOne<AdultProfile>()
            .WithMany()
            .HasForeignKey(entity => entity.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var trip = modelBuilder.Entity<FamilyTrip>();
        trip.ToTable("FamilyTrips");
        trip.HasKey(entity => entity.Id);
        trip.Property(entity => entity.IsCompleted).HasDefaultValue(false);
        trip.Property(entity => entity.Version).IsConcurrencyToken();
        trip.Property(entity => entity.Title).HasMaxLength(100).IsRequired();
        trip.Property(entity => entity.PrimaryDestination).HasMaxLength(200).IsRequired();
        trip.Property(entity => entity.StartDate).HasColumnType("date");
        trip.Property(entity => entity.EndDate).HasColumnType("date");
        trip.Property(entity => entity.Description).HasMaxLength(1000);
        trip.Property(entity => entity.TransportationNotes).HasMaxLength(1000);
        trip.Property(entity => entity.AccommodationNotes).HasMaxLength(1000);
        trip.Property(entity => entity.ExpectedArrivalDate).HasColumnType("date");
        trip.Property(entity => entity.ExpectedArrivalTime).HasColumnType("time");
        trip.Property(entity => entity.ExpectedArrivalTimeZone).HasMaxLength(128);
        trip.Property(entity => entity.AudienceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        trip.HasIndex(entity => new { entity.FamilyGroupId, entity.StartDate });
        trip.HasOne<FamilyGroup>().WithMany().HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        trip.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.CreatorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var traveler = modelBuilder.Entity<FamilyTripTraveler>();
        traveler.ToTable("FamilyTripTravelers");
        traveler.HasKey(entity => new { entity.TripId, entity.ProfileId });
        traveler.HasOne<FamilyTrip>().WithMany().HasForeignKey(entity => entity.TripId)
            .OnDelete(DeleteBehavior.Restrict);
        traveler.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var tripAudience = modelBuilder.Entity<FamilyTripAudienceMember>();
        tripAudience.ToTable("FamilyTripAudienceMembers");
        tripAudience.HasKey(entity => new { entity.TripId, entity.ProfileId });
        tripAudience.HasOne<FamilyTrip>().WithMany().HasForeignKey(entity => entity.TripId)
            .OnDelete(DeleteBehavior.Restrict);
        tripAudience.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var contact = modelBuilder.Entity<FamilyTripContact>();
        contact.ToTable("FamilyTripContacts");
        contact.HasKey(entity => entity.Id);
        contact.Property(entity => entity.Name).HasMaxLength(100).IsRequired();
        contact.Property(entity => entity.RelationshipRole).HasMaxLength(100);
        contact.Property(entity => entity.Phone).HasMaxLength(32);
        contact.Property(entity => entity.Email).HasMaxLength(254);
        contact.HasIndex(entity => entity.TripId);
        contact.HasOne<FamilyTrip>().WithMany().HasForeignKey(entity => entity.TripId)
            .OnDelete(DeleteBehavior.Restrict);

        var note = modelBuilder.Entity<FamilyTripNote>();
        note.ToTable("FamilyTripNotes");
        note.HasKey(entity => entity.Id);
        note.Property(entity => entity.Content).HasMaxLength(1000).IsRequired();
        note.Property(entity => entity.State).HasConversion<string>().HasMaxLength(32).IsRequired();
        note.Property(entity => entity.RowVersion).IsRowVersion();
        note.HasIndex(entity => new { entity.TripId, entity.State, entity.CreatedAt });
        note.HasOne<FamilyTrip>().WithMany().HasForeignKey(entity => entity.TripId)
            .OnDelete(DeleteBehavior.Restrict);
        note.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.AuthorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var concern = modelBuilder.Entity<Concern>();
        concern.ToTable("Concerns");
        concern.HasKey(entity => entity.Id);
        concern.Property(entity => entity.CreationReason).HasMaxLength(1000).IsRequired();
        concern.Property(entity => entity.State).HasConversion<string>().HasMaxLength(24).IsRequired();
        concern.Property(entity => entity.FreezeReason).HasConversion<string>().HasMaxLength(40);
        concern.Property(entity => entity.FrozenAt).HasColumnType("datetimeoffset(7)");
        concern.Property(entity => entity.ResolvedAt).HasColumnType("datetimeoffset(7)");
        concern.Property(entity => entity.CreatorAccessRevokedAt).HasColumnType("datetimeoffset(7)");
        concern.Property(entity => entity.ResolutionNote).HasMaxLength(2000);
        concern.Property(entity => entity.CreatedAt).HasColumnType("datetimeoffset(7)");
        concern.Property(entity => entity.LastAuthorizedMutationAt).HasColumnType("datetimeoffset(7)");
        concern.Property(entity => entity.RowVersion).IsRowVersion();
        concern.HasIndex(entity => new { entity.FamilyGroupId, entity.CreatedAt });
        concern.HasIndex(entity => new { entity.State, entity.FrozenAt, entity.ResolvedAt });
        concern.HasOne<FamilyGroup>().WithMany().HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        concern.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.CreatorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        concern.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.SubjectProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        concern.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(entity => entity.CreatorMembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        var concernGrant = modelBuilder.Entity<ConcernParticipantGrant>();
        concernGrant.ToTable("ConcernParticipantGrants");
        concernGrant.HasKey(entity => entity.Id);
        concernGrant.Property(entity => entity.SelectedAt).HasColumnType("datetimeoffset(7)");
        concernGrant.Property(entity => entity.RemovedAt).HasColumnType("datetimeoffset(7)");
        concernGrant.HasIndex(entity => new { entity.ConcernId, entity.ProfileId })
            .IsUnique().HasFilter("[RemovedAt] IS NULL");
        concernGrant.HasIndex(entity => new { entity.ConcernId, entity.ProfileId, entity.RemovedAt });
        concernGrant.HasIndex(entity => new { entity.AccountId, entity.RemovedAt });
        concernGrant.HasOne<Concern>().WithMany().HasForeignKey(entity => entity.ConcernId)
            .OnDelete(DeleteBehavior.Restrict);
        concernGrant.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        concernGrant.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(entity => entity.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        var concernEntry = modelBuilder.Entity<ConcernEntry>();
        concernEntry.ToTable("ConcernEntries");
        concernEntry.HasKey(entity => entity.Id);
        concernEntry.Property(entity => entity.Kind).HasConversion<string>().HasMaxLength(24).IsRequired();
        concernEntry.Property(entity => entity.Category).HasConversion<string>().HasMaxLength(24).IsRequired();
        concernEntry.Property(entity => entity.Text).HasMaxLength(2000).IsRequired();
        concernEntry.Property(entity => entity.SourceDescription).HasMaxLength(500);
        concernEntry.Property(entity => entity.Explanation).HasMaxLength(1000);
        concernEntry.Property(entity => entity.RecordedAt).HasColumnType("datetimeoffset(7)");
        concernEntry.Property(entity => entity.EventAt).HasColumnType("datetimeoffset(7)");
        concernEntry.HasIndex(entity => new { entity.ConcernId, entity.RecordedAt });
        concernEntry.HasIndex(entity => new { entity.ConcernId, entity.RelatedEntryId });
        concernEntry.HasOne<Concern>().WithMany().HasForeignKey(entity => entity.ConcernId)
            .OnDelete(DeleteBehavior.Restrict);
        concernEntry.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.ContributorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        concernEntry.HasOne<ConcernEntry>().WithMany().HasForeignKey(entity => entity.RelatedEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        var concernAudit = modelBuilder.Entity<ConcernAuditEvent>();
        concernAudit.ToTable("ConcernAuditEvents");
        concernAudit.HasKey(entity => entity.Id);
        concernAudit.Property(entity => entity.Action).HasMaxLength(48).IsRequired();
        concernAudit.Property(entity => entity.OccurredAt).HasColumnType("datetimeoffset(7)");
        concernAudit.Property(entity => entity.UtcHourBucket).HasColumnType("datetimeoffset(7)");
        concernAudit.Property(entity => entity.FirstAttemptAt).HasColumnType("datetimeoffset(7)");
        concernAudit.Property(entity => entity.LastAttemptAt).HasColumnType("datetimeoffset(7)");
        concernAudit.HasIndex(entity => entity.OccurredAt);
        concernAudit.HasIndex(entity => new { entity.ConcernId, entity.OccurredAt });
        concernAudit.HasIndex(entity => new { entity.ActorAccountId, entity.Action, entity.UtcHourBucket })
            .IsUnique().HasFilter("[DeniedAttemptCount] > 0");
        // Deliberately no Concern FK: audit retention is independent of product-history deletion.

        ConfigureIncidents(modelBuilder);
        ConfigureSharedFamilyDates(modelBuilder);
    }

    private static void ConfigureSharedFamilyDates(ModelBuilder modelBuilder)
    {
        var date = modelBuilder.Entity<SharedFamilyDate>();
        date.ToTable("SharedFamilyDates");
        date.HasKey(d => d.Id);
        date.Property(d => d.Title).HasMaxLength(SharedFamilyDate.TitleLimit).IsRequired();
        date.Property(d => d.Description).HasMaxLength(SharedFamilyDate.DescriptionLimit).IsRequired();
        date.Property(d => d.Date).HasColumnType("date");
        date.Property(d => d.Version).IsConcurrencyToken();
        date.HasIndex(d => new { d.FamilyGroupId, d.Date });
        date.HasIndex(d => d.RetainUntilUtc);
        date.HasOne<FamilyGroup>().WithMany().HasForeignKey(d => d.FamilyGroupId).OnDelete(DeleteBehavior.Restrict);
        date.HasOne<AdultProfile>().WithMany().HasForeignKey(d => d.CreatorProfileId).OnDelete(DeleteBehavior.Restrict);
        // Membership identity is an authorization snapshot, not a storage dependency.
        var association = modelBuilder.Entity<SharedFamilyDateAssociation>();
        association.ToTable("SharedFamilyDateAssociations");
        association.HasKey(a => new { a.DateId, a.ProfileId });
        association.HasOne<SharedFamilyDate>().WithMany().HasForeignKey(a => a.DateId).OnDelete(DeleteBehavior.Restrict);
        association.HasOne<AdultProfile>().WithMany().HasForeignKey(a => a.ProfileId).OnDelete(DeleteBehavior.Restrict);
        association.HasIndex(a => a.MembershipId);
    }

    // All Incident relationships are Restrict so whole-aggregate deletion stays explicit and ordered.
    private static void ConfigureIncidents(ModelBuilder modelBuilder)
    {
        var incident = modelBuilder.Entity<SafetyIncident>();
        incident.ToTable("Incidents");
        incident.HasKey(entity => entity.Id);
        incident.Property(entity => entity.State).HasConversion<string>().HasMaxLength(24).IsRequired();
        incident.Property(entity => entity.FreezeReason).HasConversion<string>().HasMaxLength(40);
        incident.Property(entity => entity.OutcomeCategory).HasConversion<string>().HasMaxLength(24);
        incident.Property(entity => entity.CreatedAt).HasColumnType("datetimeoffset(7)");
        incident.Property(entity => entity.LastAuthorizedMutationAt).HasColumnType("datetimeoffset(7)");
        incident.Property(entity => entity.FrozenAt).HasColumnType("datetimeoffset(7)");
        incident.Property(entity => entity.ResolvedAt).HasColumnType("datetimeoffset(7)");
        incident.Property(entity => entity.ContactRestoredAt).HasColumnType("datetimeoffset(7)");
        incident.Property(entity => entity.ResolutionSummary).HasMaxLength(2000);
        incident.Property(entity => entity.OutcomeText).HasMaxLength(2000);
        incident.Property(entity => entity.OutcomeSourceDescription).HasMaxLength(500);
        incident.Property(entity => entity.AssistanceProvided).HasMaxLength(2000);
        incident.Property(entity => entity.UnresolvedFollowUp).HasMaxLength(2000);
        incident.Property(entity => entity.Lessons).HasMaxLength(2000);
        incident.Property(entity => entity.RowVersion).IsRowVersion();
        incident.HasIndex(entity => new { entity.FamilyGroupId, entity.CreatedAt });
        incident.HasIndex(entity => new { entity.State, entity.LastAuthorizedMutationAt });
        incident.HasIndex(entity => new { entity.State, entity.FrozenAt, entity.ResolvedAt });
        incident.HasOne<FamilyGroup>().WithMany().HasForeignKey(entity => entity.FamilyGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        incident.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.CreatedByProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        incident.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.SubjectProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        // Deliberately no FK for CoordinatorGrantId (avoids an Incident <-> grant cycle) or for
        // OriginatingConcernId (non-authorizing lineage; Concern expiry must neither cascade nor block).

        var grant = modelBuilder.Entity<IncidentParticipantGrant>();
        grant.ToTable("IncidentParticipantGrants");
        grant.HasKey(entity => entity.Id);
        grant.Property(entity => entity.SelectedAt).HasColumnType("datetimeoffset(7)");
        grant.Property(entity => entity.RemovedAt).HasColumnType("datetimeoffset(7)");
        grant.HasIndex(entity => new { entity.IncidentId, entity.ProfileId })
            .IsUnique().HasFilter("[RemovedAt] IS NULL");
        grant.HasIndex(entity => new { entity.AccountId, entity.RemovedAt });
        grant.HasOne<SafetyIncident>().WithMany().HasForeignKey(entity => entity.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);
        grant.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.ProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        grant.HasOne<FamilyGroupMembership>().WithMany().HasForeignKey(entity => entity.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        var coordinatorChange = modelBuilder.Entity<IncidentCoordinatorChange>();
        coordinatorChange.ToTable("IncidentCoordinatorChanges");
        coordinatorChange.HasKey(entity => entity.Id);
        coordinatorChange.Property(entity => entity.ChangedAt).HasColumnType("datetimeoffset(7)");
        coordinatorChange.HasIndex(entity => new { entity.IncidentId, entity.ChangedAt });
        coordinatorChange.HasOne<SafetyIncident>().WithMany().HasForeignKey(entity => entity.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);

        var task = modelBuilder.Entity<IncidentTask>();
        task.ToTable("IncidentTasks");
        task.HasKey(entity => entity.Id);
        task.Property(entity => entity.Description).HasMaxLength(500).IsRequired();
        task.Property(entity => entity.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        task.Property(entity => entity.CancelReason).HasMaxLength(1000);
        task.Property(entity => entity.CreatedAt).HasColumnType("datetimeoffset(7)");
        task.Property(entity => entity.CompletedAt).HasColumnType("datetimeoffset(7)");
        task.Property(entity => entity.CancelledAt).HasColumnType("datetimeoffset(7)");
        task.Property(entity => entity.RowVersion).IsRowVersion();
        task.HasIndex(entity => new { entity.IncidentId, entity.CreatedAt });
        task.HasOne<SafetyIncident>().WithMany().HasForeignKey(entity => entity.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);
        task.HasOne<IncidentParticipantGrant>().WithMany().HasForeignKey(entity => entity.AssigneeGrantId)
            .OnDelete(DeleteBehavior.Restrict);
        task.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.AssigneeProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        var entry = modelBuilder.Entity<IncidentEntry>();
        entry.ToTable("IncidentEntries");
        entry.HasKey(entity => entity.Id);
        entry.Property(entity => entity.Kind).HasConversion<string>().HasMaxLength(24).IsRequired();
        entry.Property(entity => entity.Category).HasConversion<string>().HasMaxLength(24).IsRequired();
        entry.Property(entity => entity.Text).HasMaxLength(2000);
        entry.Property(entity => entity.SourceDescription).HasMaxLength(500);
        entry.Property(entity => entity.Explanation).HasMaxLength(1000);
        entry.Property(entity => entity.AttemptedByDescription).HasMaxLength(200);
        entry.Property(entity => entity.TargetLabel).HasMaxLength(200);
        entry.Property(entity => entity.ContactMethod).HasConversion<string>().HasMaxLength(24);
        entry.Property(entity => entity.ContactOutcome).HasConversion<string>().HasMaxLength(24);
        entry.Property(entity => entity.RecordedAt).HasColumnType("datetimeoffset(7)");
        entry.Property(entity => entity.EventAt).HasColumnType("datetimeoffset(7)");
        entry.HasIndex(entity => new { entity.IncidentId, entity.RecordedAt });
        entry.HasIndex(entity => new { entity.IncidentId, entity.RelatedEntryId });
        entry.HasOne<SafetyIncident>().WithMany().HasForeignKey(entity => entity.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);
        entry.HasOne<AdultProfile>().WithMany().HasForeignKey(entity => entity.ContributorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        entry.HasOne<IncidentEntry>().WithMany().HasForeignKey(entity => entity.RelatedEntryId)
            .OnDelete(DeleteBehavior.Restrict);
        entry.HasOne<IncidentTask>().WithMany().HasForeignKey(entity => entity.RelatedTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        var audit = modelBuilder.Entity<IncidentAuditEvent>();
        audit.ToTable("IncidentAuditEvents");
        audit.HasKey(entity => entity.Id);
        audit.Property(entity => entity.Action).HasMaxLength(48).IsRequired();
        audit.Property(entity => entity.OccurredAt).HasColumnType("datetimeoffset(7)");
        audit.Property(entity => entity.UtcHourBucket).HasColumnType("datetimeoffset(7)");
        audit.Property(entity => entity.FirstAttemptAt).HasColumnType("datetimeoffset(7)");
        audit.Property(entity => entity.LastAttemptAt).HasColumnType("datetimeoffset(7)");
        audit.HasIndex(entity => entity.OccurredAt);
        audit.HasIndex(entity => new { entity.IncidentId, entity.OccurredAt });
        audit.HasIndex(entity => new { entity.ActorAccountId, entity.Action, entity.UtcHourBucket })
            .IsUnique().HasFilter("[DeniedAttemptCount] > 0");
        // Deliberately no Incident FK: audit retention is independent of product-history deletion.
    }
}
