using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilySafety.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BoundConcernDeniedAuditSummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeniedAttemptCount",
                table: "ConcernAuditEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstAttemptAt",
                table: "ConcernAuditEvents",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAttemptAt",
                table: "ConcernAuditEvents",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UtcHourBucket",
                table: "ConcernAuditEvents",
                type: "datetimeoffset(7)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConcernAuditEvents_ActorAccountId_Action_UtcHourBucket",
                table: "ConcernAuditEvents",
                columns: new[] { "ActorAccountId", "Action", "UtcHourBucket" },
                unique: true,
                filter: "[DeniedAttemptCount] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConcernAuditEvents_ActorAccountId_Action_UtcHourBucket",
                table: "ConcernAuditEvents");

            migrationBuilder.DropColumn(
                name: "DeniedAttemptCount",
                table: "ConcernAuditEvents");

            migrationBuilder.DropColumn(
                name: "FirstAttemptAt",
                table: "ConcernAuditEvents");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "ConcernAuditEvents");

            migrationBuilder.DropColumn(
                name: "UtcHourBucket",
                table: "ConcernAuditEvents");
        }
    }
}
