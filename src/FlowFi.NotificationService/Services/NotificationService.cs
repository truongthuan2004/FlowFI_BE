using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interface;

namespace FlowFi.NotificationService.Services;

public sealed class NotificationService(INotificationRepository notifications) : INotificationService
{
    public async Task<PagedNotificationsResponse> GetNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead, string? type, string? channel, CancellationToken cancellationToken)
    {
        var items = await notifications.GetNotificationsAsync(userId, page, pageSize, isRead, type, channel, cancellationToken);
        var total = await notifications.CountNotificationsAsync(userId, isRead, type, channel, cancellationToken);
        return new PagedNotificationsResponse(items.Select(MapNotification).ToList(), page, pageSize, total, (long)Math.Ceiling(total / (double)Math.Max(pageSize, 1)));
    }

    public async Task<GetUnreadCountResponse> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
        => new(await notifications.CountUnreadAsync(userId, cancellationToken));

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Content = request.Content,
            Channel = request.Channel,
            NotificationType = request.NotificationType,
            Priority = request.Priority,
            TargetUrl = request.TargetUrl,
            Metadata = request.Metadata?.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        var created = await notifications.AddNotificationAsync(entity, cancellationToken);
        return MapNotification(created);
    }

    public async Task<NotificationDto?> GetNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var notification = await notifications.GetNotificationAsync(userId, id, cancellationToken);
        return notification is null ? null : MapNotification(notification);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var notification = await notifications.GetNotificationAsync(userId, id, cancellationToken);
        if (notification is null) return;
        notification.IsRead = true;
        notification.ReadAt = DateTimeOffset.UtcNow;
        await notifications.UpdateNotificationAsync(notification, cancellationToken);
    }

    public Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
        => Task.FromResult(0);

    public Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => notifications.DeleteNotificationAsync(userId, id, cancellationToken);

    public Task<PushDevice> RegisterDeviceAsync(PushDevice device, CancellationToken cancellationToken)
        => notifications.AddDeviceAsync(device, cancellationToken);

    public Task<DeviceSyncState> SyncDeviceAsync(DeviceSyncRequest request, CancellationToken cancellationToken)
        => notifications.UpsertDeviceSyncStateAsync(new DeviceSyncState
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            DeviceFingerprint = request.DeviceFingerprint,
            LastSyncedAt = request.LastSyncedAt
        }, cancellationToken);

    public Task<NotificationSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken)
        => notifications.GetSettingsAsync(userId, cancellationToken);

    public Task<NotificationSettingsDto> UpdateSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
        => notifications.UpsertSettingsAsync(new NotificationSettingsDto(
            Guid.NewGuid(),
            userId,
            request.EnableEmail ?? true,
            request.EnablePush ?? true,
            request.EnableInApp ?? true,
            request.EnableBudgetWarning ?? true,
            request.EnableTransactionAlert ?? true,
            request.EnableSystemAlert ?? true,
            request.QuietHoursStart,
            request.QuietHoursEnd,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow), cancellationToken);

    private static NotificationDto MapNotification(Notification notification)
        => new(
            notification.Id,
            notification.UserId,
            notification.Title,
            notification.Content,
            notification.NotificationType,
            notification.Channel,
            notification.Priority,
            notification.IsRead,
            notification.ReadAt,
            notification.SentAt,
            notification.CreatedAt,
            notification.UpdatedAt,
            notification.TargetUrl,
            null);
}
