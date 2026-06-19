using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlowFi.Common.Api;

internal static class FlowFiValidationErrors
{
    public static IReadOnlyDictionary<string, string[]> FromFailures(IEnumerable<ValidationFailure> failures)
    {
        return failures
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());
    }

    public static IReadOnlyDictionary<string, string[]> FromModelState(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => ToCamelCase(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The input was not valid."
                        : error.ErrorMessage)
                    .ToArray());
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
