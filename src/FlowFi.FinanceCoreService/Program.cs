using FlowFi.FinanceCoreService.Config;
using FlowFi.Common.Configuration;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;
using FlowFi.FinanceCoreService.Grpc;
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

builder.Services.AddFinanceService(builder.Configuration);

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
