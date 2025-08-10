using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Models;

[Index(nameof(ThreadId), nameof(Timestamp))]
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

    [JsonIgnore]
    public ChatThread Thread { get; set; } = null!;

    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("role")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageRole Role { get; set; } = MessageRole.User;

    [Required]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Column("timestamp", TypeName = "timestamptz")]
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}