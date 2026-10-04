using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdultMembershipInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FamilyGroupMemberships_FamilyGroupId",
                table: "FamilyGroupMemberships");

            migrationBuilder.AddColumn<int>(
                name: "CapacityLimit",
                table: "FamilyGroups",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FamilyGroups",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Family Group");

            migrationBuilder.CreateTable(
                name: "MembershipInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedByMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    NormalizedRecipientEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TerminalAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AcceptedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipInvitations_Accounts_AcceptedByAccountId",
                        column: x => x.AcceptedByAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipInvitations_FamilyGroupMemberships_IssuedByMembershipId",
                        column: x => x.IssuedByMembershipId,
                        principalTable: "FamilyGroupMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipInvitations_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyGroupMemberships_FamilyGroupId_IsActive",
                table: "FamilyGroupMemberships",
                columns: new[] { "FamilyGroupId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_AcceptedByAccountId",
                table: "MembershipInvitations",
                column: "AcceptedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_FamilyGroupId_NormalizedRecipientEmail",
                table: "MembershipInvitations",
                columns: new[] { "FamilyGroupId", "NormalizedRecipientEmail" },
                unique: true,
                filter: "[Status] = N'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_IssuedByMembershipId",
                table: "MembershipInvitations",
                column: "IssuedByMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_TokenHash",
                table: "MembershipInvitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipInvitations");

            migrationBuilder.DropIndex(
                name: "IX_FamilyGroupMemberships_FamilyGroupId_IsActive",
                table: "FamilyGroupMemberships");

            migrationBuilder.DropColumn(
                name: "CapacityLimit",
                table: "FamilyGroups");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "FamilyGroups");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyGroupMemberships_FamilyGroupId",
                table: "FamilyGroupMemberships",
                column: "FamilyGroupId");
        }
    }
}
