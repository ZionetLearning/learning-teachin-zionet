using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models.Games;

/// <summary>
/// Response model received from Accessor service for all student histories
/// </summary>
public class GetAllHistoriesAccessorResponse
{
    public required IEnumerable<StudentExerciseHistory> Items { get; set; } = new List<StudentExerciseHistory>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
