using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AuthUserService.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (user, tokens) = await authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponse<RegisterResponse>.Ok(
                new RegisterResponse(user, tokens),
                "Đăng ký tài khoản thành công"));
        }
        catch (InvalidOperationException ex) when (ex.Message == "EMAIL_EXISTS")
        {
            return BadRequest(ApiResponse.Error("Email đã được sử dụng", "EMAIL_EXISTS", "Vui lòng sử dụng email khác"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (user, tokens) = await authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse<LoginResponse>.Ok(
                new LoginResponse(user, tokens),
                "Đăng nhập thành công"));
        }
        catch (InvalidOperationException ex) when (ex.Message == "INVALID_CREDENTIALS")
        {
            return Unauthorized(ApiResponse.Error("Email hoặc mật khẩu không chính xác", "INVALID_CREDENTIALS"));
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthTokensDto>>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tokens = await authService.RefreshAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthTokensDto>.Ok(tokens, "Làm mới token thành công"));
        }
        catch (InvalidOperationException ex) when (ex.Message == "INVALID_TOKEN" || ex.Message == "TOKEN_EXPIRED")
        {
            return Unauthorized(ApiResponse.Error("Token không hợp lệ hoặc đã hết hạn", ex.Message));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("Đăng xuất thành công"));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);
        // Luôn trả 200 để tránh email enumeration
        return Ok(ApiResponse.Ok("Nếu email tồn tại trong hệ thống, hướng dẫn khôi phục sẽ được gửi"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.ResetPasswordAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok("Đặt lại mật khẩu thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error("Token không hợp lệ hoặc đã hết hạn", ex.Message));
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await authService.ChangePasswordAsync(userId, request, cancellationToken);
            return Ok(ApiResponse.Ok("Đổi mật khẩu thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error("Không thể đổi mật khẩu", ex.Message));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
