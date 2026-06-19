using System.Text.Json;
using System.Text.Json.Serialization;
using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Services;

public class FinanceAuditService : IFinanceAuditService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private readonly FinanceDbContext _dbContext;

    public FinanceAuditService(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FinanceAuditLog> LogAsync(
        Guid userId,
        string entityType,
        Guid entityId,
        AuditAction action,
        object? oldData,
        object? newData,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        var normalizedEntityType = entityType.Trim().ToUpperInvariant();
        if (normalizedEntityType.Length > 30)
        {
            throw new ArgumentException(
                "Entity type cannot exceed 30 characters.",
                nameof(entityType));
        }

        var auditLog = new FinanceAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EntityType = normalizedEntityType,
            EntityId = entityId,
            Action = action.ToString().ToUpperInvariant(),
            OldData = Serialize(oldData),
            NewData = Serialize(newData),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.FinanceAuditLogs.AddAsync(auditLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return auditLog;
    }

    public Task<FinanceAuditLog> LogCreateAsync<T>(
        Guid userId,
        string entityType,
        Guid entityId,
        T newData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newData);
        return LogAsync(
            userId,
            entityType,
            entityId,
            AuditAction.Create,
            oldData: null,
            newData,
            cancellationToken);
    }

    public Task<FinanceAuditLog> LogUpdateAsync<TOld, TNew>(
        Guid userId,
        string entityType,
        Guid entityId,
        TOld oldData,
        TNew newData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(oldData);
        ArgumentNullException.ThrowIfNull(newData);
        return LogAsync(
            userId,
            entityType,
            entityId,
            AuditAction.Update,
            oldData,
            newData,
            cancellationToken);
    }

    public Task<FinanceAuditLog> LogDeleteAsync<T>(
        Guid userId,
        string entityType,
        Guid entityId,
        T oldData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(oldData);
        return LogAsync(
            userId,
            entityType,
            entityId,
            AuditAction.Delete,
            oldData,
            newData: null,
            cancellationToken);
    }

    private static JsonDocument? Serialize(object? data)
    {
        return data is null
            ? null
            : JsonSerializer.SerializeToDocument(
                data,
                data.GetType(),
                SerializerOptions);
    }
}

