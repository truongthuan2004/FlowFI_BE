using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using FlowFi.AnalyticsService.Messaging;
using FlowFi.Contracts.Events;
using Xunit;
using AnalyticsDomainService = FlowFi.AnalyticsService.Services.AnalyticsService;

namespace FlowFi.AnalyticsService.Tests;

public sealed class AnalyticsServiceTests
{
    [Fact]
    public async Task AddGoalContributionAsync_marks_goal_achieved_and_publishes_progress()
    {
        var userId = Guid.NewGuid();
        var goal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Emergency fund",
            TargetAmount = 100m,
            CurrentAmount = 90m,
            CurrencyCode = "VND",
            PriorityLevel = "High",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var repository = new InMemoryAnalyticsRepository();
        repository.SavingGoals.Add(goal);
        var publisher = new CapturingAnalyticsEventPublisher();
        var service = new AnalyticsDomainService(repository, publisher);

        var created = await service.AddGoalContributionAsync(
            userId,
            goal.Id,
            new CreateGoalContributionRequest(10m, null, null, null, "top up"),
            CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(100m, goal.CurrentAmount);
        Assert.Equal("Achieved", goal.Status);
        Assert.NotNull(goal.AchievedAt);
        var progressEvent = Assert.Single(publisher.Published, x => x.RoutingKey == "goal.progress.updated");
        Assert.IsType<GoalProgressUpdated>(progressEvent.Message);
        Assert.Equal(100m, ((GoalProgressUpdated)progressEvent.Message).ProgressPercent);
    }

    [Fact]
    public async Task ProcessFinanceTransactionEventAsync_recalculates_matching_budget_progress()
    {
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var budget = CreateBudget(userId, tagId, 100m);

        var repository = new InMemoryAnalyticsRepository();
        repository.Budgets.Add(budget);
        var service = new AnalyticsDomainService(repository, new CapturingAnalyticsEventPublisher());

        await service.ProcessFinanceTransactionEventAsync(
            "finance.transaction.created",
            new FinanceTransactionEvent(
                Guid.NewGuid(),
                userId,
                Guid.NewGuid(),
                tagId,
                "Food",
                60m,
                "EXPENSE",
                "VND",
                new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
                DateTimeOffset.UtcNow),
            CancellationToken.None);

        var progress = await service.GetBudgetProgressAsync(userId, budget.Id, CancellationToken.None);

        Assert.NotNull(progress);
        Assert.Equal(60m, progress.SpentAmount);
        Assert.Equal(40m, progress.RemainingAmount);
        Assert.Equal(60m, progress.UsagePercent);
        Assert.Equal(1, progress.TransactionCount);
    }

    [Fact]
    public async Task ProcessFinanceTransactionEventAsync_publishes_budget_exceeded_only_when_crossing_limit()
    {
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var budget = CreateBudget(userId, tagId, 100m);

        var repository = new InMemoryAnalyticsRepository();
        repository.Budgets.Add(budget);
        var publisher = new CapturingAnalyticsEventPublisher();
        var service = new AnalyticsDomainService(repository, publisher);
        var transactionId = Guid.NewGuid();
        var transactionEvent = new FinanceTransactionEvent(
            transactionId,
            userId,
            Guid.NewGuid(),
            tagId,
            "Food",
            120m,
            "EXPENSE",
            "VND",
            new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
            DateTimeOffset.UtcNow);

        await service.ProcessFinanceTransactionEventAsync("finance.transaction.created", transactionEvent, CancellationToken.None);
        await service.ProcessFinanceTransactionEventAsync("finance.transaction.updated", transactionEvent, CancellationToken.None);

        var exceeded = Assert.Single(publisher.Published, x => x.RoutingKey == "budget.exceeded");
        var message = Assert.IsType<BudgetExceeded>(exceeded.Message);
        Assert.Equal(userId, message.UserId);
        Assert.Equal(budget.Id, message.BudgetId);
        Assert.Equal(120m, message.CurrentAmount);
        Assert.Equal(100m, message.LimitAmount);
    }

    [Fact]
    public async Task ProcessFinanceTransactionEventAsync_removes_deleted_transaction_from_progress()
    {
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var budget = CreateBudget(userId, tagId, 100m);

        var repository = new InMemoryAnalyticsRepository();
        repository.Budgets.Add(budget);
        var service = new AnalyticsDomainService(repository, new CapturingAnalyticsEventPublisher());
        var transactionEvent = new FinanceTransactionEvent(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            tagId,
            "Food",
            60m,
            "EXPENSE",
            "VND",
            new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
            DateTimeOffset.UtcNow);

        await service.ProcessFinanceTransactionEventAsync("finance.transaction.created", transactionEvent, CancellationToken.None);
        await service.ProcessFinanceTransactionEventAsync("finance.transaction.deleted", transactionEvent, CancellationToken.None);

        var progress = await service.GetBudgetProgressAsync(userId, budget.Id, CancellationToken.None);

        Assert.NotNull(progress);
        Assert.Equal(0m, progress.SpentAmount);
        Assert.Equal(100m, progress.RemainingAmount);
        Assert.Equal(0m, progress.UsagePercent);
        Assert.Equal(0, progress.TransactionCount);
    }

    [Fact]
    public async Task GetFinancialSummaryAsync_calculates_monthly_income_expense_and_goals()
    {
        var userId = Guid.NewGuid();
        var repository = new InMemoryAnalyticsRepository();
        repository.SavingGoals.Add(new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Trip",
            TargetAmount = 100m,
            CurrentAmount = 100m,
            Status = "Achieved",
            CreatedAt = DateTimeOffset.UtcNow
        });
        repository.SavingGoals.Add(new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Laptop",
            TargetAmount = 500m,
            CurrentAmount = 100m,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        var service = new AnalyticsDomainService(repository, new CapturingAnalyticsEventPublisher());

        await service.ProcessFinanceTransactionEventAsync(
            "finance.transaction.created",
            CreateTransactionEvent(userId, 1_000m, "INCOME"),
            CancellationToken.None);
        await service.ProcessFinanceTransactionEventAsync(
            "finance.transaction.created",
            CreateTransactionEvent(userId, 300m, "EXPENSE"),
            CancellationToken.None);

        var summary = await service.GetFinancialSummaryAsync(
            userId,
            new FinancialSummaryQuery { PeriodType = "Monthly", Year = 2026, Month = 1 },
            CancellationToken.None);

        Assert.NotNull(summary);
        Assert.Equal(1_000m, summary.TotalIncome);
        Assert.Equal(300m, summary.TotalExpense);
        Assert.Equal(700m, summary.NetSaving);
        Assert.Equal(2, summary.TransactionCount);
        Assert.Equal(1, summary.ActiveGoalCount);
        Assert.Equal(1, summary.AchievedGoalCount);
    }

    private static Budget CreateBudget(Guid userId, Guid? tagId, decimal amount)
    {
        return new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TagId = tagId,
            TagName = tagId.HasValue ? "Food" : null,
            Name = "Food January",
            PeriodType = "Monthly",
            BudgetAmount = amount,
            WarningThresholdPercent = 80,
            CurrencyCode = "VND",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 31),
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static FinanceTransactionEvent CreateTransactionEvent(Guid userId, decimal amount, string type)
    {
        return new FinanceTransactionEvent(
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "General",
            amount,
            type,
            "VND",
            new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
            DateTimeOffset.UtcNow);
    }

    private sealed class CapturingAnalyticsEventPublisher : IAnalyticsEventPublisher
    {
        public List<PublishedEvent> Published { get; } = [];

        public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken)
        {
            Published.Add(new PublishedEvent(routingKey, message!));
            return Task.CompletedTask;
        }
    }

