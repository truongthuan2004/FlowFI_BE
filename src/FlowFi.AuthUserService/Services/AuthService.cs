using System.Security.Claims;
using System.Security.Cryptography;
using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using FlowFi.AuthUserService.Security;
using FlowFi.Common.Authentication;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.AuthUserService.Services;

public sealed class AuthService(
    IUserRepository users,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    RabbitMqPublisher publisher) : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromMinutes(30);

    public async Task<(UserDto User, AuthTokensDto Tokens)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var existing = await users.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("EMAIL_EXISTS");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            AuthProvider = "LOCAL",
            IsVerified = false,
            CurrencyCode = "VND",
            Role = UserRoles.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await users.AddUserAsync(user, cancellationToken);

        // Create tokens
        var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        var tokens = CreateTokens(user.Id, refreshToken.Token, user.Role);

        // Log
        await AddUserLogAsync(user.Id, "REGISTER", "SUCCESS", null, null, null, cancellationToken);

        // Publish event
        try
        {
            await publisher.PublishAsync("user.created", new UserCreated(user.Id, user.Email), cancellationToken);
        }
        catch { }

        return (MapUser(user), tokens);
    }

    public async Task<(UserDto User, AuthTokensDto Tokens)> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOperationException("INVALID_CREDENTIALS");

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await AddUserLogAsync(user.Id, "LOGIN", "FAILED", null, null, "WRONG_PASSWORD", cancellationToken);
            throw new InvalidOperationException("INVALID_CREDENTIALS");
        }

        var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        var tokens = CreateTokens(user.Id, refreshToken.Token, user.Role);

        await AddUserLogAsync(user.Id, "LOGIN", "SUCCESS", null, null, null, cancellationToken);

        return (MapUser(user), tokens);
    }

    public async Task<AuthTokensDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = await users.GetRefreshTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new InvalidOperationException("INVALID_TOKEN");

        if (refreshToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("TOKEN_EXPIRED");
        }

        // Create new token
        var newToken = await CreateRefreshTokenAsync(refreshToken.UserId, cancellationToken);

        // Revoke old token
        await users.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        var user = await users.GetUserAsync(refreshToken.UserId, cancellationToken)
            ?? throw new InvalidOperationException("USER_NOT_FOUND");

        return CreateTokens(refreshToken.UserId, newToken.Token, user.Role);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var token = await users.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        await users.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (token is not null)
        {
            await AddUserLogAsync(token.UserId, "LOGOUT", "SUCCESS", null, null, null, cancellationToken);
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user is null || user.AuthProvider != "LOCAL")
        {
            return;
        }

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            OtpCode = RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString("D6"),
            ExpiresAt = DateTimeOffset.UtcNow.Add(PasswordResetTokenLifetime),
            IsUsed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await users.AddPasswordResetTokenAsync(resetToken, cancellationToken);
        await AddUserLogAsync(user.Id, "FORGOT_PASSWORD", "SUCCESS", null, null, null, cancellationToken);

        // Publish event to send email
        try
        {
            await publisher.PublishAsync("auth.password-reset-requested", new PasswordResetRequested(
                user.Id,
                user.Email,
                user.FullName ?? "User",
                resetToken.Token,
                resetToken.OtpCode
            ), cancellationToken);
        }
        catch { }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOperationException("USER_NOT_FOUND");

        var resetToken = await users.GetPasswordResetTokenAsync(request.Token, cancellationToken)
            ?? throw new InvalidOperationException("INVALID_TOKEN");

        if (resetToken.UserId != user.Id)
        {
            throw new InvalidOperationException("INVALID_TOKEN");
        }

        if (resetToken.IsUsed || resetToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("TOKEN_EXPIRED");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        await users.UpdateUserAsync(user, cancellationToken);

        await users.MarkPasswordResetTokenUsedAsync(resetToken.Id, cancellationToken);
        await users.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await AddUserLogAsync(user.Id, "RESET_PASSWORD", "SUCCESS", null, null, null, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("USER_NOT_FOUND");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash ?? ""))
        {
            throw new InvalidOperationException("INVALID_CURRENT_PASSWORD");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        await users.UpdateUserAsync(user, cancellationToken);

        await users.RevokeAllUserTokensAsync(user.Id, cancellationToken);
        await AddUserLogAsync(user.Id, "CHANGE_PASSWORD", "SUCCESS", null, null, null, cancellationToken);
    }

    public async Task<UserDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("USER_NOT_FOUND");

        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;

        await users.UpdateUserAsync(user, cancellationToken);
        await AddUserLogAsync(user.Id, "UPDATE_PROFILE", "SUCCESS", null, null, null, cancellationToken);

        return MapUser(user);
    }

    public async Task<UserDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("USER_NOT_FOUND");

        if (request.CurrencyCode is not null) user.CurrencyCode = request.CurrencyCode;
        if (request.MonthlyBudgetLimit.HasValue) user.MonthlyBudgetLimit = request.MonthlyBudgetLimit;

        await users.UpdateUserAsync(user, cancellationToken);
        await AddUserLogAsync(user.Id, "UPDATE_PREFERENCES", "SUCCESS", null, null, null, cancellationToken);

        return MapUser(user);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTimeOffset.UtcNow.Add(RefreshTokenLifetime),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await users.AddRefreshTokenAsync(token, cancellationToken);
    }

    private AuthTokensDto CreateTokens(Guid userId, string refreshToken, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        return new AuthTokensDto(
            tokenService.CreateAccessToken(claims),
            refreshToken,
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.Add(RefreshTokenLifetime));
    }

    private static UserDto MapUser(User user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.AvatarUrl,
        user.DateOfBirth,
        user.CurrencyCode,
        user.MonthlyBudgetLimit,
        user.AuthProvider,
        user.IsVerified,
        user.Role,
        user.CreatedAt);

    private async Task AddUserLogAsync(Guid userId, string action, string status, string? ipAddress, string? userAgent, string? failureReason, CancellationToken cancellationToken)
    {
        try
        {
            var log = new UserLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                Status = status,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                FailureReason = failureReason,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await users.AddUserLogAsync(log, cancellationToken);
        }
        catch { }
    }
}
