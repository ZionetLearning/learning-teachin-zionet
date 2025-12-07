namespace Manager.Models.Summaries.Responses;

public sealed record GetGamePracticeSummaryResponse
{
    public required GamePracticeSummary Summary { get; init; }
    public List<GameTypeStats> ByGameType { get; init; } = new();
    public List<DailyGameStats> Daily { get; init; } = new();
    public required MistakesData Mistakes { get; init; }
}
