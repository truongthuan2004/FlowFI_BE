using FlowFi.AuthUserService.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.AuthUserService.Migrations;

[DbContext(typeof(AuthDbContext))]
[Migration("20260623160000_FixAuthUserForeignKeys")]
public partial class FixAuthUserForeignKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_password_reset_tokens_password_reset_tokens_user_id",
            table: "password_reset_tokens");

        migrationBuilder.DropForeignKey(
            name: "fk_refresh_tokens_refresh_tokens_user_id",
            table: "refresh_tokens");

        migrationBuilder.DropForeignKey(
            name: "fk_user_devices_user_devices_user_id",
            table: "user_devices");

        migrationBuilder.DropForeignKey(
            name: "fk_user_logs_user_logs_user_id",
            table: "user_logs");

        migrationBuilder.AddForeignKey(
            name: "fk_password_reset_tokens_users_user_id",
            table: "password_reset_tokens",
            column: "user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_refresh_tokens_users_user_id",
            table: "refresh_tokens",
            column: "user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_devices_users_user_id",
            table: "user_devices",
            column: "user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_logs_users_user_id",
            table: "user_logs",
            column: "user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_password_reset_tokens_users_user_id",
            table: "password_reset_tokens");

        migrationBuilder.DropForeignKey(
            name: "fk_refresh_tokens_users_user_id",
            table: "refresh_tokens");

        migrationBuilder.DropForeignKey(
            name: "fk_user_devices_users_user_id",
            table: "user_devices");

        migrationBuilder.DropForeignKey(
            name: "fk_user_logs_users_user_id",
            table: "user_logs");

        migrationBuilder.AddForeignKey(
            name: "fk_password_reset_tokens_password_reset_tokens_user_id",
            table: "password_reset_tokens",
            column: "user_id",
            principalTable: "password_reset_tokens",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_refresh_tokens_refresh_tokens_user_id",
            table: "refresh_tokens",
            column: "user_id",
            principalTable: "refresh_tokens",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_devices_user_devices_user_id",
            table: "user_devices",
            column: "user_id",
            principalTable: "user_devices",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_user_logs_user_logs_user_id",
            table: "user_logs",
            column: "user_id",
            principalTable: "user_logs",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
