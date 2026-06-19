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
    public async Task<ActionResult<PushDeviceDto>> RegisterPushToken([FromBody] RegisterPushTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
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
            IsPrimary = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await notificationService.RegisterDeviceAsync(device, cancellationToken);
        return Ok(MapDevice(created));
    }

    [HttpPost("sync")]
    public async Task<ActionResult<SyncResponse>> Sync([FromBody] DeviceSyncRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await notificationService.SyncDeviceAsync(request with { UserId = userId }, cancellationToken);

        var undelivered = await notificationService.GetUndeliveredNotificationsAsync(
            userId,
            request.DeviceFingerprint,
            request.LastSyncedAt,
            cancellationToken);

        return Ok(new SyncResponse(
            undelivered.Select(n => new SyncNotificationDto(
                n.Id,
                n.Title,
                n.Content,
                n.NotificationType,
                n.Priority,
                n.TargetUrl,
                n.CreatedAt)).ToList(),
            DateTimeOffset.UtcNow));
    }

    [HttpDelete("push-tokens/{deviceFingerprint}")]
    public async Task<IActionResult> UnregisterPushToken(string deviceFingerprint, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // In production, implement device unregistration
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private static PushDeviceDto MapDevice(PushDevice device)
        => new(
            device.Id,
            device.UserId,
            device.DeviceFingerprint,
            device.Platform,
            device.Token,
            device.DeviceName,
            device.OsVersion,
            device.AppVersion,
            device.IsActive,
            device.IsPrimary,
            device.LastSeenAt,
            device.CreatedAt);

    public sealed record RegisterPushTokenRequest(
        string DeviceFingerprint,
        string Platform,
        string PushToken,
        string? DeviceName,
        string? OsVersion,
        string? AppVersion);

    public sealed record SyncResponse(IReadOnlyList<SyncNotificationDto> Notifications, DateTimeOffset SyncedAt);

    public sealed record SyncNotificationDto(
        Guid Id,
        string Title,
        string? Content,
        string NotificationType,
        string Priority,
        string? TargetUrl,
        DateTimeOffset CreatedAt);

    public sealed record PushDeviceDto(
        Guid Id,
        Guid UserId,
        string DeviceFingerprint,
        string Platform,
        string Token,
        string? DeviceName,
        string? OsVersion,
        string? AppVersion,
        bool IsActive,
        bool IsPrimary,
        DateTimeOffset? LastSeenAt,
        DateTimeOffset CreatedAt);
}
