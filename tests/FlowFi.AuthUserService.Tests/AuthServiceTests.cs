using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using FlowFi.AuthUserService.Security;
using FlowFi.AuthUserService.Services;
using FlowFi.Common.Api;
using FlowFi.Common.Authentication;
using FlowFi.EventBus.Messaging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FlowFi.AuthUserService.Tests;

public sealed class AuthServiceTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public async Task LoginAsync_access_token_contains_user_id_claims_required_by_analytics()
    {
        var userId = Guid.NewGuid();
        var repository = new InMemoryUserRepository();
        repository.SeedUser(new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = _passwordHasher.Hash("CorrectPass123!"),
            FullName = "Test User",
            AuthProvider = "LOCAL",
            CurrencyCode = "VND",
            Role = UserRoles.User,
            CreatedAt = DateTimeOffset.UtcNow
        });
        var service = CreateService(repository);

        var (_, tokens) = await service.LoginAsync(new LoginRequest("USER@example.com", "CorrectPass123!"), CancellationToken.None);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        Assert.Equal(userId.ToString(), jwt.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(userId.ToString(), jwt.Claims.Single(x => x.Type == "sub").Value);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, "Bearer"));
        Assert.Equal(userId, principal.UserId());
    }

    [Fact]
    public async Task LogoutAsync_revokes_refresh_token_and_writes_logout_log()
    {
        var userId = Guid.NewGuid();
        var repository = new InMemoryUserRepository();
        repository.SeedUser(new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = _passwordHasher.Hash("CorrectPass123!"),
            FullName = "Test User",
            AuthProvider = "LOCAL",
            CurrencyCode = "VND",
            Role = UserRoles.User,
            CreatedAt = DateTimeOffset.UtcNow
        });
        repository.SeedRefreshToken(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "refresh-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        var service = CreateService(repository);

        await service.LogoutAsync(new LogoutRequest("refresh-token"), CancellationToken.None);

        Assert.True(repository.RefreshTokens.Single().IsRevoked);
        Assert.Contains(repository.UserLogs, log => log.UserId == userId && log.Action == "LOGOUT" && log.Status == "SUCCESS");
    }

    [Fact]
    public async Task ChangePasswordAsync_uses_authenticated_user_id_and_ignores_body_user_id()
    {
        var authenticatedUserId = Guid.NewGuid();
        var bodyUserId = Guid.NewGuid();
        var repository = new InMemoryUserRepository();
        repository.SeedUser(new User
        {
            Id = authenticatedUserId,
            Email = "user@example.com",
            PasswordHash = _passwordHasher.Hash("OldPass123!"),
            FullName = "Test User",
            AuthProvider = "LOCAL",
            CurrencyCode = "VND",
            Role = UserRoles.User,
            CreatedAt = DateTimeOffset.UtcNow
        });
        repository.SeedRefreshToken(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = authenticatedUserId,
            Token = "refresh-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        var service = CreateService(repository);

        await service.ChangePasswordAsync(
            authenticatedUserId,
            new ChangePasswordRequest(bodyUserId, "OldPass123!", "NewPass123!"),
            CancellationToken.None);

        var user = repository.Users.Single();
        Assert.True(_passwordHasher.Verify("NewPass123!", user.PasswordHash!));
        Assert.True(repository.RefreshTokens.Single().IsRevoked);
    }

    [Fact]
    public async Task ForgotPasswordAsync_creates_six_digit_otp()
    {
        var userId = Guid.NewGuid();
        var repository = new InMemoryUserRepository();
        repository.SeedUser(new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = _passwordHasher.Hash("CorrectPass123!"),
            FullName = "Test User",
            AuthProvider = "LOCAL",
            CurrencyCode = "VND",
            Role = UserRoles.User,
            CreatedAt = DateTimeOffset.UtcNow
        });
        var service = CreateService(repository);

        await service.ForgotPasswordAsync(new ForgotPasswordRequest("user@example.com"), CancellationToken.None);

        var resetToken = Assert.Single(repository.PasswordResetTokens);
        Assert.Matches("^[0-9]{6}$", resetToken.OtpCode);
    }

    private static AuthService CreateService(IUserRepository repository)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "FlowFi.Tests",
                ["Jwt:Audience"] = "FlowFi.Tests",
                ["Jwt:SigningKey"] = "0123456789abcdef0123456789abcdef"
            })
            .Build();

        return new AuthService(
            repository,
            new TokenService(configuration),
            new PasswordHasher(),
            new RabbitMqPublisher(configuration));
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];
        public List<RefreshToken> RefreshTokens { get; } = [];
        public List<PasswordResetToken> PasswordResetTokens { get; } = [];
        public List<UserLog> UserLogs { get; } = [];

        public void SeedUser(User user) => Users.Add(user);

        public void SeedRefreshToken(RefreshToken token) => RefreshTokens.Add(token);

        public Task<User> AddUserAsync(User user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.FromResult(user);
        }

        public Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(Users.FirstOrDefault(x => x.Id == id));

        public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
            => Task.FromResult(Users.FirstOrDefault(x => x.Email == email.ToLowerInvariant()));

        public Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user);

        public Task<RefreshToken> AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken)
        {
            RefreshTokens.Add(token);
            return Task.FromResult(token);
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken)
            => Task.FromResult(RefreshTokens.FirstOrDefault(x => x.Token == token && !x.IsRevoked));

        public Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken)
        {
            var refreshToken = RefreshTokens.FirstOrDefault(x => x.Token == token);
            if (refreshToken is not null)
            {
                refreshToken.IsRevoked = true;
            }

            return Task.CompletedTask;
        }

        public Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken)
        {
            foreach (var token in RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked))
            {
                token.IsRevoked = true;
            }

            return Task.CompletedTask;
        }

        public Task<PasswordResetToken> AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken)
        {
            PasswordResetTokens.Add(token);
            return Task.FromResult(token);
        }

        public Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
            => Task.FromResult(PasswordResetTokens.FirstOrDefault(x => x.Token == token && !x.IsUsed));

        public Task MarkPasswordResetTokenUsedAsync(Guid id, CancellationToken cancellationToken)
        {
            var token = PasswordResetTokens.FirstOrDefault(x => x.Id == id);
            if (token is not null)
            {
                token.IsUsed = true;
            }

            return Task.CompletedTask;
        }

        public Task AddUserLogAsync(UserLog log, CancellationToken cancellationToken)
        {
            UserLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<UserLog>> GetUserLogsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<UserLog>>(UserLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
    }
}
