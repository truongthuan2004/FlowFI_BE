using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Interface;
using FlowFi.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AuthUserService.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UsersController(IAuthService authService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        var user = await authService.GetMeAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await authService.UpdateProfileAsync(userId, request, cancellationToken));
    }

    [HttpPut("me/preferences")]
    public async Task<ActionResult<UserDto>> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await authService.UpdatePreferencesAsync(userId, request, cancellationToken));
    }

    [HttpGet("me/devices")]
    public async Task<ActionResult<IReadOnlyList<UserDeviceDto>>> GetDevices(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await authService.GetDevicesAsync(userId, cancellationToken));
    }

    [HttpDelete("me/devices/{deviceId:guid}")]
    public async Task<IActionResult> RevokeDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        await authService.RevokeDeviceAsync(userId, deviceId, cancellationToken);
        return NoContent();
    }

    [HttpGet("me/sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetSessions(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await authService.GetSessionsAsync(userId, cancellationToken));
    }

    [HttpGet("me/logs")]
    public async Task<ActionResult<IReadOnlyList<UserLogDto>>> GetLogs(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty) return Unauthorized();

        return Ok(await authService.GetLogsAsync(userId, cancellationToken));
    }
}
