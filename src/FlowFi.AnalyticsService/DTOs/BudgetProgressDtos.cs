namespace FlowFi.AnalyticsService.DTOs;

public sealed record BudgetProgressResponse(
    Guid Id,
    Guid BudgetId,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal UsagePercent,
    int TransactionCount,
    DateTimeOffset? ExceededAt,
    DateTimeOffset LastCalculatedAt,
    DateTimeOffset? UpdatedAt);
