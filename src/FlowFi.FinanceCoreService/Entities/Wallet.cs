namespace FlowFi.FinanceCoreService.Entities;

public sealed record Wallet(Guid Id, Guid UserId, string Name, string Currency, decimal Balance, string Type);

