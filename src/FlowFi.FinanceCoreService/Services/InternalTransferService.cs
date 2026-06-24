using System.Text.Json;
using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class InternalTransferService : IInternalTransferService
{
    private readonly IInternalTransferRepository _transferRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletBalanceLogRepository _walletBalanceLogRepository;
    private readonly IFinanceAuditRepository _financeAuditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InternalTransferService(
        IInternalTransferRepository transferRepository,
        IWalletRepository walletRepository,
        IWalletBalanceLogRepository walletBalanceLogRepository,
        IFinanceAuditRepository financeAuditRepository,
        IUnitOfWork unitOfWork)
    {
        _transferRepository = transferRepository;
        _walletRepository = walletRepository;
        _walletBalanceLogRepository = walletBalanceLogRepository;
        _financeAuditRepository = financeAuditRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInternalTransferResult> CreateAsync(
        CreateInternalTransferDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.FromWalletId == request.ToWalletId)
        {
            return new CreateInternalTransferResult(
                CreateInternalTransferStatus.SameWallet);
        }

        var now = DateTimeOffset.UtcNow;
        var transfer = new InternalTransfer
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FromWalletId = request.FromWalletId,
            ToWalletId = request.ToWalletId,
            Amount = request.Amount,
            Note = request.Note.Trim(),
            SyncStatus = request.SyncStatus.Trim(),
            TransferDate = request.TransferDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var wallets = await _walletRepository.GetForUpdateAsync(
                [transfer.FromWalletId, transfer.ToWalletId],
                cancellationToken);

            var sourceWallet = wallets.SingleOrDefault(
                wallet => wallet.Id == transfer.FromWalletId &&
                          wallet.UserId == transfer.UserId);
            if (sourceWallet is null)
            {
                return new InternalTransferCreationResult(
                    InternalTransferCreationStatus.SourceWalletNotFound);
            }

            var destinationWallet = wallets.SingleOrDefault(
                wallet => wallet.Id == transfer.ToWalletId &&
                          wallet.UserId == transfer.UserId);
            if (destinationWallet is null)
            {
                return new InternalTransferCreationResult(
                    InternalTransferCreationStatus.DestinationWalletNotFound);
            }

            if (sourceWallet.Balance < transfer.Amount)
            {
                return new InternalTransferCreationResult(
                    InternalTransferCreationStatus.InsufficientBalance);
            }

            var sourceOldBalance = sourceWallet.Balance;
            var destinationOldBalance = destinationWallet.Balance;
            sourceWallet.Balance -= transfer.Amount;
            destinationWallet.Balance += transfer.Amount;
            sourceWallet.UpdatedAt = transfer.CreatedAt;
            destinationWallet.UpdatedAt = transfer.CreatedAt;

            var sourceBalanceLog = new WalletBalanceLog
            {
                Id = Guid.NewGuid(),
                WalletId = sourceWallet.Id,
                TransferId = transfer.Id,
                OldBalance = sourceOldBalance,
                ChangeAmount = -transfer.Amount,
                NewBalance = sourceWallet.Balance,
                Reason = "INTERNAL_TRANSFER_OUT",
                CreatedAt = transfer.CreatedAt
            };

            var destinationBalanceLog = new WalletBalanceLog
            {
                Id = Guid.NewGuid(),
                WalletId = destinationWallet.Id,
                TransferId = transfer.Id,
                OldBalance = destinationOldBalance,
                ChangeAmount = transfer.Amount,
                NewBalance = destinationWallet.Balance,
                Reason = "INTERNAL_TRANSFER_IN",
                CreatedAt = transfer.CreatedAt
            };

            var auditLog = new FinanceAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = transfer.UserId,
                EntityType = "INTERNAL_TRANSFER",
                EntityId = transfer.Id,
                Action = "CREATE",
                OldData = null,
                NewData = JsonSerializer.SerializeToDocument(new
                {
                    transfer.Id,
                    transfer.UserId,
                    transfer.FromWalletId,
                    transfer.ToWalletId,
                    transfer.Amount,
                    transfer.Note,
                    transfer.SyncStatus,
                    transfer.TransferDate,
                    SourceOldBalance = sourceOldBalance,
                    SourceNewBalance = sourceWallet.Balance,
                    DestinationOldBalance = destinationOldBalance,
                    DestinationNewBalance = destinationWallet.Balance
                }),
                CreatedAt = transfer.CreatedAt
            };

            await _transferRepository.AddAsync(transfer, cancellationToken);
            await _walletBalanceLogRepository.AddRangeAsync(
                [sourceBalanceLog, destinationBalanceLog],
                cancellationToken);
            await _financeAuditRepository.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new InternalTransferCreationResult(
                InternalTransferCreationStatus.Success,
                transfer);
        }, cancellationToken);

        return result.Status switch
        {
            InternalTransferCreationStatus.SourceWalletNotFound =>
                new CreateInternalTransferResult(
                    CreateInternalTransferStatus.SourceWalletNotFound),
            InternalTransferCreationStatus.DestinationWalletNotFound =>
                new CreateInternalTransferResult(
                    CreateInternalTransferStatus.DestinationWalletNotFound),
            InternalTransferCreationStatus.InsufficientBalance =>
                new CreateInternalTransferResult(
                    CreateInternalTransferStatus.InsufficientBalance),
            _ => new CreateInternalTransferResult(
                CreateInternalTransferStatus.Success,
                MapToDto(result.Transfer!))
        };
    }

    private static InternalTransferDto MapToDto(InternalTransfer transfer)
    {
        return new InternalTransferDto
        {
            Id = transfer.Id,
            UserId = transfer.UserId,
            FromWalletId = transfer.FromWalletId,
            ToWalletId = transfer.ToWalletId,
            Amount = transfer.Amount,
            Note = transfer.Note,
            SyncStatus = transfer.SyncStatus,
            TransferDate = transfer.TransferDate,
            CreatedAt = transfer.CreatedAt,
            UpdatedAt = transfer.UpdatedAt
        };
    }
}
