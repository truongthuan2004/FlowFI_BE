using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.AnalyticsService.Services;

public sealed class AnalyticsService(IAnalyticsRepository repository, RabbitMqPublisher publisher) : IAnalyticsService
{
    public Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.GetBudgetsAsync(userId, cancellationToken);
    }

    public Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        if (budget.Id == Guid.Empty)
        {
            budget.Id = Guid.NewGuid();
        }

        if (budget.CreatedAt == default)
        {
            budget.CreatedAt = DateTimeOffset.UtcNow;
        }

        if (string.IsNullOrWhiteSpace(budget.PeriodType))
        {
            budget.PeriodType = "Monthly";
        }

        if (budget.WarningThresholdPercent == 0)
        {
            budget.WarningThresholdPercent = 80;
        }

        if (string.IsNullOrWhiteSpace(budget.CurrencyCode))
        {
            budget.CurrencyCode = "VND";
        }

        if (string.IsNullOrWhiteSpace(budget.Status))
        {
            budget.Status = "Active";
        }

        return repository.AddBudgetAsync(budget, cancellationToken);
    }

    public async Task<SavingGoal?> UpdateGoalProgressAsync(Guid goalId, GoalProgressRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        goal.CurrentAmount = request.CurrentAmount;
        await repository.UpdateSavingGoalAsync(goal, cancellationToken);

        var percent = goal.TargetAmount == 0 ? 0 : decimal.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2);
        await publisher.PublishAsync("goal.progress.updated", new GoalProgressUpdated(goal.UserId, goal.Id, percent), cancellationToken);
        return goal;
    }
}
