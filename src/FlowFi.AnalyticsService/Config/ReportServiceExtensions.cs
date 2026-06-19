using FlowFi.AnalyticsService.Database;
using FlowFi.AnalyticsService.Interface;
using FlowFi.AnalyticsService.Repositories;
using FlowFi.AnalyticsService.Services;
using FlowFi.EventBus.Messaging;
using FlowFi.Common.Persistence;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;

namespace FlowFi.AnalyticsService.Config;

public static class analyticserviceExtensions
{
    public static IServiceCollection Addanalyticservice(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<ReportDbContext>(configuration, "ReportDb");
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<Ianalyticservice, Services.analyticservice>();
        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
