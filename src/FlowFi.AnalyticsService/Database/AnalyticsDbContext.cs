using FlowFi.AnalyticsService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AnalyticsService.Database;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetProgress> BudgetProgresses => Set<BudgetProgress>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();
    public DbSet<GoalContribution> GoalContributions => Set<GoalContribution>();
    public DbSet<FinancialSummary> FinancialSummaries => Set<FinancialSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        ConfigureBudget(modelBuilder);
        ConfigureBudgetProgress(modelBuilder);
        ConfigureSavingGoal(modelBuilder);
        ConfigureGoalContribution(modelBuilder);
        ConfigureFinancialSummary(modelBuilder);
    }

    private static void ConfigureBudget(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("budgets", table =>
            {
                table.HasCheckConstraint("chk_budgets_period_type", "period_type IN ('Weekly', 'Monthly')");
                table.HasCheckConstraint("chk_budgets_amount", "budget_amount > 0");
                table.HasCheckConstraint("chk_budgets_warning_threshold", "warning_threshold_percent BETWEEN 1 AND 100");
                table.HasCheckConstraint("chk_budgets_date_range", "end_date >= start_date");
                table.HasCheckConstraint("chk_budgets_status", "status IN ('Active', 'Completed', 'Cancelled', 'Expired')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.TagName).HasMaxLength(100);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PeriodType).HasMaxLength(20).HasDefaultValue("Monthly").IsRequired();
            entity.Property(x => x.BudgetAmount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.WarningThresholdPercent).HasDefaultValue(80);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("VND").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(x => new { x.UserId, x.StartDate, x.EndDate })
                .HasDatabaseName("idx_budgets_user_date");
            entity.HasIndex(x => new { x.UserId, x.TagId, x.StartDate, x.EndDate })
                .HasDatabaseName("idx_budgets_user_tag_date");
            entity.HasIndex(x => new { x.UserId, x.PeriodType })
                .HasDatabaseName("idx_budgets_user_period");
            entity.HasIndex(x => new { x.UserId, x.Status })
                .HasDatabaseName("idx_budgets_user_status");
        });
    }

    private static void ConfigureBudgetProgress(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BudgetProgress>(entity =>
        {
            entity.ToTable("budget_progresses", table =>
            {
                table.HasCheckConstraint("chk_budget_progresses_spent_amount", "spent_amount >= 0");
                table.HasCheckConstraint("chk_budget_progresses_usage_percent", "usage_percent >= 0");
                table.HasCheckConstraint("chk_budget_progresses_transaction_count", "transaction_count >= 0");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.SpentAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.RemainingAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.UsagePercent).HasColumnType("numeric(7,2)").HasDefaultValue(0m);
            entity.Property(x => x.TransactionCount).HasDefaultValue(0);
            entity.Property(x => x.LastCalculatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(x => x.BudgetId)
                .IsUnique()
                .HasDatabaseName("idx_budget_progresses_budget_id");

            entity.HasOne(x => x.Budget)
                .WithOne(x => x.Progress)
                .HasForeignKey<BudgetProgress>(x => x.BudgetId)
                .HasConstraintName("fk_budget_progresses_budget")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSavingGoal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavingGoal>(entity =>
        {
            entity.ToTable("saving_goals", table =>
            {
                table.HasCheckConstraint("chk_saving_goals_target_amount", "target_amount > 0");
                table.HasCheckConstraint("chk_saving_goals_current_amount", "current_amount >= 0");
                table.HasCheckConstraint("chk_saving_goals_priority_level", "priority_level IN ('Low', 'Medium', 'High')");
                table.HasCheckConstraint("chk_saving_goals_status", "status IN ('Active', 'Achieved', 'Cancelled')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.TargetAmount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.CurrentAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("VND").IsRequired();
            entity.Property(x => x.PriorityLevel).HasMaxLength(20).HasDefaultValue("Medium");
            entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(x => new { x.UserId, x.Status })
                .HasDatabaseName("idx_saving_goals_user_status");
            entity.HasIndex(x => new { x.UserId, x.TargetDate })
                .HasDatabaseName("idx_saving_goals_user_target_date");
        });
    }

    private static void ConfigureGoalContribution(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GoalContribution>(entity =>
        {
            entity.ToTable("goal_contributions", table =>
            {
                table.HasCheckConstraint("chk_goal_contributions_amount", "amount > 0");
                table.HasCheckConstraint("chk_goal_contributions_source_type", "source_type IN ('Manual', 'AutoAllocation', 'RoundUp', 'FromTransaction')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.ContributionDate).HasDefaultValueSql("NOW()");
            entity.Property(x => x.SourceType).HasMaxLength(30).HasDefaultValue("Manual").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(x => new { x.GoalId, x.ContributionDate })
                .HasDatabaseName("idx_goal_contributions_goal_date");

            entity.HasOne(x => x.Goal)
                .WithMany(x => x.Contributions)
                .HasForeignKey(x => x.GoalId)
                .HasConstraintName("fk_goal_contributions_goal")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureFinancialSummary(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialSummary>(entity =>
        {
            entity.ToTable("financial_summaries", table =>
            {
                table.HasCheckConstraint("chk_financial_summaries_period_type", "period_type IN ('Weekly', 'Monthly')");
                table.HasCheckConstraint("chk_financial_summaries_date_range", "period_end_date >= period_start_date");
                table.HasCheckConstraint("chk_financial_summaries_week_number", "week_number IS NULL OR week_number BETWEEN 1 AND 53");
                table.HasCheckConstraint("chk_financial_summaries_month", "month IS NULL OR month BETWEEN 1 AND 12");
                table.HasCheckConstraint("chk_financial_summaries_year", "year >= 2000");
                table.HasCheckConstraint("chk_financial_summaries_amounts", "total_income >= 0 AND total_expense >= 0 AND total_budget >= 0 AND used_budget >= 0 AND budget_usage_percent >= 0");
                table.HasCheckConstraint("chk_financial_summaries_counts", "transaction_count >= 0 AND active_goal_count >= 0 AND achieved_goal_count >= 0");
                table.HasCheckConstraint("chk_financial_summaries_weekly_monthly", "((period_type = 'Weekly' AND week_number IS NOT NULL AND month IS NULL) OR (period_type = 'Monthly' AND month IS NOT NULL AND week_number IS NULL))");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.PeriodType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.TotalIncome).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.TotalExpense).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.NetSaving).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.TotalBudget).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.UsedBudget).HasColumnType("numeric(18,2)").HasDefaultValue(0m);
            entity.Property(x => x.BudgetUsagePercent).HasColumnType("numeric(7,2)").HasDefaultValue(0m);
            entity.Property(x => x.TransactionCount).HasDefaultValue(0);
            entity.Property(x => x.ActiveGoalCount).HasDefaultValue(0);
            entity.Property(x => x.AchievedGoalCount).HasDefaultValue(0);
            entity.Property(x => x.CalculatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(x => new { x.UserId, x.PeriodType, x.PeriodStartDate, x.PeriodEndDate })
                .IsUnique()
                .HasDatabaseName("uq_financial_summaries_user_period");
            entity.HasIndex(x => new { x.UserId, x.Year, x.Month })
                .HasDatabaseName("idx_financial_summaries_user_year_month");
            entity.HasIndex(x => new { x.UserId, x.Year, x.WeekNumber })
                .HasDatabaseName("idx_financial_summaries_user_year_week");
        });
    }
}
