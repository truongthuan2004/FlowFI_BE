using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;

namespace FlowFi.NotificationService.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<Notification> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken);
    Task<PushDevice> RegisterDeviceAsync(PushDevice device, CancellationToken cancellationToken);
}

