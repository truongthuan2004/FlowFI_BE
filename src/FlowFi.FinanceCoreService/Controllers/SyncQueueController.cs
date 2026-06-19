using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[Authorize]
[ApiController]
[Route("api/sync-queue")]
public class SyncQueueController : ControllerBase
{
    private readonly ISyncQueueService _service;

    public SyncQueueController(ISyncQueueService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SyncQueueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SyncQueueDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SyncQueueDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SyncQueueDto>> Enqueue(
        CreateSyncQueueDto request,
        CancellationToken cancellationToken)
    {
        var item = await _service.EnqueueAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, item);
    }
}

