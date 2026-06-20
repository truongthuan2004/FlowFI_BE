using FlowFi.AIProcessingService.Database;
using FlowFi.AIProcessingService.Interface;
using FlowFi.AIProcessingService.Repositories;
using FlowFi.AIProcessingService.Services;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;
using FlowFi.GrpcContracts.Finance;

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
        services.AddScoped<IFinanceTransactionsClient, FinanceTransactionsGrpcClient>();
        services.AddScoped<IImageTransactionService, ImageTransactionService>();
        services.AddHttpClient<IAiModelClient, AiModelClient>()
            .ConfigurePrimaryHttpMessageHandler(CreateNoProxyHandler);
        services.AddGrpcClient<FinanceTransactions.FinanceTransactionsClient>(options =>
            {
                var address = configuration["FinanceGrpc:Address"]
                    ?? throw new InvalidOperationException("FinanceGrpc:Address is required.");
                options.Address = new Uri(address);
            })
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
