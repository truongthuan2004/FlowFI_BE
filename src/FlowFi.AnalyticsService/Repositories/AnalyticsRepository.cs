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
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        db.Budgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task<SavingGoal?> GetSavingGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        return await db.SavingGoals.FindAsync([goalId], cancellationToken);
    }

    public async Task UpdateSavingGoalAsync(SavingGoal goal, CancellationToken cancellationToken)
    {
        db.SavingGoals.Update(goal);
        await db.SaveChangesAsync(cancellationToken);
    }
}
