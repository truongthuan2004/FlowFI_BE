using FlowFi.Common.Api;
using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[Authorize]
[ApiController]
[Route("budgets")]
public sealed class BudgetsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetResponse>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await analyticsService.GetBudgetsAsync(User.UserId(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var budget = await analyticsService.GetBudgetAsync(User.UserId(), id, cancellationToken);
        return budget is null ? NotFound() : Ok(budget);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetResponse>> Create(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var created = await analyticsService.CreateBudgetAsync(User.UserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BudgetResponse>> Update(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var updated = await analyticsService.UpdateBudgetAsync(User.UserId(), id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await analyticsService.DeleteBudgetAsync(User.UserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

