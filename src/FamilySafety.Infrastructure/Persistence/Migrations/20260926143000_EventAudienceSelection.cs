using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventAudienceSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyEvents_AdultProfiles_CreatorProfileId",
                        column: x => x.CreatorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyEvents_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyEventAudienceMembers",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEventAudienceMembers", x => new { x.EventId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_FamilyEventAudienceMembers_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyEventAudienceMembers_FamilyEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "FamilyEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventAudienceMembers_ProfileId",
                table: "FamilyEventAudienceMembers",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEvents_CreatorProfileId",
                table: "FamilyEvents",
                column: "CreatorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEvents_FamilyGroupId_Date",
                table: "FamilyEvents",
                columns: new[] { "FamilyGroupId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FamilyEventAudienceMembers");
            migrationBuilder.DropTable(name: "FamilyEvents");
        }
    }
}
