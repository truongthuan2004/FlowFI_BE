namespace FlowFi.AuthUserService.Entities;

public sealed record RefreshToken(
    Guid Id,
    Guid UserId,
    string TokenHash,
    string Jti,
    Guid? DeviceId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    Guid? ReplacedByTokenId,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt);
