namespace FlowFi.ApiGateway.Swagger;

public static class GatewaySwaggerExtensions
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static IApplicationBuilder UseGatewaySwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "docs";
            options.DocumentTitle = "FlowFi Gateway API Docs";

            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway");
            options.SwaggerEndpoint("/docs/auth/swagger/v1/swagger.json", "Auth Service");
            options.SwaggerEndpoint("/docs/finance/swagger/v1/swagger.json", "Finance Service");
            options.SwaggerEndpoint("/docs/ai/swagger/v1/swagger.json", "AI Service");
            options.SwaggerEndpoint("/docs/analytics/swagger/v1/swagger.json", "Analytics Service");
            options.SwaggerEndpoint("/docs/notifications/swagger/v1/swagger.json", "Notification Service");
        });

        return app;
    }
}
