using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyEventUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    HiddenAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyEventUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyEventUpdates_AdultProfiles_AuthorProfileId",
                        column: x => x.AuthorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyEventUpdates_FamilyEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "FamilyEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventUpdateRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TextAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RevisedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventUpdateRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventUpdateRevisions_AdultProfiles_EditorProfileId",
                        column: x => x.EditorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventUpdateRevisions_FamilyEventUpdates_UpdateId",
                        column: x => x.UpdateId,
                        principalTable: "FamilyEventUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventUpdateRevisions_EditorProfileId",
                table: "EventUpdateRevisions",
                column: "EditorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EventUpdateRevisions_UpdateId_RevisedAtUtc",
                table: "EventUpdateRevisions",
                columns: new[] { "UpdateId", "RevisedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventUpdates_AuthorProfileId",
                table: "FamilyEventUpdates",
                column: "AuthorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyEventUpdates_EventId_HiddenAtUtc_CreatedAtUtc",
                table: "FamilyEventUpdates",
                columns: new[] { "EventId", "HiddenAtUtc", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventUpdateRevisions");

            migrationBuilder.DropTable(
                name: "FamilyEventUpdates");
        }
    }
}
