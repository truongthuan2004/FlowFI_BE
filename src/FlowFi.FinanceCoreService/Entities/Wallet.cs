using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.Entities;

public class Wallet
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string WalletType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    [Required, MaxLength(10)] public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<InternalTransfer> OutgoingTransfers { get; set; } = new List<InternalTransfer>();
    public ICollection<InternalTransfer> IncomingTransfers { get; set; } = new List<InternalTransfer>();
    public ICollection<WalletBalanceLog> BalanceLogs { get; set; } = new List<WalletBalanceLog>();
    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
}

