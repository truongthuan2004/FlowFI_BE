using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class InternalTransferService : IInternalTransferService
{
    private readonly IInternalTransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InternalTransferService(
        IInternalTransferRepository transferRepository,
        IUnitOfWork unitOfWork)
    {
        _transferRepository = transferRepository;
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
            var creationResult = await _transferRepository.CreateAsync(
                transfer,
                cancellationToken);
            if (creationResult.Status == InternalTransferCreationStatus.Success)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return creationResult;
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

