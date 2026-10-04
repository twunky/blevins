using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventInvitationsAndRsvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyEventInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InviteeProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RsvpStatus = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    RsvpUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEventInvitations", x => x.Id);
                    table.CheckConstraint("CK_FamilyEventInvitations_RsvpStatus", "[RsvpStatus] IN ('Pending', 'Going', 'Maybe', 'NotGoing')");
                    table.ForeignKey(
                        name: "FK_FamilyEventInvitations_AdultProfiles_InviteeProfileId",
                        column: x => x.InviteeProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyEventInvitations_FamilyEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "FamilyEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventInvitations_EventId",
                table: "FamilyEventInvitations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventInvitations_EventId_InviteeProfileId",
                table: "FamilyEventInvitations",
                columns: new[] { "EventId", "InviteeProfileId" },
                unique: true,
                filter: "[RevokedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventInvitations_InviteeProfileId_RsvpStatus_EventId",
                table: "FamilyEventInvitations",
                columns: new[] { "InviteeProfileId", "RsvpStatus", "EventId" },
                filter: "[RevokedAtUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyEventInvitations");
        }
    }
}
