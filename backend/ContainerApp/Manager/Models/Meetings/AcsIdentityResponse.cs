namespace Manager.Models.Meetings;

public sealed record AcsIdentityResponse
{
    public required string AcsUserId { get; set; }
}
