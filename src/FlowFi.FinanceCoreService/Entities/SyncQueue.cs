using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FlowFi.FinanceCoreService.Entities;

public class SyncQueue
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required, MaxLength(30)] public string EntityType { get; set; } = string.Empty;
    [Required] public Guid EntityId { get; set; }
    [Required, MaxLength(20)] public string Action { get; set; } = string.Empty;
    [Required] public JsonDocument Payload { get; set; } = JsonDocument.Parse("{}");
    [Required, MaxLength(20)] public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SyncedAt { get; set; }
}
