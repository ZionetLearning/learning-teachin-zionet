using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models;

public sealed class ChatHistorySnapshot
{
    [Key]
    public Guid ThreadId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Name { get; set; } = "New chat";
    public string ChatType { get; set; } = "default";
    [Required]
    [Column(TypeName = "jsonb")]
    public required string History { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}