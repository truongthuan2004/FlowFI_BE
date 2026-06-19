namespace FlowFi.AnalyticsService.Entities;

public sealed class GoalContribution
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset ContributionDate { get; set; }
    public string SourceType { get; set; } = "Manual";
    public Guid? SourceReferenceId { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public SavingGoal? Goal { get; set; }
}
