using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SafetyIncidentDomainAndAccessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    Authorized = table.Column<bool>(type: "bit", nullable: false),
                    AffectedProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UtcHourBucket = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    FirstAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    DeniedAttemptCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoordinatorGrantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginatingConcernId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    LastAuthorizedMutationAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    FrozenAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    FreezeReason = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    ResolvedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OutcomeText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OutcomeCategory = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    OutcomeSourceDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OutcomeSourceUnknown = table.Column<bool>(type: "bit", nullable: false),
                    ContactRestoredAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    ContactRestoredTimeUnknown = table.Column<bool>(type: "bit", nullable: false),
                    AssistanceProvided = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UnresolvedFollowUp = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Lessons = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_AdultProfiles_CreatedByProfileId",
                        column: x => x.CreatedByProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_AdultProfiles_SubjectProfileId",
                        column: x => x.SubjectProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCoordinatorChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromGrantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToGrantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCoordinatorChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentCoordinatorChanges_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentParticipantGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipAccessVersion = table.Column<int>(type: "int", nullable: false),
                    SelectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    AddedByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RemovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    RemovedByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EligibilityRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentParticipantGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentParticipantGrants_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentParticipantGrants_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentParticipantGrants_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AssigneeAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeGrantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    CancelledByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledByProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentTasks_AdultProfiles_AssigneeProfileId",
                        column: x => x.AssigneeProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentTasks_IncidentParticipantGrants_AssigneeGrantId",
                        column: x => x.AssigneeGrantId,
                        principalTable: "IncidentParticipantGrants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentTasks_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContributorAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContributorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    IsOutdated = table.Column<bool>(type: "bit", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceUnknown = table.Column<bool>(type: "bit", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    EventAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    EventTimeUnknown = table.Column<bool>(type: "bit", nullable: false),
                    RelatedEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttemptedByDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactMethod = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
                    ContactOutcome = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentEntries_AdultProfiles_ContributorProfileId",
                        column: x => x.ContributorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentEntries_IncidentEntries_RelatedEntryId",
                        column: x => x.RelatedEntryId,
                        principalTable: "IncidentEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentEntries_IncidentTasks_RelatedTaskId",
                        column: x => x.RelatedTaskId,
                        principalTable: "IncidentTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentEntries_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAuditEvents_ActorAccountId_Action_UtcHourBucket",
                table: "IncidentAuditEvents",
                columns: new[] { "ActorAccountId", "Action", "UtcHourBucket" },
                unique: true,
                filter: "[DeniedAttemptCount] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAuditEvents_IncidentId_OccurredAt",
                table: "IncidentAuditEvents",
                columns: new[] { "IncidentId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAuditEvents_OccurredAt",
                table: "IncidentAuditEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCoordinatorChanges_IncidentId_ChangedAt",
                table: "IncidentCoordinatorChanges",
                columns: new[] { "IncidentId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEntries_ContributorProfileId",
                table: "IncidentEntries",
                column: "ContributorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEntries_IncidentId_RecordedAt",
                table: "IncidentEntries",
                columns: new[] { "IncidentId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEntries_IncidentId_RelatedEntryId",
                table: "IncidentEntries",
                columns: new[] { "IncidentId", "RelatedEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEntries_RelatedEntryId",
                table: "IncidentEntries",
                column: "RelatedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEntries_RelatedTaskId",
                table: "IncidentEntries",
                column: "RelatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipantGrants_AccountId_RemovedAt",
                table: "IncidentParticipantGrants",
                columns: new[] { "AccountId", "RemovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipantGrants_IncidentId_ProfileId",
                table: "IncidentParticipantGrants",
                columns: new[] { "IncidentId", "ProfileId" },
                unique: true,
                filter: "[RemovedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipantGrants_MembershipId",
                table: "IncidentParticipantGrants",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipantGrants_ProfileId",
                table: "IncidentParticipantGrants",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedByProfileId",
                table: "Incidents",
                column: "CreatedByProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_FamilyGroupId_CreatedAt",
                table: "Incidents",
                columns: new[] { "FamilyGroupId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_State_FrozenAt_ResolvedAt",
                table: "Incidents",
                columns: new[] { "State", "FrozenAt", "ResolvedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_State_LastAuthorizedMutationAt",
                table: "Incidents",
                columns: new[] { "State", "LastAuthorizedMutationAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_SubjectProfileId",
                table: "Incidents",
                column: "SubjectProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTasks_AssigneeGrantId",
                table: "IncidentTasks",
                column: "AssigneeGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTasks_AssigneeProfileId",
                table: "IncidentTasks",
                column: "AssigneeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTasks_IncidentId_CreatedAt",
                table: "IncidentTasks",
                columns: new[] { "IncidentId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentAuditEvents");

            migrationBuilder.DropTable(
                name: "IncidentCoordinatorChanges");

            migrationBuilder.DropTable(
                name: "IncidentEntries");

            migrationBuilder.DropTable(
                name: "IncidentTasks");

            migrationBuilder.DropTable(
                name: "IncidentParticipantGrants");

            migrationBuilder.DropTable(
                name: "Incidents");
        }
    }
}
