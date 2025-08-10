namespace Engine.Models;

public sealed record ChatHistoryResponse(
    string ThreadId,
    List<ChatMessage> Messages
);