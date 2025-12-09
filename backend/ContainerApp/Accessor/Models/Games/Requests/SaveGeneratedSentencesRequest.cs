using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games.Requests;

/// <summary>
/// Request model for saving generated sentences
/// </summary>
public sealed record SaveGeneratedSentencesRequest
{
    public required Guid StudentId { get; init; }
    public required GameName GameType { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required List<GeneratedSentenceItemRequest> Sentences { get; init; }
}

/// <summary>
/// Individual sentence item in the request
/// </summary>
public sealed record GeneratedSentenceItemRequest
{
    public required string Text { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required bool Nikud { get; init; }
}

