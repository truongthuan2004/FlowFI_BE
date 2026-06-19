using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AuthUserService.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UsersController(IAuthService authService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized(ApiResponse.Error("Unauthorized", "UNAUTHORIZED"));

        var user = await authService.GetMeAsync(userId, cancellationToken);
        if (user is null)
            return NotFound(ApiResponse.Error("Không tìm thấy người dùng", "USER_NOT_FOUND"));

        return Ok(ApiResponse<UserDto>.Ok(user, "Lấy thông tin người dùng thành công"));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized(ApiResponse.Error("Unauthorized", "UNAUTHORIZED"));

        try
        {
            var user = await authService.UpdateProfileAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<UserDto>.Ok(user, "Cập nhật hồ sơ thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(ex.Message, "UPDATE_FAILED"));
        }
    }

    [HttpPut("me/preferences")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized(ApiResponse.Error("Unauthorized", "UNAUTHORIZED"));

        try
        {
            var user = await authService.UpdatePreferencesAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<UserDto>.Ok(user, "Cập nhật tùy chỉnh thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(ex.Message, "UPDATE_FAILED"));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
