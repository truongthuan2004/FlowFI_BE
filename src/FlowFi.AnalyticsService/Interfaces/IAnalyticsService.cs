using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;

namespace FlowFi.AnalyticsService.Interfaces;

public interface IAnalyticsService
{
    Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task<SavingGoal?> UpdateGoalProgressAsync(Guid goalId, GoalProgressRequest request, CancellationToken cancellationToken);
}

