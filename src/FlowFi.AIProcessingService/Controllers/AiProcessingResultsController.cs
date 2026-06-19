using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AIProcessingService.Controllers;

[ApiController]
[Route("api/ai-processing/results")]
public sealed class AiProcessingResultsController(IAiProcessingService aiProcessingService) : ControllerBase
{
    [HttpGet("{requestId:guid}")]
    public async Task<IActionResult> GetResult(Guid requestId, CancellationToken cancellationToken)
    {
        var result = await aiProcessingService.GetResultByRequestIdAsync(requestId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateResult([FromBody] CreateAiProcessingResultDto dto, CancellationToken cancellationToken)
    {
        var result = await aiProcessingService.CreateResultAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetResult), new { requestId = result.RequestId }, result);
    }
}
