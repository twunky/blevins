using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventNeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyEventNeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ResponsibleProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEventNeeds", x => x.Id);
                    table.CheckConstraint("CK_EventNeeds_Category", "[Category] IS NULL OR [Category] IN ('Food', 'Supplies', 'Help', 'Transport', 'Other')");
                    table.CheckConstraint("CK_EventNeeds_Responsibility", "([Status] = 'Open' AND [ResponsibleProfileId] IS NULL) OR ([Status] <> 'Open' AND [ResponsibleProfileId] IS NOT NULL)");
                    table.CheckConstraint("CK_EventNeeds_Status", "[Status] IN ('Open', 'Claimed', 'Assigned', 'Completed')");
                    table.ForeignKey(
                        name: "FK_FamilyEventNeeds_AdultProfiles_ResponsibleProfileId",
                        column: x => x.ResponsibleProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyEventNeeds_FamilyEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "FamilyEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventNeedResponsibilityChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NeedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorStatus = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventNeedResponsibilityChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventNeedResponsibilityChanges_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventNeedResponsibilityChanges_FamilyEventNeeds_NeedId",
                        column: x => x.NeedId,
                        principalTable: "FamilyEventNeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventNeedResponsibilityChanges_NeedId_OccurredAtUtc",
                table: "EventNeedResponsibilityChanges",
                columns: new[] { "NeedId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EventNeedResponsibilityChanges_ProfileId",
                table: "EventNeedResponsibilityChanges",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventNeeds_EventId_ResponsibleProfileId_Status",
                table: "FamilyEventNeeds",
                columns: new[] { "EventId", "ResponsibleProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventNeeds_ResponsibleProfileId",
                table: "FamilyEventNeeds",
                column: "ResponsibleProfileId");

            // These triggers are local to Need responsibility. Loss and provenance
            // commit with the eligibility change, including multi-row SQL changes.
            // They never acquire a Family lock after an Event lock. Application
            // participation/Need mutations acquire Family before protected reads.
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_EventNeeds_InvitationEligibility ON FamilyEventInvitations
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @changes TABLE (NeedId uniqueidentifier, ProfileId uniqueidentifier,
                        PriorStatus nvarchar(16), Reason nvarchar(40), OccurredAtUtc datetimeoffset);
                    UPDATE n SET Status = 'Open', ResponsibleProfileId = NULL,
                        UpdatedAtUtc = COALESCE(i.RevokedAtUtc, i.RsvpUpdatedAtUtc, SYSUTCDATETIME()), Version = NEWID()
                    OUTPUT deleted.Id, deleted.ResponsibleProfileId, deleted.Status,
                        CASE WHEN i.Id IS NULL OR i.RevokedAtUtc IS NOT NULL THEN 'InvitationRevoked' ELSE 'RsvpNotGoing' END,
                        inserted.UpdatedAtUtc INTO @changes
                    FROM FamilyEventNeeds n
                    JOIN deleted d ON d.EventId = n.EventId AND d.InviteeProfileId = n.ResponsibleProfileId
                    LEFT JOIN inserted i ON i.Id = d.Id
                    WHERE n.Status IN ('Claimed', 'Assigned') AND d.RevokedAtUtc IS NULL
                        AND (i.Id IS NULL OR i.RevokedAtUtc IS NOT NULL
                            OR (d.RsvpStatus <> 'NotGoing' AND i.RsvpStatus = 'NotGoing'));
                    INSERT EventNeedResponsibilityChanges (Id, NeedId, ProfileId, PriorStatus, Status, Reason, OccurredAtUtc)
                        SELECT NEWID(), NeedId, ProfileId, PriorStatus, 'Open', Reason, OccurredAtUtc FROM @changes;
                END
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_EventNeeds_VisibilityLoss ON FamilyEventAudienceMembers
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @changes TABLE (NeedId uniqueidentifier, ProfileId uniqueidentifier,
                        PriorStatus nvarchar(16), OccurredAtUtc datetimeoffset);
                    UPDATE n SET Status = 'Open', ResponsibleProfileId = NULL,
                        UpdatedAtUtc = SYSUTCDATETIME(), Version = NEWID()
                    OUTPUT deleted.Id, deleted.ResponsibleProfileId, deleted.Status, inserted.UpdatedAtUtc INTO @changes
                    FROM FamilyEventNeeds n
                    JOIN deleted d ON d.EventId = n.EventId AND d.ProfileId = n.ResponsibleProfileId
                    WHERE n.Status IN ('Claimed', 'Assigned')
                        AND NOT EXISTS (SELECT 1 FROM inserted i WHERE i.EventId = d.EventId AND i.ProfileId = d.ProfileId);
                    INSERT EventNeedResponsibilityChanges (Id, NeedId, ProfileId, PriorStatus, Status, Reason, OccurredAtUtc)
                        SELECT NEWID(), NeedId, ProfileId, PriorStatus, 'Open', 'EventVisibilityLost', OccurredAtUtc FROM @changes;
                END
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_EventNeeds_MembershipLoss ON FamilyGroupMemberships
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @changes TABLE (NeedId uniqueidentifier, ProfileId uniqueidentifier,
                        PriorStatus nvarchar(16), OccurredAtUtc datetimeoffset);
                    UPDATE n SET Status = 'Open', ResponsibleProfileId = NULL,
                        UpdatedAtUtc = SYSUTCDATETIME(), Version = NEWID()
                    OUTPUT deleted.Id, deleted.ResponsibleProfileId, deleted.Status, inserted.UpdatedAtUtc INTO @changes
                    FROM FamilyEventNeeds n
                    JOIN FamilyEvents e ON e.Id = n.EventId
                    JOIN deleted d ON d.FamilyGroupId = e.FamilyGroupId AND d.AdultProfileId = n.ResponsibleProfileId
                    WHERE n.Status IN ('Claimed', 'Assigned') AND d.IsActive = 1
                        AND NOT EXISTS (SELECT 1 FROM inserted i WHERE i.FamilyGroupId = d.FamilyGroupId
                            AND i.AdultProfileId = d.AdultProfileId AND i.IsActive = 1);
                    INSERT EventNeedResponsibilityChanges (Id, NeedId, ProfileId, PriorStatus, Status, Reason, OccurredAtUtc)
                        SELECT NEWID(), NeedId, ProfileId, PriorStatus, 'Open', 'MembershipLost', OccurredAtUtc FROM @changes;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER TR_EventNeeds_InvitationEligibility;");
            migrationBuilder.Sql("DROP TRIGGER TR_EventNeeds_VisibilityLoss;");
            migrationBuilder.Sql("DROP TRIGGER TR_EventNeeds_MembershipLoss;");
            migrationBuilder.DropTable(
                name: "EventNeedResponsibilityChanges");

            migrationBuilder.DropTable(
                name: "FamilyEventNeeds");
        }
    }
}
