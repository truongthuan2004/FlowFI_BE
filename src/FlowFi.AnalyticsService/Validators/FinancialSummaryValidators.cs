using FluentValidation;
using FlowFi.AnalyticsService.DTOs;

namespace FlowFi.AnalyticsService.Validators;

public sealed class FinancialSummaryQueryValidator : AbstractValidator<FinancialSummaryQuery>
{
    public FinancialSummaryQueryValidator()
    {
        RuleFor(x => x.PeriodType)
            .NotEmpty()
            .Must(AnalyticsValidationRules.BeValidBudgetPeriod)
            .WithMessage("PeriodType must be Weekly or Monthly.");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000);

        When(x => string.Equals(x.PeriodType, "Monthly", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Month)
                .NotNull()
                .InclusiveBetween(1, 12);
            RuleFor(x => x.WeekNumber)
                .Null()
                .WithMessage("WeekNumber must be empty for Monthly summaries.");
        });

        When(x => string.Equals(x.PeriodType, "Weekly", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.WeekNumber)
                .NotNull()
                .InclusiveBetween(1, 53);
            RuleFor(x => x.Month)
                .Null()
                .WithMessage("Month must be empty for Weekly summaries.");
        });
    }
}
