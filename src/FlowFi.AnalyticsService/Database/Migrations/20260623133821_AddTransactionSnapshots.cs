using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.AnalyticsService.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transaction_snapshots",
                columns: table => new
                {
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tag_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_snapshots", x => x.transaction_id);
                    table.CheckConstraint("chk_transaction_snapshots_amount", "amount >= 0");
                    table.CheckConstraint("chk_transaction_snapshots_type", "type IN ('INCOME', 'EXPENSE')");
                });

            migrationBuilder.CreateIndex(
                name: "idx_transaction_snapshots_deleted_at",
                table: "transaction_snapshots",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_transaction_snapshots_user_date",
                table: "transaction_snapshots",
                columns: new[] { "user_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "idx_transaction_snapshots_user_tag_date",
                table: "transaction_snapshots",
                columns: new[] { "user_id", "tag_id", "transaction_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_snapshots");
        }
    }
}
