namespace FlowFi.NotificationService.DTOs;

public sealed record CreateNotificationRequest(Guid UserId, string Channel, string Subject, string Body);

