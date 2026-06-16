using Microsoft.Extensions.Options;

namespace FlowFi.ApiGateway.Aggregation;

public sealed class GatewayAggregationService(
    IHttpClientFactory httpClientFactory,
    IOptions<ServiceCatalogOptions> options)
{
    public async Task<GatewayOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var checks = options.Value.Services.Select(service => CheckServiceAsync(service, cancellationToken));
        var services = await Task.WhenAll(checks);

        return new GatewayOverviewResponse(
            "FlowFi API Gateway",
            DateTimeOffset.UtcNow,
            services.OrderBy(x => x.Name).ToArray());
    }

    private async Task<ServiceHealthResponse> CheckServiceAsync(ServiceCatalogEntry service, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient(service.Name);
            using var response = await client.GetAsync(service.HealthPath, cancellationToken);

            return new ServiceHealthResponse(
                service.Name,
                service.PublicPath,
                service.BaseUrl,
                response.IsSuccessStatusCode ? "healthy" : "unhealthy",
                (int)response.StatusCode);
        }
        catch
        {
            return new ServiceHealthResponse(service.Name, service.PublicPath, service.BaseUrl, "unreachable", null);
        }
    }
}

public sealed record GatewayOverviewResponse(string Name, DateTimeOffset GeneratedAt, ServiceHealthResponse[] Services);

public sealed record ServiceHealthResponse(string Name, string PublicPath, string BaseUrl, string Status, int? StatusCode);

