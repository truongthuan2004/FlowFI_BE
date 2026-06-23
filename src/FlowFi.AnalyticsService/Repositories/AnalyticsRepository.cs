using FlowFi.AnalyticsService.Database;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AnalyticsService.Repositories;

public sealed class AnalyticsRepository(AnalyticsDbContext db) : IAnalyticsRepository
{
    public async Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Budgets
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
    {
        return await db.Budgets
            .FirstOrDefaultAsync(
                x => x.Id == budgetId && x.UserId == userId && x.DeletedAt == null,
                cancellationToken);
    }

    public async Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        db.Budgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        db.Budgets.Update(budget);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SavingGoal>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.SavingGoals
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderBy(x => x.TargetDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SavingGoal?> GetSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        return await db.SavingGoals
            .FirstOrDefaultAsync(
                x => x.Id == goalId && x.UserId == userId && x.DeletedAt == null,
                cancellationToken);
    }

    public async Task<SavingGoal> AddSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        db.SavingGoals.Add(goal);
        await db.SaveChangesAsync(cancellationToken);
        return goal;
    }

    public async Task UpdateSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        db.SavingGoals.Update(goal);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GoalContribution>> GetGoalContributionsAsync(Guid goalId, CancellationToken cancellationToken)
    {
        return await db.GoalContributions
            .AsNoTracking()
            .Where(x => x.GoalId == goalId && x.DeletedAt == null)
            .OrderByDescending(x => x.ContributionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<GoalContribution> AddGoalContributionAsync(GoalContribution contribution, CancellationToken cancellationToken)
    {
        db.GoalContributions.Add(contribution);
        await db.SaveChangesAsync(cancellationToken);
        return contribution;
    }

    public async Task<TransactionSnapshot?> GetTransactionSnapshotAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        return await db.TransactionSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);
    }

    public async Task UpsertTransactionSnapshotAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken)
    {
        var existing = await db.TransactionSnapshots
            .FirstOrDefaultAsync(x => x.TransactionId == snapshot.TransactionId, cancellationToken);

        if (existing is null)
        {
            db.TransactionSnapshots.Add(snapshot);
        }
        else
        {
            existing.UserId = snapshot.UserId;
            existing.WalletId = snapshot.WalletId;
            existing.TagId = snapshot.TagId;
            existing.TagName = snapshot.TagName;
            existing.Amount = snapshot.Amount;
            existing.Type = snapshot.Type;
            existing.CurrencyCode = snapshot.CurrencyCode;
            existing.TransactionDate = snapshot.TransactionDate;
            existing.OccurredAt = snapshot.OccurredAt;
            existing.UpdatedAt = snapshot.UpdatedAt;
            existing.DeletedAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTransactionSnapshotAsync(Guid transactionId, DateTimeOffset deletedAt, CancellationToken cancellationToken)
    {
        var existing = await db.TransactionSnapshots
            .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.DeletedAt = deletedAt;
        existing.UpdatedAt = deletedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Budget>> GetBudgetsForTransactionAsync(
        Guid userId,
        DateOnly transactionDate,
        Guid? tagId,
        CancellationToken cancellationToken)
    {
        return await db.Budgets
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .Where(x => x.StartDate <= transactionDate && x.EndDate >= transactionDate)
            .Where(x => x.TagId == null || x.TagId == tagId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TransactionSnapshot>> GetTransactionsForBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        var (start, endExclusive) = ToDateTimeOffsetRange(budget.StartDate, budget.EndDate);
        var query = db.TransactionSnapshots
            .AsNoTracking()
            .Where(x => x.UserId == budget.UserId && x.DeletedAt == null)
            .Where(x => x.Type == "EXPENSE")
            .Where(x => x.TransactionDate >= start && x.TransactionDate < endExclusive);

        if (budget.TagId.HasValue)
        {
            query = query.Where(x => x.TagId == budget.TagId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<BudgetProgress?> GetBudgetProgressAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
    {
        return await db.BudgetProgresses
            .AsNoTracking()
            .Include(x => x.Budget)
            .FirstOrDefaultAsync(
                x => x.BudgetId == budgetId && x.Budget != null && x.Budget.UserId == userId,
                cancellationToken);
    }

    public async Task UpsertBudgetProgressAsync(BudgetProgress progress, CancellationToken cancellationToken)
    {
        var existing = await db.BudgetProgresses
            .FirstOrDefaultAsync(x => x.BudgetId == progress.BudgetId, cancellationToken);

        if (existing is null)
        {
            db.BudgetProgresses.Add(progress);
        }
        else
        {
            existing.SpentAmount = progress.SpentAmount;
            existing.RemainingAmount = progress.RemainingAmount;
            existing.UsagePercent = progress.UsagePercent;
            existing.TransactionCount = progress.TransactionCount;
            existing.ExceededAt = progress.ExceededAt;
            existing.LastCalculatedAt = progress.LastCalculatedAt;
            existing.UpdatedAt = progress.UpdatedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TransactionSnapshot>> GetTransactionsForPeriodAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var (start, endExclusive) = ToDateTimeOffsetRange(startDate, endDate);
        return await db.TransactionSnapshots
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .Where(x => x.TransactionDate >= start && x.TransactionDate < endExclusive)
            .ToListAsync(cancellationToken);
    }

    public async Task<FinancialSummary?> GetFinancialSummaryAsync(
        Guid userId,
        string periodType,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        return await db.FinancialSummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId
                    && x.PeriodType == periodType
                    && x.PeriodStartDate == startDate
                    && x.PeriodEndDate == endDate,
                cancellationToken);
    }

    public async Task UpsertFinancialSummaryAsync(FinancialSummary summary, CancellationToken cancellationToken)
    {
        var existing = await db.FinancialSummaries
            .FirstOrDefaultAsync(
                x => x.UserId == summary.UserId
                    && x.PeriodType == summary.PeriodType
                    && x.PeriodStartDate == summary.PeriodStartDate
                    && x.PeriodEndDate == summary.PeriodEndDate,
                cancellationToken);

        if (existing is null)
        {
            db.FinancialSummaries.Add(summary);
        }
        else
        {
            existing.WeekNumber = summary.WeekNumber;
            existing.Month = summary.Month;
            existing.Year = summary.Year;
            existing.TotalIncome = summary.TotalIncome;
            existing.TotalExpense = summary.TotalExpense;
            existing.NetSaving = summary.NetSaving;
            existing.TotalBudget = summary.TotalBudget;
            existing.UsedBudget = summary.UsedBudget;
            existing.BudgetUsagePercent = summary.BudgetUsagePercent;
            existing.TransactionCount = summary.TransactionCount;
            existing.ActiveGoalCount = summary.ActiveGoalCount;
            existing.AchievedGoalCount = summary.AchievedGoalCount;
            existing.CalculatedAt = summary.CalculatedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static (DateTimeOffset Start, DateTimeOffset EndExclusive) ToDateTimeOffsetRange(DateOnly startDate, DateOnly endDate)
    {
        var start = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endExclusive = new DateTimeOffset(endDate.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return (start, endExclusive);
    }
}
