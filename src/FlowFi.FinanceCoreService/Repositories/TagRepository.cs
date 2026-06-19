using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Repositories;

public class TagRepository : ITagRepository
{
    private readonly FinanceDbContext _dbContext;

    public TagRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Tag?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);
    }

    public async Task<Tag> AddAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Tags.AddAsync(tag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return tag;
    }

    public async Task UpdateAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Tags.Update(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

