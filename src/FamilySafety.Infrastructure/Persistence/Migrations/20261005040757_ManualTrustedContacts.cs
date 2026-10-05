using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ManualTrustedContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustedContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    NormalizedPhone = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true, collation: "Latin1_General_100_BIN2"),
                    EmailAddress = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true, collation: "Latin1_General_100_BIN2"),
                    RelationshipLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedContacts", x => x.Id);
                    table.CheckConstraint("CK_TrustedContacts_ContactMethod", "([PhoneNumber] IS NOT NULL AND [NormalizedPhone] IS NOT NULL) OR ([EmailAddress] IS NOT NULL AND [NormalizedEmail] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_TrustedContacts_AdultProfiles_OwnerProfileId",
                        column: x => x.OwnerProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrustedContacts_OwnerProfileId_NormalizedEmail",
                table: "TrustedContacts",
                columns: new[] { "OwnerProfileId", "NormalizedEmail" },
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedContacts_OwnerProfileId_NormalizedPhone",
                table: "TrustedContacts",
                columns: new[] { "OwnerProfileId", "NormalizedPhone" },
                unique: true,
                filter: "[NormalizedPhone] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustedContacts");
        }
    }
}
