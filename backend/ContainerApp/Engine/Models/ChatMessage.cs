namespace Engine.Models;

public sealed record ChatMessage(
    string ThreadId,
    string UserId,
    string Message,
    string Role, // "user" or "assistant"
    DateTime Timestamp
);