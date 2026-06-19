namespace FlowFi.AnalyticsService.Entities;

public sealed class SavingGoal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public DateOnly? TargetDate { get; set; }
    public string PriorityLevel { get; set; } = "Medium";
    public string Status { get; set; } = "Active";
    public DateTimeOffset? AchievedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<GoalContribution> Contributions { get; set; } = [];
}
