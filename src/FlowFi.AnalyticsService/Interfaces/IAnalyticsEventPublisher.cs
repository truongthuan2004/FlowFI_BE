namespace FlowFi.AnalyticsService.Interfaces;

public interface IAnalyticsEventPublisher
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken);
}
