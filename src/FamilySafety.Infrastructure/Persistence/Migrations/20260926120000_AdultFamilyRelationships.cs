using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdultFamilyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecondProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposerLabel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyRelationships_AdultProfiles_ProposerProfileId",
                        column: x => x.ProposerProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyRelationships_AdultProfiles_RecipientProfileId",
                        column: x => x.RecipientProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyRelationships_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationships_FamilyGroupId_FirstProfileId_SecondProfileId",
                table: "FamilyRelationships",
                columns: new[] { "FamilyGroupId", "FirstProfileId", "SecondProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationships_FamilyGroupId_Status",
                table: "FamilyRelationships",
                columns: new[] { "FamilyGroupId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationships_ProposerProfileId",
                table: "FamilyRelationships",
                column: "ProposerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationships_RecipientProfileId",
                table: "FamilyRelationships",
                column: "RecipientProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FamilyRelationships");
        }
    }
}
