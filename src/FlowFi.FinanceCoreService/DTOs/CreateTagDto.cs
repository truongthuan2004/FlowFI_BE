using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateTagDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Icon { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Color { get; set; } = string.Empty;
}

