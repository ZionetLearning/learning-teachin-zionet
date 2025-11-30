namespace Accessor.Models.Classes.Requests;

/// <summary>
/// Request model for removing members from a class
/// </summary>
public sealed record RemoveMembersRequest
{
    public required IReadOnlyList<Guid> UserIds { get; init; }
}

