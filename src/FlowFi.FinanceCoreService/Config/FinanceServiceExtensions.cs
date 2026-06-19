using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Interfaces;
using FlowFi.FinanceCoreService.Repositories;
using FlowFi.FinanceCoreService.Services;
using FlowFi.EventBus.Messaging;
using FlowFi.Common.Persistence;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;

namespace FlowFi.FinanceCoreService.Config;

public static class FinanceServiceExtensions
{
    public static IServiceCollection AddFinanceService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<FinanceDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<IFinanceRepository, FinanceRepository>();
        services.AddScoped<IFinanceService, Services.FinanceService>();
        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
