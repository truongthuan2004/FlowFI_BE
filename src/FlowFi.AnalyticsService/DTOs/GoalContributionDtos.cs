namespace FlowFi.AnalyticsService.DTOs;

public sealed record CreateGoalContributionRequest(
    decimal Amount,
    DateTimeOffset? ContributionDate,
    string? SourceType,
    Guid? SourceReferenceId,
    string? Note);

public sealed record GoalContributionResponse(
    Guid Id,
    Guid GoalId,
    decimal Amount,
    DateTimeOffset ContributionDate,
    string SourceType,
    Guid? SourceReferenceId,
    string? Note,
    DateTimeOffset CreatedAt);
