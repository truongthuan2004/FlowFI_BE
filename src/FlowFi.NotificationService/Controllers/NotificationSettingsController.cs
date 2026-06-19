using FlowFi.Common.Api;
using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.NotificationService.Controllers;

[ApiController]
[Authorize]
[Route("notification-settings")]
public sealed class NotificationSettingsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationSettingsDto>> Get(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        var settings = await notificationService.GetSettingsAsync(userId, cancellationToken);
        return settings is null ? NotFound() : Ok(settings);
    }

    [HttpPut]
    public async Task<ActionResult<NotificationSettingsDto>> Update([FromBody] UpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await notificationService.UpdateSettingsAsync(userId, request, cancellationToken));
    }
}
