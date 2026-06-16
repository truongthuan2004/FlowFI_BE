using Microsoft.AspNetCore.Builder;

namespace FlowFi.Common.Middleware;

public static class FlowFiMiddlewareExtensions
{
    public static IApplicationBuilder UseFlowFiErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FlowFiErrorHandlingMiddleware>();
    }

    public static IApplicationBuilder UseFlowFiCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FlowFiCorrelationIdMiddleware>();
    }

    public static IApplicationBuilder UseFlowFiRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FlowFiRequestLoggingMiddleware>();
    }
}

