using FlowFi.AnalyticsService.Entities;

namespace FlowFi.AnalyticsService.Interfaces;

public interface IAnalyticsRepository
{
    Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task<SavingGoal?> GetSavingGoalAsync(Guid goalId, CancellationToken cancellationToken);
    Task UpdateSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken);
}
