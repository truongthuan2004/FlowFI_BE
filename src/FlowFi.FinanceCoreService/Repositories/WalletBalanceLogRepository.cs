using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class WalletBalanceLogRepository : IWalletBalanceLogRepository
{
    private readonly FinanceDbContext _dbContext;

    public WalletBalanceLogRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WalletBalanceLog>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WalletBalanceLogs
            .AsNoTracking()
            .OrderByDescending(balanceLog => balanceLog.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WalletBalanceLog>> GetByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WalletBalanceLogs
            .AsNoTracking()
            .Where(balanceLog => balanceLog.WalletId == walletId)
            .OrderByDescending(balanceLog => balanceLog.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<WalletBalanceLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.WalletBalanceLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(balanceLog => balanceLog.Id == id, cancellationToken);
    }

    public async Task<WalletBalanceLog> AddAsync(
        WalletBalanceLog balanceLog,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.WalletBalanceLogs.AddAsync(balanceLog, cancellationToken);
        return balanceLog;
    }

    public async Task AddRangeAsync(
        IEnumerable<WalletBalanceLog> balanceLogs,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.WalletBalanceLogs.AddRangeAsync(balanceLogs, cancellationToken);
    }

    public Task UpdateAsync(
        WalletBalanceLog balanceLog,
        CancellationToken cancellationToken = default)
    {
        _dbContext.WalletBalanceLogs.Update(balanceLog);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        WalletBalanceLog balanceLog,
        CancellationToken cancellationToken = default)
    {
        _dbContext.WalletBalanceLogs.Remove(balanceLog);
        return Task.CompletedTask;
    }
}
