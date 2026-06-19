using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.Entities;

public class Tag
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string Type { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Icon { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string Color { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
}

