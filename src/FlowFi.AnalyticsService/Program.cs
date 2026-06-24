using FlowFi.AnalyticsService.Config;
using FlowFi.AnalyticsService.Database;
using FlowFi.Common.Configuration;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;

EnvironmentFile.Load("ANALYTICS");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAnalyticsService(builder.Configuration);

var app = builder.Build();

await app.MigrateDatabaseOnStartupAsync<AnalyticsDbContext>();

app.UseFlowFiErrorHandling();
app.UseFlowFiCorrelationId();
app.UseFlowFiRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseFlowFiSwagger();
app.UseFlowFiStatusCodeEnvelope();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
