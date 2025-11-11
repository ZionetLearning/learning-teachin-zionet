using System.ComponentModel.DataAnnotations;
using Engine.Models.Games;

namespace Engine.Models.Chat;

public sealed record EngineExplainMistakeRequest
{
    public required string RequestId { get; init; }

    public Guid ThreadId { get; init; } = Guid.NewGuid();

    public required Guid UserId { get; init; }

    public required Guid AttemptId { get; init; }

    public required GameName GameType { get; init; }

    public required ChatType ChatType { get; init; } = ChatType.ExplainMistake;

    public required long SentAt { get; init; }

    [Range(1, 172800, ErrorMessage = "TtlSeconds must be between 1 and 172800 (max two day).")]
    public required int TtlSeconds { get; init; }
}