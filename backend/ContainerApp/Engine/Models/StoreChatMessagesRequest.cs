namespace Engine.Models;

public sealed record StoreChatMessagesRequest(
    string ThreadId,
    string UserMessage,
    string AssistantMessage
);