using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandaloneCheckIns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorMembershipVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.CheckConstraint("CK_CheckIns_Status", "[Status] IN ('ImOkay','Arrived','LeavingNow','Delayed','PlansChanged')");
                    table.ForeignKey(
                        name: "FK_CheckIns_AdultProfiles_AuthorProfileId",
                        column: x => x.AuthorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_FamilyGroupMemberships_AuthorMembershipId",
                        column: x => x.AuthorMembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckInRecipients",
                columns: table => new
                {
                    CheckInId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipVersion = table.Column<int>(type: "int", nullable: false),
                    IsInvalidated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInRecipients", x => new { x.CheckInId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_CheckInRecipients_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckInRecipients_CheckIns_CheckInId",
                        column: x => x.CheckInId,
                        principalTable: "CheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckInRecipients_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInRecipients_MembershipId",
                table: "CheckInRecipients",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInRecipients_ProfileId_IsInvalidated",
                table: "CheckInRecipients",
                columns: new[] { "ProfileId", "IsInvalidated" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_AuthorMembershipId",
                table: "CheckIns",
                column: "AuthorMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_AuthorProfileId_CreatedAtUtc",
                table: "CheckIns",
                columns: new[] { "AuthorProfileId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_FamilyGroupId",
                table: "CheckIns",
                column: "FamilyGroupId");

            migrationBuilder.Sql("""
                CREATE TRIGGER TR_CheckIns_MembershipLoss ON FamilyGroupMemberships
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE r SET IsInvalidated = 1
                    FROM CheckInRecipients r
                    JOIN CheckIns c ON c.Id = r.CheckInId
                    WHERE r.IsInvalidated = 0 AND EXISTS (
                        SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE (d.Id = c.AuthorMembershipId OR d.Id = r.MembershipId)
                          AND d.IsActive = 1
                          AND (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> d.AccessVersion
                            OR i.AdultProfileId <> d.AdultProfileId OR i.AccountId <> d.AccountId
                            OR i.FamilyGroupId <> d.FamilyGroupId)
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_CheckIns_MembershipLoss");
            migrationBuilder.DropTable(
                name: "CheckInRecipients");

            migrationBuilder.DropTable(
                name: "CheckIns");
        }
    }
}
