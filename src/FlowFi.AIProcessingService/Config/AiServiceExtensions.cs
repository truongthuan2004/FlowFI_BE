using FlowFi.AIProcessingService.Database;
using FlowFi.AIProcessingService.Interfaces;
using FlowFi.AIProcessingService.Repositories;
using FlowFi.AIProcessingService.Services;
using FlowFi.Common.Persistence;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;

namespace FlowFi.AIProcessingService.Config;

public static class AiServiceExtensions
{
    public static IServiceCollection AddAiService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<AiDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<IAiService, Services.AiService>();
        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
