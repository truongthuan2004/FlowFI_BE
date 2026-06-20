using FlowFi.AIProcessingService.Database;
using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AIProcessingService.Repositories;

public sealed class AiProcessingRepository(AiProcessingDbContext dbContext) : IAiProcessingRepository
{
    public async Task<IReadOnlyList<AiProcessingRequest>> GetRequestsAsync(Guid? userId, CancellationToken cancellationToken)
    {
        var query = dbContext.AiProcessingRequests
            .Include(x => x.Result)
            .OrderByDescending(x => x.CreatedAt)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<AiProcessingRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.AiProcessingRequests
            .Include(x => x.Result)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void AddRequest(AiProcessingRequest request)
    {
        dbContext.AiProcessingRequests.Add(request);
    }

    public void UpdateRequest(AiProcessingRequest request)
    {
        dbContext.AiProcessingRequests.Update(request);
    }

    public Task<AiProcessingResult?> GetResultByRequestIdAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return dbContext.AiProcessingResults
            .Include(x => x.Request)
            .FirstOrDefaultAsync(x => x.RequestId == requestId, cancellationToken);
    }

    public void AddResult(AiProcessingResult result)
    {
        dbContext.AiProcessingResults.Add(result);
    }
}
