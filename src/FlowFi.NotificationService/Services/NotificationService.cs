using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Entities;
using FlowFi.NotificationService.Interfaces;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.NotificationService.Services;

public sealed class NotificationService(INotificationRepository repository, RabbitMqPublisher publisher) : INotificationService
{
    public Task<IReadOnlyList<Notification>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.GetNotificationsAsync(userId, cancellationToken);
    }

    public async Task<Notification> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var notification = new Notification(
            Guid.NewGuid(),
            request.UserId,
            request.Channel,
            request.Subject,
            request.Body,
            "pending",
            DateTimeOffset.UtcNow,
            null);

        var created = await repository.AddNotificationAsync(notification, cancellationToken);
        await publisher.PublishAsync(
            "notification.requested",
            new NotificationRequested(request.UserId, request.Channel, request.Subject, request.Body),
            cancellationToken);

        return created;
    }

    public Task<PushDevice> RegisterDeviceAsync(PushDevice device, CancellationToken cancellationToken)
    {
        var entity = device with
        {
            Id = device.Id == Guid.Empty ? Guid.NewGuid() : device.Id,
            LastSeenAt = DateTimeOffset.UtcNow
        };

        return repository.AddDeviceAsync(entity, cancellationToken);
    }
}
