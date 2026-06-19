using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateTransactionDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid WalletId { get; set; }

    [Required]
    public Guid TagId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }

    [Required, RegularExpression("^(INCOME|EXPENSE)$")]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Note { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Source { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string SyncStatus { get; set; } = string.Empty;

    public DateTimeOffset TransactionDate { get; set; }
}

