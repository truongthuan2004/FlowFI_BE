using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public interface ISyncQueueService
{
    Task<IReadOnlyList<SyncQueueDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SyncQueueDto> EnqueueAsync(
        CreateSyncQueueDto request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        Guid id,
        SyncQueueStatus status,
        string? lastError = null,
        CancellationToken cancellationToken = default);
}

