using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Interface;

public interface IWalletService
{
    Task<IReadOnlyList<WalletDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WalletDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WalletDto> CreateAsync(Guid userId, CreateWalletDto request, CancellationToken cancellationToken = default);
    Task<WalletDto?> UpdateAsync(Guid id, UpdateWalletDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

