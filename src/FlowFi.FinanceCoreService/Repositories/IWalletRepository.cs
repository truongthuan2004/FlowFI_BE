using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface IWalletRepository
{
    Task<IReadOnlyList<Wallet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Wallet?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Wallet?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Wallet>> GetForUpdateAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
    Task<Wallet> AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task DeleteAsync(Wallet wallet, CancellationToken cancellationToken = default);
}

