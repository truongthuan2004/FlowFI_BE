using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;

namespace FlowFi.NotificationService.Interface;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead, string? type, string? channel, CancellationToken cancellationToken);
    Task<long> CountNotificationsAsync(Guid userId, bool? isRead, string? type, string? channel, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken);
    Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> GetNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<Notification> UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken);
    Task DeleteNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<PushDevice> AddDeviceAsync(PushDevice device, CancellationToken cancellationToken);
    Task<DeviceSyncState> UpsertDeviceSyncStateAsync(DeviceSyncState state, CancellationToken cancellationToken);
    Task<NotificationSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationSettingsDto> UpsertSettingsAsync(NotificationSettingsDto settings, CancellationToken cancellationToken);
}
