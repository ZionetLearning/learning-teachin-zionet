namespace Manager.Services.Clients.Accessor.Models.Achievements;

public class AchievementAccessorModel
{
    public required Guid AchievementId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public required string Feature { get; set; }
    public required int TargetCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
