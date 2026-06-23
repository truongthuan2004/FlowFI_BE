using FlowFi.ApiGateway.Aggregation;
using FlowFi.ApiGateway.Authentication;
using FlowFi.ApiGateway.Authorization;
using FlowFi.ApiGateway.ErrorHandling;
using FlowFi.ApiGateway.RateLimiting;
using FlowFi.ApiGateway.RequestLogging;
using FlowFi.ApiGateway.Routing;
using FlowFi.ApiGateway.Swagger;
using FlowFi.Common.Configuration;

EnvironmentFile.Load("GATEWAY");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddGatewayAuthentication(builder.Configuration);
builder.Services.AddGatewayAuthorization();
builder.Services.AddGatewayRateLimiting();
builder.Services.AddGatewayRouting(builder.Configuration);
builder.Services.AddGatewaySwagger();
builder.Services.AddGatewayAggregation(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseGatewayErrorHandling();
app.UseGatewayRequestLogging();
app.UseGatewayRateLimiting();
app.UseGatewayAuthentication();
app.UseGatewayAuthorization();
app.UseGatewaySwagger();

app.MapGet("/", () => Results.Ok(new
{
    name = "FlowFi API Gateway",
    docs = "/docs",
    health = "/health",
    aggregation = "/aggregation/overview",
    services = new[] { "auth", "finance", "ai", "analytics", "notifications", "ws" }
}));

app.MapHealthChecks("/health");
app.MapGatewayAggregation();
app.MapGatewayRoutes();

app.Run();
