using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAdultProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignInIdentifier = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdultProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    ContactEmailVisibility = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ContactPhoneVisibility = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BirthdayMonth = table.Column<byte>(type: "tinyint", nullable: true),
                    BirthdayDay = table.Column<byte>(type: "tinyint", nullable: true),
                    BirthdayVisibility = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdultProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdultProfiles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyGroupMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdultProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyGroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyGroupMemberships_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyGroupMemberships_AdultProfiles_AdultProfileId",
                        column: x => x.AdultProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyGroupMemberships_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyAdministratorAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyAdministratorAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyAdministratorAssignments_FamilyGroupMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SignInIdentifier",
                table: "Accounts",
                column: "SignInIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdultProfiles_AccountId",
                table: "AdultProfiles",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyAdministratorAssignments_MembershipId",
                table: "FamilyAdministratorAssignments",
                column: "MembershipId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyGroupMemberships_AccountId_FamilyGroupId",
                table: "FamilyGroupMemberships",
                columns: new[] { "AccountId", "FamilyGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyGroupMemberships_AdultProfileId",
                table: "FamilyGroupMemberships",
                column: "AdultProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyGroupMemberships_FamilyGroupId",
                table: "FamilyGroupMemberships",
                column: "FamilyGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyAdministratorAssignments");

            migrationBuilder.DropTable(
                name: "FamilyGroupMemberships");

            migrationBuilder.DropTable(
                name: "AdultProfiles");

            migrationBuilder.DropTable(
                name: "FamilyGroups");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
