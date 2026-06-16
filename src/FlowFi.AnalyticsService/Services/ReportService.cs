using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interface;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.AnalyticsService.Services;

public sealed class analyticservice(IReportRepository repository, RabbitMqPublisher publisher) : Ianalyticservice
{
    public Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.GetBudgetsAsync(userId, cancellationToken);
    }

    public Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        var entity = budget with { Id = budget.Id == Guid.Empty ? Guid.NewGuid() : budget.Id };
        return repository.AddBudgetAsync(entity, cancellationToken);
    }

    public async Task<Goal?> UpdateGoalProgressAsync(Guid goalId, GoalProgressRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        var updated = goal with { CurrentAmount = request.CurrentAmount };
        await repository.UpdateGoalAsync(updated, cancellationToken);

        var percent = updated.TargetAmount == 0 ? 0 : decimal.Round(updated.CurrentAmount / updated.TargetAmount * 100, 2);
        await publisher.PublishAsync("goal.progress.updated", new GoalProgressUpdated(updated.UserId, updated.Id, percent), cancellationToken);
        return updated;
    }
}
