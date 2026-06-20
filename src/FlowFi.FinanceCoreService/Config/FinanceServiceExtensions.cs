using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;
using FlowFi.EventBus.Messaging;
using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Repositories;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;

namespace FlowFi.FinanceCoreService.Config;

public static class FinanceServiceExtensions
{
    public static IServiceCollection AddFinanceService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFlowFiPostgres<FinanceDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        services.AddSingleton<RabbitMqPublisher>();
        services.AddGrpc();

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IInternalTransferRepository, InternalTransferRepository>();
        services.AddScoped<IInternalTransferService, InternalTransferService>();
        services.AddScoped<IFinanceAuditService, FinanceAuditService>();
        services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
        services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
        services.AddScoped<ISyncQueueRepository, SyncQueueRepository>();
        services.AddScoped<ISyncQueueService, SyncQueueService>();

        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
