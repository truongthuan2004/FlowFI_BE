namespace FlowFi.AuthUserService.Entities;

public sealed class UserLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? FailureReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
