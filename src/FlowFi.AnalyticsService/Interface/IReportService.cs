using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;

namespace FlowFi.AnalyticsService.Interface;

public interface Ianalyticservice
{
    Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken);
    Task<Goal?> UpdateGoalProgressAsync(Guid goalId, GoalProgressRequest request, CancellationToken cancellationToken);
}

