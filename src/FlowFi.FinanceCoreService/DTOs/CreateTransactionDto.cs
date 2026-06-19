using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateTransactionDto : IValidatableObject
{
    public Guid TagId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }

    [Required, RegularExpression("(?i)^(INCOME|EXPENSE)$")]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Note { get; set; } = string.Empty;

    [Required, StringLength(30), RegularExpression("(?i)^(MANUAL|AI|IMPORT|RECURRING)$")]
    public string Source { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TagId == Guid.Empty)
        {
            yield return new ValidationResult(
                "TagId must be a valid non-empty UUID.",
                [nameof(TagId)]);
        }
    }
}

