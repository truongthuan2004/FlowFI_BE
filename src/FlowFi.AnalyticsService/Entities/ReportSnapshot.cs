namespace FlowFi.AnalyticsService.Entities;

public sealed record analyticsnapshot(Guid Id, Guid UserId, string Period, string ContentJson, DateTimeOffset CreatedAt);

