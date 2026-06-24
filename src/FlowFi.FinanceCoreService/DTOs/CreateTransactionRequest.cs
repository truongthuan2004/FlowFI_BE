using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateTransactionRequest : IValidatableObject
{
    public Guid WalletId { get; set; }

    public Guid TagId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }

    [Required, RegularExpression("(?i)^(INCOME|EXPENSE)$")]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Note { get; set; } = string.Empty;

    [Required, StringLength(30), RegularExpression("(?i)^(MANUAL|AI|IMPORT|RECURRING)$")]
    public string Source { get; set; } = string.Empty;

    public DateTimeOffset? TransactionDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (WalletId == Guid.Empty)
        {
            yield return new ValidationResult(
                "WalletId must be a valid non-empty UUID.",
                [nameof(WalletId)]);
        }

        if (TagId == Guid.Empty)
        {
            yield return new ValidationResult(
                "TagId must be a valid non-empty UUID.",
                [nameof(TagId)]);
        }
    }
}
