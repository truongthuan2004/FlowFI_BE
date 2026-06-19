using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AIProcessingService.Controllers;

[ApiController]
[Route("api/ai-processing/requests")]
public sealed class AiProcessingRequestsController(IAiProcessingService aiProcessingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var requests = await aiProcessingService.GetRequestsAsync(userId, cancellationToken);
        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRequest(Guid id, CancellationToken cancellationToken)
    {
        var request = await aiProcessingService.GetRequestAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateAiProcessingRequestDto dto, CancellationToken cancellationToken)
    {
        var request = await aiProcessingService.CreateRequestAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
    }
}
