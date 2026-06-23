namespace FlowFi.AnalyticsService.DTOs;

public sealed class FinancialSummaryQuery
{
    public string? PeriodType { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? WeekNumber { get; set; }
}

public sealed record FinancialSummaryResponse(
    Guid Id,
    Guid UserId,
    string PeriodType,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    int? WeekNumber,
    int? Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetSaving,
    decimal TotalBudget,
    decimal UsedBudget,
    decimal BudgetUsagePercent,
    int TransactionCount,
    int ActiveGoalCount,
    int AchievedGoalCount,
    DateTimeOffset CalculatedAt);
