using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.NotificationService.Services;

public sealed class NotificationService(
    INotificationRepository notifications) : INotificationService
{
    public async Task<PagedNotificationsResponse> GetNotificationsAsync(
        Guid userId, int page, int pageSize, bool? isRead, string? type, string? channel, CancellationToken cancellationToken)
    {
        var items = await notifications.GetNotificationsAsync(userId, page, pageSize, isRead, type, channel, cancellationToken);
        var total = await notifications.CountNotificationsAsync(userId, isRead, type, channel, cancellationToken);
        return new PagedNotificationsResponse(
            items.Select(MapNotification).ToList(),
            page,
            pageSize,
            total,
            (long)Math.Ceiling(total / (double)Math.Max(pageSize, 1)));
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

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var notificationsToUpdate = await notifications.GetNotificationsAsync(userId, 1, 1000, false, null, null, cancellationToken);

        foreach (var notification in notificationsToUpdate)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
            await notifications.UpdateNotificationAsync(notification, cancellationToken);
        }

        return notificationsToUpdate.Count;
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => await notifications.DeleteNotificationAsync(userId, id, cancellationToken);

    public async Task<PushDevice> RegisterDeviceAsync(PushDevice device, CancellationToken cancellationToken)
    {
        device.CreatedAt = DateTimeOffset.UtcNow;
        device.UpdatedAt = DateTimeOffset.UtcNow;
        return await notifications.AddDeviceAsync(device, cancellationToken);
    }

    public async Task<DeviceSyncState> SyncDeviceAsync(DeviceSyncRequest request, CancellationToken cancellationToken)
    {
        var state = new DeviceSyncState
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            DeviceFingerprint = request.DeviceFingerprint,
            LastSyncedAt = request.LastSyncedAt,
            SyncStatus = "SUCCESS",
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await notifications.UpsertDeviceSyncStateAsync(state, cancellationToken);
    }

    public async Task<NotificationSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken)
        => await notifications.GetSettingsAsync(userId, cancellationToken);

    public async Task<NotificationSettingsDto> UpdateSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
    {
        var existingSettings = await notifications.GetSettingsAsync(userId, cancellationToken);

        var dto = new NotificationSettingsDto(
            existingSettings?.Id ?? Guid.Empty,
            userId,
            request.EnableEmail ?? existingSettings?.EnableEmail ?? true,
            request.EnablePush ?? existingSettings?.EnablePush ?? true,
            request.EnableInApp ?? existingSettings?.EnableInApp ?? true,
            request.EnableBudgetWarning ?? existingSettings?.EnableBudgetWarning ?? true,
            request.EnableTransactionAlert ?? existingSettings?.EnableTransactionAlert ?? true,
            request.EnableSystemAlert ?? existingSettings?.EnableSystemAlert ?? true,
            request.QuietHoursStart,
            request.QuietHoursEnd,
            existingSettings?.CreatedAt ?? DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        return await notifications.UpsertSettingsAsync(dto, cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetUndeliveredNotificationsAsync(
        Guid userId,
        string deviceFingerprint,
        DateTimeOffset since,
        CancellationToken cancellationToken)
    {
        var allNotifications = await notifications.GetNotificationsAsync(userId, 1, 100, false, null, null, cancellationToken);
        return allNotifications
            .Where(n => n.CreatedAt > since)
            .ToList();
    }

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
            notification.Metadata is not null ? System.Text.Json.JsonSerializer.Deserialize<object>(notification.Metadata) : null);
}
