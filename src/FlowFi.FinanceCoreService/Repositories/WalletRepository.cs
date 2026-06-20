using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly FinanceDbContext _dbContext;

    public WalletRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Wallet>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .AsNoTracking()
            .OrderBy(wallet => wallet.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Wallet?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(wallet => wallet.Id == id, cancellationToken);
    }

    public async Task<Wallet> AddAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
        return wallet;
    }

    public Task UpdateAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Wallets.Update(wallet);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Wallets.Remove(wallet);
        return Task.CompletedTask;
    }
}

