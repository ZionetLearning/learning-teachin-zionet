namespace Manager.Models.Users;

public sealed class CallerContextDto
{
    public string? CallerRole { get; init; }
    public Guid CallerId { get; init; }
}
