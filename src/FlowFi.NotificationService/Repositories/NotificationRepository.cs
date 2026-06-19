using FlowFi.NotificationService.Database;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.NotificationService.Repositories;

public sealed class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<PushDevice> AddDeviceAsync(PushDevice device, CancellationToken cancellationToken)
    {
        db.PushDevices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return device;
    }
}

