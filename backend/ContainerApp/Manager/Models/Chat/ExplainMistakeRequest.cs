using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Chat;

public sealed record ExplainMistakeRequest
{
    [Required(ErrorMessage = "AttemptId is required.")]
    [JsonPropertyName("attemptId")]
    public Guid AttemptId { get; init; }

    [Required(ErrorMessage = "ThreadId is required.")]
    [MinLength(36, ErrorMessage = "ThreadId must be at least 36 characters.")]
    [JsonPropertyName("threadId")]
    public string ThreadId { get; init; } = string.Empty;

    [Required(ErrorMessage = "GameType is required.")]
    [MinLength(1, ErrorMessage = "GameType must be at least 1 character.")]
    [MaxLength(50, ErrorMessage = "GameType cannot exceed 50 characters.")]
    [JsonPropertyName("gameType")]
    public string GameType { get; init; } = string.Empty;

    [JsonPropertyName("chatType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatType ChatType { get; init; } = ChatType.ExplainMistake;
}