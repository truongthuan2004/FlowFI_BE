using FlowFi.AnalyticsService.Database;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AnalyticsService.Repositories;

public sealed class ReportRepository(ReportDbContext db) : IReportRepository
{
    public async Task<IReadOnlyList<Budget>> GetBudgetsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Budgets.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Budget> AddBudgetAsync(Budget budget, CancellationToken cancellationToken)
    {
        db.Budgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task<Goal?> GetGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        return await db.Goals.FindAsync([goalId], cancellationToken);
    }

    public async Task UpdateGoalAsync(Goal goal, CancellationToken cancellationToken)
    {
        db.Goals.Update(goal);
        await db.SaveChangesAsync(cancellationToken);
    }
}

