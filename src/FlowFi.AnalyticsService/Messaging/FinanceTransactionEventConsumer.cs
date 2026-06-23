using System.Text;
using System.Text.Json;
using FlowFi.AnalyticsService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlowFi.AnalyticsService.Messaging;

public sealed class FinanceTransactionEventConsumer(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<FinanceTransactionEventConsumer> logger) : BackgroundService
{
    private static readonly string[] RoutingKeys =
    [
        "finance.transaction.created",
        "finance.transaction.updated",
        "finance.transaction.deleted"
    ];

    private IConnection? _connection;
    private IModel? _channel;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"] ?? "localhost",
                UserName = configuration["RabbitMq:Username"] ?? "guest",
                Password = configuration["RabbitMq:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("flowfi.events", ExchangeType.Topic, durable: true);
            _channel.QueueDeclare("flowfi.analytics-service", durable: true, exclusive: false, autoDelete: false);

            foreach (var routingKey in RoutingKeys)
            {
                _channel.QueueBind("flowfi.analytics-service", "flowfi.events", routingKey);
            }

            logger.LogInformation("Analytics finance transaction consumer started.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to connect to RabbitMQ. Analytics finance transaction consumer is disabled.");
        }

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var financeEvent = JsonSerializer.Deserialize<FinanceTransactionEvent>(
                    message,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (financeEvent is null)
                {
                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    return;
                }

                using var scope = serviceProvider.CreateScope();
                var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                await analytics.ProcessFinanceTransactionEventAsync(eventArgs.RoutingKey, financeEvent, stoppingToken);
                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process finance transaction event.");
                _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume("flowfi.analytics-service", autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
}
