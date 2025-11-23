using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for student mistakes
/// </summary>
public sealed record GetMistakesAccessorResponse
{
    public required IReadOnlyList<ExerciseMistakes> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}
