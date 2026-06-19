namespace FlowFi.NotificationService.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }

    public string NotificationType { get; set; } = "SYSTEM";
    // BUDGET_WARNING, SYSTEM, TRANSACTION

    public string Channel { get; set; } = "IN_APP";
    // IN_APP, EMAIL, PUSH

    public string Priority { get; set; } = "NORMAL";
    // LOW, NORMAL, HIGH, URGENT

    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }

    public string? TargetUrl { get; set; }
    public string? Metadata { get; set; }
    public string? ImageUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
