using FlowFi.AuthUserService.Entities;

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
    string Status,
    DateTimeOffset? EmailVerifiedAt,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record UserDeviceDto(
    Guid Id,
    Guid UserId,
    string DeviceFingerprint,
    string? DeviceName,
    string Platform,
    string? OsVersion,
    string? AppVersion,
    bool IsTrusted,
    DateTimeOffset? LastActiveAt,
    DateTimeOffset? LastSyncedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record SessionDto(
    Guid Id,
    Guid UserId,
    Guid? DeviceId,
    string AccessJti,
    DateTimeOffset StartedAt,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? EndedAt,
    string? IpAddress,
    string? UserAgent);

public sealed record UserLogDto(
    Guid Id,
    Guid? UserId,
    string Action,
    string Status,
    string? ResourceType,
    Guid? ResourceId,
    string? IpAddress,
    string? UserAgent,
    string? FailureReason,
    object? Metadata,
    DateTimeOffset CreatedAt);
