namespace FlowFi.ApiGateway.Aggregation;

public sealed class ServiceCatalogOptions
{
    public ServiceCatalogEntry[] Services { get; set; } = [];
}

public sealed class ServiceCatalogEntry
{
    public string Name { get; set; } = "";
    public string PublicPath { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string HealthPath { get; set; } = "/health";
}

