namespace FlowFi.NotificationService.Entities;

public sealed class PushDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string DeviceFingerprint { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string Platform { get; set; } = string.Empty; // ANDROID, IOS

    public string Token { get; set; } = string.Empty;
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; }

    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
