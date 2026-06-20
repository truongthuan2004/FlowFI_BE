using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface IFinanceAuditRepository
{
    Task AddAsync(
        FinanceAuditLog auditLog,
        CancellationToken cancellationToken = default);
}
