using FluentValidation;
using FlowFi.AnalyticsService.DTOs;

namespace FlowFi.AnalyticsService.Validators;

public sealed class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PeriodType)
            .Must(AnalyticsValidationRules.BeValidBudgetPeriod)
            .WithMessage("PeriodType must be Weekly or Monthly.");
        RuleFor(x => x.BudgetAmount).GreaterThan(0);
        RuleFor(x => x.WarningThresholdPercent)
            .InclusiveBetween(1, 100)
            .When(x => x.WarningThresholdPercent.HasValue);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}

public sealed class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
{
    public UpdateBudgetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PeriodType)
            .Must(AnalyticsValidationRules.BeValidBudgetPeriod)
            .WithMessage("PeriodType must be Weekly or Monthly.");
        RuleFor(x => x.BudgetAmount).GreaterThan(0);
        RuleFor(x => x.WarningThresholdPercent)
            .InclusiveBetween(1, 100)
            .When(x => x.WarningThresholdPercent.HasValue);
        RuleFor(x => x.Status)
            .Must(AnalyticsValidationRules.BeValidBudgetStatus)
            .WithMessage("Status must be Active, Completed, Cancelled, or Expired.");
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}
