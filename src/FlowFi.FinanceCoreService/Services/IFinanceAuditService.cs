using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Services;

public interface IFinanceAuditService
{
    Task<FinanceAuditLog> LogAsync(
        Guid userId,
        string entityType,
        Guid entityId,
        AuditAction action,
        object? oldData,
        object? newData,
        CancellationToken cancellationToken = default);

    Task<FinanceAuditLog> LogCreateAsync<T>(
        Guid userId,
        string entityType,
        Guid entityId,
        T newData,
        CancellationToken cancellationToken = default);

    Task<FinanceAuditLog> LogUpdateAsync<TOld, TNew>(
        Guid userId,
        string entityType,
        Guid entityId,
        TOld oldData,
        TNew newData,
        CancellationToken cancellationToken = default);

    Task<FinanceAuditLog> LogDeleteAsync<T>(
        Guid userId,
        string entityType,
        Guid entityId,
        T oldData,
        CancellationToken cancellationToken = default);
}

