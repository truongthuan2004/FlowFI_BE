namespace FlowFi.ApiGateway.RequestLogging;

public static class GatewayRequestLoggingExtensions
{
    public static IApplicationBuilder UseGatewayRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GatewayRequestLoggingMiddleware>();
    }
}

