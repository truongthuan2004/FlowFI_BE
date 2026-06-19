using FluentValidation;
using FlowFi.AnalyticsService.DTOs;

namespace FlowFi.AnalyticsService.Validators;

public sealed class CreateGoalContributionRequestValidator : AbstractValidator<CreateGoalContributionRequest>
{
    public CreateGoalContributionRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.SourceType)
            .Must(AnalyticsValidationRules.BeValidContributionSourceType)
            .WithMessage("SourceType must be Manual, AutoAllocation, RoundUp, or FromTransaction.");
    }
}
