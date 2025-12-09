using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for saving generated sentences
/// Matches Accessor's SaveGeneratedSentencesResponse
/// </summary>
public sealed record SaveGeneratedSentencesAccessorResponse
{
    public required IReadOnlyList<AttemptedSentenceResult> Sentences { get; init; }
}

