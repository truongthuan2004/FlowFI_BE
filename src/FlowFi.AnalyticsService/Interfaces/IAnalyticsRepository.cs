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
}
