using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AIProcessingService.Controllers;

[ApiController]
[Route("insights")]
public sealed class InsightsController(IAiService aiService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AiInsight>>> Get(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await aiService.GetInsightsAsync(userId, cancellationToken));
    }
}

