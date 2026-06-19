namespace FlowFi.NotificationService.DTOs;

public sealed record NotificationSettingsDto(
    Guid Id,
    Guid UserId,
    bool EnableEmail,
    bool EnablePush,
    bool EnableInApp,
    bool EnableBudgetWarning,
    bool EnableTransactionAlert,
    bool EnableSystemAlert,
    TimeOnly? QuietHoursStart,
    TimeOnly? QuietHoursEnd,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string? Content,
    string NotificationType,
    string Channel,
    string Priority,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset? SentAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? TargetUrl,
    object? Metadata);

public sealed record NotificationDeliveryDto(
    Guid Id,
    Guid NotificationId,
    string Channel,
    string DeliveryStatus,
    string? ProviderMessageId,
    string? ErrorMessage,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record PagedNotificationsResponse(
    IReadOnlyList<NotificationDto> Items,
    int Page,
    int PageSize,
    long TotalItems,
    long TotalPages);

public sealed record GetUnreadCountResponse(long UnreadCount);
