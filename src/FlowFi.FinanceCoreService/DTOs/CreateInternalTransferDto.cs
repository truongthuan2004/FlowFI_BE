using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateInternalTransferDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid FromWalletId { get; set; }

    [Required]
    public Guid ToWalletId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }

    [Required]
    public string Note { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string SyncStatus { get; set; } = string.Empty;

    public DateTimeOffset TransferDate { get; set; }
}

