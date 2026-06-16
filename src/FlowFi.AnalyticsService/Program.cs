using FlowFi.AnalyticsService.Config;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Addanalyticservice(builder.Configuration);

var app = builder.Build();

app.UseFlowFiErrorHandling();
app.UseFlowFiCorrelationId();
app.UseFlowFiRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseFlowFiSwagger();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
