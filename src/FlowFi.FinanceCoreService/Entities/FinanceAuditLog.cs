using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FlowFi.FinanceCoreService.Entities;

public class FinanceAuditLog
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required, MaxLength(30)] public string EntityType { get; set; } = string.Empty;
    [Required] public Guid EntityId { get; set; }
    [Required, MaxLength(20)] public string Action { get; set; } = string.Empty;
    public JsonDocument? OldData { get; set; }
    public JsonDocument? NewData { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

