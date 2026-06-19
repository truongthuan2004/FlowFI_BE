using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.NotificationService.Controllers;

[ApiController]
[Authorize]
[Route("notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedNotificationsResponse>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? type = null,
        [FromQuery] string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await notificationService.GetNotificationsAsync(userId, page, pageSize, isRead, type, channel, cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<GetUnreadCountResponse>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await notificationService.GetUnreadCountAsync(userId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var notification = await notificationService.GetNotificationAsync(userId, id, cancellationToken);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await notificationService.MarkAsReadAsync(userId, id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<MarkAllReadResponse>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var count = await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new MarkAllReadResponse(count));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await notificationService.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("internal/send")]
    public async Task<ActionResult<NotificationDto>> Send([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
        => Ok(await notificationService.CreateNotificationAsync(request, cancellationToken));

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public sealed record MarkAllReadResponse(int MarkedCount);
}