    private sealed record PublishedEvent(string RoutingKey, object Message);

    private sealed class InMemoryAnalyticsRepository : IAnalyticsRepository
    {
        public List<Budget> Budgets { get; } = [];
        public List<BudgetProgress> BudgetProgresses { get; } = [];
        public List<SavingGoal> SavingGoals { get; } = [];
        public List<GoalContribution> Contributions { get; } = [];
        public List<TransactionSnapshot> TransactionSnapshots { get; } = [];
        public List<FinancialSummary> FinancialSummaries { get; } = [];

        public Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Budget>>(Budgets.Where(x => x.UserId == userId && x.DeletedAt is null).ToList());

        public Task<Budget?> GetBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
            => Task.FromResult(Budgets.FirstOrDefault(x => x.UserId == userId && x.Id == budgetId && x.DeletedAt is null));

        public Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken)
        {
            Budgets.Add(budget);
            return Task.FromResult(budget);
        }

        public Task UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<IReadOnlyList<SavingGoal>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<SavingGoal>>(SavingGoals.Where(x => x.UserId == userId && x.DeletedAt is null).ToList());

        public Task<SavingGoal?> GetSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
            => Task.FromResult(SavingGoals.FirstOrDefault(x => x.UserId == userId && x.Id == goalId && x.DeletedAt is null));

