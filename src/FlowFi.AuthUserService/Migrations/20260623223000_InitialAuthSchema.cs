using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.AuthUserService.Migrations;

public partial class InitialAuthSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: true),
                full_name = table.Column<string>(type: "text", nullable: true),
                avatar_url = table.Column<string>(type: "text", nullable: true),
                date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                currency_code = table.Column<string>(type: "text", nullable: false),
                monthly_budget_limit = table.Column<decimal>(type: "numeric", nullable: true),
                auth_provider = table.Column<string>(type: "text", nullable: false),
                is_verified = table.Column<bool>(type: "boolean", nullable: false),
                role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "User"),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "password_reset_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token = table.Column<string>(type: "text", nullable: false),
                otp_code = table.Column<string>(type: "text", nullable: true),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_used = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_password_reset_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_password_reset_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token = table.Column<string>(type: "text", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_devices",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                device_id = table.Column<string>(type: "text", nullable: false),
                device_name = table.Column<string>(type: "text", nullable: true),
                platform = table.Column<string>(type: "text", nullable: false),
                push_token = table.Column<string>(type: "text", nullable: true),
                last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_devices", x => x.id);
                table.ForeignKey(
                    name: "fk_user_devices_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_logs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                ip_address = table.Column<string>(type: "text", nullable: true),
                user_agent = table.Column<string>(type: "text", nullable: true),
                failure_reason = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_logs", x => x.id);
                table.ForeignKey(
                    name: "fk_user_logs_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_password_reset_tokens_user_id",
            table: "password_reset_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id",
            table: "refresh_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_devices_user_id",
            table: "user_devices",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_logs_user_id",
            table: "user_logs",
            column: "user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "password_reset_tokens");
        migrationBuilder.DropTable(name: "refresh_tokens");
        migrationBuilder.DropTable(name: "user_devices");
        migrationBuilder.DropTable(name: "user_logs");
        migrationBuilder.DropTable(name: "users");
    }
}
