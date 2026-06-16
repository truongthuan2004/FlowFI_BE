namespace FlowFi.NotificationService.Entities;

public sealed record PushDevice(Guid Id, Guid UserId, string Platform, string Token, bool IsActive, DateTimeOffset LastSeenAt);

