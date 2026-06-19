namespace FlowFi.AnalyticsService.Entities;

public sealed class FinancialSummary
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public int? WeekNumber { get; set; }
    public int? Month { get; set; }
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetSaving { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal UsedBudget { get; set; }
    public decimal BudgetUsagePercent { get; set; }
    public int TransactionCount { get; set; }
    public int ActiveGoalCount { get; set; }
    public int AchievedGoalCount { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
}
