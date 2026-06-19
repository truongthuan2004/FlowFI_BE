using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface ISyncQueueRepository
{
    Task<IReadOnlyList<SyncQueueItem>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SyncQueueItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SyncQueueItem> AddAsync(
        SyncQueueItem item,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SyncQueueItem item,
        CancellationToken cancellationToken = default);
}

