using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Accessor.Models;

[Table("ChatMessages")]
public class ChatMessage
{
    [Key]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Required]
    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [ForeignKey(nameof(ThreadId))]
    public ChatThread Thread { get; set; } = null!;

    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("role")]
    [EnumDataType(typeof(MessageRole))]
    public string Role { get; set; } = MessageRole.User;

    [Required]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Column("created_at_ts")]
    [JsonIgnore]
    public long CreatedAtTs { get; set; }

    [Required]
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}