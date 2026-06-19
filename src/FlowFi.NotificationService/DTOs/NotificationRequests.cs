namespace FlowFi.NotificationService.DTOs;

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string? Content,
    string NotificationType,
    string Channel,
    string Priority = "NORMAL",
    string? TargetUrl = null,
    object? Metadata = null);

public sealed record UpdateNotificationSettingsRequest(
    bool? EnableEmail,
    bool? EnablePush,
    bool? EnableInApp,
    bool? EnableBudgetWarning,
    bool? EnableTransactionAlert,
    bool? EnableSystemAlert,
    TimeOnly? QuietHoursStart,
    TimeOnly? QuietHoursEnd);

public sealed record DeviceSyncRequest(
    Guid UserId,
    string DeviceFingerprint,
    DateTimeOffset LastSyncedAt);

public sealed record RegisterPushTokenRequest(
    Guid UserId,
    string DeviceFingerprint,
    string Platform,
    string PushToken,
    string? DeviceName,
    string? OsVersion,
    string? AppVersion);
