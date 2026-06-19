namespace FlowFi.AuthUserService.Entities;

public sealed record User(
    Guid Id,
    string Email,
    string DisplayName,
    string? PasswordHash,
    DateTimeOffset CreatedAt);
