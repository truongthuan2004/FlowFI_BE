using FlowFi.AuthUserService.Entities;

namespace FlowFi.AuthUserService.Interface;

public interface IUserRepository
{
    // User
    Task<User> AddUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken);

    // Refresh Tokens
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken);

    // Password Reset
    Task<PasswordResetToken> AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken);
    Task MarkPasswordResetTokenUsedAsync(Guid id, CancellationToken cancellationToken);

    // User Logs
    Task AddUserLogAsync(UserLog log, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserLog>> GetUserLogsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
