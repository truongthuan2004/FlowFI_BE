using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetByUserAndTypeAsync(
        Guid userId,
        string type,
        CancellationToken cancellationToken = default);
    Task<Tag?> FindByUserTypeAndNameAsync(
        Guid userId,
        string type,
        string name,
        CancellationToken cancellationToken = default);
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
    Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);
}

