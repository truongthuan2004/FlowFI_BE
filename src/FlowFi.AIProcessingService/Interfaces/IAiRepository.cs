using FlowFi.AIProcessingService.Entities;

namespace FlowFi.AIProcessingService.Interfaces;

public interface IAiRepository
{
    Task<AiJob> AddJobAsync(AiJob job, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiInsight>> GetInsightsAsync(Guid userId, CancellationToken cancellationToken);
}

