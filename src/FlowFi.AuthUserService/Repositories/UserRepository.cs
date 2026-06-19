using FlowFi.AuthUserService.Database;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AuthUserService.Repositories;

public sealed class UserRepository(AuthDbContext db) : IUserRepository
{
    public async Task<User> AddUserAsync(User user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
        => db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        => db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return;

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<LoginDevice> AddLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken)
    {
        db.LoginDevices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return device;
    }

    public async Task<IReadOnlyList<LoginDevice>> GetLoginDevicesAsync(Guid userId, CancellationToken cancellationToken)
        => await db.LoginDevices.Where(x => x.UserId == userId).ToListAsync(cancellationToken);

    public Task<OAuthIdentity?> GetOAuthIdentityAsync(string provider, string providerUserId, CancellationToken cancellationToken)
        => db.OAuthIdentities.FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId, cancellationToken);

    public async Task<OAuthIdentity> AddOAuthIdentityAsync(OAuthIdentity identity, CancellationToken cancellationToken)
    {
        db.OAuthIdentities.Add(identity);
        await db.SaveChangesAsync(cancellationToken);
        return identity;
    }

    public async Task<RefreshToken?> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string jti, CancellationToken cancellationToken)
        => db.RefreshTokens.FirstOrDefaultAsync(x => x.Jti == jti, cancellationToken);

    public async Task RevokeRefreshTokenAsync(string jti, CancellationToken cancellationToken)
    {
        var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Jti == jti, cancellationToken);
        if (token is null) return;

        var revoked = token with { RevokedAt = DateTimeOffset.UtcNow };
        db.RefreshTokens.Update(revoked);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetToken?> AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);
        return token;
    }

    public Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
        => db.PasswordResetTokens.FirstOrDefaultAsync(x => x.TokenHash == token || x.OtpHash == token, cancellationToken);

    public async Task MarkPasswordResetTokenUsedAsync(Guid id, CancellationToken cancellationToken)
    {
        var token = await db.PasswordResetTokens.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (token is null) return;

        var updated = token with { UsedAt = DateTimeOffset.UtcNow };
        db.PasswordResetTokens.Update(updated);
        await db.SaveChangesAsync(cancellationToken);
    }
}
