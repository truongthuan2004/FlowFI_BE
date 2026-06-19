using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateSyncQueueDto : IValidatableObject
{
    [Required]
    public Guid UserId { get; set; }

    [Required, StringLength(30)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public Guid EntityId { get; set; }

    [Required, RegularExpression("^(CREATE|UPDATE|DELETE)$")]
    public string Action { get; set; } = string.Empty;

    public JsonElement Payload { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Payload.ValueKind == JsonValueKind.Undefined)
        {
            yield return new ValidationResult(
                "Payload is required.",
                new[] { nameof(Payload) });
        }
    }
}

