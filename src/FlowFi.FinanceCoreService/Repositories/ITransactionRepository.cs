using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface ITransactionRepository
{
    Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction> AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task DeleteAsync(Transaction transaction, CancellationToken cancellationToken = default);
}

