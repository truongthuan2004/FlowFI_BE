using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;

namespace FlowFi.AuthUserService.Interface;

public interface IAuthService
{
    Task<(UserDto User, AuthTokensDto Tokens, bool IsNewDevice, bool RequiresAdditionalVerification)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<(UserDto User, AuthTokensDto Tokens, bool IsNewDevice, bool RequiresAdditionalVerification)> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<(UserDto User, AuthTokensDto Tokens, bool IsNewUser, bool IsNewDevice)> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken);
    Task<AuthTokensDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
    Task LogoutAllAsync(LogoutAllRequest request, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<UserDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<UserDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserDeviceDto>> GetDevicesAsync(Guid userId, CancellationToken cancellationToken);
    Task RevokeDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserLogDto>> GetLogsAsync(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<LoginDevice> RegisterLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken);
}
