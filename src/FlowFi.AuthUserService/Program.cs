using FlowFi.AuthUserService.Config;
using FlowFi.AuthUserService.Database;
using FlowFi.Common.Configuration;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;

EnvironmentFile.Load("AUTH");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddAuthService(builder.Configuration);

var app = builder.Build();

await app.MigrateDatabaseOnStartupAsync<AuthDbContext>();

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
