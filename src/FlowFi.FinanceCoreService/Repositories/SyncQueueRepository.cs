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

    public async Task<IReadOnlyList<SyncQueue>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncQueue
            .AsNoTracking()
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<SyncQueue?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SyncQueue
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<SyncQueue> AddAsync(
        SyncQueue item,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SyncQueue.AddAsync(item, cancellationToken);
        return item;
    }

    public Task UpdateAsync(
        SyncQueue item,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SyncQueue.Update(item);
        return Task.CompletedTask;
    }
}

