using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class SyncQueueRepository : ISyncQueueRepository
{
    private readonly FinanceDbContext _dbContext;

    public SyncQueueRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncQueue
            .AsNoTracking()
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<SyncQueueItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SyncQueue
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<SyncQueueItem> AddAsync(
        SyncQueueItem item,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SyncQueue.AddAsync(item, cancellationToken);
        return item;
    }

    public Task UpdateAsync(
        SyncQueueItem item,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SyncQueue.Update(item);
        return Task.CompletedTask;
    }
}

