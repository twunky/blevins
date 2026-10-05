using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SharedFamilyDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedFamilyDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorMembershipVersion = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UnavailableAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RetainUntilUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedFamilyDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedFamilyDates_AdultProfiles_CreatorProfileId",
                        column: x => x.CreatorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedFamilyDates_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharedFamilyDateAssociations",
                columns: table => new
                {
                    DateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedFamilyDateAssociations", x => new { x.DateId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_SharedFamilyDateAssociations_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedFamilyDateAssociations_SharedFamilyDates_DateId",
                        column: x => x.DateId,
                        principalTable: "SharedFamilyDates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedFamilyDateAssociations_MembershipId",
                table: "SharedFamilyDateAssociations",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedFamilyDateAssociations_ProfileId",
                table: "SharedFamilyDateAssociations",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedFamilyDates_CreatorProfileId",
                table: "SharedFamilyDates",
                column: "CreatorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedFamilyDates_FamilyGroupId_Date",
                table: "SharedFamilyDates",
                columns: new[] { "FamilyGroupId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_SharedFamilyDates_RetainUntilUtc",
                table: "SharedFamilyDates",
                column: "RetainUntilUtc");

            // Same membership-loss trigger convention as Event Needs. Capture first loss even if
            // membership is subsequently reactivated before any application read/maintenance.
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_SharedFamilyDates_MembershipLoss ON FamilyGroupMemberships
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @lost TABLE (MembershipId uniqueidentifier PRIMARY KEY, LostAt datetimeoffset);
                    INSERT @lost
                    SELECT d.Id, CASE WHEN i.EndedAt IS NOT NULL AND (d.EndedAt IS NULL OR i.EndedAt <> d.EndedAt)
                        THEN i.EndedAt ELSE CONVERT(datetimeoffset, SYSUTCDATETIME()) END
                    FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                    WHERE d.IsActive = 1 AND (i.Id IS NULL OR i.IsActive = 0
                        OR i.AccessVersion <> d.AccessVersion OR i.FamilyGroupId <> d.FamilyGroupId
                        OR i.AdultProfileId <> d.AdultProfileId OR i.AccountId <> d.AccountId);

                    -- Domain End already advances AccessVersion. Normalize direct SQL membership
                    -- endings too, so restoring old date rows/grants cannot revive the association.
                    UPDATE m SET AccessVersion = d.AccessVersion + 1,
                        EndedAt = l.LostAt
                    FROM FamilyGroupMemberships m JOIN deleted d ON d.Id = m.Id
                    JOIN inserted i ON i.Id = d.Id JOIN @lost l ON l.MembershipId = d.Id
                    WHERE i.AccessVersion = d.AccessVersion AND d.IsActive = 1 AND i.IsActive = 0;

                    UPDATE date SET UnavailableAtUtc = losses.LostAt,
                        RetainUntilUtc = DATEADD(day, 30, losses.LostAt), Version = NEWID()
                    FROM SharedFamilyDates date
                    CROSS APPLY (SELECT MIN(l.LostAt) LostAt FROM @lost l
                        WHERE l.MembershipId = date.CreatorMembershipId
                            OR EXISTS (SELECT 1 FROM SharedFamilyDateAssociations a
                                WHERE a.DateId = date.Id AND a.MembershipId = l.MembershipId)) losses
                    WHERE date.UnavailableAtUtc IS NULL AND losses.LostAt IS NOT NULL;

                    DELETE a FROM SharedFamilyDateAssociations a
                    JOIN @lost l ON l.MembershipId = a.MembershipId;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER TR_SharedFamilyDates_MembershipLoss;");
            migrationBuilder.DropTable(
                name: "SharedFamilyDateAssociations");

            migrationBuilder.DropTable(
                name: "SharedFamilyDates");
        }
    }
}
