namespace FlowFi.AnalyticsService.Validators;

internal static class AnalyticsValidationRules
{
    public static readonly string[] BudgetPeriods = ["Weekly", "Monthly"];
    public static readonly string[] BudgetStatuses = ["Active", "Completed", "Cancelled", "Expired"];
    public static readonly string[] PriorityLevels = ["Low", "Medium", "High"];
    public static readonly string[] SavingGoalStatuses = ["Active", "Achieved", "Cancelled"];
    public static readonly string[] ContributionSourceTypes = ["Manual", "AutoAllocation", "RoundUp", "FromTransaction"];

    public static bool BeValidBudgetPeriod(string? value)
    {
        return IsMissingOrAllowed(value, BudgetPeriods);
    }

    public static bool BeValidBudgetStatus(string? value)
    {
        return IsMissingOrAllowed(value, BudgetStatuses);
    }

    public static bool BeValidPriorityLevel(string? value)
    {
        return IsMissingOrAllowed(value, PriorityLevels);
    }

    public static bool BeValidSavingGoalStatus(string? value)
    {
        return IsMissingOrAllowed(value, SavingGoalStatuses);
    }

    public static bool BeValidContributionSourceType(string? value)
    {
        return IsMissingOrAllowed(value, ContributionSourceTypes);
    }

    public static string NormalizeOrDefault(string? value, string defaultValue, IReadOnlyCollection<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return allowedValues.First(allowed => string.Equals(allowed, value, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMissingOrAllowed(string? value, IEnumerable<string> allowedValues)
    {
        return string.IsNullOrWhiteSpace(value)
            || allowedValues.Any(allowed => string.Equals(allowed, value, StringComparison.OrdinalIgnoreCase));
    }
}
