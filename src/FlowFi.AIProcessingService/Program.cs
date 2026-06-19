using FlowFi.AIProcessingService.Config;
using FlowFi.Common.Configuration;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;

EnvironmentFile.Load("AI");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddAiService(builder.Configuration);

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
