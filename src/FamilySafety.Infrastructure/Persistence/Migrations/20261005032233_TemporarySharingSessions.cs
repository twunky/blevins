using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TemporarySharingSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorMembershipVersion = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StoppedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingSessions", x => x.Id);
                    table.CheckConstraint("CK_SharingSessions_Duration", "[EndsAtUtc] > [StartedAtUtc] AND [EndsAtUtc] <= DATEADD(hour, 24, [StartedAtUtc])");
                    table.ForeignKey(
                        name: "FK_SharingSessions_AdultProfiles_AuthorProfileId",
                        column: x => x.AuthorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingSessions_FamilyGroupMemberships_AuthorMembershipId",
                        column: x => x.AuthorMembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingSessions_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingCurrentLocations",
                columns: table => new
                {
                    SharingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Accuracy = table.Column<double>(type: "float", nullable: false),
                    CapturedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingCurrentLocations", x => x.SharingSessionId);
                    table.CheckConstraint("CK_SharingCurrentLocations_Coordinates", "[Latitude] >= -90 AND [Latitude] <= 90 AND [Longitude] >= -180 AND [Longitude] <= 180 AND [Accuracy] >= 0");
                    table.ForeignKey(
                        name: "FK_SharingCurrentLocations_SharingSessions_SharingSessionId",
                        column: x => x.SharingSessionId,
                        principalTable: "SharingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharingSessionRecipients",
                columns: table => new
                {
                    SharingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipVersion = table.Column<int>(type: "int", nullable: false),
                    IsInvalidated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingSessionRecipients", x => new { x.SharingSessionId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_SharingSessionRecipients_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingSessionRecipients_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingSessionRecipients_SharingSessions_SharingSessionId",
                        column: x => x.SharingSessionId,
                        principalTable: "SharingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharingSessionRecipients_MembershipId",
                table: "SharingSessionRecipients",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingSessionRecipients_ProfileId_IsInvalidated",
                table: "SharingSessionRecipients",
                columns: new[] { "ProfileId", "IsInvalidated" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingSessions_AuthorMembershipId",
                table: "SharingSessions",
                column: "AuthorMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingSessions_AuthorProfileId_EndsAtUtc",
                table: "SharingSessions",
                columns: new[] { "AuthorProfileId", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingSessions_FamilyGroupId",
                table: "SharingSessions",
                column: "FamilyGroupId");
            // Permanent stale-grant invalidation also covers operational off/on toggles without AccessVersion changes.
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_SharingSessions_MembershipLoss ON FamilyGroupMemberships
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE r SET IsInvalidated = 1
                    FROM SharingSessionRecipients r JOIN SharingSessions s ON s.Id = r.SharingSessionId
                    WHERE r.IsInvalidated = 0 AND EXISTS (
                        SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE (d.Id = s.AuthorMembershipId OR d.Id = r.MembershipId) AND d.IsActive = 1
                          AND (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> d.AccessVersion
                            OR i.AdultProfileId <> d.AdultProfileId OR i.AccountId <> d.AccountId
                            OR i.FamilyGroupId <> d.FamilyGroupId)
                    );
                    UPDATE s SET StoppedAtUtc = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'), Version = Version + 1
                    FROM SharingSessions s
                    WHERE s.StoppedAtUtc IS NULL AND EXISTS (
                        SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE d.Id = s.AuthorMembershipId AND d.IsActive = 1
                          AND (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> d.AccessVersion
                            OR i.AdultProfileId <> d.AdultProfileId OR i.AccountId <> d.AccountId
                            OR i.FamilyGroupId <> d.FamilyGroupId)
                    );
                    DELETE l FROM SharingCurrentLocations l JOIN SharingSessions s ON s.Id = l.SharingSessionId
                    WHERE s.StoppedAtUtc IS NOT NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_SharingSessions_MembershipLoss");
            migrationBuilder.DropTable(
                name: "SharingCurrentLocations");

            migrationBuilder.DropTable(
                name: "SharingSessionRecipients");

            migrationBuilder.DropTable(
                name: "SharingSessions");
        }
    }
}
