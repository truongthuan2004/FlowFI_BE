using FlowFi.Common.Api;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace FlowFi.Common.Filters;

public sealed class FlowFiValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new List<ValidationFailure>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validator = ResolveValidator(context.HttpContext.RequestServices, argument.GetType());
            if (validator is null)
            {
                continue;
            }

            var validationResult = await validator.ValidateAsync(
                CreateValidationContext(argument),
                context.HttpContext.RequestAborted);

            failures.AddRange(validationResult.Errors);
        }

        if (failures.Count > 0)
        {
            var errors = FlowFiValidationErrors.FromFailures(failures);
            context.Result = new ObjectResult(FlowFiApiEnvelope.Fail("Validation failed.", errors, context.HttpContext.TraceIdentifier))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            return;
        }

        await next();
    }

    private static IValidator? ResolveValidator(IServiceProvider services, Type argumentType)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
        return services.GetService(validatorType) as IValidator;
    }

    private static IValidationContext CreateValidationContext(object argument)
    {
        var contextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
        return (IValidationContext)Activator.CreateInstance(contextType, argument)!;
    }
}
