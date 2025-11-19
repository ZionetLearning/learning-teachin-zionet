using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for student mistakes
/// </summary>
public class GetMistakesAccessorResponse
{
    public required IEnumerable<ExerciseMistakes> Items { get; set; } = new List<ExerciseMistakes>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
