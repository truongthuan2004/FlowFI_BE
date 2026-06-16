namespace FlowFi.AIProcessingService.Entities;

public sealed record AiJob(Guid Id, Guid UserId, string Kind, string SourceUri, string Status, DateTimeOffset CreatedAt);

