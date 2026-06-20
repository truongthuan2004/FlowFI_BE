using System.Text.Json;
using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class InternalTransferRepository : IInternalTransferRepository
{
    private readonly FinanceDbContext _dbContext;

    public InternalTransferRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InternalTransferCreationResult> CreateAsync(
        InternalTransfer transfer,
        CancellationToken cancellationToken = default)
    {
        // The service owns the transaction; a stable lock order avoids opposite-transfer deadlocks.
            var wallets = await _dbContext.Wallets
                .FromSqlInterpolated($"""
                    SELECT * FROM wallets
                    WHERE id = {transfer.FromWalletId} OR id = {transfer.ToWalletId}
                    ORDER BY id
                    FOR UPDATE
                    """)
                .ToListAsync(cancellationToken);

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

            await _dbContext.InternalTransfers.AddAsync(transfer, cancellationToken);
            await _dbContext.WalletBalanceLogs.AddRangeAsync(
                new[] { sourceBalanceLog, destinationBalanceLog },
                cancellationToken);
            await _dbContext.FinanceAuditLogs.AddAsync(auditLog, cancellationToken);
            return new InternalTransferCreationResult(
                InternalTransferCreationStatus.Success,
                transfer);
    }
}

