using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.NotificationService.Controllers;

[ApiController]
[Route("notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedNotificationsResponse>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isRead = null, [FromQuery] string? type = null, [FromQuery] string? channel = null, CancellationToken cancellationToken = default)
        => Ok(await notificationService.GetNotificationsAsync(GetCurrentUserId(), page, pageSize, isRead, type, channel, cancellationToken));

    [HttpGet("unread-count")]
    public async Task<ActionResult<GetUnreadCountResponse>> GetUnreadCount(CancellationToken cancellationToken)
        => Ok(await notificationService.GetUnreadCountAsync(GetCurrentUserId(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var notification = await notificationService.GetNotificationAsync(GetCurrentUserId(), id, cancellationToken);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await notificationService.MarkAsReadAsync(GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await notificationService.MarkAllAsReadAsync(GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await notificationService.DeleteAsync(GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPost("internal/send")]
    public async Task<ActionResult<NotificationDto>> Send([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
        => Ok(await notificationService.CreateNotificationAsync(request, cancellationToken));

    private Guid GetCurrentUserId() => Guid.Empty;
}
