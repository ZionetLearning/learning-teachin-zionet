using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Manager.Services.Clients.Engine.Models;

namespace Manager.Models;

public sealed record AiResponseModel
{
    [Required(ErrorMessage = "Id is required.")]
    [MinLength(1, ErrorMessage = "Id cannot be empty.")]
    public string Id { get; init; } = string.Empty;
    [Required(ErrorMessage = "ThreadId is required.")]
    [MinLength(1, ErrorMessage = "ThreadId cannot be empty.")]
    public required string ThreadId { get; init; }

    public string Answer { get; init; } = string.Empty;

    public long AnsweredAtUnix { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    [Required(ErrorMessage = "Status is required.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatAnswerStatus Status { get; init; } = ChatAnswerStatus.Ok;
    public string? Error { get; init; }
}