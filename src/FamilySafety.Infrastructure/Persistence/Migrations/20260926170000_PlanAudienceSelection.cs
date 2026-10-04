using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    public partial class PlanAudienceSelection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityOrDestination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    ExpectedReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpectedReturnTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ExpectedCheckInDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpectedCheckInTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    AudienceType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyPlans_AdultProfiles_CreatorProfileId",
                        column: x => x.CreatorProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyPlans_FamilyGroups_FamilyGroupId",
                        column: x => x.FamilyGroupId,
                        principalTable: "FamilyGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyPlanAudienceMembers",
                columns: table => new
                {
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPlanAudienceMembers", x => new { x.PlanId, x.ProfileId });
                    table.ForeignKey(
                        name: "FK_FamilyPlanAudienceMembers_AdultProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AdultProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyPlanAudienceMembers_FamilyPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "FamilyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyPlanAudienceMembers_ProfileId",
                table: "FamilyPlanAudienceMembers",
                column: "ProfileId");
            migrationBuilder.CreateIndex(
                name: "IX_FamilyPlans_CreatorProfileId",
                table: "FamilyPlans",
                column: "CreatorProfileId");
            migrationBuilder.CreateIndex(
                name: "IX_FamilyPlans_FamilyGroupId_Date",
                table: "FamilyPlans",
                columns: new[] { "FamilyGroupId", "Date" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FamilyPlanAudienceMembers");
            migrationBuilder.DropTable(name: "FamilyPlans");
        }
    }
}
