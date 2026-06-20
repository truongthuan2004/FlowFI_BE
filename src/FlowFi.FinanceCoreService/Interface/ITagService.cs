using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Interface;

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagDto>> GetByUserAndTypeAsync(
        Guid userId,
        string type,
        CancellationToken cancellationToken = default);
    Task<TagDto> GetOrCreateAsync(
        CreateTagDto request,
        CancellationToken cancellationToken = default);
    Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TagDto> CreateAsync(CreateTagDto request, CancellationToken cancellationToken = default);
    Task<TagDto?> UpdateAsync(Guid id, UpdateTagDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

