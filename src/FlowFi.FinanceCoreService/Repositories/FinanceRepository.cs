using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public sealed class FinanceRepository(FinanceDbContext db) : IFinanceRepository
{
    public async Task<IReadOnlyList<Wallet>> GetWalletsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Wallets.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Wallet> AddWalletAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task<IReadOnlyList<Transaction>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Transactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction> AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return transaction;
    }
}

