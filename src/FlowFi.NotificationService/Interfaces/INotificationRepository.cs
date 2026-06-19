using FlowFi.NotificationService.Entities;

namespace FlowFi.NotificationService.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken);
    Task<PushDevice> AddDeviceAsync(PushDevice device, CancellationToken cancellationToken);
}

