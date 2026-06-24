namespace FlowFi.AnalyticsService.Messaging;

public sealed record FinanceTransactionEvent(
    Guid TransactionId,
    Guid UserId,
    Guid WalletId,
    Guid? TagId,
    string? TagName,
    decimal Amount,
    string Type,
    string CurrencyCode,
    DateTimeOffset TransactionDate,
    DateTimeOffset OccurredAt);
