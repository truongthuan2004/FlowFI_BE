using FluentValidation;
using FlowFi.AnalyticsService.DTOs;

namespace FlowFi.AnalyticsService.Validators;

public sealed class CreateSavingGoalRequestValidator : AbstractValidator<CreateSavingGoalRequest>
{
    public CreateSavingGoalRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CurrentAmount.HasValue);
        RuleFor(x => x.PriorityLevel)
            .Must(AnalyticsValidationRules.BeValidPriorityLevel)
            .WithMessage("PriorityLevel must be Low, Medium, or High.");
    }
}

public sealed class GetSavingGoalsQueryValidator : AbstractValidator<GetSavingGoalsQuery>
{
    public GetSavingGoalsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class UpdateSavingGoalRequestValidator : AbstractValidator<UpdateSavingGoalRequest>
{
    public UpdateSavingGoalRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CurrentAmount.HasValue);
        RuleFor(x => x.PriorityLevel)
            .Must(AnalyticsValidationRules.BeValidPriorityLevel)
            .WithMessage("PriorityLevel must be Low, Medium, or High.");
        RuleFor(x => x.Status)
            .Must(AnalyticsValidationRules.BeValidSavingGoalStatus)
            .WithMessage("Status must be Active, Achieved, or Cancelled.");
    }
}

public sealed class UpdateGoalProgressRequestValidator : AbstractValidator<UpdateGoalProgressRequest>
{
    public UpdateGoalProgressRequestValidator()
    {
        RuleFor(x => x.CurrentAmount).GreaterThanOrEqualTo(0);
    }
}
