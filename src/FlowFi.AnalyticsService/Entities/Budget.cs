namespace FlowFi.AnalyticsService.Entities;

public sealed class Budget
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TagId { get; set; }
    public string? TagName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PeriodType { get; set; } = "Monthly";
    public decimal BudgetAmount { get; set; }
    public int WarningThresholdPercent { get; set; } = 80;
    public string CurrencyCode { get; set; } = "VND";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public BudgetProgress? Progress { get; set; }
}
