using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[ApiController]
[Route("budgets")]
public sealed class BudgetsController(Ianalyticservice analyticservice) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Budget>>> Get(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await analyticservice.GetBudgetsAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Budget>> Create(Budget budget, CancellationToken cancellationToken)
    {
        var created = await analyticservice.CreateBudgetAsync(budget, cancellationToken);
        return Created($"/budgets/{created.Id}", created);
    }
}

