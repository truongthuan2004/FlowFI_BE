using FlowFi.FinanceCoreService.Config;
using FlowFi.Common.Middleware;
using FlowFi.Common.OpenApi;

var builder = WebApplication.CreateBuilder(args);

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
app.MapControllers();

app.Run();
