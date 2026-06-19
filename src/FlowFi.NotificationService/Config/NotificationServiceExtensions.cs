using FlowFi.NotificationService.Database;
using FlowFi.NotificationService.Interfaces;
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
        services.AddFlowFiPostgres<NotificationDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, Services.NotificationService>();
        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
