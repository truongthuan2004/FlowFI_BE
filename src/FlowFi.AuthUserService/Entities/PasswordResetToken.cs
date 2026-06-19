namespace FlowFi.AuthUserService.Entities;

public sealed record PasswordResetToken(
    Guid Id,
    Guid UserId,
    string? TokenHash,
    string? OtpHash,
    string ResetMethod,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? UsedAt,
    int AttemptCount,
    DateTimeOffset CreatedAt);
