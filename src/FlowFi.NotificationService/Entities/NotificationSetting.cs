namespace FlowFi.NotificationService.Entities;

public sealed class NotificationSetting
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EnableEmail { get; set; } = true;
    public bool EnablePush { get; set; } = true;
    public bool EnableInApp { get; set; } = true;
    public bool EnableBudgetWarning { get; set; } = true;
    public bool EnableTransactionAlert { get; set; } = true;
    public bool EnableSystemAlert { get; set; } = true;
    public TimeOnly? QuietHoursStart { get; set; }
    public TimeOnly? QuietHoursEnd { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
