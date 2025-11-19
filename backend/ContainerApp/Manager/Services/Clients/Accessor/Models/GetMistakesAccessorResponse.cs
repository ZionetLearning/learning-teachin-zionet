using Manager.Models.Games;

namespace Manager.Services.Clients.Accessor.Models;

/// <summary>
/// Response model received from Accessor service for student mistakes
/// </summary>
public class GetMistakesAccessorResponse
{
    public required IEnumerable<MistakeDto> Items { get; set; } = new List<MistakeDto>();
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
}
