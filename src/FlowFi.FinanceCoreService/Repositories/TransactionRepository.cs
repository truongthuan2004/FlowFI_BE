using System.Text.Json;
using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _dbContext;

    public TransactionRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionCreationResult> CreateAsync(
        Transaction transaction,
        decimal balanceChange,
        CancellationToken cancellationToken = default)
    {
        // The service owns the UnitOfWork transaction. This lock remains held until it commits.
        var wallet = await _dbContext.Wallets
            .FromSqlInterpolated(
                $"SELECT * FROM wallets WHERE id = {transaction.WalletId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        // Validate that the wallet exists and belongs to the authenticated user.
        if (wallet is null || wallet.UserId != transaction.UserId)
        {
            return new TransactionCreationResult(TransactionCreationStatus.WalletNotFound);
        }

        if (!wallet.IsActive)
        {
            return new TransactionCreationResult(TransactionCreationStatus.WalletInactive);
        }

        var tag = await _dbContext.Tags.SingleOrDefaultAsync(
                tag => tag.Id == transaction.TagId && tag.UserId == transaction.UserId,
                cancellationToken);

        if (tag is null)
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
                Reason = transaction.Type,
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

        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
        await _dbContext.WalletBalanceLogs.AddAsync(balanceLog, cancellationToken);
        await _dbContext.FinanceAuditLogs.AddAsync(auditLog, cancellationToken);
        return new TransactionCreationResult(
            TransactionCreationStatus.Success,
            transaction);
    }
}

