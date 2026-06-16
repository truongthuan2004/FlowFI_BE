namespace FlowFi.ApiGateway.Routing;

public static class GatewayRoutingExtensions
{
    public static IServiceCollection AddGatewayRouting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"));
        return services;
    }

    public static IEndpointRouteBuilder MapGatewayRoutes(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapReverseProxy(proxyPipeline =>
        {
            proxyPipeline.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Gateway", "FlowFi.ApiGateway");
                await next();
            });
        });

        return endpoints;
    }
}

