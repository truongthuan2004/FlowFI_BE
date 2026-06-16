using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FlowFi.Common.Middleware;

public sealed class FlowFiCorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers);
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }

    private static string ResolveCorrelationId(IHeaderDictionary headers)
    {
        return headers.TryGetValue(HeaderName, out StringValues values) && !StringValues.IsNullOrEmpty(values)
            ? values.ToString()
            : Guid.NewGuid().ToString("N");
    }
}

