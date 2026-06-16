namespace FlowFi.FinanceCoreService.Entities;

public sealed record Transaction(Guid Id, Guid UserId, Guid WalletId, Guid? CategoryId, decimal Amount, string Currency, string Direction, string? Note, DateTimeOffset OccurredAt, DateTimeOffset CreatedAt);

