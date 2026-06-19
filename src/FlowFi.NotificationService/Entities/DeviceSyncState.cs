namespace FlowFi.NotificationService.Entities;

public sealed class DeviceSyncState
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;

    public DateTimeOffset LastSyncedAt { get; set; }
    public string SyncStatus { get; set; } = "SUCCESS"; // SUCCESS, FAILED, IN_PROGRESS

    public string? ErrorMessage { get; set; }
    public int RecordVersion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
