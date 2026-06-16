namespace FlowFi.NotificationService.Entities;

public sealed record DeviceSyncState(Guid Id, Guid UserId, string DeviceId, DateTimeOffset LastSyncedAt);

