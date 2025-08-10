namespace Engine.Models;

public sealed record StoreChatMessagesRequest(
    string ThreadId,
    string UserId,
    string UserMessage,
    string AssistantMessage
);