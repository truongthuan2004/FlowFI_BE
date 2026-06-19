using FlowFi.Common.Api;
using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.NotificationService.Controllers;

[ApiController]
[Authorize]
[Route("devices")]
public sealed class DevicesController(INotificationService notificationService) : ControllerBase
{
    [HttpPost("push-tokens")]
    public async Task<ActionResult<PushDevice>> RegisterPushToken([FromBody] RegisterPushTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        var device = new PushDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeviceFingerprint = request.DeviceFingerprint,
            Platform = request.Platform,
            Token = request.PushToken,
            DeviceName = request.DeviceName,
            OsVersion = request.OsVersion,
            AppVersion = request.AppVersion,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        return Ok(await notificationService.RegisterDeviceAsync(device, cancellationToken));
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] DeviceSyncRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        await notificationService.SyncDeviceAsync(request with { UserId = userId }, cancellationToken);
        return NoContent();
    }

    public sealed record RegisterPushTokenRequest(
        string DeviceFingerprint,
        string Platform,
        string PushToken,
        string? DeviceName,
        string? OsVersion,
        string? AppVersion);
}
