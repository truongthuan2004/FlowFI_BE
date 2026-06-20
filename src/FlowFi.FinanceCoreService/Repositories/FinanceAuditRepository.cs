using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public sealed class FinanceAuditRepository(FinanceDbContext dbContext)
    : IFinanceAuditRepository
{
    public async Task AddAsync(
        FinanceAuditLog auditLog,
        CancellationToken cancellationToken = default)
    {
        await dbContext.FinanceAuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
