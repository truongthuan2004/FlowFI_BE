namespace FlowFi.AnalyticsService.DTOs;

public sealed record CreateSavingGoalRequest(
    string Name,
    string? Description,
    decimal TargetAmount,
    decimal? CurrentAmount,
    string? CurrencyCode,
    DateOnly? TargetDate,
    string? PriorityLevel);

public sealed record UpdateSavingGoalRequest(
    string Name,
    string? Description,
    decimal TargetAmount,
    decimal? CurrentAmount,
    string? CurrencyCode,
    DateOnly? TargetDate,
    string? PriorityLevel,
    string? Status);

public sealed record UpdateGoalProgressRequest(decimal CurrentAmount);

public sealed record SavingGoalResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string? Description,
    decimal TargetAmount,
    decimal CurrentAmount,
    string CurrencyCode,
    DateOnly? TargetDate,
    string PriorityLevel,
    string Status,
    DateTimeOffset? AchievedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
