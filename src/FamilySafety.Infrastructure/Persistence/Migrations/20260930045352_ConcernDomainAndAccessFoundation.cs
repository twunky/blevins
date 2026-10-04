using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConcernDomainAndAccessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndedAt",
                table: "FamilyGroupMemberships",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConcernAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcernId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    Authorized = table.Column<bool>(type: "bit", nullable: false),
                    AffectedParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcernAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Concerns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorMembershipAccessVersion = table.Column<int>(type: "int", nullable: false),
                    SubjectProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    LastAuthorizedMutationAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    FrozenAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    FreezeReason = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    CreatorAccessRevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    ResolvedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concerns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concerns_AdultProfiles_CreatorProfileId",
                        column: x => x.CreatorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Concerns_AdultProfiles_SubjectProfileId",
                        column: x => x.SubjectProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Concerns_FamilyGroupMemberships_CreatorMembershipId",
                        column: x => x.CreatorMembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Concerns_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConcernEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcernId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContributorAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContributorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourceDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceUnknown = table.Column<bool>(type: "bit", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    EventAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    RelatedEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsOutdated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcernEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConcernEntries_AdultProfiles_ContributorProfileId",
                        column: x => x.ContributorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConcernEntries_ConcernEntries_RelatedEntryId",
                        column: x => x.RelatedEntryId,
                        principalTable: "ConcernEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConcernEntries_Concerns_ConcernId",
                        column: x => x.ConcernId,
                        principalTable: "Concerns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConcernParticipantGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcernId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipAccessVersion = table.Column<int>(type: "int", nullable: false),
                    SelectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false),
                    RemovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    EligibilityRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcernParticipantGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConcernParticipantGrants_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConcernParticipantGrants_Concerns_ConcernId",
                        column: x => x.ConcernId,
                        principalTable: "Concerns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConcernParticipantGrants_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConcernAuditEvents_ConcernId_OccurredAt",
                table: "ConcernAuditEvents",
                columns: new[] { "ConcernId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConcernAuditEvents_OccurredAt",
                table: "ConcernAuditEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConcernEntries_ConcernId_RecordedAt",
                table: "ConcernEntries",
                columns: new[] { "ConcernId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConcernEntries_ConcernId_RelatedEntryId",
                table: "ConcernEntries",
                columns: new[] { "ConcernId", "RelatedEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConcernEntries_ContributorProfileId",
                table: "ConcernEntries",
                column: "ContributorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcernEntries_RelatedEntryId",
                table: "ConcernEntries",
                column: "RelatedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcernParticipantGrants_AccountId_RemovedAt",
                table: "ConcernParticipantGrants",
                columns: new[] { "AccountId", "RemovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConcernParticipantGrants_ConcernId_ProfileId",
                table: "ConcernParticipantGrants",
                columns: new[] { "ConcernId", "ProfileId" },
                unique: true,
                filter: "[RemovedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConcernParticipantGrants_ConcernId_ProfileId_RemovedAt",
                table: "ConcernParticipantGrants",
                columns: new[] { "ConcernId", "ProfileId", "RemovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConcernParticipantGrants_MembershipId",
                table: "ConcernParticipantGrants",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcernParticipantGrants_ProfileId",
                table: "ConcernParticipantGrants",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Concerns_CreatorMembershipId",
                table: "Concerns",
                column: "CreatorMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Concerns_CreatorProfileId",
                table: "Concerns",
                column: "CreatorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Concerns_FamilyGroupId_CreatedAt",
                table: "Concerns",
                columns: new[] { "FamilyGroupId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Concerns_State_FrozenAt_ResolvedAt",
                table: "Concerns",
                columns: new[] { "State", "FrozenAt", "ResolvedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Concerns_SubjectProfileId",
                table: "Concerns",
                column: "SubjectProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcernAuditEvents");

            migrationBuilder.DropTable(
                name: "ConcernEntries");

            migrationBuilder.DropTable(
                name: "ConcernParticipantGrants");

            migrationBuilder.DropTable(
                name: "Concerns");

            migrationBuilder.DropColumn(
                name: "EndedAt",
                table: "FamilyGroupMemberships");
        }
    }
}
