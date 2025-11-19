namespace Manager.Models.Games;

/// <summary>
/// Response model for getting all student histories (admin/teacher view)
/// </summary>
public sealed class GetAllHistoriesResponse
{
    public required IEnumerable<SummaryHistoryWithStudentDto> Items { get; set; } = new List<SummaryHistoryWithStudentDto>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
