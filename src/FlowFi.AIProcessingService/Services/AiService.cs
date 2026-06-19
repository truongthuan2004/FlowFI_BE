using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interfaces;

namespace FlowFi.AIProcessingService.Services;

public sealed class AiService(IAiRepository repository) : IAiService
{
    public Task<AiJob> QueueJobAsync(AiJob job, CancellationToken cancellationToken)
    {
        var entity = job with
        {
            Id = job.Id == Guid.Empty ? Guid.NewGuid() : job.Id,
            Status = "queued",
            CreatedAt = DateTimeOffset.UtcNow
        };

        return repository.AddJobAsync(entity, cancellationToken);
    }

    public Task<IReadOnlyList<AiInsight>> GetInsightsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.GetInsightsAsync(userId, cancellationToken);
    }
}

