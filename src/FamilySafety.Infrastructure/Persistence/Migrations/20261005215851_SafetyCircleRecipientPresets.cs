using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SafetyCircleRecipientPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SafetyCircles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerMembershipVersion = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_100_BIN2"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyCircles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyCircles_AdultProfiles_OwnerProfileId",
                        column: x => x.OwnerProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafetyCircles_FamilyGroupMemberships_OwnerMembershipId",
                        column: x => x.OwnerMembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafetyCircles_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SafetyCircleMembers",
                columns: table => new
                {
                    SafetyCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyCircleMembers", x => new { x.SafetyCircleId, x.MemberProfileId });
                    table.ForeignKey(
                        name: "FK_SafetyCircleMembers_AdultProfiles_MemberProfileId",
                        column: x => x.MemberProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafetyCircleMembers_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SafetyCircleMembers_SafetyCircles_SafetyCircleId",
                        column: x => x.SafetyCircleId,
                        principalTable: "SafetyCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SafetyCircleMembers_MemberProfileId",
                table: "SafetyCircleMembers",
                column: "MemberProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyCircleMembers_MembershipId",
                table: "SafetyCircleMembers",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyCircles_FamilyGroupId",
                table: "SafetyCircles",
                column: "FamilyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyCircles_OwnerMembershipId",
                table: "SafetyCircles",
                column: "OwnerMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyCircles_OwnerProfileId_FamilyGroupId_NormalizedName",
                table: "SafetyCircles",
                columns: new[] { "OwnerProfileId", "FamilyGroupId", "NormalizedName" },
                unique: true);
            // Delete presets only. Materialized target grants remain independent.
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_SafetyCircles_MembershipLoss ON FamilyGroupMemberships
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    DELETE c FROM SafetyCircles c WHERE EXISTS (
                        SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE d.Id = c.OwnerMembershipId AND
                          (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> c.OwnerMembershipVersion
                            OR i.AdultProfileId <> c.OwnerProfileId OR i.AccountId <> d.AccountId
                            OR i.FamilyGroupId <> c.FamilyGroupId)
                    );
                    UPDATE c SET Version = NEWID(), UpdatedAtUtc = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')
                    FROM SafetyCircles c WHERE EXISTS (
                        SELECT 1 FROM SafetyCircleMembers r JOIN deleted d ON d.Id = r.MembershipId
                        LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE r.SafetyCircleId = c.Id AND
                          (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> r.MembershipVersion
                            OR i.AdultProfileId <> r.MemberProfileId OR i.AccountId <> d.AccountId
                            OR i.FamilyGroupId <> c.FamilyGroupId)
                    );
                    DELETE r FROM SafetyCircleMembers r JOIN SafetyCircles c ON c.Id = r.SafetyCircleId
                    WHERE EXISTS (
                        SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE d.Id = r.MembershipId AND
                          (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> r.MembershipVersion
                            OR i.AdultProfileId <> r.MemberProfileId OR i.AccountId <> d.AccountId
                            OR i.FamilyGroupId <> c.FamilyGroupId)
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_SafetyCircles_MembershipLoss");
            migrationBuilder.DropTable(
                name: "SafetyCircleMembers");

            migrationBuilder.DropTable(
                name: "SafetyCircles");
        }
    }
}
