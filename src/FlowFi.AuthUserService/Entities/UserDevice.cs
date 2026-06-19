namespace FlowFi.AuthUserService.Entities;

public sealed class UserDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string? PushToken { get; set; }

    public DateTimeOffset? LastSyncedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
