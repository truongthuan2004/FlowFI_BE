using FlowFi.Common.Authentication;
using FlowFi.Common.Configuration;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;
using FlowFi.EventBus.Messaging;
using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.Grpc;
using FlowFi.FinanceCoreService.Repositories;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;

EnvironmentFile.Load("FINANCE");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var httpPort = builder.Configuration.GetValue("ServicePorts:Http", 5102);
var grpcPort = builder.Configuration.GetValue("ServicePorts:Grpc", 7102);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(httpPort, endpoint => endpoint.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(grpcPort, endpoint => endpoint.Protocols = HttpProtocols.Http2);
});

builder.Services.AddFlowFiPostgres<FinanceDbContext>(builder.Configuration);
builder.Services.AddFlowFiJwt(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddGrpc();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IInternalTransferRepository, InternalTransferRepository>();
builder.Services.AddScoped<IInternalTransferService, InternalTransferService>();
builder.Services.AddScoped<IWalletBalanceLogRepository, WalletBalanceLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFinanceAuditRepository, FinanceAuditRepository>();
builder.Services.AddScoped<IFinanceAuditService, FinanceAuditService>();
builder.Services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
builder.Services.AddScoped<ISyncQueueRepository, SyncQueueRepository>();
builder.Services.AddScoped<ISyncQueueService, SyncQueueService>();

builder.Services.AddControllers();
builder.Services.AddFlowFiSwagger();

var app = builder.Build();

app.UseFlowFiErrorHandling();
app.UseFlowFiCorrelationId();
app.UseFlowFiRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseFlowFiSwagger();
}

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();
}

app.MapHealthChecks("/health").AllowAnonymous();
app.MapGrpcService<FinanceTransactionsGrpcService>();
app.MapControllers();

app.Run();
