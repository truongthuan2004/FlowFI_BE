using FlowFi.Common.Api;
using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AnalyticsService.Controllers;

[Authorize]
[ApiController]
[Route("financial-summaries")]
public sealed class FinancialSummariesController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FinancialSummaryResponse>> Get(
        [FromQuery] FinancialSummaryQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await analyticsService.GetFinancialSummaryAsync(User.UserId(), query, cancellationToken));
    }
}
