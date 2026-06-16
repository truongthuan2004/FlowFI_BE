namespace FlowFi.NotificationService.Entities;

public sealed record Notification(Guid Id, Guid UserId, string Channel, string Subject, string Body, string Status, DateTimeOffset CreatedAt, DateTimeOffset? SentAt);

