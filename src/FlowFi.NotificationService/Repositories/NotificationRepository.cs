using FlowFi.NotificationService.Database;
using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.NotificationService.Repositories;

public sealed class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetNotificationsAsync(
        Guid userId, int page, int pageSize, bool? isRead, string? type, string? channel, CancellationToken cancellationToken)
    {
        var query = db.Notifications.Where(x => x.UserId == userId);

        if (isRead.HasValue)
            query = query.Where(x => x.IsRead == isRead);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(x => x.NotificationType == type);
        if (!string.IsNullOrEmpty(channel))
            query = query.Where(x => x.Channel == channel);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountNotificationsAsync(Guid userId, bool? isRead, string? type, string? channel, CancellationToken cancellationToken)
    {
        var query = db.Notifications.Where(x => x.UserId == userId);

        if (isRead.HasValue)
            query = query.Where(x => x.IsRead == isRead);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(x => x.NotificationType == type);
        if (!string.IsNullOrEmpty(channel))
            query = query.Where(x => x.Channel == channel);

        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<Notification?> GetNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        return await db.Notifications
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    }

    public async Task<Notification> UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task DeleteNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var notification = await GetNotificationAsync(userId, id, cancellationToken);
        if (notification != null)
        {
            db.Notifications.Remove(notification);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PushDevice> AddDeviceAsync(PushDevice device, CancellationToken cancellationToken)
    {
        db.PushDevices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return device;
    }

    public async Task<DeviceSyncState> UpsertDeviceSyncStateAsync(DeviceSyncState state, CancellationToken cancellationToken)
    {
        var existing = await db.DeviceSyncStates
            .FirstOrDefaultAsync(x => x.UserId == state.UserId && x.DeviceFingerprint == state.DeviceFingerprint, cancellationToken);

        if (existing != null)
        {
            existing.LastSyncedAt = state.LastSyncedAt;
            existing.SyncStatus = state.SyncStatus;
            existing.ErrorMessage = state.ErrorMessage;
            existing.RecordVersion = state.RecordVersion;
        }
        else
        {
            db.DeviceSyncStates.Add(state);
        }

        await db.SaveChangesAsync(cancellationToken);
        return existing ?? state;
    }

    public async Task<NotificationSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await db.NotificationSettings
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null) return null;

        return new NotificationSettingsDto(
            settings.Id,
            settings.UserId,
            settings.EnableEmail,
            settings.EnablePush,
            settings.EnableInApp,
            settings.EnableBudgetWarning,
            settings.EnableTransactionAlert,
            settings.EnableSystemAlert,
            settings.QuietHoursStart,
            settings.QuietHoursEnd,
            settings.CreatedAt,
            settings.UpdatedAt);
    }

    public async Task<NotificationSettingsDto> UpsertSettingsAsync(NotificationSettingsDto dto, CancellationToken cancellationToken)
    {
        var existing = await db.NotificationSettings
            .FirstOrDefaultAsync(x => x.UserId == dto.UserId, cancellationToken);

        if (existing != null)
        {
            existing.EnableEmail = dto.EnableEmail;
            existing.EnablePush = dto.EnablePush;
            existing.EnableInApp = dto.EnableInApp;
            existing.EnableBudgetWarning = dto.EnableBudgetWarning;
            existing.EnableTransactionAlert = dto.EnableTransactionAlert;
            existing.EnableSystemAlert = dto.EnableSystemAlert;
            existing.QuietHoursStart = dto.QuietHoursStart;
            existing.QuietHoursEnd = dto.QuietHoursEnd;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            existing = new NotificationSetting
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                UserId = dto.UserId,
                EnableEmail = dto.EnableEmail,
                EnablePush = dto.EnablePush,
                EnableInApp = dto.EnableInApp,
                EnableBudgetWarning = dto.EnableBudgetWarning,
                EnableTransactionAlert = dto.EnableTransactionAlert,
                EnableSystemAlert = dto.EnableSystemAlert,
                QuietHoursStart = dto.QuietHoursStart,
                QuietHoursEnd = dto.QuietHoursEnd,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.NotificationSettings.Add(existing);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new NotificationSettingsDto(
            existing.Id,
            existing.UserId,
            existing.EnableEmail,
            existing.EnablePush,
            existing.EnableInApp,
            existing.EnableBudgetWarning,
            existing.EnableTransactionAlert,
            existing.EnableSystemAlert,
            existing.QuietHoursStart,
            existing.QuietHoursEnd,
            existing.CreatedAt,
            existing.UpdatedAt);
    }
}
