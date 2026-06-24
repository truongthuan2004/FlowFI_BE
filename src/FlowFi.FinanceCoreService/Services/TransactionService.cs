using System.Text.Json;
using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IWalletBalanceLogRepository _walletBalanceLogRepository;
    private readonly IFinanceAuditRepository _financeAuditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        ITagRepository tagRepository,
        IWalletBalanceLogRepository walletBalanceLogRepository,
        IFinanceAuditRepository financeAuditRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
        _tagRepository = tagRepository;
        _walletBalanceLogRepository = walletBalanceLogRepository;
        _financeAuditRepository = financeAuditRepository;
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

        if (request.Amount <= 0)
        {
            return new CreateTransactionResult(CreateTransactionStatus.InvalidAmount);
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
            var wallet = await _walletRepository.GetForUpdateAsync(
                transaction.WalletId,
                cancellationToken);
            if (wallet is null || wallet.UserId != transaction.UserId)
            {
                return new TransactionCreationResult(TransactionCreationStatus.WalletNotFound);
            }

            if (!wallet.IsActive)
            {
                return new TransactionCreationResult(TransactionCreationStatus.WalletInactive);
            }

            var tag = await _tagRepository.GetByIdAsync(transaction.TagId, cancellationToken);
            if (tag is null || tag.UserId != transaction.UserId)
            {
                return new TransactionCreationResult(TransactionCreationStatus.TagNotFound);
            }

            if (!string.Equals(tag.Type, transaction.Type, StringComparison.OrdinalIgnoreCase))
            {
                return new TransactionCreationResult(TransactionCreationStatus.TagTypeMismatch);
            }

            var oldBalance = wallet.Balance;
            var newBalance = oldBalance + balanceChange;
            if (newBalance < 0)
            {
                return new TransactionCreationResult(TransactionCreationStatus.InsufficientBalance);
            }

            wallet.Balance = newBalance;
            wallet.UpdatedAt = transaction.CreatedAt;

            var balanceLog = new WalletBalanceLog
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                TransactionId = transaction.Id,
                OldBalance = oldBalance,
                ChangeAmount = balanceChange,
                NewBalance = newBalance,
                Reason = "TRANSACTION_CREATED",
                CreatedAt = transaction.CreatedAt
            };

            var auditLog = new FinanceAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = transaction.UserId,
                EntityType = "TRANSACTION",
                EntityId = transaction.Id,
                Action = "CREATE",
                OldData = null,
                NewData = JsonSerializer.SerializeToDocument(new
                {
                    transaction.Id,
                    transaction.UserId,
                    transaction.WalletId,
                    transaction.TagId,
                    transaction.Amount,
                    transaction.Type,
                    transaction.Title,
                    transaction.Note,
                    transaction.Source,
                    transaction.SyncStatus,
                    transaction.TransactionDate,
                    OldBalance = oldBalance,
                    NewBalance = newBalance
                }),
                CreatedAt = transaction.CreatedAt
            };

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _walletBalanceLogRepository.AddAsync(balanceLog, cancellationToken);
            await _financeAuditRepository.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TransactionCreationResult(
                TransactionCreationStatus.Success,
                transaction);
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
