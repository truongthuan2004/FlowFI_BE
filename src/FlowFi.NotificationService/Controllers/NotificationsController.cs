using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.NotificationService.Controllers;

[ApiController]
[Route("notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Notification>>> Get(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await notificationService.GetNotificationsAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Notification>> Create(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var created = await notificationService.CreateNotificationAsync(request, cancellationToken);
        return Accepted($"/notifications/{created.Id}", created);
    }
}

