using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games.Responses;

/// <summary>
/// DTO representing a mistake in response
/// </summary>
public sealed record MistakeResponseDto
{
    public required Guid ExerciseId { get; init; }
    public required Guid AttemptId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required List<MistakeAttemptResponseDto> Mistakes { get; init; }
}

/// <summary>
/// DTO representing a single mistake attempt in response
/// </summary>
public sealed record MistakeAttemptResponseDto
{
    public required Guid AttemptId { get; init; }
    public required List<string> WrongAnswer { get; init; }
    public required decimal Accuracy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

