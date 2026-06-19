using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<CreateTransactionResult> CreateAsync(
        CreateTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var type = request.Type.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            WalletId = request.WalletId,
            TagId = request.TagId,
            Amount = request.Amount,
            Type = type,
            Title = request.Title.Trim(),
            Note = request.Note.Trim(),
            Source = request.Source.Trim(),
            SyncStatus = request.SyncStatus.Trim(),
            TransactionDate = request.TransactionDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        var balanceChange = type == "EXPENSE" ? -request.Amount : request.Amount;
        var result = await _transactionRepository.CreateAsync(
            transaction,
            balanceChange,
            cancellationToken);

        return result.Status switch
        {
            TransactionCreationStatus.WalletNotFound =>
                new CreateTransactionResult(CreateTransactionStatus.WalletNotFound),
            TransactionCreationStatus.TagNotFound =>
                new CreateTransactionResult(CreateTransactionStatus.TagNotFound),
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

