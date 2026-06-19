namespace FlowFi.Contracts.Events;

public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record UserCreated(Guid UserId, string Email) : IntegrationEvent;
public sealed record TransactionCreated(Guid UserId, Guid TransactionId, decimal Amount, string Currency) : IntegrationEvent;
public sealed record TransactionUpdated(Guid UserId, Guid TransactionId) : IntegrationEvent;
public sealed record WalletTransferred(Guid UserId, Guid SourceWalletId, Guid DestinationWalletId, decimal Amount) : IntegrationEvent;
public sealed record BudgetExceeded(Guid UserId, Guid BudgetId, decimal CurrentAmount, decimal LimitAmount) : IntegrationEvent;
public sealed record GoalProgressUpdated(Guid UserId, Guid GoalId, decimal ProgressPercent) : IntegrationEvent;
public sealed record NotificationRequested(Guid UserId, string Channel, string Subject, string Body) : IntegrationEvent;
public sealed record PasswordResetRequested(Guid UserId, string Email, string FullName, string Token, string OtpCode) : IntegrationEvent;

