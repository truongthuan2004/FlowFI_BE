using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AIProcessingService.Controllers;

[ApiController]
[Route("jobs")]
public sealed class AiJobsController(IAiService aiService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AiJob>> Create(AiJob job, CancellationToken cancellationToken)
    {
        var created = await aiService.QueueJobAsync(job, cancellationToken);
        return Accepted($"/jobs/{created.Id}", created);
    }
}

