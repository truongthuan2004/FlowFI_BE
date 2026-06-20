using FlowFi.AIProcessingService.Database;
using FlowFi.AIProcessingService.Interface;
using FlowFi.AIProcessingService.Repositories;
using FlowFi.AIProcessingService.Services;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;

namespace FlowFi.AIProcessingService.Config;

public static class AiServiceExtensions
{
    public static IServiceCollection AddAiService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ImageUploadOptions>(configuration.GetSection("ImageUpload"));
        services.Configure<AiProviderOptions>(configuration.GetSection("AiProvider"));
        services.AddFlowFiPostgres<AiProcessingDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddScoped<IAiProcessingRepository, AiProcessingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAiProcessingService, Services.AiProcessingService>();
        services.AddScoped<IReceiptParserService, ReceiptParserService>();
        services.AddScoped<IImageStorageService, LocalImageStorageService>();
        services.AddHttpClient<IAiModelClient, AiModelClient>()
            .ConfigurePrimaryHttpMessageHandler(CreateNoProxyHandler);
        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }

    private static HttpClientHandler CreateNoProxyHandler()
    {
        return new HttpClientHandler
        {
            UseProxy = false
        };
    }
}
