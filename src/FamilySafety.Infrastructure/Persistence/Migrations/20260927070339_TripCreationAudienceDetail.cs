using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TripCreationAudienceDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessVersion",
                table: "FamilyGroupMemberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FamilyTrips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AudienceType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimaryDestination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TransportationNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AccommodationNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpectedArrivalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpectedArrivalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ExpectedArrivalTimeZone = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyTrips_AdultProfiles_CreatorProfileId",
                        column: x => x.CreatorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyTrips_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyTripAudienceMembers",
                columns: table => new
                {
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipAccessVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTripAudienceMembers", x => new { x.TripId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_FamilyTripAudienceMembers_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyTripAudienceMembers_FamilyTrips_TripId",
                        column: x => x.TripId,
                        principalTable: "FamilyTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyTripContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RelationshipRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTripContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyTripContacts_FamilyTrips_TripId",
                        column: x => x.TripId,
                        principalTable: "FamilyTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyTripTravelers",
                columns: table => new
                {
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyTripTravelers", x => new { x.TripId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_FamilyTripTravelers_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyTripTravelers_FamilyTrips_TripId",
                        column: x => x.TripId,
                        principalTable: "FamilyTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTripAudienceMembers_ProfileId",
                table: "FamilyTripAudienceMembers",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTripContacts_TripId",
                table: "FamilyTripContacts",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTrips_CreatorProfileId",
                table: "FamilyTrips",
                column: "CreatorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTrips_FamilyGroupId_StartDate",
                table: "FamilyTrips",
                columns: new[] { "FamilyGroupId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyTripTravelers_ProfileId",
                table: "FamilyTripTravelers",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyTripAudienceMembers");

            migrationBuilder.DropTable(
                name: "FamilyTripContacts");

            migrationBuilder.DropTable(
                name: "FamilyTripTravelers");

            migrationBuilder.DropTable(
                name: "FamilyTrips");

            migrationBuilder.DropColumn(
                name: "AccessVersion",
                table: "FamilyGroupMemberships");
        }
    }
}
