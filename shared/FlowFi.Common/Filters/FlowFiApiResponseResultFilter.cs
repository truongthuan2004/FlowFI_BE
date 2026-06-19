using FlowFi.Common.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FlowFi.Common.Filters;

public sealed class FlowFiApiResponseResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is FlowFiApiEnvelope)
            {
                await next();
                return;
            }

            if (IsSuccessStatusCode(objectResult.StatusCode))
            {
                objectResult.Value = FlowFiApiEnvelope.Ok(
                    objectResult.Value,
                    null,
                    context.HttpContext.TraceIdentifier);
            }
            else if (ShouldWriteClientError(objectResult.StatusCode))
            {
                objectResult.Value = FlowFiApiEnvelope.Fail(
                    ResolveMessage(objectResult.StatusCode!.Value),
                    null,
                    context.HttpContext.TraceIdentifier);
            }

            await next();
            return;
        }

        if (context.Result is IStatusCodeActionResult statusCodeResult
            && ShouldWriteClientError(statusCodeResult.StatusCode))
        {
            var statusCode = statusCodeResult.StatusCode!.Value;
            context.Result = new ObjectResult(FlowFiApiEnvelope.Fail(
                ResolveMessage(statusCode),
                null,
                context.HttpContext.TraceIdentifier))
            {
                StatusCode = statusCode
            };
        }

        await next();
    }

    private static bool IsSuccessStatusCode(int? statusCode)
    {
        var code = statusCode ?? StatusCodes.Status200OK;
        return code is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices
            && code != StatusCodes.Status204NoContent;
    }

    private static bool ShouldWriteClientError(int? statusCode)
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
