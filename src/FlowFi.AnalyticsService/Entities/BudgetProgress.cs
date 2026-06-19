namespace FlowFi.AnalyticsService.Entities;

public sealed class BudgetProgress
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal UsagePercent { get; set; }
    public int TransactionCount { get; set; }
    public DateTimeOffset? ExceededAt { get; set; }
    public DateTimeOffset LastCalculatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Budget? Budget { get; set; }
}
