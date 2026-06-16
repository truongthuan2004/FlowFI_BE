using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Interface;

public interface IFinanceService
{
    Task<IReadOnlyList<Wallet>> GetWalletsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Wallet> CreateWalletAsync(Wallet wallet, CancellationToken cancellationToken);
    Task<IReadOnlyList<Transaction>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
}

