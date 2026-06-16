using FlowFi.NotificationService.Entities;

namespace FlowFi.NotificationService.Interface;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken);
    Task<PushDevice> AddDeviceAsync(PushDevice device, CancellationToken cancellationToken);
}

