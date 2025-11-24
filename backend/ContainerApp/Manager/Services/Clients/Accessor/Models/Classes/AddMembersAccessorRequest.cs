namespace Manager.Services.Clients.Accessor.Models.Classes;

/// <summary>
/// Request model sent to Accessor service for adding members
/// </summary>
public sealed record AddMembersAccessorRequest
{
    public required IReadOnlyList<Guid> UserIds { get; init; }
    public required Guid AddedBy { get; init; }
}
