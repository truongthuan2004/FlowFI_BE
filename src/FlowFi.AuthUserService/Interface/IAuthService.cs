using FlowFi.AuthUserService.DTOs;

namespace FlowFi.AuthUserService.Interface;

public interface IAuthService
{
    Task<(UserDto User, AuthTokensDto Tokens)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<(UserDto User, AuthTokensDto Tokens)> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthTokensDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken);

    Task<UserDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<UserDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken);
}
