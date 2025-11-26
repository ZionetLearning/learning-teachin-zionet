namespace Manager.Models.Achievements;

public sealed record AchievementDto
{
    public required Guid AchievementId { get; init; }
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Type { get; init; }
    public required string Feature { get; init; }
    public required int TargetCount { get; init; }
    public required bool IsUnlocked { get; init; }
    public DateTime? UnlockedAt { get; init; }
}
