using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface IInternalTransferRepository
{
    Task<IReadOnlyList<InternalTransfer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InternalTransfer>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);
    Task<InternalTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InternalTransfer> AddAsync(
        InternalTransfer transfer,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(InternalTransfer transfer, CancellationToken cancellationToken = default);
    Task DeleteAsync(InternalTransfer transfer, CancellationToken cancellationToken = default);
}

