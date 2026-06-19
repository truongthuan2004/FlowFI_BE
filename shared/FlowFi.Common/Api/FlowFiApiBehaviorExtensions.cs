using FlowFi.Common.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FlowFi.Common.Api;

public static class FlowFiApiBehaviorExtensions
{
    public static IMvcBuilder AddFlowFiApiBehavior(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<FlowFiValidationActionFilter>();
        builder.Services.AddScoped<FlowFiApiResponseResultFilter>();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = FlowFiValidationErrors.FromModelState(context.ModelState);
                return new ObjectResult(FlowFiApiEnvelope.Fail("Validation failed.", errors, context.HttpContext.TraceIdentifier))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            };
        });

        builder.AddMvcOptions(options =>
        {
            options.Filters.AddService<FlowFiValidationActionFilter>();
            options.Filters.AddService<FlowFiApiResponseResultFilter>();
        });

        return builder;
    }
}
