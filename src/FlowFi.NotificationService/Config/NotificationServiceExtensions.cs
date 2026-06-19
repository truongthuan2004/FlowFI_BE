using FlowFi.NotificationService.Database;
using FlowFi.NotificationService.Interface;
using FlowFi.NotificationService.Repositories;
using FlowFi.NotificationService.Services;
using FlowFi.EventBus.Messaging;
using FlowFi.Common.Persistence;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;

namespace FlowFi.NotificationService.Config;

public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<NotificationDbContext>(configuration, "NotificationDb");
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, Services.NotificationService>();

        // Add background service for consuming events
        services.AddHostedService<NotificationEventConsumer>();

        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
