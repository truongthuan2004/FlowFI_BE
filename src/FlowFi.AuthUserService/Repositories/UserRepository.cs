using FlowFi.AuthUserService.Database;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AuthUserService.Repositories;

public sealed class UserRepository(AuthDbContext db) : IUserRepository
{
    // User
    public async Task<User> AddUserAsync(User user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
        => db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        => db.Users.FirstOrDefaultAsync(x => x.Email == email.ToLower(), cancellationToken);

    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        user.UpdatedAt = DateTimeOffset.UtcNow;
        db.Users.Update(user);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    // Refresh Tokens
    public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);
        return token;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken)
        => db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token && !x.IsRevoked, cancellationToken);

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
        if (refreshToken is not null)
        {
            refreshToken.IsRevoked = true;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    // Password Reset
    public async Task<PasswordResetToken> AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);
        return token;
    }

    public Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
        => db.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.Token == token && !x.IsUsed, cancellationToken);

    public async Task MarkPasswordResetTokenUsedAsync(Guid id, CancellationToken cancellationToken)
    {
        var token = await db.PasswordResetTokens.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (token is not null)
        {
            token.IsUsed = true;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    // User Logs
    public async Task AddUserLogAsync(UserLog log, CancellationToken cancellationToken)
    {
        db.UserLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserLog>> GetUserLogsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        => await db.UserLogs
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
}
