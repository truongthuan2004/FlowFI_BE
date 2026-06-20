namespace FlowFi.ApiGateway.Aggregation;

public static class GatewayAggregationExtensions
{
    public static IServiceCollection AddGatewayAggregation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceCatalogOptions>(configuration.GetSection("ServiceCatalog"));
        services.AddHttpClient();

        foreach (var service in configuration.GetSection("ServiceCatalog:Services").Get<ServiceCatalogEntry[]>() ?? [])
        {
            services.AddHttpClient(service.Name, client =>
            {
                client.BaseAddress = new Uri(service.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            });
        }

        services.AddScoped<GatewayAggregationService>();
        return services;
    }

    public static IEndpointRouteBuilder MapGatewayAggregation(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/aggregation/overview", async (GatewayAggregationService aggregation, CancellationToken cancellationToken) =>
        {
            var overview = await aggregation.GetOverviewAsync(cancellationToken);
            return Results.Ok(overview);
        })
        .WithName("GatewayOverview")
        .WithTags("API Aggregation");

        return endpoints;
    }
}
