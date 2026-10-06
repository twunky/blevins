using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HouseholdGrouping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyHouseholds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_100_BIN2"),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyHouseholds", x => x.Id);
                    table.UniqueConstraint("AK_FamilyHouseholds_Id_FamilyGroupId", x => new { x.Id, x.FamilyGroupId });
                    table.ForeignKey(
                        name: "FK_FamilyHouseholds_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdAssignments",
                columns: table => new
                {
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdAssignments", x => new { x.FamilyGroupId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_HouseholdAssignments_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HouseholdAssignments_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HouseholdAssignments_FamilyHouseholds_HouseholdId_FamilyGroupId",
                        columns: x => new { x.HouseholdId, x.FamilyGroupId },
                        principalTable: "FamilyHouseholds",
                        principalColumns: new[] { "Id", "FamilyGroupId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyHouseholds_FamilyGroupId_NormalizedName",
                table: "FamilyHouseholds",
                columns: new[] { "FamilyGroupId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdAssignments_HouseholdId_FamilyGroupId",
                table: "HouseholdAssignments",
                columns: new[] { "HouseholdId", "FamilyGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdAssignments_MembershipId",
                table: "HouseholdAssignments",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdAssignments_ProfileId",
                table: "HouseholdAssignments",
                column: "ProfileId");

            // Defense in depth for writes outside the bounded API: exact current adult membership.
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_HouseholdAssignments_Eligibility ON HouseholdAssignments
                AFTER INSERT, UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1 FROM inserted a WHERE NOT EXISTS (
                            SELECT 1 FROM FamilyGroupMemberships m JOIN AdultProfiles p
                              ON p.Id = m.AdultProfileId AND p.AccountId = m.AccountId
                            WHERE m.Id = a.MembershipId AND m.FamilyGroupId = a.FamilyGroupId
                              AND m.AdultProfileId = a.ProfileId AND m.AccessVersion = a.MembershipVersion AND m.IsActive = 1
                        )
                    ) THROW 51010, 'Household assignment requires current same-Family membership.', 1;
                END
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER TR_Households_MembershipLoss ON FamilyGroupMemberships
                AFTER UPDATE, DELETE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE h SET Version = NEWID(), UpdatedAtUtc = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')
                    FROM FamilyHouseholds h WHERE EXISTS (
                        SELECT 1 FROM HouseholdAssignments a JOIN deleted d ON d.Id = a.MembershipId
                        LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE a.HouseholdId = h.Id AND
                          (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> a.MembershipVersion
                           OR i.AdultProfileId <> a.ProfileId OR i.AccountId <> d.AccountId OR i.FamilyGroupId <> a.FamilyGroupId)
                    );
                    DELETE a FROM HouseholdAssignments a WHERE EXISTS (
                        SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.Id = d.Id
                        WHERE d.Id = a.MembershipId AND
                          (i.Id IS NULL OR i.IsActive = 0 OR i.AccessVersion <> a.MembershipVersion
                           OR i.AdultProfileId <> a.ProfileId OR i.AccountId <> d.AccountId OR i.FamilyGroupId <> a.FamilyGroupId)
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Households_MembershipLoss");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_HouseholdAssignments_Eligibility");
            migrationBuilder.DropTable(
                name: "HouseholdAssignments");

            migrationBuilder.DropTable(
                name: "FamilyHouseholds");
        }
    }
}
