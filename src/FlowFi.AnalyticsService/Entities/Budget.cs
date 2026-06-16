namespace FlowFi.AnalyticsService.Entities;

public sealed record Budget(Guid Id, Guid UserId, string Name, decimal LimitAmount, decimal CurrentAmount, DateOnly StartDate, DateOnly EndDate);

