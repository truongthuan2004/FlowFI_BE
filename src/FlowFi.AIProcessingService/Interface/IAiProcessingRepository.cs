using FlowFi.AIProcessingService.Entities;

namespace FlowFi.AIProcessingService.Interface;

public interface IAiProcessingRepository
{
    Task<IReadOnlyList<AiProcessingRequest>> GetRequestsAsync(Guid? userId, CancellationToken cancellationToken);
    Task<AiProcessingRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken);
    void AddRequest(AiProcessingRequest request);
    void UpdateRequest(AiProcessingRequest request);
    Task<AiProcessingResult?> GetResultByRequestIdAsync(Guid requestId, CancellationToken cancellationToken);
    void AddResult(AiProcessingResult result);
}
