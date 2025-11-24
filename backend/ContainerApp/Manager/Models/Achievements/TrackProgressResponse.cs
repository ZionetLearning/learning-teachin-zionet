namespace Manager.Models.Achievements;

public sealed record TrackProgressResponse
{
    public required bool Success { get; init; }
    public required int NewCount { get; init; }
    public required List<string> UnlockedAchievements { get; init; }
}
