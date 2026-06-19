using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;

namespace FlowFi.NotificationService.Interface;

public interface INotificationService
{
    Task<PagedNotificationsResponse> GetNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead, string? type, string? channel, CancellationToken cancellationToken);
    Task<GetUnreadCountResponse> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken);
    Task<NotificationDto?> GetNotificationAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<PushDevice> RegisterDeviceAsync(PushDevice device, CancellationToken cancellationToken);
    Task<DeviceSyncState> SyncDeviceAsync(DeviceSyncRequest request, CancellationToken cancellationToken);
    Task<NotificationSettingsDto?> GetSettingsAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationSettingsDto> UpdateSettingsAsync(Guid userId, UpdateNotificationSettingsRequest request, CancellationToken cancellationToken);
}
