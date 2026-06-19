using System.Text;
using System.Text.Json;
using FlowFi.Contracts.Events;
using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlowFi.NotificationService.Services;

public sealed class NotificationEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationEventConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public NotificationEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<NotificationEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("flowfi.events", ExchangeType.Topic, durable: true);

            _channel.QueueDeclare("flowfi.notification-service", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "transaction.#");
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "budget.#");
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "goal.#");

            _logger.LogInformation("Notification Event Consumer started");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Consumer will not process events.");
        }

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            _logger.LogWarning("RabbitMQ channel not available. Event consumer not started.");
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                await ProcessMessageAsync(routingKey, message, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("flowfi.notification-service", false, consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(string routingKey, string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (routingKey.StartsWith("transaction."))
        {
            var transactionEvent = JsonSerializer.Deserialize<TransactionCreated>(message, options);
            if (transactionEvent is not null)
            {
                var request = new CreateNotificationRequest(
                    transactionEvent.UserId,
                    "Giao dịch mới",
                    $"Giao dịch {transactionEvent.Amount:N0} {transactionEvent.Currency} đã được ghi nhận.",
                    "TRANSACTION",
                    "IN_APP",
                    "NORMAL");

                await notificationService.CreateNotificationAsync(request, cancellationToken);
            }
        }
        else if (routingKey.StartsWith("budget."))
        {
            var budgetEvent = JsonSerializer.Deserialize<BudgetExceeded>(message, options);
            if (budgetEvent is not null)
            {
                var percentage = (budgetEvent.CurrentAmount / budgetEvent.LimitAmount * 100).ToString("F0");
                var request = new CreateNotificationRequest(
                    budgetEvent.UserId,
                    "Cảnh báo ngân sách",
                    $"Ngân sách của bạn đã sử dụng {percentage}% (vượt mức giới hạn).",
                    "BUDGET_WARNING",
                    "PUSH",
                    "HIGH");

                await notificationService.CreateNotificationAsync(request, cancellationToken);
            }
        }
        else if (routingKey.StartsWith("goal."))
        {
            var goalEvent = JsonSerializer.Deserialize<GoalProgressUpdated>(message, options);
            if (goalEvent is not null)
            {
                var request = new CreateNotificationRequest(
                    goalEvent.UserId,
                    "Cập nhật mục tiêu",
                    $"Mục tiêu của bạn đã đạt {goalEvent.ProgressPercent:F0}% tiến độ.",
                    "SYSTEM",
                    "IN_APP",
                    "NORMAL");

                await notificationService.CreateNotificationAsync(request, cancellationToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
}
