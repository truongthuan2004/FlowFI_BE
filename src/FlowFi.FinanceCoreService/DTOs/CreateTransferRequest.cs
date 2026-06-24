using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateTransferRequest : IValidatableObject
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid FromWalletId { get; set; }

    [Required]
    public Guid ToWalletId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }

    [Required, StringLength(2000)]
    public string Note { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string SyncStatus { get; set; } = string.Empty;

    public DateTimeOffset TransferDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserId == Guid.Empty)
        {
            yield return new ValidationResult(
                "UserId must be a valid non-empty UUID.",
                [nameof(UserId)]);
        }

        if (FromWalletId == Guid.Empty)
        {
            yield return new ValidationResult(
                "FromWalletId must be a valid non-empty UUID.",
                [nameof(FromWalletId)]);
        }

        if (ToWalletId == Guid.Empty)
        {
            yield return new ValidationResult(
                "ToWalletId must be a valid non-empty UUID.",
                [nameof(ToWalletId)]);
        }
    }
}
