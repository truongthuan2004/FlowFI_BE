using FlowFi.AIProcessingService.Entities;

namespace FlowFi.AIProcessingService.Interfaces;

public interface IAiService
{
    Task<AiJob> QueueJobAsync(AiJob job, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiInsight>> GetInsightsAsync(Guid userId, CancellationToken cancellationToken);
}

