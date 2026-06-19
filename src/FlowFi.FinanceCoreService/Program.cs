using FlowFi.FinanceCoreService.Config;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFinanceService(builder.Configuration);

var app = builder.Build();

app.UseFlowFiErrorHandling();
app.UseFlowFiCorrelationId();
app.UseFlowFiRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseFlowFiSwagger("FlowFi Finance Core Service");

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
