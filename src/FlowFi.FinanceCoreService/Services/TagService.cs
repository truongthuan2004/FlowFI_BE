using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TagDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        return tags.Select(MapToDto).ToList();
    }

    public async Task<TagDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        return tag is null ? null : MapToDto(tag);
    }

    public async Task<IReadOnlyList<TagDto>> GetByUserAndTypeAsync(
        Guid userId,
        string type,
        CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetByUserAndTypeAsync(
            userId,
            type.Trim().ToUpperInvariant(),
            cancellationToken);
        return tags.Select(MapToDto).ToList();
    }

    public async Task<TagDto> GetOrCreateAsync(
        CreateTagDto request,
        CancellationToken cancellationToken = default)
    {
        var type = request.Type.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        var existing = await _tagRepository.FindByUserTypeAndNameAsync(
            request.UserId,
            type,
            name,
            cancellationToken);
        if (existing is not null)
        {
            return MapToDto(existing);
        }

        request.Type = type;
        request.Name = name;
        return await CreateAsync(request, cancellationToken);
    }

    public async Task<TagDto> CreateAsync(
        CreateTagDto request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Icon = request.Icon.Trim(),
            Color = request.Color.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdTag = await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(createdTag);
    }

    public async Task<TagDto?> UpdateAsync(
        Guid id,
        UpdateTagDto request,
        CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        if (tag is null)
        {
            return null;
        }

        tag.UserId = request.UserId;
        tag.Name = request.Name.Trim();
        tag.Type = request.Type.Trim();
        tag.Icon = request.Icon.Trim();
        tag.Color = request.Color.Trim();
        tag.UpdatedAt = DateTimeOffset.UtcNow;

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(tag);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        if (tag is null)
        {
            return false;
        }

        await _tagRepository.DeleteAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            UserId = tag.UserId,
            Name = tag.Name,
            Type = tag.Type,
            Icon = tag.Icon,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }
}

