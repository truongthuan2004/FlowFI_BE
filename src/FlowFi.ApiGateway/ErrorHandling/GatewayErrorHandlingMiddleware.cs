using System.Net;

namespace FlowFi.ApiGateway.ErrorHandling;

public sealed class GatewayErrorHandlingMiddleware(RequestDelegate next, ILogger<GatewayErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled gateway error for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/500",
                title = "Gateway error",
                status = 500,
                traceId = context.TraceIdentifier
            });
        }
    }
}

