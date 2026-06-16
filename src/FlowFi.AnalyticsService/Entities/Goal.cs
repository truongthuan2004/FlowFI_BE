namespace FlowFi.AnalyticsService.Entities;

public sealed record Goal(Guid Id, Guid UserId, string Name, decimal TargetAmount, decimal CurrentAmount, DateOnly TargetDate);

