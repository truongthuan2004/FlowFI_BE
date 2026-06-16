namespace FlowFi.AIProcessingService.Entities;

public sealed record AiInsight(Guid Id, Guid UserId, string InsightType, string Content, decimal? Confidence, DateTimeOffset CreatedAt);

