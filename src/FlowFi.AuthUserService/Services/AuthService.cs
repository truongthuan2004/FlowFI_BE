using System.Security.Claims;
using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using FlowFi.AuthUserService.Security;
using FlowFi.Common.Authentication;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.AuthUserService.Services;

public sealed class AuthService(IUserRepository users, ITokenService tokenService, IPasswordHasher passwordHasher, RabbitMqPublisher publisher) : IAuthService
{
    public async Task<(UserDto User, AuthTokensDto Tokens, bool IsNewDevice, bool RequiresAdditionalVerification)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var existing = await users.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User(
            Guid.NewGuid(),
            request.Email,
            request.FullName,
            passwordHasher.Hash(request.Password),
            DateTimeOffset.UtcNow);

        await users.AddUserAsync(user, cancellationToken);

        var tokens = CreateTokens(user.Id);
        try
        {
            await publisher.PublishAsync("user.created", new UserCreated(user.Id, user.Email), cancellationToken);
        }
        catch
        {
            // Log but don't fail registration if RabbitMQ is unavailable
        }

        return (MapUser(user), tokens, true, false);
    }

    public async Task<(UserDto User, AuthTokensDto Tokens, bool IsNewDevice, bool RequiresAdditionalVerification)> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOperationException("Invalid credentials.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var tokens = CreateTokens(user.Id);
        return (MapUser(user), tokens, true, false);
    }

    public async Task<(UserDto User, AuthTokensDto Tokens, bool IsNewUser, bool IsNewDevice)> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserByEmailAsync("google.user@example.com", cancellationToken)
            ?? await users.AddUserAsync(new User(Guid.NewGuid(), "google.user@example.com", "Google User", null, DateTimeOffset.UtcNow), cancellationToken);

        var tokens = CreateTokens(user.Id);
        return (MapUser(user), tokens, true, true);
    }

    public Task<AuthTokensDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        => Task.FromResult(CreateTokens(Guid.NewGuid()));

    public Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task LogoutAllAsync(LogoutAllRequest request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public async Task<UserDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
        var updated = user with
        {
            DisplayName = request.FullName ?? user.DisplayName
        };

        await users.UpdateUserAsync(updated, cancellationToken);
        return MapUser(updated);
    }

    public async Task<UserDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
        await users.UpdateUserAsync(user, cancellationToken);
        return MapUser(user);
    }

    public async Task<IReadOnlyList<UserDeviceDto>> GetDevicesAsync(Guid userId, CancellationToken cancellationToken)
        => (await users.GetLoginDevicesAsync(userId, cancellationToken)).Select(MapDevice).ToList();

    public Task RevokeDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<SessionDto>>(Array.Empty<SessionDto>());

    public Task<IReadOnlyList<UserLogDto>> GetLogsAsync(Guid userId, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<UserLogDto>>(Array.Empty<UserLogDto>());

    public Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
        => users.GetUserAsync(id, cancellationToken);

    public async Task<LoginDevice> RegisterLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken)
        => await users.AddLoginDeviceAsync(device, cancellationToken);

    private AuthTokensDto CreateTokens(Guid userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString())
        };

        return new AuthTokensDto(
            tokenService.CreateAccessToken(claims),
            tokenService.CreateRefreshToken(),
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddDays(30));
    }

    private static UserDto MapUser(User user)
        => new(user.Id, user.Email, user.DisplayName, null, null, "VND", null, "LOCAL", "ACTIVE", null, null, user.CreatedAt, null);

    private static UserDeviceDto MapDevice(LoginDevice device)
        => new(device.Id, device.UserId, device.DeviceName, device.DeviceName, "IOS", null, null, true, device.LastSeenAt, device.LastSeenAt, device.LastSeenAt, null);
}
