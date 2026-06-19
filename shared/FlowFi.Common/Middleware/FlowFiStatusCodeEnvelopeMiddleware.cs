using FlowFi.Common.Api;
using Microsoft.AspNetCore.Http;

namespace FlowFi.Common.Middleware;

public sealed class FlowFiStatusCodeEnvelopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.HasStarted
            || context.Response.StatusCode == StatusCodes.Status204NoContent
            || !ShouldWriteEnvelope(context.Response.StatusCode)
            || !string.IsNullOrWhiteSpace(context.Response.ContentType))
        {
            return;
        }

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(FlowFiApiEnvelope.Fail(
            ResolveMessage(context.Response.StatusCode),
            null,
            context.TraceIdentifier));
    }

    private static bool ShouldWriteEnvelope(int statusCode)
    {
        return statusCode is StatusCodes.Status400BadRequest
            or StatusCodes.Status401Unauthorized
            or StatusCodes.Status403Forbidden
            or StatusCodes.Status404NotFound
            or StatusCodes.Status405MethodNotAllowed;
    }

    private static string ResolveMessage(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request.",
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            StatusCodes.Status403Forbidden => "Forbidden.",
            StatusCodes.Status404NotFound => "Resource not found.",
            StatusCodes.Status405MethodNotAllowed => "Method not allowed.",
            _ => "Request failed."
        };
    }
}
