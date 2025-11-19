namespace Manager.Models.Users;

public sealed record CallerContextDto
{
    public string? CallerRole { get; init; }
    public Guid CallerId { get; init; }
}
