using FlowFi.ApiGateway.Aggregation;
using FlowFi.ApiGateway.Authentication;
using FlowFi.ApiGateway.Authorization;
using FlowFi.ApiGateway.ErrorHandling;
using FlowFi.ApiGateway.RateLimiting;
using FlowFi.ApiGateway.RequestLogging;
using FlowFi.ApiGateway.Routing;
using FlowFi.ApiGateway.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGatewayAuthentication(builder.Configuration);
builder.Services.AddGatewayAuthorization();
builder.Services.AddGatewayRateLimiting();
builder.Services.AddGatewayRouting(builder.Configuration);
builder.Services.AddGatewaySwagger();
builder.Services.AddGatewayAggregation(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

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

