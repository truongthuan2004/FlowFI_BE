using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AuthUserService.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Ok(new { result.User, result.Tokens, result.IsNewDevice, result.RequiresAdditionalVerification });
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(new { result.User, result.Tokens, result.IsNewDevice, result.RequiresAdditionalVerification });
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<object>> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.GoogleLoginAsync(request, cancellationToken);
        return Ok(new { result.User, result.Tokens, result.IsNewUser, result.IsNewDevice });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokensDto>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(await authService.RefreshAsync(request, cancellationToken));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll([FromBody] LogoutAllRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAllAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
