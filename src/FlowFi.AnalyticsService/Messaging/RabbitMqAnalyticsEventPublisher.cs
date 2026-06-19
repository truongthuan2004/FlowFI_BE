using FlowFi.AnalyticsService.Interfaces;
using FlowFi.EventBus.Messaging;

namespace FlowFi.AnalyticsService.Messaging;

public sealed class RabbitMqAnalyticsEventPublisher(RabbitMqPublisher publisher) : IAnalyticsEventPublisher
{
    public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken)
    {
        return publisher.PublishAsync(routingKey, message, cancellationToken);
    }
}
