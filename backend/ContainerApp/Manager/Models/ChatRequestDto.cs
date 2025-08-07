namespace Manager.Models;

public sealed record ChatRequestDto(
    string ThreadId,
    string UserMessage,
    string ChatType = "default"
    );
