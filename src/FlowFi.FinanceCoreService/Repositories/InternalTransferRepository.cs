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

    public async Task<IReadOnlyList<InternalTransfer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.InternalTransfers
            .AsNoTracking()
            .OrderByDescending(transfer => transfer.TransferDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InternalTransfer>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.InternalTransfers
            .AsNoTracking()
            .Where(transfer => transfer.FromWalletId == walletId || transfer.ToWalletId == walletId)
            .OrderByDescending(transfer => transfer.TransferDate)
            .ToListAsync(cancellationToken);
    }

    public Task<InternalTransfer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InternalTransfers
            .AsNoTracking()
            .FirstOrDefaultAsync(transfer => transfer.Id == id, cancellationToken);
    }

    public async Task<InternalTransfer> AddAsync(
        InternalTransfer transfer,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.InternalTransfers.AddAsync(transfer, cancellationToken);
        return transfer;
    }

    public Task UpdateAsync(
        InternalTransfer transfer,
        CancellationToken cancellationToken = default)
    {
        _dbContext.InternalTransfers.Update(transfer);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        InternalTransfer transfer,
        CancellationToken cancellationToken = default)
    {
        _dbContext.InternalTransfers.Remove(transfer);
        return Task.CompletedTask;
    }
}
