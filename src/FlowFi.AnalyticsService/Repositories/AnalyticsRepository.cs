using FlowFi.AnalyticsService.Database;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AnalyticsService.Repositories;

public sealed class AnalyticsRepository(AnalyticsDbContext db) : IAnalyticsRepository
{
    public async Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Budgets
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetBudgetAsync(Guid userId, Guid budgetId, CancellationToken cancellationToken)
    {
        return await db.Budgets
            .FirstOrDefaultAsync(
                x => x.Id == budgetId && x.UserId == userId && x.DeletedAt == null,
                cancellationToken);
    }

    public async Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        db.Budgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        db.Budgets.Update(budget);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SavingGoal>> GetSavingGoalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.SavingGoals
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .OrderBy(x => x.TargetDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SavingGoal?> GetSavingGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        return await db.SavingGoals
            .FirstOrDefaultAsync(
                x => x.Id == goalId && x.UserId == userId && x.DeletedAt == null,
                cancellationToken);
    }

    public async Task<SavingGoal> AddSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        db.SavingGoals.Add(goal);
        await db.SaveChangesAsync(cancellationToken);
        return goal;
    }

    public async Task UpdateSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        db.SavingGoals.Update(goal);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GoalContribution>> GetGoalContributionsAsync(Guid goalId, CancellationToken cancellationToken)
    {
        return await db.GoalContributions
            .AsNoTracking()
            .Where(x => x.GoalId == goalId && x.DeletedAt == null)
            .OrderByDescending(x => x.ContributionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<GoalContribution> AddGoalContributionAsync(GoalContribution contribution, CancellationToken cancellationToken)
    {
        db.GoalContributions.Add(contribution);
        await db.SaveChangesAsync(cancellationToken);
        return contribution;
    }
}
