namespace Manager.Models.Summaries.Responses;

public sealed class GetPeriodAchievementsResponse
{
    public List<UnlockedAchievement> UnlockedInPeriod { get; init; } = new();
}

public sealed class UnlockedAchievement
{
    public required Guid AchievementId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Feature { get; init; }
    public required DateTime UnlockedAt { get; init; }
}
