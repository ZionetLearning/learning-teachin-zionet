namespace Manager.Services.Clients.Accessor.Models.Achievements;

public class UserProgressAccessorModel
{
    public required Guid UserProgressId { get; set; }
    public required Guid UserId { get; set; }
    public required string Feature { get; set; }
    public int Count { get; set; }
    public DateTime LastUpdated { get; set; }
}
