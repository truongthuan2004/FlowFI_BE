using FlowFi.AIProcessingService.Entities;

namespace FlowFi.AIProcessingService.Interface;

public interface IAiProcessingRepository
{
    Task<IReadOnlyList<AiProcessingRequest>> GetRequestsAsync(Guid? userId, CancellationToken cancellationToken);
    Task<AiProcessingRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken);
    Task<AiProcessingRequest> AddRequestAsync(AiProcessingRequest request, CancellationToken cancellationToken);
    Task<AiProcessingRequest> UpdateRequestAsync(AiProcessingRequest request, CancellationToken cancellationToken);
    Task<AiProcessingResult?> GetResultByRequestIdAsync(Guid requestId, CancellationToken cancellationToken);
    Task<AiProcessingResult> AddResultAsync(AiProcessingResult result, CancellationToken cancellationToken);
}
