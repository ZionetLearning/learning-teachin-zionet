namespace Manager.Models.Achievements;

public class TrackProgressResponse
{
    public bool Success { get; set; }
    public int NewCount { get; set; }
    public List<string> UnlockedAchievements { get; set; } = new();
}
