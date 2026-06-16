using FlowFi.AnalyticsService.Entities;

namespace FlowFi.AnalyticsService.Interface;

public interface IReportRepository
{
    Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task<Goal?> GetGoalAsync(Guid goalId, CancellationToken cancellationToken);
    Task UpdateGoalAsync(Goal goal, CancellationToken cancellationToken);
}

