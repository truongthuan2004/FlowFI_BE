using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[ApiController]
[Route("budgets")]
public sealed class BudgetsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetResponse>>> Get([FromQuery] GetBudgetsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await analyticsService.GetBudgetsAsync(query.UserId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var budget = await analyticsService.GetBudgetAsync(id, cancellationToken);
        return budget is null ? NotFound() : Ok(budget);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetResponse>> Create(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var created = await analyticsService.CreateBudgetAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BudgetResponse>> Update(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var updated = await analyticsService.UpdateBudgetAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await analyticsService.DeleteBudgetAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

