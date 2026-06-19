using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Entities;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[ApiController]
[Route("goals")]
public sealed class GoalsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpPost("{goalId:guid}/progress")]
    public async Task<ActionResult<SavingGoal>> UpdateProgress(Guid goalId, GoalProgressRequest request, CancellationToken cancellationToken)
    {
        var updated = await analyticsService.UpdateGoalProgressAsync(goalId, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}

