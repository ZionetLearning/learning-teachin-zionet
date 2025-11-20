namespace Manager.Models.Achievements;

public class TrackProgressRequest
{
    public required Guid UserId { get; set; }
    public required string Feature { get; set; }
    public int IncrementBy { get; set; } = 1;
}
