namespace Manager.Services.Clients.Accessor.Models.Users;

public sealed record GetUsersForCallerAccessorRequest
{
    public string? CallerRole { get; init; }
    public Guid CallerId { get; init; }
}
