using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTransactionResult> CreateAsync(
        Guid userId,
        Guid walletId,
        CreateTransactionDto request,
        DateTimeOffset? transactionDate,
        CancellationToken cancellationToken = default)
    {
        if (walletId == Guid.Empty)
        {
            return new CreateTransactionResult(CreateTransactionStatus.InvalidWalletId);
        }

        var type = request.Type.Trim().ToUpperInvariant();
        if (type is not ("INCOME" or "EXPENSE"))
        {
            return new CreateTransactionResult(CreateTransactionStatus.InvalidTransactionType);
        }

        var now = DateTimeOffset.UtcNow;
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletId = walletId,
            TagId = request.TagId,
            Amount = request.Amount,
            Type = type,
            Title = request.Title.Trim(),
            Note = request.Note.Trim(),
            Source = request.Source.Trim().ToUpperInvariant(),
            SyncStatus = "SYNCED",
            TransactionDate = transactionDate ?? now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var balanceChange = type switch
        {
            "INCOME" => request.Amount,
            "EXPENSE" => -request.Amount,
            _ => throw new InvalidOperationException("Unsupported transaction type.")
        };
        var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var creationResult = await _transactionRepository.CreateAsync(
                transaction,
                balanceChange,
                cancellationToken);
            if (creationResult.Status == TransactionCreationStatus.Success)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return creationResult;
        }, cancellationToken);

        return result.Status switch
        {
            TransactionCreationStatus.WalletNotFound =>
                new CreateTransactionResult(CreateTransactionStatus.WalletNotFound),
            TransactionCreationStatus.WalletInactive =>
                new CreateTransactionResult(CreateTransactionStatus.WalletInactive),
            TransactionCreationStatus.TagNotFound =>
                new CreateTransactionResult(CreateTransactionStatus.TagNotFound),
            TransactionCreationStatus.TagTypeMismatch =>
                new CreateTransactionResult(CreateTransactionStatus.TagTypeMismatch),
            TransactionCreationStatus.InsufficientBalance =>
                new CreateTransactionResult(CreateTransactionStatus.InsufficientBalance),
            _ => new CreateTransactionResult(
                CreateTransactionStatus.Success,
                MapToDto(result.Transaction!))
        };
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            WalletId = transaction.WalletId,
            TagId = transaction.TagId,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Title = transaction.Title,
            Note = transaction.Note,
            Source = transaction.Source,
            SyncStatus = transaction.SyncStatus,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}

