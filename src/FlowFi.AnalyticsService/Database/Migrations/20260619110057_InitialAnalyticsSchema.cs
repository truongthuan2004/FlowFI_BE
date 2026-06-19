using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.AnalyticsService.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialAnalyticsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS pgcrypto;""");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_updated_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at = NOW();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.CreateTable(
                name: "budgets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tag_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    period_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Monthly"),
                    budget_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    warning_threshold_percent = table.Column<int>(type: "integer", nullable: false, defaultValue: 80),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_budgets", x => x.id);
                    table.CheckConstraint("chk_budgets_amount", "budget_amount > 0");
                    table.CheckConstraint("chk_budgets_date_range", "end_date >= start_date");
                    table.CheckConstraint("chk_budgets_period_type", "period_type IN ('Weekly', 'Monthly')");
                    table.CheckConstraint("chk_budgets_status", "status IN ('Active', 'Completed', 'Cancelled', 'Expired')");
                    table.CheckConstraint("chk_budgets_warning_threshold", "warning_threshold_percent BETWEEN 1 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "financial_summaries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: true),
                    month = table.Column<int>(type: "integer", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: false),
                    total_income = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    total_expense = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    net_saving = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    total_budget = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    used_budget = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    budget_usage_percent = table.Column<decimal>(type: "numeric(7,2)", nullable: false, defaultValue: 0m),
                    transaction_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    active_goal_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    achieved_goal_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_financial_summaries", x => x.id);
                    table.CheckConstraint("chk_financial_summaries_amounts", "total_income >= 0 AND total_expense >= 0 AND total_budget >= 0 AND used_budget >= 0 AND budget_usage_percent >= 0");
                    table.CheckConstraint("chk_financial_summaries_counts", "transaction_count >= 0 AND active_goal_count >= 0 AND achieved_goal_count >= 0");
                    table.CheckConstraint("chk_financial_summaries_date_range", "period_end_date >= period_start_date");
                    table.CheckConstraint("chk_financial_summaries_month", "month IS NULL OR month BETWEEN 1 AND 12");
                    table.CheckConstraint("chk_financial_summaries_period_type", "period_type IN ('Weekly', 'Monthly')");
                    table.CheckConstraint("chk_financial_summaries_week_number", "week_number IS NULL OR week_number BETWEEN 1 AND 53");
                    table.CheckConstraint("chk_financial_summaries_weekly_monthly", "((period_type = 'Weekly' AND week_number IS NOT NULL AND month IS NULL) OR (period_type = 'Monthly' AND month IS NOT NULL AND week_number IS NULL))");
                    table.CheckConstraint("chk_financial_summaries_year", "year >= 2000");
                });

            migrationBuilder.CreateTable(
                name: "saving_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    target_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    priority_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    achieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saving_goals", x => x.id);
                    table.CheckConstraint("chk_saving_goals_current_amount", "current_amount >= 0");
                    table.CheckConstraint("chk_saving_goals_priority_level", "priority_level IN ('Low', 'Medium', 'High')");
                    table.CheckConstraint("chk_saving_goals_status", "status IN ('Active', 'Achieved', 'Cancelled')");
                    table.CheckConstraint("chk_saving_goals_target_amount", "target_amount > 0");
                });

            migrationBuilder.CreateTable(
                name: "budget_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    spent_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    remaining_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    usage_percent = table.Column<decimal>(type: "numeric(7,2)", nullable: false, defaultValue: 0m),
                    transaction_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    exceeded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_calculated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_budget_progresses", x => x.id);
                    table.CheckConstraint("chk_budget_progresses_spent_amount", "spent_amount >= 0");
                    table.CheckConstraint("chk_budget_progresses_transaction_count", "transaction_count >= 0");
                    table.CheckConstraint("chk_budget_progresses_usage_percent", "usage_percent >= 0");
                    table.ForeignKey(
                        name: "fk_budget_progresses_budget",
                        column: x => x.budget_id,
                        principalTable: "budgets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "goal_contributions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    contribution_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    source_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Manual"),
                    source_reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goal_contributions", x => x.id);
                    table.CheckConstraint("chk_goal_contributions_amount", "amount > 0");
                    table.CheckConstraint("chk_goal_contributions_source_type", "source_type IN ('Manual', 'AutoAllocation', 'RoundUp', 'FromTransaction')");
                    table.ForeignKey(
                        name: "fk_goal_contributions_goal",
                        column: x => x.goal_id,
                        principalTable: "saving_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_budget_progresses_budget_id",
                table: "budget_progresses",
                column: "budget_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_budgets_user_date",
                table: "budgets",
                columns: new[] { "user_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "idx_budgets_user_period",
                table: "budgets",
                columns: new[] { "user_id", "period_type" });

            migrationBuilder.CreateIndex(
                name: "idx_budgets_user_status",
                table: "budgets",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_budgets_user_tag_date",
                table: "budgets",
                columns: new[] { "user_id", "tag_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "idx_financial_summaries_user_year_month",
                table: "financial_summaries",
                columns: new[] { "user_id", "year", "month" });

            migrationBuilder.CreateIndex(
                name: "idx_financial_summaries_user_year_week",
                table: "financial_summaries",
                columns: new[] { "user_id", "year", "week_number" });

            migrationBuilder.CreateIndex(
                name: "uq_financial_summaries_user_period",
                table: "financial_summaries",
                columns: new[] { "user_id", "period_type", "period_start_date", "period_end_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_goal_contributions_goal_date",
                table: "goal_contributions",
                columns: new[] { "goal_id", "contribution_date" });

            migrationBuilder.CreateIndex(
                name: "idx_saving_goals_user_status",
                table: "saving_goals",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_saving_goals_user_target_date",
                table: "saving_goals",
                columns: new[] { "user_id", "target_date" });

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_budgets_set_updated_at
                BEFORE UPDATE ON budgets
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_budget_progresses_set_updated_at
                BEFORE UPDATE ON budget_progresses
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_saving_goals_set_updated_at
                BEFORE UPDATE ON saving_goals
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_goal_contributions_set_updated_at
                BEFORE UPDATE ON goal_contributions
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_goal_contributions_set_updated_at ON goal_contributions;""");
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_saving_goals_set_updated_at ON saving_goals;""");
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_budget_progresses_set_updated_at ON budget_progresses;""");
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_budgets_set_updated_at ON budgets;""");
            migrationBuilder.Sql("""DROP FUNCTION IF EXISTS set_updated_at();""");

            migrationBuilder.DropTable(
                name: "budget_progresses");

            migrationBuilder.DropTable(
                name: "financial_summaries");

            migrationBuilder.DropTable(
                name: "goal_contributions");

            migrationBuilder.DropTable(
                name: "budgets");

            migrationBuilder.DropTable(
                name: "saving_goals");
        }
    }
}
