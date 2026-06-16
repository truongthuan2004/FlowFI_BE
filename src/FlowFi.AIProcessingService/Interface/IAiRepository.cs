using FlowFi.AIProcessingService.Entities;

namespace FlowFi.AIProcessingService.Interface;

public interface IAiRepository
{
    Task<AiJob> AddJobAsync(AiJob job, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiInsight>> GetInsightsAsync(Guid userId, CancellationToken cancellationToken);
}

