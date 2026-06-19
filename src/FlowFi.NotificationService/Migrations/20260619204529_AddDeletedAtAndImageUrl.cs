using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtAndImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_primary",
                table: "push_devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "push_devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "enable_goal_reminder",
                table: "notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enable_saving_streak",
                table: "notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "notification_settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "quiet_hours_enabled",
                table: "notification_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "device_sync_states",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "device_sync_states",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_push_devices_platform",
                table: "push_devices",
                column: "platform");

            migrationBuilder.CreateIndex(
                name: "ix_push_devices_token",
                table: "push_devices",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "ix_push_devices_user_id_device_fingerprint",
                table: "push_devices",
                columns: new[] { "user_id", "device_fingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_notification_type",
                table: "notifications",
                column: "notification_type");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_settings_user_id",
                table: "notification_settings",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_sync_states_user_id_device_fingerprint",
                table: "device_sync_states",
                columns: new[] { "user_id", "device_fingerprint" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_push_devices_platform",
                table: "push_devices");

            migrationBuilder.DropIndex(
                name: "ix_push_devices_token",
                table: "push_devices");

            migrationBuilder.DropIndex(
                name: "ix_push_devices_user_id_device_fingerprint",
                table: "push_devices");

            migrationBuilder.DropIndex(
                name: "ix_notifications_created_at",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_notification_type",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_id_is_read",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notification_settings_user_id",
                table: "notification_settings");

            migrationBuilder.DropIndex(
                name: "ix_device_sync_states_user_id_device_fingerprint",
                table: "device_sync_states");

            migrationBuilder.DropColumn(
                name: "is_primary",
                table: "push_devices");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "push_devices");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "enable_goal_reminder",
                table: "notification_settings");

            migrationBuilder.DropColumn(
                name: "enable_saving_streak",
                table: "notification_settings");

            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "notification_settings");

            migrationBuilder.DropColumn(
                name: "quiet_hours_enabled",
                table: "notification_settings");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "device_sync_states");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "device_sync_states");
        }
    }
}
