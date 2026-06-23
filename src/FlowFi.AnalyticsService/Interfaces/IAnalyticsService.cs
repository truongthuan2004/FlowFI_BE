using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Messaging;
namespace FlowFi.AnalyticsService.Interfaces;

public interface IAnalyticsService
{
    Task<IReadOnlyList<BudgetResponse>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken);
    Task<BudgetResponse?> GetBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken);
    Task<BudgetResponse> CreateBudgetAsync(Guid userId, CreateBudgetRequest request, CancellationToken cancellationToken);
    Task<BudgetResponse?> UpdateBudgetAsync(Guid userId, Guid budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SavingGoalResponse>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken);
    Task<SavingGoalResponse?> GetSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken);
    Task<SavingGoalResponse> CreateSavingGoalAsync(Guid userId, CreateSavingGoalRequest request, CancellationToken cancellationToken);
    Task<SavingGoalResponse?> UpdateSavingGoalAsync(Guid userId, Guid goalId, UpdateSavingGoalRequest request, CancellationToken cancellationToken);
    Task<SavingGoalResponse?> UpdateGoalProgressAsync(Guid userId, Guid goalId, UpdateGoalProgressRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GoalContributionResponse>?> GetGoalContributionsAsync(Guid userId, Guid goalId, CancellationToken cancellationToken);
    Task<GoalContributionResponse?> AddGoalContributionAsync(Guid userId, Guid goalId, CreateGoalContributionRequest request, CancellationToken cancellationToken);
    Task<BudgetProgressResponse?> GetBudgetProgressAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken);
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid userId, FinancialSummaryQuery query, CancellationToken cancellationToken);
    Task ProcessFinanceTransactionEventAsync(string routingKey, FinanceTransactionEvent financeEvent, CancellationToken cancellationToken);
}

