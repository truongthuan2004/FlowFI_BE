using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _dbContext;

    public TransactionRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.WalletId == walletId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);
    }

    public async Task<Transaction> AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
        return transaction;
    }

    public Task UpdateAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }
}
