using FlowFi.AuthUserService.Entities;

namespace FlowFi.AuthUserService.Interface;

public interface IUserRepository
{
    Task<User> AddUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);

    Task<LoginDevice> AddLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken);
    Task<IReadOnlyList<LoginDevice>> GetLoginDevicesAsync(Guid userId, CancellationToken cancellationToken);

    Task<OAuthIdentity?> GetOAuthIdentityAsync(string provider, string providerUserId, CancellationToken cancellationToken);
    Task<OAuthIdentity> AddOAuthIdentityAsync(OAuthIdentity identity, CancellationToken cancellationToken);

    Task<RefreshToken?> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string jti, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string jti, CancellationToken cancellationToken);

    Task<PasswordResetToken?> AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken);
    Task MarkPasswordResetTokenUsedAsync(Guid id, CancellationToken cancellationToken);
}
