using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WalletDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var wallets = await _walletRepository.GetAllAsync(cancellationToken);
        return wallets.Select(MapToDto).ToList();
    }

    public async Task<WalletDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(id, cancellationToken);
        return wallet is null ? null : MapToDto(wallet);
    }

    public async Task<WalletDto> CreateAsync(
        Guid userId,
        CreateWalletDto request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            WalletType = request.WalletType.Trim(),
            Balance = request.Balance,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdWallet = await _walletRepository.AddAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(createdWallet);
    }

    public async Task<WalletDto?> UpdateAsync(
        Guid id,
        UpdateWalletDto request,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(id, cancellationToken);
        if (wallet is null)
        {
            return null;
        }

        wallet.UserId = request.UserId;
        wallet.Name = request.Name.Trim();
        wallet.WalletType = request.WalletType.Trim();
        wallet.Balance = request.Balance;
        wallet.Currency = request.Currency.Trim().ToUpperInvariant();
        wallet.IsActive = request.IsActive;
        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(wallet);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByIdAsync(id, cancellationToken);
        if (wallet is null)
        {
            return false;
        }

        await _walletRepository.DeleteAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static WalletDto MapToDto(Wallet wallet)
    {
        return new WalletDto
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            Name = wallet.Name,
            WalletType = wallet.WalletType,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            IsActive = wallet.IsActive,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }
}

