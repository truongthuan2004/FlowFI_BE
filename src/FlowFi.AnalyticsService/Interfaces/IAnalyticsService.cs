using FlowFi.AnalyticsService.DTOs;
namespace FlowFi.AnalyticsService.Interfaces;

public interface IAnalyticsService
{
    Task<IReadOnlyList<BudgetResponse>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<BudgetResponse?> GetBudgetAsync(Guid budgetId, CancellationToken cancellationToken);
    Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken);
    Task<BudgetResponse?> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteBudgetAsync(Guid budgetId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SavingGoalResponse>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken);
    Task<SavingGoalResponse?> GetSavingGoalAsync(Guid goalId, CancellationToken cancellationToken);
    Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
    Task<SavingGoalResponse?> UpdateSavingGoalAsync(Guid goalId, UpdateSavingGoalRequest request, CancellationToken cancellationToken);
    Task<SavingGoalResponse?> UpdateGoalProgressAsync(Guid goalId, UpdateGoalProgressRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteSavingGoalAsync(Guid goalId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GoalContributionResponse>?> GetGoalContributionsAsync(Guid goalId, CancellationToken cancellationToken);
    Task<GoalContributionResponse?> AddGoalContributionAsync(Guid goalId, CreateGoalContributionRequest request, CancellationToken cancellationToken);
}

