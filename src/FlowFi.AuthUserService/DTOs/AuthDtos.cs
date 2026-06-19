namespace FlowFi.AuthUserService.DTOs;

public sealed record AuthTokensDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record UserDto(
    Guid Id,
    string Email,
    string? FullName,
    string? AvatarUrl,
    DateOnly? DateOfBirth,
    string CurrencyCode,
    decimal? MonthlyBudgetLimit,
    string AuthProvider,
    bool IsVerified,
    string Role,
    DateTimeOffset CreatedAt);

public sealed record RegisterResponse(
    UserDto User,
    AuthTokensDto Tokens);

public sealed record LoginResponse(
    UserDto User,
    AuthTokensDto Tokens);
