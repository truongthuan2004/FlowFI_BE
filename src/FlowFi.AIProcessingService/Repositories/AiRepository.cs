using FlowFi.AIProcessingService.Database;
using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AIProcessingService.Repositories;

public sealed class AiRepository(AiDbContext db) : IAiRepository
{
    public async Task<AiJob> AddJobAsync(AiJob job, CancellationToken cancellationToken)
    {
        db.AiJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<IReadOnlyList<AiInsight>> GetInsightsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Insights
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

