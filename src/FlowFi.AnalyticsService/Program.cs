using FlowFi.AnalyticsService.Config;
using FlowFi.AnalyticsService.Database;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAnalyticsService(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await db.Database.MigrateAsync();
}

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
