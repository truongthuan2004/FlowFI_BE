using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface ISyncQueueRepository
{
    Task<IReadOnlyList<SyncQueue>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SyncQueue?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SyncQueue> AddAsync(
        SyncQueue item,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SyncQueue item,
        CancellationToken cancellationToken = default);
}

