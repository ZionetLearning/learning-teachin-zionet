using System.ComponentModel.DataAnnotations;

namespace Accessor.Models;

public class IdempotencyRecord
{
    [Required]
    public string Key { get; set; } = null!;
    [Required]
    public int TaskId { get; set; }
    [Required]
    public string Status { get; set; } = "Done";
}
