using System.Text;
using System.Text.Json;
using FlowFi.Contracts.Events;
using FlowFi.NotificationService.DTOs;
using FlowFi.NotificationService.Interface;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlowFi.NotificationService.Services;

public sealed class NotificationEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationEventConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public NotificationEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<NotificationEventConsumer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:Host"] ?? "localhost",
                UserName = _configuration["RabbitMq:Username"] ?? "guest",
                Password = _configuration["RabbitMq:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("flowfi.events", ExchangeType.Topic, durable: true);

            _channel.QueueDeclare("flowfi.notification-service", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "transaction.#");
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "budget.#");
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "goal.#");
            _channel.QueueBind("flowfi.notification-service", "flowfi.events", "auth.#");

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
        else if (routingKey.StartsWith("auth."))
        {
            if (routingKey == "auth.password-reset-requested")
            {
                var resetEvent = JsonSerializer.Deserialize<PasswordResetRequested>(message, options);
                if (resetEvent is not null)
                {
                    var subject = "Yêu cầu khôi phục mật khẩu";
                    var currentYear = DateTime.UtcNow.Year.ToString();
                    var body = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; padding: 30px; border: 1px solid #e0e0e0; border-radius: 12px; background-color: #ffffff; box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);"">
    <div style=""text-align: center; margin-bottom: 25px;"">
        <h2 style=""color: #4f46e5; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px;"">FlowFi</h2>
    </div>
    <div style=""border-bottom: 1px solid #f0f0f0; padding-bottom: 20px; margin-bottom: 20px;"">
        <h3 style=""color: #1f2937; margin-top: 0; font-size: 20px; font-weight: 600;"">Khôi phục mật khẩu tài khoản</h3>
        <p style=""color: #4b5563; font-size: 15px; line-height: 1.6;"">Xin chào <strong>{resetEvent.FullName}</strong>,</p>
        <p style=""color: #4b5563; font-size: 15px; line-height: 1.6;"">Chúng tôi nhận được yêu cầu khôi phục mật khẩu cho tài khoản của bạn tại hệ thống FlowFi. Vui lòng sử dụng mã OTP dưới đây để hoàn tất quá trình thiết lập lại mật khẩu:</p>
    </div>
    <div style=""text-align: center; margin: 30px 0;"">
        <span style=""font-size: 32px; font-weight: 700; letter-spacing: 6px; background-color: #f3f4f6; color: #4f46e5; padding: 12px 30px; border-radius: 8px; border: 1px dashed #cbd5e1; display: inline-block;"">{resetEvent.OtpCode}</span>
    </div>
    <div style=""border-top: 1px solid #f0f0f0; padding-top: 20px; margin-top: 20px;"">
        <p style=""color: #4b5563; font-size: 15px; line-height: 1.6;"">Hoặc bạn có thể nhấp trực tiếp vào đường dẫn dưới đây:</p>
        <div style=""text-align: center; margin: 25px 0;"">
            <a href=""http://localhost:5173/reset-password?email={Uri.EscapeDataString(resetEvent.Email)}&token={resetEvent.Token}"" style=""background: linear-gradient(135deg, #4f46e5 0%, #4338ca 100%); color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2);"">Đặt lại mật khẩu</a>
        </div>
        <p style=""font-size: 13px; color: #ef4444; line-height: 1.5; margin-bottom: 0;""><strong>Lưu ý:</strong> Mã OTP và liên kết này chỉ có hiệu lực trong vòng 30 phút. Nếu bạn không yêu cầu hành động này, vui lòng bỏ qua email và bảo mật tài khoản của mình.</p>
    </div>
    <hr style=""border: 0; border-top: 1px solid #f0f0f0; margin: 30px 0 20px 0;"">
    <p style=""font-size: 12px; color: #9ca3af; text-align: center; margin: 0;"">© {currentYear} FlowFi. Tất cả các quyền được bảo lưu.</p>
</div>
";
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    await emailSender.SendEmailAsync(resetEvent.Email, subject, body, isHtml: true, cancellationToken);

                    var request = new CreateNotificationRequest(
                        resetEvent.UserId,
                        "Khôi phục mật khẩu",
                        $"Yêu cầu khôi phục mật khẩu đã được gửi đến email {resetEvent.Email}.",
                        "SYSTEM",
                        "EMAIL",
                        "HIGH");

                    await notificationService.CreateNotificationAsync(request, cancellationToken);
                }
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
