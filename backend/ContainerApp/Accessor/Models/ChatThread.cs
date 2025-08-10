using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Accessor.Models;

[Table("ChatThreads")]
public class ChatThread
{
    [Key]
    [JsonPropertyName("threadId")]
    public Guid ThreadId { get; set; }

    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    [Required]
    [JsonPropertyName("chatName")]
    public string ChatName { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("chatType")]
    public string ChatType { get; set; } = "default";

    [Required]
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonIgnore]
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

}
