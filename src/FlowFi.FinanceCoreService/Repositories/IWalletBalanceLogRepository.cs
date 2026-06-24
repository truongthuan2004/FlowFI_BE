using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface IWalletBalanceLogRepository
{
    Task<IReadOnlyList<WalletBalanceLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WalletBalanceLog>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);
    Task<WalletBalanceLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WalletBalanceLog> AddAsync(
        WalletBalanceLog balanceLog,
        CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IEnumerable<WalletBalanceLog> balanceLogs,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(WalletBalanceLog balanceLog, CancellationToken cancellationToken = default);
    Task DeleteAsync(WalletBalanceLog balanceLog, CancellationToken cancellationToken = default);
}
