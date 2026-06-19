using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using FlowFi.AnalyticsService.Validators;
using FlowFi.Contracts.Events;

namespace FlowFi.AnalyticsService.Services;

public sealed class AnalyticsService(IAnalyticsRepository repository, IAnalyticsEventPublisher publisher) : IAnalyticsService
{
    public async Task<IReadOnlyList<BudgetResponse>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var budgets = await repository.GetBudgetsAsync(userId, cancellationToken);
        return budgets.Select(ToBudgetResponse).ToList();
    }

    public async Task<BudgetResponse?> GetBudgetAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(budgetId, cancellationToken);
        return budget is null ? null : ToBudgetResponse(budget);
    }

    public async Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TagId = request.TagId,
            TagName = request.TagName,
            Name = request.Name.Trim(),
            PeriodType = NormalizeOrDefault(request.PeriodType, "Monthly", AnalyticsValidationRules.BudgetPeriods),
            BudgetAmount = request.BudgetAmount,
            WarningThresholdPercent = request.WarningThresholdPercent ?? 80,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Active",
            CreatedAt = now
        };

        var created = await repository.AddBudgetAsync(budget, cancellationToken);
        return ToBudgetResponse(created);
    }

    public async Task<BudgetResponse?> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(budgetId, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        budget.TagId = request.TagId;
        budget.TagName = request.TagName;
        budget.Name = request.Name.Trim();
        budget.PeriodType = NormalizeOrDefault(request.PeriodType, budget.PeriodType, AnalyticsValidationRules.BudgetPeriods);
        budget.BudgetAmount = request.BudgetAmount;
        budget.WarningThresholdPercent = request.WarningThresholdPercent ?? budget.WarningThresholdPercent;
        budget.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode, budget.CurrencyCode);
        budget.StartDate = request.StartDate;
        budget.EndDate = request.EndDate;
        budget.Status = NormalizeOrDefault(request.Status, budget.Status, AnalyticsValidationRules.BudgetStatuses);
        budget.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.UpdateBudgetAsync(budget, cancellationToken);
        return ToBudgetResponse(budget);
    }

    public async Task<bool> DeleteBudgetAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await repository.GetBudgetAsync(budgetId, cancellationToken);
        if (budget is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        budget.DeletedAt = now;
        budget.UpdatedAt = now;
        await repository.UpdateBudgetAsync(budget, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<SavingGoalResponse>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var goals = await repository.GetSavingGoalsAsync(userId, cancellationToken);
        return goals.Select(ToSavingGoalResponse).ToList();
    }

    public async Task<SavingGoalResponse?> GetSavingGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        return goal is null ? null : ToSavingGoalResponse(goal);
    }

    public async Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var goal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Description = request.Description,
            TargetAmount = request.TargetAmount,
            CurrentAmount = request.CurrentAmount ?? 0m,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            TargetDate = request.TargetDate,
            PriorityLevel = NormalizeOrDefault(request.PriorityLevel, "Medium", AnalyticsValidationRules.PriorityLevels),
            Status = "Active",
            CreatedAt = now
        };

        var created = await repository.AddSavingGoalAsync(goal, cancellationToken);
        return ToSavingGoalResponse(created);
    }

    public async Task<SavingGoalResponse?> UpdateSavingGoalAsync(Guid goalId, UpdateSavingGoalRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        goal.Name = request.Name.Trim();
        goal.Description = request.Description;
        goal.TargetAmount = request.TargetAmount;
        goal.CurrentAmount = request.CurrentAmount ?? goal.CurrentAmount;
        goal.CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode, goal.CurrencyCode);
        goal.TargetDate = request.TargetDate;
        goal.PriorityLevel = NormalizeOrDefault(request.PriorityLevel, goal.PriorityLevel, AnalyticsValidationRules.PriorityLevels);
        goal.Status = NormalizeOrDefault(request.Status, goal.Status, AnalyticsValidationRules.SavingGoalStatuses);
        goal.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        return ToSavingGoalResponse(goal);
    }

    public async Task<SavingGoalResponse?> UpdateGoalProgressAsync(Guid goalId, UpdateGoalProgressRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        goal.CurrentAmount = request.CurrentAmount;
        goal.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        await PublishGoalProgressAsync(goal, cancellationToken);
        return ToSavingGoalResponse(goal);
    }

    public async Task<bool> DeleteSavingGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        goal.DeletedAt = now;
        goal.UpdatedAt = now;
        await repository.UpdateSavingGoalAsync(goal, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<GoalContributionResponse>?> GetGoalContributionsAsync(Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        var contributions = await repository.GetGoalContributionsAsync(goalId, cancellationToken);
        return contributions.Select(ToGoalContributionResponse).ToList();
    }

    public async Task<GoalContributionResponse?> AddGoalContributionAsync(Guid goalId, CreateGoalContributionRequest request, CancellationToken cancellationToken)
    {
        var goal = await repository.GetSavingGoalAsync(goalId, cancellationToken);
        if (goal is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var contribution = new GoalContribution
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            Amount = request.Amount,
            ContributionDate = request.ContributionDate ?? now,
            SourceType = NormalizeOrDefault(request.SourceType, "Manual", AnalyticsValidationRules.ContributionSourceTypes),
            SourceReferenceId = request.SourceReferenceId,
            Note = request.Note,
            CreatedAt = now
        };

        goal.CurrentAmount += request.Amount;
        goal.UpdatedAt = now;

        var created = await repository.AddGoalContributionAsync(contribution, cancellationToken);
        return ToGoalContributionResponse(created);
    }

    private async Task PublishGoalProgressAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        var percent = goal.TargetAmount == 0 ? 0 : decimal.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2);
        await publisher.PublishAsync("goal.progress.updated", new GoalProgressUpdated(goal.UserId, goal.Id, percent), cancellationToken);
    }

    private static BudgetResponse ToBudgetResponse(Budget budget)
    {
        return new BudgetResponse(
            budget.Id,
            budget.UserId,
            budget.TagId,
            budget.TagName,
            budget.Name,
            budget.PeriodType,
            budget.BudgetAmount,
            budget.WarningThresholdPercent,
            budget.CurrencyCode,
            budget.StartDate,
            budget.EndDate,
            budget.Status,
            budget.CreatedAt,
            budget.UpdatedAt);
    }

    private static SavingGoalResponse ToSavingGoalResponse(SavingGoal goal)
    {
        return new SavingGoalResponse(
            goal.Id,
            goal.UserId,
            goal.Name,
            goal.Description,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.CurrencyCode,
            goal.TargetDate,
            goal.PriorityLevel,
            goal.Status,
            goal.AchievedAt,
            goal.CreatedAt,
            goal.UpdatedAt);
    }

    private static GoalContributionResponse ToGoalContributionResponse(GoalContribution contribution)
    {
        return new GoalContributionResponse(
            contribution.Id,
            contribution.GoalId,
            contribution.Amount,
            contribution.ContributionDate,
            contribution.SourceType,
            contribution.SourceReferenceId,
            contribution.Note,
            contribution.CreatedAt);
    }

    private static string NormalizeOrDefault(string? value, string defaultValue, IReadOnlyCollection<string> allowedValues)
    {
        return AnalyticsValidationRules.NormalizeOrDefault(value, defaultValue, allowedValues);
    }

    private static string NormalizeCurrencyCode(string? value, string defaultValue = "VND")
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToUpperInvariant();
    }
}