        public Task<SavingGoal> AddSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
        {
            SavingGoals.Add(goal);
            return Task.FromResult(goal);
        }

        public Task UpdateSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<IReadOnlyList<GoalContribution>> GetGoalContributionsAsync(Guid goalId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GoalContribution>>(Contributions.Where(x => x.GoalId == goalId && x.DeletedAt is null).ToList());

        public Task<GoalContribution> AddGoalContributionAsync(GoalContribution contribution, CancellationToken cancellationToken)
        {
            Contributions.Add(contribution);
            return Task.FromResult(contribution);
        }

        public Task<TransactionSnapshot?> GetTransactionSnapshotAsync(Guid transactionId, CancellationToken cancellationToken)
            => Task.FromResult(TransactionSnapshots.FirstOrDefault(x => x.TransactionId == transactionId));

        public Task UpsertTransactionSnapshotAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken)
        {
            var existing = TransactionSnapshots.FirstOrDefault(x => x.TransactionId == snapshot.TransactionId);
            if (existing is null)
            {
                TransactionSnapshots.Add(snapshot);
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
                existing.DeletedAt = null;
                existing.UpdatedAt = snapshot.UpdatedAt;
            }

            return Task.CompletedTask;
        }

        public Task DeleteTransactionSnapshotAsync(Guid transactionId, DateTimeOffset deletedAt, CancellationToken cancellationToken)
        {
            var existing = TransactionSnapshots.FirstOrDefault(x => x.TransactionId == transactionId);
            if (existing is not null)
            {
                existing.DeletedAt = deletedAt;
                existing.UpdatedAt = deletedAt;
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Budget>> GetBudgetsForTransactionAsync(Guid userId, DateOnly transactionDate, Guid? tagId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Budget>>(Budgets
                .Where(x => x.UserId == userId && x.DeletedAt is null)
                .Where(x => x.StartDate <= transactionDate && x.EndDate >= transactionDate)
                .Where(x => x.TagId is null || x.TagId == tagId)
                .ToList());

        public Task<IReadOnlyList<TransactionSnapshot>> GetTransactionsForBudgetAsync(Budget budget, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TransactionSnapshot>>(TransactionSnapshots
                .Where(x => x.UserId == budget.UserId && x.DeletedAt is null)
                .Where(x => x.Type == "EXPENSE")
                .Where(x => ToDateOnly(x.TransactionDate) >= budget.StartDate && ToDateOnly(x.TransactionDate) <= budget.EndDate)
                .Where(x => budget.TagId is null || x.TagId == budget.TagId)
                .ToList());

        public Task<BudgetProgress?> GetBudgetProgressAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
            => Task.FromResult(BudgetProgresses.FirstOrDefault(x => x.BudgetId == budgetId));

        public Task UpsertBudgetProgressAsync(BudgetProgress progress, CancellationToken cancellationToken)
        {
            var existing = BudgetProgresses.FirstOrDefault(x => x.BudgetId == progress.BudgetId);
            if (existing is null)
            {
                BudgetProgresses.Add(progress);
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

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TransactionSnapshot>> GetTransactionsForPeriodAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TransactionSnapshot>>(TransactionSnapshots
                .Where(x => x.UserId == userId && x.DeletedAt is null)
                .Where(x => ToDateOnly(x.TransactionDate) >= startDate && ToDateOnly(x.TransactionDate) <= endDate)
                .ToList());

        public Task<FinancialSummary?> GetFinancialSummaryAsync(Guid userId, string periodType, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
            => Task.FromResult(FinancialSummaries.FirstOrDefault(x =>
                x.UserId == userId &&
                x.PeriodType == periodType &&
                x.PeriodStartDate == startDate &&
                x.PeriodEndDate == endDate));

        public Task UpsertFinancialSummaryAsync(FinancialSummary summary, CancellationToken cancellationToken)
        {
            var existing = FinancialSummaries.FirstOrDefault(x =>
                x.UserId == summary.UserId &&
                x.PeriodType == summary.PeriodType &&
                x.PeriodStartDate == summary.PeriodStartDate &&
                x.PeriodEndDate == summary.PeriodEndDate);

            if (existing is null)
            {
                FinancialSummaries.Add(summary);
            }
            else
            {
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

            return Task.CompletedTask;
        }

        private static DateOnly ToDateOnly(DateTimeOffset value)
            => DateOnly.FromDateTime(value.Date);
    }
}
