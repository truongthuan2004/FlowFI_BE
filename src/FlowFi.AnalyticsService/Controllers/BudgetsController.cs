using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[ApiController]
[Route("budgets")]
public sealed class BudgetsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Budget>>> Get(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await analyticsService.GetBudgetsAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Budget>> Create(Budget budget, CancellationToken cancellationToken)
    {
        var created = await analyticsService.CreateBudgetAsync(budget, cancellationToken);
        return Created($"/budgets/{created.Id}", created);
    }
}

