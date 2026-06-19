using FlowFi.AuthUserService.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.AuthUserService.Migrations;

[DbContext(typeof(AuthDbContext))]
[Migration("20260620050000_NormalizeAuthTimestampsToUtc")]
public sealed class NormalizeAuthTimestampsToUtc : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        AlterToTimestampWithTimeZone(migrationBuilder, "users", "created_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "users", "updated_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "users", "deleted_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "refresh_tokens", "expires_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "refresh_tokens", "created_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "password_reset_tokens", "expires_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "password_reset_tokens", "created_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "user_devices", "last_synced_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "user_devices", "created_at");
        AlterToTimestampWithTimeZone(migrationBuilder, "user_logs", "created_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        AlterToTimestampWithoutTimeZone(migrationBuilder, "users", "created_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "users", "updated_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "users", "deleted_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "refresh_tokens", "expires_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "refresh_tokens", "created_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "password_reset_tokens", "expires_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "password_reset_tokens", "created_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "user_devices", "last_synced_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "user_devices", "created_at");
        AlterToTimestampWithoutTimeZone(migrationBuilder, "user_logs", "created_at");
    }

    private static void AlterToTimestampWithTimeZone(
        MigrationBuilder migrationBuilder,
        string table,
        string column)
    {
        migrationBuilder.Sql($"""
            ALTER TABLE {table}
            ALTER COLUMN {column} TYPE timestamp with time zone
            USING {column} AT TIME ZONE 'UTC';
            """);
    }

    private static void AlterToTimestampWithoutTimeZone(
        MigrationBuilder migrationBuilder,
        string table,
        string column)
    {
        migrationBuilder.Sql($"""
            ALTER TABLE {table}
            ALTER COLUMN {column} TYPE timestamp without time zone
            USING {column} AT TIME ZONE 'UTC';
            """);
    }
}
