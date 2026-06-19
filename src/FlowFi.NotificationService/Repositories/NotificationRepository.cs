using FlowFi.NotificationService.Database;
using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interface;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.NotificationService.Repositories;

public sealed class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    // Notifications
    public async Task<IReadOnlyList<Notification>> GetNotificationsAsync(
        Guid userId, int page, int pageSize, bool? isRead, string? type, string? channel, CancellationToken cancellationToken)
    {
        var query = db.Notifications.Where(x => x.UserId == userId && x.DeletedAt == null);

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
        var query = db.Notifications.Where(x => x.UserId == userId && x.DeletedAt == null);

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
            .Where(x => x.UserId == userId && !x.IsRead && x.DeletedAt == null)
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
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    public async Task<Notification> UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        notification.UpdatedAt = DateTimeOffset.UtcNow;
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task DeleteNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var notification = await GetNotificationAsync(userId, id, cancellationToken);
        if (notification != null)
        {
            notification.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    // Push Devices
    public async Task<PushDevice> AddDeviceAsync(PushDevice device, CancellationToken cancellationToken)
    {
        var existing = await GetDeviceByFingerprintAsync(device.UserId, device.DeviceFingerprint, cancellationToken);
        if (existing is not null)
        {
            existing.Token = device.Token;
            existing.IsActive = true;
            existing.LastSeenAt = DateTimeOffset.UtcNow;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await UpdateDeviceAsync(existing, cancellationToken);
            return existing;
        }

        // Deactivate other primary devices if this is set as primary
        if (device.IsPrimary)
        {
            var otherPrimary = await db.PushDevices
                .Where(x => x.UserId == device.UserId && x.IsPrimary && x.Id != device.Id)
                .ToListAsync(cancellationToken);
            foreach (var d in otherPrimary)
            {
                d.IsPrimary = false;
            }
        }

        db.PushDevices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return device;
    }

    public Task<PushDevice?> GetDeviceByFingerprintAsync(Guid userId, string fingerprint, CancellationToken cancellationToken)
        => db.PushDevices.FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceFingerprint == fingerprint, cancellationToken);

    public async Task<IReadOnlyList<PushDevice>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken)
        => await db.PushDevices.Where(x => x.UserId == userId && x.IsActive).OrderByDescending(x => x.LastSeenAt).ToListAsync(cancellationToken);

    public async Task UpdateDeviceAsync(PushDevice device, CancellationToken cancellationToken)
    {
        device.UpdatedAt = DateTimeOffset.UtcNow;
        db.PushDevices.Update(device);
        await db.SaveChangesAsync(cancellationToken);
    }

    // Device Sync
    public async Task<DeviceSyncState> UpsertDeviceSyncStateAsync(DeviceSyncState state, CancellationToken cancellationToken)
    {
        var existing = await db.DeviceSyncStates
            .FirstOrDefaultAsync(x => x.UserId == state.UserId && x.DeviceFingerprint == state.DeviceFingerprint, cancellationToken);

        if (existing != null)
        {
            existing.LastSyncedAt = state.LastSyncedAt;
            existing.SyncStatus = state.SyncStatus;
            existing.ErrorMessage = state.ErrorMessage;
            existing.RecordVersion++;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            state.RecordVersion = 1;
            state.CreatedAt = DateTimeOffset.UtcNow;
            state.UpdatedAt = DateTimeOffset.UtcNow;
            db.DeviceSyncStates.Add(state);
        }

        await db.SaveChangesAsync(cancellationToken);
        return existing ?? state;
    }

    // Settings
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
