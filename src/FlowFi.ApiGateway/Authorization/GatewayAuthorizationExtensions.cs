namespace FlowFi.ApiGateway.Authorization;

public static class GatewayAuthorizationExtensions
{
    public static IServiceCollection AddGatewayAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthenticatedClient", policy => policy.RequireAuthenticatedUser());
        });

        return services;
    }

    public static IApplicationBuilder UseGatewayAuthorization(this IApplicationBuilder app)
    {
        return app.UseAuthorization();
    }
}

