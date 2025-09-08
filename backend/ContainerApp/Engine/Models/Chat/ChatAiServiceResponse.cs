using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Engine.Models.Chat;

public record ChatAiServiceResponse
{
    public required string RequestId { get; init; }
    public required Guid ThreadId { get; init; }
    public ChatMessage? Answer { get; set; }

    public ChatHistory UpdatedHistory { get; set; } = new ChatHistory();
    public ChatAnswerStatus Status { get; set; } = ChatAnswerStatus.Ok;
    public string? Error { get; set; }
}
