namespace FlowFi.AuthUserService.Entities;

public sealed class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;
    public string? OtpCode { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
