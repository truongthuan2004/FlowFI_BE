using FluentValidation;
using FlowFi.AnalyticsService.Database;
using FlowFi.AnalyticsService.Interfaces;
using FlowFi.AnalyticsService.Messaging;
using FlowFi.AnalyticsService.Repositories;
using FlowFi.AnalyticsService.Services;
using FlowFi.AnalyticsService.Validators;
using FlowFi.EventBus.Messaging;
using FlowFi.Common.Api;
using FlowFi.Common.Persistence;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;

namespace FlowFi.AnalyticsService.Config;

public static class AnalyticsServiceExtensions
{
    public static IServiceCollection AddAnalyticsService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<AnalyticsDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<RabbitMqPublisher>();
        services.AddSingleton<IAnalyticsEventPublisher, RabbitMqAnalyticsEventPublisher>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<IAnalyticsService, Services.AnalyticsService>();
        services.AddValidatorsFromAssemblyContaining<CreateBudgetRequestValidator>();
        services.AddControllers().AddFlowFiApiBehavior();
        services.AddFlowFiSwagger();
        return services;
    }
}
