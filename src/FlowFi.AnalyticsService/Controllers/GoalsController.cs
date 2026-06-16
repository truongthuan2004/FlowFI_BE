using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[ApiController]
[Route("goals")]
public sealed class GoalsController(Ianalyticservice analyticservice) : ControllerBase
{
    [HttpPost("{goalId:guid}/progress")]
    public async Task<ActionResult<Goal>> UpdateProgress(Guid goalId, GoalProgressRequest request, CancellationToken cancellationToken)
    {
        var updated = await analyticservice.UpdateGoalProgressAsync(goalId, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}

