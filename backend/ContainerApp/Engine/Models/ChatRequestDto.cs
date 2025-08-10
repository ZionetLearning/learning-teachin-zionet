namespace Engine.Models;

public sealed record ChatRequestDto(
    string ThreadId,
    string UserMessage,
    string UserId,
    string ChatType = "default"
    );
