namespace Manager.Models.Meetings;

public sealed record AcsTokenResponse
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
    public required string GroupId { get; set; }
}
