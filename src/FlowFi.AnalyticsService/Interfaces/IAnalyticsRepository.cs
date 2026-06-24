using FlowFi.AnalyticsService.Entities;

namespace FlowFi.AnalyticsService.Interfaces;

public interface IAnalyticsRepository
{
    Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Budget?> GetBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken);
    Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task<IReadOnlyList<SavingGoal>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken);
    Task<SavingGoal?> GetSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken);
    Task<SavingGoal> AddSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken);
    Task UpdateSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken);
    Task<IReadOnlyList<GoalContribution>> GetGoalContributionsAsync(Guid goalId, CancellationToken cancellationToken);
    Task<GoalContribution> AddGoalContributionAsync(GoalContribution contribution, CancellationToken cancellationToken);
    Task<TransactionSnapshot?> GetTransactionSnapshotAsync(Guid transactionId, CancellationToken cancellationToken);
    Task UpsertTransactionSnapshotAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken);
    Task DeleteTransactionSnapshotAsync(Guid transactionId, DateTimeOffset deletedAt, CancellationToken cancellationToken);
    Task<IReadOnlyList<Budget>> GetBudgetsForTransactionAsync(Guid userId, DateOnly transactionDate, Guid? tagId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TransactionSnapshot>> GetTransactionsForBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task<BudgetProgress?> GetBudgetProgressAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken);
    Task UpsertBudgetProgressAsync(BudgetProgress progress, CancellationToken cancellationToken);
    Task<IReadOnlyList<TransactionSnapshot>> GetTransactionsForPeriodAsync(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task<FinancialSummary?> GetFinancialSummaryAsync(Guid userId, string periodType, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task UpsertFinancialSummaryAsync(FinancialSummary summary, CancellationToken cancellationToken);
}
