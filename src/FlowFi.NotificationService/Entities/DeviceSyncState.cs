namespace FlowFi.NotificationService.Entities;

public sealed class DeviceSyncState
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public DateTimeOffset LastSyncedAt { get; set; }
    public string SyncStatus { get; set; } = "SUCCESS";
    public string? ErrorMessage { get; set; }
    public int RecordVersion { get; set; }
}
