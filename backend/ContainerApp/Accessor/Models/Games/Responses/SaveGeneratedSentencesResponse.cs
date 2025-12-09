namespace Accessor.Models.Games.Responses;

/// <summary>
/// Response model for saving generated sentences
/// </summary>
public sealed record SaveGeneratedSentencesResponse
{
    public required IReadOnlyList<GeneratedSentenceResultDto> Sentences { get; init; }
}
