namespace FlowFi.AnalyticsService.Entities;

public sealed record SavingStreak(Guid Id, Guid UserId, int Days, DateOnly LastActiveDate);

