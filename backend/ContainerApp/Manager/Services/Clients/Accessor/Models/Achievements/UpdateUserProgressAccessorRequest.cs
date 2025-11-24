namespace Manager.Services.Clients.Accessor.Models.Achievements;

public class UpdateUserProgressAccessorRequest
{
    public required string Feature { get; set; }
    public int Count { get; set; }
}
