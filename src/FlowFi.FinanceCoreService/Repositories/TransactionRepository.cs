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
        await using var databaseTransaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Lock the wallet row until commit so concurrent balance updates cannot be lost.
            var wallet = await _dbContext.Wallets
                .FromSqlInterpolated(
                    $"SELECT * FROM wallets WHERE id = {transaction.WalletId} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);

            if (wallet is null || wallet.UserId != transaction.UserId)
            {
                await databaseTransaction.RollbackAsync(cancellationToken);
                return new TransactionCreationResult(TransactionCreationStatus.WalletNotFound);
            }

            var tagExists = await _dbContext.Tags.AnyAsync(
                tag => tag.Id == transaction.TagId && tag.UserId == transaction.UserId,
                cancellationToken);

            if (!tagExists)
            {
                await databaseTransaction.RollbackAsync(cancellationToken);
                return new TransactionCreationResult(TransactionCreationStatus.TagNotFound);
            }

            var oldBalance = wallet.Balance;
            var newBalance = oldBalance + balanceChange;
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
            await _dbContext.SaveChangesAsync(cancellationToken);
            await databaseTransaction.CommitAsync(cancellationToken);

            return new TransactionCreationResult(
                TransactionCreationStatus.Success,
                transaction);
        }
        catch
        {
            await databaseTransaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}

