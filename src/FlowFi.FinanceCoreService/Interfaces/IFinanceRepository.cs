using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Interfaces;

public interface IFinanceRepository
{
    Task<IReadOnlyList<Wallet>> GetWalletsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Wallet> AddWalletAsync(Wallet wallet, CancellationToken cancellationToken);
    Task<IReadOnlyList<Transaction>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Transaction> AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
}

