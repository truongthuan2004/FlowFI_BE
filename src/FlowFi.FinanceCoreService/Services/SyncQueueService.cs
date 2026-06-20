using System.Text.Json;
using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class SyncQueueService : ISyncQueueService
{
    private readonly ISyncQueueRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SyncQueueService(ISyncQueueRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SyncQueueDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<SyncQueueDto> EnqueueAsync(
        CreateSyncQueueDto request,
        CancellationToken cancellationToken = default)
    {
        var item = new SyncQueueItem
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            EntityType = request.EntityType.Trim().ToUpperInvariant(),
            EntityId = request.EntityId,
            Action = request.Action.Trim().ToUpperInvariant(),
            Payload = JsonDocument.Parse(request.Payload.GetRawText()),
            Status = "PENDING",
            RetryCount = 0,
            LastError = null,
            CreatedAt = DateTimeOffset.UtcNow,
            SyncedAt = null
        };

        var created = await _repository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(created);
    }

    public async Task<bool> UpdateStatusAsync(
        Guid id,
        SyncQueueStatus status,
        string? lastError = null,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.Status = status.ToString().ToUpperInvariant();
        item.LastError = status == SyncQueueStatus.Failed ? lastError : null;
        item.SyncedAt = status == SyncQueueStatus.Synced
            ? DateTimeOffset.UtcNow
            : null;

        if (status == SyncQueueStatus.Failed)
        {
            item.RetryCount++;
        }

        await _repository.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static SyncQueueDto MapToDto(SyncQueueItem item)
    {
        return new SyncQueueDto
        {
            Id = item.Id,
            UserId = item.UserId,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            Action = item.Action,
            Payload = item.Payload.RootElement.Clone(),
            Status = item.Status,
            RetryCount = item.RetryCount,
            LastError = item.LastError,
            CreatedAt = item.CreatedAt,
            SyncedAt = item.SyncedAt
        };
    }
}

