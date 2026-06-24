using System.Globalization;
using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using FlowFi.AnalyticsService.Messaging;
using FlowFi.AnalyticsService.Validators;
using FlowFi.Contracts.Events;

namespace FlowFi.AnalyticsService.Services;

public sealed class AnalyticsService(IAnalyticsRepository repository, IAnalyticsEventPublisher publisher) : IAnalyticsService
{
    public async Task<IReadOnlyList<BudgetResponse>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var budgets = await repository.GetBudgetsAsync(userId, cancellationToken);
        return budgets.Select(ToBudgetResponse).ToList();
    }

    public async Task<BudgetResponse?> GetBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(userId, budgetId, cancellationToken);
        return budget is null ? null : ToBudgetResponse(budget);
    }

    public async Task<BudgetResponse> CreateBudgetAsync(Guid userId, CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TagId = request.TagId,
            TagName = request.TagName,
            Name = request.Name.Trim(),
            PeriodType = NormalizeOrDefault(request.PeriodType, "Monthly", AnalyticsValidationRules.BudgetPeriods),
            BudgetAmount = request.BudgetAmount,
            WarningThresholdPercent = request.WarningThresholdPercent ?? 80,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Active",
            CreatedAt = now
        };

        var created = await repository.AddBudgetAsync(budget, cancellationToken);
        await RecalculateBudgetProgressAsync(created, cancellationToken);
        return ToBudgetResponse(created);
    }

    public async Task<BudgetResponse?> UpdateBudgetAsync(Guid userId, Guid budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(userId, budgetId, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        budget.TagId = request.TagId;
        budget.TagName = request.TagName;
        budget.Name = request.Name.Trim();
        budget.PeriodType = NormalizeOrDefault(request.PeriodType, budget.PeriodType, AnalyticsValidationRules.BudgetPeriods);
        budget.BudgetAmount = request.BudgetAmount;
        budget.WarningThresholdPercent = request.WarningThresholdPercent ?? budget.WarningThresholdPercent;
        budget.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode, budget.CurrencyCode);
        budget.StartDate = request.StartDate;
        budget.EndDate = request.EndDate;
        budget.Status = NormalizeOrDefault(request.Status, budget.Status, AnalyticsValidationRules.BudgetStatuses);
        budget.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.UpdateBudgetAsync(budget, cancellationToken);
        await RecalculateBudgetProgressAsync(budget, cancellationToken);
        return ToBudgetResponse(budget);
    }

    public async Task<bool> DeleteBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(userId, budgetId, cancellationToken);
        if (budget is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        budget.DeletedAt = now;
        budget.UpdatedAt = now;
        await repository.UpdateBudgetAsync(budget, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<SavingGoalResponse>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var goals = await repository.GetSavingGoalsAsync(userId, cancellationToken);
        return goals.Select(ToSavingGoalResponse).ToList();
    }

    public async Task<SavingGoalResponse?> GetSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(userId, goalId, cancellationToken);
        return goal is null ? null : ToSavingGoalResponse(goal);
    }

    public async Task<SavingGoalResponse> CreateSavingGoalAsync(Guid userId, CreateSavingGoalRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var goal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description,
            TargetAmount = request.TargetAmount,
            CurrentAmount = request.CurrentAmount ?? 0m,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            TargetDate = request.TargetDate,
            PriorityLevel = NormalizeOrDefault(request.PriorityLevel, "Medium", AnalyticsValidationRules.PriorityLevels),
            Status = "Active",
            CreatedAt = now
        };

        var created = await repository.AddSavingGoalAsync(goal, cancellationToken);
        return ToSavingGoalResponse(created);
    }

    public async Task<SavingGoalResponse?> UpdateSavingGoalAsync(Guid userId, Guid goalId, UpdateSavingGoalRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(userId, goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        goal.Name = request.Name.Trim();
        goal.Description = request.Description;
        goal.TargetAmount = request.TargetAmount;
        goal.CurrentAmount = request.CurrentAmount ?? goal.CurrentAmount;
        goal.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode, goal.CurrencyCode);
        goal.TargetDate = request.TargetDate;
        goal.PriorityLevel = NormalizeOrDefault(request.PriorityLevel, goal.PriorityLevel, AnalyticsValidationRules.PriorityLevels);
        goal.Status = NormalizeOrDefault(request.Status, goal.Status, AnalyticsValidationRules.SavingGoalStatuses);
        goal.UpdatedAt = DateTimeOffset.UtcNow;
        ApplyGoalProgressState(goal, goal.UpdatedAt.Value);

        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        return ToSavingGoalResponse(goal);
    }

    public async Task<SavingGoalResponse?> UpdateGoalProgressAsync(Guid userId, Guid goalId, UpdateGoalProgressRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(userId, goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        goal.CurrentAmount = request.CurrentAmount;
        var now = DateTimeOffset.UtcNow;
        goal.UpdatedAt = now;
        ApplyGoalProgressState(goal, now);
        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        await PublishGoalProgressAsync(goal, cancellationToken);
        return ToSavingGoalResponse(goal);
    }

    public async Task<bool> DeleteSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(userId, goalId, cancellationToken);
        if (goal is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        goal.DeletedAt = now;
        goal.UpdatedAt = now;
        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<GoalContributionResponse>?> GetGoalContributionsAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(userId, goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        var contributions = await repository.GetGoalContributionsAsync(goalId, cancellationToken);
        return contributions.Select(ToGoalContributionResponse).ToList();
    }

    public async Task<GoalContributionResponse?> AddGoalContributionAsync(Guid userId, Guid goalId, CreateGoalContributionRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(userId, goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var contribution = new GoalContribution
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            Amount = request.Amount,
            ContributionDate = request.ContributionDate ?? now,
            SourceType = NormalizeOrDefault(request.SourceType, "Manual", AnalyticsValidationRules.ContributionSourceTypes),
            SourceReferenceId = request.SourceReferenceId,
            Note = request.Note,
            CreatedAt = now
        };

        goal.CurrentAmount += request.Amount;
        goal.UpdatedAt = now;
        ApplyGoalProgressState(goal, now);

        var created = await repository.AddGoalContributionAsync(contribution, cancellationToken);
        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        await PublishGoalProgressAsync(goal, cancellationToken);
        return ToGoalContributionResponse(created);
    }

    public async Task<BudgetProgressResponse?> GetBudgetProgressAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(userId, budgetId, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        var progress = await RecalculateBudgetProgressAsync(budget, cancellationToken, publishExceededEvent: false);
        return ToBudgetProgressResponse(progress);
    }

    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid userId, FinancialSummaryQuery query, CancellationToken cancellationToken)
    {
        var period = ResolvePeriod(query);
        var summary = await RecalculateFinancialSummaryAsync(userId, period, cancellationToken);
        return ToFinancialSummaryResponse(summary);
    }

    public async Task ProcessFinanceTransactionEventAsync(string routingKey, FinanceTransactionEvent financeEvent, CancellationToken cancellationToken)
    {
        var previous = await repository.GetTransactionSnapshotAsync(financeEvent.TransactionId, cancellationToken);
        if (routingKey.EndsWith(".deleted", StringComparison.OrdinalIgnoreCase))
        {
            if (previous is null)
            {
                return;
            }

            await repository.DeleteTransactionSnapshotAsync(financeEvent.TransactionId, financeEvent.OccurredAt, cancellationToken);
            await RecalculateAffectedAnalyticsAsync(previous, cancellationToken);
            return;
        }

        if (!routingKey.EndsWith(".created", StringComparison.OrdinalIgnoreCase)
            && !routingKey.EndsWith(".updated", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var snapshot = ToTransactionSnapshot(financeEvent, previous);
        await repository.UpsertTransactionSnapshotAsync(snapshot, cancellationToken);
        await RecalculateAffectedAnalyticsAsync(snapshot, cancellationToken);

        if (previous is not null && AffectsDifferentAnalyticsScope(previous, snapshot))
        {
            await RecalculateAffectedAnalyticsAsync(previous, cancellationToken);
        }
    }

    private async Task PublishGoalProgressAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        var percent = goal.TargetAmount == 0 ? 0 : decimal.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2);
        await publisher.PublishAsync("goal.progress.updated", new GoalProgressUpdated(goal.UserId, goal.Id, percent), cancellationToken);
    }

    private async Task RecalculateAffectedAnalyticsAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken)
    {
        var transactionDate = ToDateOnly(snapshot.TransactionDate);
        var budgets = await repository.GetBudgetsForTransactionAsync(snapshot.UserId, transactionDate, snapshot.TagId, cancellationToken);
        foreach (var budget in budgets)
        {
            await RecalculateBudgetProgressAsync(budget, cancellationToken);
        }

        await RecalculateFinancialSummaryAsync(snapshot.UserId, ResolveMonthlyPeriod(transactionDate), cancellationToken);
        await RecalculateFinancialSummaryAsync(snapshot.UserId, ResolveWeeklyPeriod(transactionDate), cancellationToken);
    }

    private async Task<BudgetProgress> RecalculateBudgetProgressAsync(
        Budget budget,
        CancellationToken cancellationToken,
        bool publishExceededEvent = true)
    {
        var existing = await repository.GetBudgetProgressAsync(budget.UserId, budget.Id, cancellationToken);
        var transactions = await repository.GetTransactionsForBudgetAsync(budget, cancellationToken);
        var spentAmount = transactions.Sum(x => x.Amount);
        var transactionCount = transactions.Count;
        var now = DateTimeOffset.UtcNow;
        var wasExceeded = existing?.ExceededAt is not null;
        var isExceeded = spentAmount > budget.BudgetAmount;

        var progress = existing ?? new BudgetProgress
        {
            Id = Guid.NewGuid(),
            BudgetId = budget.Id
        };

        progress.SpentAmount = spentAmount;
        progress.RemainingAmount = budget.BudgetAmount - spentAmount;
        progress.UsagePercent = budget.BudgetAmount <= 0
            ? 0
            : decimal.Round(spentAmount / budget.BudgetAmount * 100m, 2);
        progress.TransactionCount = transactionCount;
        progress.ExceededAt = isExceeded ? existing?.ExceededAt ?? now : null;
        progress.LastCalculatedAt = now;
        progress.UpdatedAt = existing is null ? null : now;

        await repository.UpsertBudgetProgressAsync(progress, cancellationToken);

        if (publishExceededEvent && isExceeded && !wasExceeded)
        {
            await publisher.PublishAsync(
                "budget.exceeded",
                new BudgetExceeded(budget.UserId, budget.Id, spentAmount, budget.BudgetAmount),
                cancellationToken);
        }

        return progress;
    }

    private async Task<FinancialSummary> RecalculateFinancialSummaryAsync(
        Guid userId,
        PeriodInfo period,
        CancellationToken cancellationToken)
    {
        var transactions = await repository.GetTransactionsForPeriodAsync(
            userId,
            period.StartDate,
            period.EndDate,
            cancellationToken);
        var budgets = (await repository.GetBudgetsAsync(userId, cancellationToken))
            .Where(x => x.StartDate <= period.EndDate && x.EndDate >= period.StartDate)
            .ToList();
        var goals = await repository.GetSavingGoalsAsync(userId, cancellationToken);

        var totalIncome = transactions
            .Where(x => string.Equals(x.Type, "INCOME", StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Amount);
        var totalExpense = transactions
            .Where(x => string.Equals(x.Type, "EXPENSE", StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Amount);
        var progressByBudget = new List<BudgetProgress>();
        foreach (var budget in budgets)
        {
            progressByBudget.Add(await RecalculateBudgetProgressAsync(
                budget,
                cancellationToken,
                publishExceededEvent: false));
        }

        var totalBudget = budgets.Sum(x => x.BudgetAmount);
        var usedBudget = progressByBudget.Sum(x => x.SpentAmount);
        var existing = await repository.GetFinancialSummaryAsync(
            userId,
            period.PeriodType,
            period.StartDate,
            period.EndDate,
            cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var summary = existing ?? new FinancialSummary
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeriodType = period.PeriodType,
            PeriodStartDate = period.StartDate,
            PeriodEndDate = period.EndDate
        };

        summary.WeekNumber = period.WeekNumber;
        summary.Month = period.Month;
        summary.Year = period.Year;
        summary.TotalIncome = totalIncome;
        summary.TotalExpense = totalExpense;
        summary.NetSaving = totalIncome - totalExpense;
        summary.TotalBudget = totalBudget;
        summary.UsedBudget = usedBudget;
        summary.BudgetUsagePercent = totalBudget <= 0
            ? 0
            : decimal.Round(usedBudget / totalBudget * 100m, 2);
        summary.TransactionCount = transactions.Count;
        summary.ActiveGoalCount = goals.Count(x => string.Equals(x.Status, "Active", StringComparison.OrdinalIgnoreCase));
        summary.AchievedGoalCount = goals.Count(x => string.Equals(x.Status, "Achieved", StringComparison.OrdinalIgnoreCase));
        summary.CalculatedAt = now;

        await repository.UpsertFinancialSummaryAsync(summary, cancellationToken);
        return summary;
    }

    private static TransactionSnapshot ToTransactionSnapshot(FinanceTransactionEvent financeEvent, TransactionSnapshot? previous)
    {
        var now = DateTimeOffset.UtcNow;
        return new TransactionSnapshot
        {
            TransactionId = financeEvent.TransactionId,
            UserId = financeEvent.UserId,
            WalletId = financeEvent.WalletId,
            TagId = financeEvent.TagId,
            TagName = financeEvent.TagName,
            Amount = financeEvent.Amount,
            Type = financeEvent.Type.Trim().ToUpperInvariant(),
            CurrencyCode = NormalizeCurrencyCode(financeEvent.CurrencyCode),
            TransactionDate = financeEvent.TransactionDate,
            OccurredAt = financeEvent.OccurredAt,
            CreatedAt = previous?.CreatedAt ?? now,
            UpdatedAt = previous is null ? null : now,
            DeletedAt = null
        };
    }

    private static bool AffectsDifferentAnalyticsScope(TransactionSnapshot previous, TransactionSnapshot current)
    {
        return previous.UserId != current.UserId
            || previous.TagId != current.TagId
            || ToDateOnly(previous.TransactionDate) != ToDateOnly(current.TransactionDate);
    }

    private static void ApplyGoalProgressState(SavingGoal goal, DateTimeOffset now)
    {
        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.Status = "Achieved";
            goal.AchievedAt ??= now;
            return;
        }

        if (string.Equals(goal.Status, "Achieved", StringComparison.OrdinalIgnoreCase))
        {
            goal.Status = "Active";
            goal.AchievedAt = null;
        }
    }

    private static PeriodInfo ResolvePeriod(FinancialSummaryQuery query)
    {
        if (string.Equals(query.PeriodType, "Weekly", StringComparison.OrdinalIgnoreCase))
        {
            var weekNumber = query.WeekNumber ?? throw new InvalidOperationException("WeekNumber is required for Weekly summaries.");
            return ResolveWeeklyPeriod(query.Year, weekNumber);
        }

        var month = query.Month ?? throw new InvalidOperationException("Month is required for Monthly summaries.");
        var startDate = new DateOnly(query.Year, month, 1);
        return new PeriodInfo(
            "Monthly",
            startDate,
            startDate.AddMonths(1).AddDays(-1),
            null,
            month,
            query.Year);
    }

    private static PeriodInfo ResolveMonthlyPeriod(DateOnly transactionDate)
    {
        var startDate = new DateOnly(transactionDate.Year, transactionDate.Month, 1);
        return new PeriodInfo(
            "Monthly",
            startDate,
            startDate.AddMonths(1).AddDays(-1),
            null,
            transactionDate.Month,
            transactionDate.Year);
    }

    private static PeriodInfo ResolveWeeklyPeriod(DateOnly transactionDate)
    {
        var isoYear = ISOWeek.GetYear(transactionDate.ToDateTime(TimeOnly.MinValue));
        var isoWeek = ISOWeek.GetWeekOfYear(transactionDate.ToDateTime(TimeOnly.MinValue));
        return ResolveWeeklyPeriod(isoYear, isoWeek);
    }

    private static PeriodInfo ResolveWeeklyPeriod(int year, int weekNumber)
    {
        var startDate = DateOnly.FromDateTime(ISOWeek.ToDateTime(year, weekNumber, DayOfWeek.Monday));
        return new PeriodInfo(
            "Weekly",
            startDate,
            startDate.AddDays(6),
            weekNumber,
            null,
            year);
    }

    private static BudgetResponse ToBudgetResponse(Budget budget)
    {
        return new BudgetResponse(
            budget.Id,
            budget.UserId,
            budget.TagId,
            budget.TagName,
            budget.Name,
            budget.PeriodType,
            budget.BudgetAmount,
            budget.WarningThresholdPercent,
            budget.CurrencyCode,
            budget.StartDate,
            budget.EndDate,
            budget.Status,
            budget.CreatedAt,
            budget.UpdatedAt);
    }

    private static SavingGoalResponse ToSavingGoalResponse(SavingGoal goal)
    {
        return new SavingGoalResponse(
            goal.Id,
            goal.UserId,
            goal.Name,
            goal.Description,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.CurrencyCode,
            goal.TargetDate,
            goal.PriorityLevel,
            goal.Status,
            goal.AchievedAt,
            goal.CreatedAt,
            goal.UpdatedAt);
    }

    private static GoalContributionResponse ToGoalContributionResponse(GoalContribution contribution)
    {
        return new GoalContributionResponse(
            contribution.Id,
            contribution.GoalId,
            contribution.Amount,
            contribution.ContributionDate,
            contribution.SourceType,
            contribution.SourceReferenceId,
            contribution.Note,
            contribution.CreatedAt);
    }

    private static BudgetProgressResponse ToBudgetProgressResponse(BudgetProgress progress)
    {
        return new BudgetProgressResponse(
            progress.Id,
            progress.BudgetId,
            progress.SpentAmount,
            progress.RemainingAmount,
            progress.UsagePercent,
            progress.TransactionCount,
            progress.ExceededAt,
            progress.LastCalculatedAt,
            progress.UpdatedAt);
    }

    private static FinancialSummaryResponse ToFinancialSummaryResponse(FinancialSummary summary)
    {
        return new FinancialSummaryResponse(
            summary.Id,
            summary.UserId,
            summary.PeriodType,
            summary.PeriodStartDate,
            summary.PeriodEndDate,
            summary.WeekNumber,
            summary.Month,
            summary.Year,
            summary.TotalIncome,
            summary.TotalExpense,
            summary.NetSaving,
            summary.TotalBudget,
            summary.UsedBudget,
            summary.BudgetUsagePercent,
            summary.TransactionCount,
            summary.ActiveGoalCount,
            summary.AchievedGoalCount,
            summary.CalculatedAt);
    }

    private static string NormalizeOrDefault(string? value, string defaultValue, IReadOnlyCollection<string> allowedValues)
    {
        return AnalyticsValidationRules.NormalizeOrDefault(value, defaultValue, allowedValues);
    }

    private static string NormalizeCurrencyCode(string? value, string defaultValue = "VND")
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToUpperInvariant();
    }

    private static DateOnly ToDateOnly(DateTimeOffset value)
    {
        return DateOnly.FromDateTime(value.Date);
    }

    private sealed record PeriodInfo(
        string PeriodType,
        DateOnly StartDate,
        DateOnly EndDate,
        int? WeekNumber,
        int? Month,
        int Year);
}
