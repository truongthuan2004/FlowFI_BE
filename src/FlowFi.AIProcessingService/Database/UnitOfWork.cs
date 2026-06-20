using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Database;

public sealed class UnitOfWork(AiProcessingDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
