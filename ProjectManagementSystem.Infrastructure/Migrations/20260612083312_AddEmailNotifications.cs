using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailEnabled",
                table: "SystemConfig",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmailFromAddress",
                table: "SystemConfig",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "SystemConfig",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "SystemConfig",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "SystemConfig",
                type: "int",
                nullable: false,
                defaultValue: 587);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "SystemConfig",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AtRiskNotifiedAt",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimesheetReminderStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    WeekStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReminderCount = table.Column<int>(type: "int", nullable: false),
                    LastReminderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsFrozen = table.Column<bool>(type: "bit", nullable: false),
                    FreezeNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredByManagerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetReminderStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimesheetReminderStates_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetReminderStates_ResourceId_WeekStartDate",
                table: "TimesheetReminderStates",
                columns: new[] { "ResourceId", "WeekStartDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimesheetReminderStates");

            migrationBuilder.DropColumn(
                name: "EmailEnabled",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "EmailFromAddress",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "SystemConfig");

            migrationBuilder.DropColumn(
                name: "AtRiskNotifiedAt",
                table: "Projects");
        }
    }
}
