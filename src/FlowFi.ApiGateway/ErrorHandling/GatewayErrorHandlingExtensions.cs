namespace FlowFi.ApiGateway.ErrorHandling;

public static class GatewayErrorHandlingExtensions
{
    public static IApplicationBuilder UseGatewayErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GatewayErrorHandlingMiddleware>();
    }
}

