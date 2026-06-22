using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceForcePasswordChangeWithPasswordExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Users
                SET PasswordExpiresAt = CASE
                    WHEN IsForcePasswordChange = 1 THEN GETUTCDATE()
                    ELSE DATEADD(month, 3, GETUTCDATE())
                END
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PasswordExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "IsForcePasswordChange",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForcePasswordChange",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE Users
                SET IsForcePasswordChange = CASE
                    WHEN PasswordExpiresAt <= GETUTCDATE() THEN 1
                    ELSE 0
                END
                """);

            migrationBuilder.DropColumn(
                name: "PasswordExpiresAt",
                table: "Users");
        }
    }
}
