namespace Manager.Models;

public sealed record ChatResponseDto(
    string AssistantMessage,
    string ThreadId
    );
