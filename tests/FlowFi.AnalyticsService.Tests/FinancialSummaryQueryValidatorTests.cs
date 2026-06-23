using FlowFi.AnalyticsService.DTOs;
using FlowFi.AnalyticsService.Validators;
using Xunit;

namespace FlowFi.AnalyticsService.Tests;

public sealed class FinancialSummaryQueryValidatorTests
{
    private readonly FinancialSummaryQueryValidator _validator = new();

    [Fact]
    public void Validate_rejects_monthly_query_without_month()
    {
        var result = _validator.Validate(new FinancialSummaryQuery
        {
            PeriodType = "Monthly",
            Year = 2026
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(FinancialSummaryQuery.Month));
    }

    [Fact]
    public void Validate_rejects_weekly_query_without_week_number()
    {
        var result = _validator.Validate(new FinancialSummaryQuery
        {
            PeriodType = "Weekly",
            Year = 2026
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(FinancialSummaryQuery.WeekNumber));
    }
}
