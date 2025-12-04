namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Request model sent to Accessor service for removing members
/// </summary>
public sealed record RemoveMembersAccessorRequest
{
    public required IReadOnlyList<Guid> UserIds { get; init; }
}
