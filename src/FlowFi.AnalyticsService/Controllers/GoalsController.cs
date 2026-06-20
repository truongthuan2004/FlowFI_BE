using FlowFi.Common.Api;
using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[Authorize]
[ApiController]
[Route("goals")]
public sealed class GoalsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SavingGoalResponse>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await analyticsService.GetSavingGoalsAsync(User.UserId(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SavingGoalResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var goal = await analyticsService.GetSavingGoalAsync(User.UserId(), id, cancellationToken);
        return goal is null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public async Task<ActionResult<SavingGoalResponse>> Create(CreateSavingGoalRequest request, CancellationToken cancellationToken)
    {
        var created = await analyticsService.CreateSavingGoalAsync(User.UserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SavingGoalResponse>> Update(Guid id, UpdateSavingGoalRequest request, CancellationToken cancellationToken)
    {
        var updated = await analyticsService.UpdateSavingGoalAsync(User.UserId(), id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("{goalId:guid}/progress")]
    public async Task<ActionResult<SavingGoalResponse>> UpdateProgress(Guid goalId, UpdateGoalProgressRequest request, CancellationToken cancellationToken)
    {
        var updated = await analyticsService.UpdateGoalProgressAsync(User.UserId(), goalId, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await analyticsService.DeleteSavingGoalAsync(User.UserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{goalId:guid}/contributions")]
    public async Task<ActionResult<IReadOnlyList<GoalContributionResponse>>> GetContributions(Guid goalId, CancellationToken cancellationToken)
    {
        var contributions = await analyticsService.GetGoalContributionsAsync(User.UserId(), goalId, cancellationToken);
        return contributions is null ? NotFound() : Ok(contributions);
    }

    [HttpPost("{goalId:guid}/contributions")]
    public async Task<ActionResult<GoalContributionResponse>> CreateContribution(
        Guid goalId,
        CreateGoalContributionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await analyticsService.AddGoalContributionAsync(User.UserId(), goalId, request, cancellationToken);
        return created is null
            ? NotFound()
            : Created($"/goals/{goalId}/contributions/{created.Id}", created);
    }
}

