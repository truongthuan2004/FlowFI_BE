namespace FlowFi.AnalyticsService.DTOs;

public sealed record GetBudgetsQuery(Guid UserId);

public sealed record CreateBudgetRequest(
    Guid UserId,
    Guid? TagId,
    string? TagName,
    string Name,
    string? PeriodType,
    decimal BudgetAmount,
    int? WarningThresholdPercent,
    string? CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate);

public sealed record UpdateBudgetRequest(
    Guid? TagId,
    string? TagName,
    string Name,
    string? PeriodType,
    decimal BudgetAmount,
    int? WarningThresholdPercent,
    string? CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Status);

public sealed record BudgetResponse(
    Guid Id,
    Guid UserId,
    Guid? TagId,
    string? TagName,
    string Name,
    string PeriodType,
    decimal BudgetAmount,
    int WarningThresholdPercent,
    string CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
