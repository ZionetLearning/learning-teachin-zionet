using System.Text.Json;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Models.Chat;

public record ChatAiServiceResponse
{
    public required string RequestId { get; init; }
    public required Guid ThreadId { get; init; }
    public ChatMessage? Answer { get; set; }
    public JsonElement UpdatedHistory { get; set; }
    public ChatAnswerStatus Status { get; set; } = ChatAnswerStatus.Ok;
    public string? Error { get; set; }
}
