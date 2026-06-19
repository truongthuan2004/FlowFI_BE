using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class RecurringTransactionRepository : IRecurringTransactionRepository
{
    private readonly FinanceDbContext _dbContext;

    public RecurringTransactionRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecurringTransaction>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecurringTransactions
            .AsNoTracking()
            .OrderBy(transaction => transaction.NextRunAt)
            .ToListAsync(cancellationToken);
    }

    public Task<RecurringTransaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RecurringTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);
    }

    public async Task<RecurringTransaction> AddAsync(
        RecurringTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RecurringTransactions.AddAsync(transaction, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(
        RecurringTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RecurringTransactions.Update(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        RecurringTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RecurringTransactions.Remove(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

