using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Manager.Models.UserGameConfiguration;

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
    [JsonPropertyName("gameType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GameName GameType { get; init; }

    [JsonPropertyName("chatType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatType ChatType { get; init; } = ChatType.ExplainMistake;
}