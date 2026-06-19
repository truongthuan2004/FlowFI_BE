using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.NotificationService.Controllers;

[ApiController]
[Route("devices")]
public sealed class DevicesController(INotificationService notificationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PushDevice>> Register(PushDevice device, CancellationToken cancellationToken)
    {
        var created = await notificationService.RegisterDeviceAsync(device, cancellationToken);
        return Created($"/devices/{created.Id}", created);
    }
}

