using FlowFi.Common.Authentication;

namespace FlowFi.ApiGateway.Authentication;

public static class GatewayAuthenticationExtensions
{
    public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiJwt(configuration);
        return services;
    }

    public static IApplicationBuilder UseGatewayAuthentication(this IApplicationBuilder app)
    {
        return app.UseAuthentication();
    }
}
