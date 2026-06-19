using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AuthUserService.Controllers;

[ApiController]
[Route("login-devices")]
public sealed class LoginDevicesController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LoginDevice>> Register(LoginDevice device, CancellationToken cancellationToken)
    {
        var registered = await authService.RegisterLoginDeviceAsync(device, cancellationToken);
        return Accepted($"/login-devices/{registered.Id}", registered);
    }
}

