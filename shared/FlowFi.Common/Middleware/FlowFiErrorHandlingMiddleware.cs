using System.Net;
using FlowFi.Common.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FlowFi.Common.Middleware;

public sealed class FlowFiErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<FlowFiErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled service error for {Method} {Path}. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(FlowFiApiEnvelope.Fail(
                "Internal service error.",
                null,
                context.TraceIdentifier));
        }
    }
}

